using Domain.Dto.Application;
using Domain.Entities;
using Domain.Entities.Application;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Areas.Applicants.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using School_Management_System.Models;
using SendGrid.Helpers.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace School_Management_System.Controllers
{
    [Area("Applicants")]
    [Route("Applicants/[action]")]
    [Authorize(Roles = "applicant,student")]
    public class ApplicantsController : Controller
    {
        bool app_completed = false;
        private readonly SMSDbContext _dbcontext;
        private readonly IWebHostEnvironment _env;
        public ApplicantsController(SMSDbContext dbcontext, IWebHostEnvironment env)
        {
            _dbcontext = dbcontext;
            _env = env;
        }
      
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity.Name;
            var user = await _dbcontext.Bandas.AsNoTracking().FirstOrDefaultAsync(b => b.Ban_username == email);
            if (user == null) return Unauthorized();

            var applicant = await _dbcontext.Applicants
                .AsNoTracking() // Avoid EF concurrency issues
                .FirstOrDefaultAsync(a => a.CreatedBy == user.BanId);

            // DEFAULT STATE (NO APPLICATION)
            int step = 0;
            int progress = 0;
            string nextMessage = "Please start your application.";
            bool showChallanButton = false;
            bool showViewApplication = false;
            bool canApply = true;

            if (applicant != null)
            {
                canApply = false;
                showViewApplication = true;

                switch (applicant.Status)
                {
                    case "Completed":
                        step = 1;
                        progress = 25;
                        nextMessage = "Application submitted. Please proceed to payment.";
                        canApply = false;
                        break;
                    case "FeePaid":
                        step = 2;
                        progress = 50;
                        nextMessage = "Fee paid.Awaiting for verification.";
                        showChallanButton = true;
                        break;
                    case "Verified":
                        step = 3;
                        progress = 70;
                        nextMessage = " Verifcation Complete.";
                        showChallanButton = true;
                        break;
                    case "Admitted":
                        step = 4;
                        progress = 100;
                        nextMessage = "Admission completed successfully.";
                        showChallanButton = true;
                        break;
                }
            }

            ViewBag.Step = step;
            ViewBag.Progress = progress;
            ViewBag.NextMessage = nextMessage;
            ViewBag.CanApply = canApply;
            ViewBag.ShowViewApplication = showViewApplication;
            ViewBag.ShowChallanButton = showChallanButton;

            return RedirectToAction("Application");
        }


        [HttpGet]
        public async Task<IActionResult> Application()
        {
            var model = new ApplicantViewModel();

            // Get current user
            var email = User.Identity.Name;
            var user = await _dbcontext.Bandas.FirstOrDefaultAsync(b => b.Ban_username == email);

            // Default ViewBag
            ViewBag.Step = 0;
            ViewBag.Progress = 0;
            ViewBag.NextMessage = "Please start your application.";
            ViewBag.CanApply = true;
            ViewBag.ShowViewApplication = false;
            ViewBag.ShowChallanButton = false;

            if (user != null)
            {
                var applicant = await _dbcontext.Applicants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.CreatedBy == user.BanId);

                if (applicant != null)
                {
                    ViewBag.CanApply = false;
                    ViewBag.ShowViewApplication = true;

                    switch (applicant.Status)
                    {
                        case "Completed":
                            ViewBag.Step = 1;
                            ViewBag.Progress = 25;
                            ViewBag.NextMessage = "Application submitted. Awaiting verification.";
                            break;

                        case "FeePaid":
                            ViewBag.Step = 2;
                            ViewBag.Progress = 50;
                            ViewBag.NextMessage = "Fee paid.Awaiting for verification.";
                            ViewBag.ShowChallanButton = true;
                            break;
                        case "Verified":
                            ViewBag.Step = 3;
                            ViewBag.Progress = 75;
                            ViewBag.NextMessage = " Verifcation Complete.";
                            ViewBag.ShowChallanButton = true;
                            break;

                        case "Admitted":
                            ViewBag.Step = 4;
                            ViewBag.Progress = 100;
                            ViewBag.NextMessage = "Admission completed successfully.";
                            ViewBag.ShowChallanButton = true;
                            break;
                    }
                }
            }

            // --- Load Classes safely to avoid EF concurrency issue ---
            var categoryDisplayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "preprimary", "Pre Primary" },
        { "primary", "Primary" },
        { "middle", "Middle" },
        { "high", "High" }
    };

            var classes = await _dbcontext.Classes
                .Where(c => c.Active)
                .AsNoTracking()
                .ToListAsync();

            var groupedData = classes
                .GroupBy(c => c.Category)
                .Select(g => new ClassCategoryViewModel
                {
                    Category = g.Key,
                    DisplayCategory = categoryDisplayMap.ContainsKey(g.Key.Trim().ToLower())
                                      ? categoryDisplayMap[g.Key.Trim().ToLower()]
                                      : g.Key,
                    Classes = g.OrderBy(c => c.Id).ToList(),
                    MinId = (int)g.Min(c => c.Id),
                    QuickLinksPerClass = new Dictionary<int, List<QuickLink>>()
                })
                .OrderBy(g => g.MinId)
                .ToList();

            // Attach QuickLinks
            foreach (var category in groupedData)
            {
                foreach (var cls in category.Classes)
                {
                    var links = await _dbcontext.QuickLinks
                        .AsNoTracking()
                        .Where(q => q.ClassId == cls.Id)
                        .ToListAsync();

                    category.QuickLinksPerClass[(int)cls.Id] = links;
                }
            }

            model.ClassCategoryList = groupedData;

            // Order tabs
            var orderMap = new Dictionary<string, int>
    {
        { "preprimary", 1 },
        { "primary", 2 },
        { "middle", 3 },
        { "high", 4 }
    };

            ViewBag.EducationTab = classes
                .Select(c => c.Category.Trim().ToLower().Replace(" ", ""))
                .Distinct()
                .OrderBy(c => orderMap.ContainsKey(c) ? orderMap[c] : int.MaxValue)
                .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromBody] AppliedClassViewModel model)
        {
            try
            {
                if (model == null || model.AppliedForClassId == null || model.AppliedForClassId == 0)
                    return BadRequest(new { success = false, message = "Invalid class selection" });

                var user=_dbcontext.Bandas.FirstOrDefault(b => b.Ban_username == User.Identity.Name);

                var applicant = new Applicant
                {
                    AppliedForClassId = model.AppliedForClassId,
                    CreatedBy = user.BanId,
                };

                _dbcontext.Applicants.Add(applicant);
                await _dbcontext.SaveChangesAsync();

                return Ok(new { success = true, applicantId = applicant.Id });
            }
            catch (Exception ex)
            {
                // log error properly
                Console.WriteLine(ex);
                Console.WriteLine(ex.ToString());
                // return JSON instead of HTML
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveStep2([FromForm] ApplicantViewModel model)
        {
            bool modelValidity = model.ApplicantId == 0 || model.CreateApplicant.BFormNumber == null || 
                                 model.CreateApplicant.FullName == null  || model.CreateApplicant.MotherTongue == null || 
                                 model.CreateApplicant.BFormFile == null || model.CreateApplicant.Contact == null ||
                                 model.CreateApplicant.DateOfBirth == null || model.CreateApplicant.Gender == null || 
                                 model.CreateApplicant.ProfilePic == null;
            if (modelValidity)
                return BadRequest("Invalid step 2 data.");
            try
            {
                var applicant = await _dbcontext.Applicants.FindAsync(model.ApplicantId);
                var applicant_doc = await _dbcontext.Applicants.FindAsync(model.ApplicantId);
                if (applicant == null)
                {
                    return NotFound(new { success = false, message = "Applicant not found" });
                }

                applicant.FullName = model.CreateApplicant.FullName;
                applicant.Gender = model.CreateApplicant.Gender;
                applicant.DateOfBirth = model.CreateApplicant.DateOfBirth;
                applicant.MotherTongue = model.CreateApplicant.MotherTongue;
                applicant.BFormNumber = model.CreateApplicant.BFormNumber;
                applicant.emailAddress = User.Identity.Name;
                if (model.CreateApplicant.BFormFile != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = AFileName(model.ApplicantId, 1, "BFM") + Path.GetExtension(model.CreateApplicant.BFormFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await model.CreateApplicant.BFormFile.CopyToAsync(stream);

                    //applicant.BFormFilePath = "/uploads/" + fileName;
                }


                if (model.CreateApplicant.ProfilePic != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = AFileName(model.ApplicantId, 1, "PIC") + Path.GetExtension(model.CreateApplicant.ProfilePic.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await model.CreateApplicant.ProfilePic.CopyToAsync(stream);

                    //applicant.ProfilePicFilePath = "/uploads/" + fileName;
                }

                _dbcontext.Applicants.Update(applicant);
                await _dbcontext.SaveChangesAsync();
                try
                {
                    return Ok(new { success = true, applicantId = applicant.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                // log error properly
                Console.WriteLine(ex);

                // return JSON instead of HTML
                return StatusCode(500, new { success = false, message = ex.Message });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SaveStep3([FromForm] ApplicantViewModel model)
        {
            if (model.ApplicantId == 0)
            {

                return NotFound(new { success = false, message = "Applicant not found" });
            }
            if (model.ApplicantEducation.Category <= 2)
            {
                var applicant = await _dbcontext.Applicants.FindAsync(model.ApplicantId);

                if (applicant == null)
                    return NotFound(new { success = false, message = "Applicant not found" });

                applicant.classCategory = model.ApplicantEducation.Category;
                _dbcontext.Applicants.Update(applicant);
                await _dbcontext.SaveChangesAsync();
                return Ok(new { success = true, applicantId = model.ApplicantId });
            }
            bool modelValidity= model.ApplicantEducation.PrevSchool ==null 
                || model.ApplicantEducation.PreviousSchoolCertid== null 
                || model.ApplicantEducation.Percentage==0 
                || model.ApplicantEducation.YearsAttended==null
                || model.ApplicantEducation.Grade==null;

            if (!modelValidity)
            {
                try
                {
                    var applicant = await _dbcontext.Applicants.FindAsync(model.ApplicantId);
                    if (applicant == null)
                        return NotFound(new { success = false, message = "Applicant not found" });

                    applicant.PrevSchool = model.ApplicantEducation.PrevSchool;
                    applicant.Percentage = model.ApplicantEducation.Percentage ?? 0; // handle null
                    applicant.YearsAttended = model.ApplicantEducation.YearsAttended;
                    applicant.Grade = model.ApplicantEducation.Grade;
                    applicant.classCategory = model.ApplicantEducation.Category;

                    if (model.ApplicantEducation.PreviousSchoolCertid != null)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = AFileName(model.ApplicantId, 1, "PSC") + Path.GetExtension(model.ApplicantEducation.PreviousSchoolCertid.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await model.ApplicantEducation.PreviousSchoolCertid.CopyToAsync(stream);

                        //applicant.PreviousSchoolCertFilePath = "/uploads/" + fileName;
                    }
                    if (model.ApplicantEducation.PreviousSchoolLeavCertid != null)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = AFileName(model.ApplicantId, 1, "PSLC") + Path.GetExtension(model.ApplicantEducation.PreviousSchoolCertid.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await model.ApplicantEducation.PreviousSchoolLeavCertid.CopyToAsync(stream);

                        //applicant.PreviousSchoolLeavCertFilePath = "/uploads/" + fileName;
                    }

                    _dbcontext.Applicants.Update(applicant);
                    await _dbcontext.SaveChangesAsync();

                    return Ok(new { success = true, applicantId = applicant.Id });
                }
                catch (Exception ex)
                {
                    // log error properly
                    Console.WriteLine(ex);

                    // return JSON instead of HTML
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }
            return StatusCode(500, new { success = false, message = "Model Not Valid" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveStep4([FromForm]ApplicantViewModel model, string Siblings, string Guardians)
        {
            
            if (!string.IsNullOrEmpty(Guardians))
                model.Guardians = JsonConvert.DeserializeObject<List<CreateGuardianViewModel>>(Guardians);
            for (int i = 0; i < model.Guardians.Count; i++)
            {
                model.Guardians[i].CnicFront = Request.Form.Files[$"Guardians[{i}].CnicFront"];
                model.Guardians[i].CnicBack = Request.Form.Files[$"Guardians[{i}].CnicBack"];
            }

            try
            {
                foreach (var guard in model.Guardians)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string cnicFrontPath = "";
                    string cnicBackPath = "";
                    if (guard.CnicFront != null && guard.CnicFront.Length > 0)
                    {
                        var fileNameFront = AFileName(model.ApplicantId, 1, "GNICF")
                                            + Path.GetExtension(guard.CnicFront.FileName);
                        var filePathFront = Path.Combine(uploadsFolder, fileNameFront);

                        using (var stream = new FileStream(filePathFront, FileMode.Create))
                            await guard.CnicFront.CopyToAsync(stream);

                        cnicFrontPath = "/uploads/" + fileNameFront;
                    }
                    if (guard.CnicBack != null && guard.CnicBack.Length > 0)
                    {
                        var fileNameBack = AFileName(model.ApplicantId, 1, "GNICB")
                                           + Path.GetExtension(guard.CnicBack.FileName);
                        var filePathBack = Path.Combine(uploadsFolder, fileNameBack);

                        using (var stream = new FileStream(filePathBack, FileMode.Create))
                            await guard.CnicBack.CopyToAsync(stream);

                        cnicBackPath = "/uploads/" + fileNameBack;
                    }

                    var appGuard = new ApplicantGuardian
                    {
                        ApplicantId = model.ApplicantId,
                        FullName = guard.GuardName,
                        RelationWithApplicant = guard.GuardRelation,
                        Gender = guard.GuardGender,
                        ContactNumber = guard.Contact,
                        Occupation = guard.Occupation,
                        Address = guard.Address,
                        Email = guard.Email,
                        Cnic = guard.CNIC,
                        CnicFileName = AFileName(model.ApplicantId, 1, "GNICF") + " || " + AFileName(model.ApplicantId, 1, "GNICB")
                    };
                    _dbcontext.ApplicantGuardians.Add(appGuard);
                }

               
                
                await _dbcontext.SaveChangesAsync();
                return Json(new { success = true, message = "Step 4 data saved successfully" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message: ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                errorMessage = ex.Message;
                Console.WriteLine(errorMessage);

                return StatusCode(500, new { success = false, message = errorMessage });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveStep5([FromForm] ApplicantViewModel model, string Siblings) 
        {
            if (!string.IsNullOrEmpty(Siblings))
                model.Siblings = JsonConvert.DeserializeObject<List<AddSiblingViewModel>>(Siblings);
            try 
            {
                foreach (var sib in model.Siblings)
                {
                    if (sib.Name != null && sib.Name.Trim() != "")
                    {
                        var sibling = new ApplicantSibling
                        {
                            ApplicantId = model.ApplicantId,
                            Name = sib.Name,
                            classNo = sib.ClassText,
                            Bform = sib.BForm,
                            SchoolId = 1
                        };
                        _dbcontext.ApplicantSiblings.Add(sibling);
                    }
                }
                await _dbcontext.SaveChangesAsync();
                return Json(new { success = true, message = "Step 5 data saved successfully" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                errorMessage = ex.Message;
                Console.WriteLine(errorMessage);

                return StatusCode(500, new { success = false, message = errorMessage });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ApplicationSummary(int applicantId)
        {
            var applicant = await _dbcontext.Applicants
                .Include(x => x.Guardians)
                .Include(x => x.Siblings)
                .FirstOrDefaultAsync(x => x.Id == applicantId);

            if (applicant == null)
                return NotFound();

            var model = new ApplicantViewModel
            {
                ApplicantId = applicant.Id,
                IsClassSelectionCompleted = applicant.AppliedForClassId > 0,
                IsPersonalInfoCompleted = !string.IsNullOrWhiteSpace(applicant.FullName)
                                           && !string.IsNullOrWhiteSpace(applicant.Gender),
                IsEducationInfoCompleted = applicant.classCategory > 0,
                IsGuardianSiblingInfoCompleted = applicant.Guardians != null && applicant.Guardians.Any() && applicant.Siblings != null && applicant.Siblings.Any(),
                
            };
            if (model.IsClassSelectionCompleted &&
                model.IsPersonalInfoCompleted &&
                model.IsEducationInfoCompleted &&
                model.IsGuardianSiblingInfoCompleted)
            {
                applicant.Status = "ReadyForSubmission";
                _dbcontext.Update(applicant);
                await _dbcontext.SaveChangesAsync();
            }

            return PartialView("_ApplicationSummaryPartial", model);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitApplication([FromForm] ApplicantViewModel model)
        {
            if (model.ApplicantId <= 0)
                return Json(new { success = false, message = "Invalid Applicant" });

            var applicant = await _dbcontext.Applicants.FirstOrDefaultAsync(x => x.Id == model.ApplicantId);
            if (applicant == null)
                return Json(new { success = false, message = "Applicant not found" });

            if (applicant.Status != "ReadyForSubmission")
                return Json(new { success = false, message = "Complete all steps first" });

            applicant.Status = "Completed";
            applicant.ApplicationDate = DateTime.Now;

            _dbcontext.Update(applicant);
            await _dbcontext.SaveChangesAsync();

            // ✅ Return JSON with redirect to Dashboard
            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Application", "Dashboard")
            });
        }


        private string AFileName(long ApplicantID,int schoolID,string format)
        {
            string filename;
            filename = "SID" + schoolID.ToString() + "AID" + ApplicantID.ToString() + format + (DateTimeOffset.Now.ToString()).Substring(0, 18);
            filename = filename.Replace("/", "");
            filename = filename.Replace(":", "");
            filename = filename.Replace(" ", "");
            return filename;
        }

        //==================0 VIEW APPLICATION SECTION 0===========================
        [HttpGet]
        public async Task<IActionResult> GetApplications()
        {
            var email = User.Identity.Name;
            var user = await _dbcontext.Bandas.FirstOrDefaultAsync(b => b.Ban_username == email);

            if (user == null) return Unauthorized();

            var applicant = await _dbcontext.Applicants
                .AsNoTracking()
                .Include(a => a.Guardians)
                .Include(a => a.Siblings)
                .FirstOrDefaultAsync(a => a.CreatedBy == user.BanId);

            if (applicant == null)
                return PartialView("_NoApplicationPartial");

            var model = new ApplicantViewModel
            {
                ApplicantId = applicant.Id,
                CreateApplicant = new CreateApplicantViewModel { FullName = applicant.FullName, status = applicant.Status,
                    /*ProfilePicFilePath = applicant.ProfilePicFilePath,*/ Gender=applicant.Gender, DateOfBirth=applicant.DateOfBirth,
                     MotherTongue=applicant.MotherTongue, BFormNumber=applicant.BFormNumber,Contact=applicant.contact},
                AppliedClass = new AppliedClassViewModel { classDesc = fetchClass(applicant.AppliedForClassId) },
                Guardians = GetGuardiansByApplicantId(applicant.Id),
                Siblings = GetSiblingsByApplicantId(applicant.Id),
                
            };

            return PartialView("_ViewApplicationPartial", model);
        }
        private string fetchClass(long? id)
        {
            var cls = _dbcontext.Classes.FirstOrDefault(x => x.Id == id);
            return cls.Description;
        }
        public List<CreateGuardianViewModel> GetGuardiansByApplicantId(long applicantId)
        {
            return _dbcontext.ApplicantGuardians
                .Where(g => g.ApplicantId == applicantId)
                .Select(g => new CreateGuardianViewModel
                {
                    GuardName=g.FullName,
                    GuardRelation=g.RelationWithApplicant,
                    CNIC=g.Cnic,
                    Occupation=g.Occupation,
                    Contact=g.ContactNumber
                })
                .ToList();
        }
        public List<AddSiblingViewModel> GetSiblingsByApplicantId(long applicantId)
        {
            return _dbcontext.ApplicantSiblings
                .Where(g => g.ApplicantId == applicantId)
                .Select(g => new AddSiblingViewModel
                {
                    Name=g.Name,
                    BForm=g.Bform,
                    ClassText=g.classNo
                })
                .ToList();
        }

    }
}