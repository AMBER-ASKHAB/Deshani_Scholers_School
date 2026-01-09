using Domain.Dto;
using Domain.Dto.Admin;
using Domain.Entities;
using Domain.Entities.Application;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Areas.Admin.Models;
using SendGrid.Helpers.Mail;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;

namespace School_Management_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly SMSDbContext mDB;

        public AdminDashboardController(SMSDbContext db)
        {
            mDB = db;
        }
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            long schoolID = 1;
            ViewBag.Classes = mDB.Classes.Where(x => x.SchoolId == schoolID).ToList();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetFeeBySchool(string category)
        {
            if (category != "All")
            {
                var fee = await mDB.feeAndCharges
                        .Where(x => x.fac_category == category)
                        .Select(c => new
                        {
                            Category = c.fac_category,
                            FeeHead = c.fac_feeHead,
                            feeId = c.fac_id,
                        })
                        .ToListAsync();
                return Json(fee);
            }

            if (category == "All")
            {
                var fee = await mDB.feeAndCharges
                        .Select(c => new
                        {
                            Category = c.fac_category,
                            FeeHead = c.fac_feeHead,
                            feeId = c.fac_id,
                        })
                        .ToListAsync();
                return Json(fee);
            }
            return Json("");
        }
        [HttpGet]
        public JsonResult GetFeeByClass(long classID, string type)
        {
            long schoolID = 1;

            if (classID != -1)
            {
                // Specific class fees that have an amount record for that class
                var fees = (from fa in mDB.feeAndChargesAmounts
                            join f in mDB.feeAndCharges
                                on fa.fcm_FeeHead equals f.fac_id
                            where fa.fcm_clsID == classID
                            select new
                            {
                                facId = f.fac_id,         // fee head id
                                fcmId = fa.fcm_id,       // amount record id
                                feeHead = f.fac_feeHead,
                                feeAmount = fa.fcm_amount
                            }).ToList();

                return Json(fees);
            }
            else
            {
                // Admission / Others: show fee heads in this category, and include existing amount record (if any)
                var fees = (from f in mDB.feeAndCharges
                            where f.fac_schid == schoolID && f.fac_category == type
                            select new
                            {
                                facId = f.fac_id,
                                // try to find existing amount record for this fee head (for the school; not class-specific)
                                fcmId = mDB.feeAndChargesAmounts
                                            .Where(fa => fa.fcm_FeeHead == f.fac_id)
                                            .Select(fa => (long?)fa.fcm_id)
                                            .FirstOrDefault(),
                                feeHead = f.fac_feeHead,
                                feeAmount = mDB.feeAndChargesAmounts
                                                .Where(fa => fa.fcm_FeeHead == f.fac_id)
                                                .Select(fa => (int?)fa.fcm_amount)
                                                .FirstOrDefault() ?? 0
                            }).ToList();

                return Json(fees);
            }
        }
        [HttpGet]
        public JsonResult GetFeeHeads(string category)
        {
            var feeHeads = mDB.feeAndCharges
                .Where(x => x.fac_category == category)
                .Select(f => new
                {
                    id = f.fac_id,
                    feeHEAD = f.fac_feeHead
                }).ToList();

            return Json(feeHeads);
        }
        [HttpGet]
        public JsonResult GetClassTeachingMode(long id)
        {
            var result = mDB.Classes
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    subjectWiseTeaching = c.SubjectWiseTeaching
                })
                .FirstOrDefault();

            return Json(result);
        }

        [HttpGet]
        public JsonResult GetSectionByClass(long id)
        {
            var sections = mDB.classSections.Where(x => x.cse_classId == id)
                .Select(f => new
                {
                    id = f.cse_id,
                    prefix = f.cse_prefix
                }).ToList();
            return Json(sections);
        }
        [HttpGet]
        public JsonResult GetTeachers()
        {
            var teachers = mDB.schoolStaff
                .Select(f => new
                {
                    id = f.stf_id,
                    name = f.stf_name
                }).ToList();
            return Json(teachers);
        }
        [HttpGet]
        public JsonResult GetSubjectsByClass(long classId)
        {
            var subjects = (
                from cs in mDB.ClassSubjects
                join s in mDB.Subjects
                    on cs.csb_subjectID equals s.sub_id
                where cs.csb_classid == classId
                select new
                {
                    id = cs.csb_subjectID,
                    name = s.sub_description
                }
            ).ToList();

            return Json(subjects);
        }
       
        private static List<string> GenerateTimeSlots()
        {
            return new List<string>
            {
                "08:30-09:15",
                "09:15-10:00",
                "10:00-10:45",
                "10:45-11:30",
                "12:00-12:45", // after break
                "12:45-13:30",
                "13:30-14:15"
            };
        }

        [HttpPost]
        public IActionResult AllocateTeacher([FromBody] AllocateTeacher model)
        {
            long classId = model.classId;
            long sectionId = model.sectionId;
            long subjectId = model.subjectId;
            long teacherId = model.teacherId;

            var timeSlots = GenerateTimeSlots();

            foreach (var slot in timeSlots)
            {
                bool teacherBusy = mDB.ClassTimeTable.Any(x =>
                    x.ctt_timeSlot == slot &&
                    (x.ctt_Monday.Contains($"|{teacherId}") ||
                     x.ctt_Tuesday.Contains($"|{teacherId}") ||
                     x.ctt_Wednesday.Contains($"|{teacherId}") ||
                     x.ctt_Thursday.Contains($"|{teacherId}") ||
                     x.ctt_Friday.Contains($"|{teacherId}") ||
                     x.ctt_Saturday.Contains($"|{teacherId}"))
                );

                if (teacherBusy) continue;
                bool classBusy = mDB.ClassTimeTable.Any(x =>
                   x.ctt_timeSlot == slot &&
                   x.ctt_classid == classId &&
                   x.ctt_sectionid == sectionId
               );

                if (classBusy) continue;
                var entry = new ClassTimeTable
                {
                    ctt_schid = 1,
                    ctt_classid = classId,
                    ctt_sectionid = sectionId,
                    ctt_timeSlot = slot,
                    ctt_Monday = $"{subjectId}|{teacherId}",
                    ctt_Tuesday = $"{subjectId}|{teacherId}",
                    ctt_Wednesday = $"{subjectId}|{teacherId}",
                    ctt_Thursday = $"{subjectId}|{teacherId}",
                    ctt_Friday = $"{subjectId}|{teacherId}",
                    ctt_Saturday = $"{subjectId}|{teacherId}"
                };

                mDB.ClassTimeTable.Add(entry);
                mDB.SaveChanges();
                return Ok("Teacher allocated successfully");
            }

            return BadRequest("Teacher already occupied in all time slots");
        }
        [HttpGet]
        public IActionResult GetTimeTable(int classId, int sectionId)
        {
            var data = mDB.ClassTimeTable
                .Where(t => t.ctt_classid == classId && t.ctt_sectionid == sectionId)
                .OrderBy(t => t.ctt_timeSlot)
                .ToList();

            var result = data.Select(t => new
            {
                timeSlot = t.ctt_timeSlot,
                monday = ResolveCell(t.ctt_Monday),
                tuesday = ResolveCell(t.ctt_Tuesday),
                wednesday = ResolveCell(t.ctt_Wednesday),
                thursday = ResolveCell(t.ctt_Thursday),
                friday = ResolveCell(t.ctt_Friday),
                saturday = ResolveCell(t.ctt_Saturday)
            });

            return Json(result);
        }

        // ⭐ CORE LOGIC (SubjectID | TeacherID parsing)
        private string ResolveCell(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";

            var parts = value.Split('|');
            int subjectId = int.Parse(parts[0]);
            int teacherId = int.Parse(parts[1]);

            var subject =
                    (from cs in mDB.ClassSubjects
                     join s in mDB.Subjects
                         on cs.csb_subjectID equals s.sub_id
                     where cs.csb_subjectID == subjectId
                     select s.sub_description)
                    .FirstOrDefault();

            var teacher = mDB.schoolStaff
                .Where(t => t.stf_id == teacherId)
                .Select(t => t.stf_name)
                .FirstOrDefault();

            return $"{subject}<br/><small>{teacher}</small>";
        }

        [HttpGet]
        public JsonResult GetFeeDetails(long id)
        {
            // try as fcm_id first
            var feeAmountRecord = (from fa in mDB.feeAndChargesAmounts
                                   join f in mDB.feeAndCharges on fa.fcm_FeeHead equals f.fac_id
                                   where fa.fcm_id == id
                                   select new
                                   {
                                       fcmId = fa.fcm_id,
                                       category = f.fac_category,
                                       classID = fa.fcm_clsID,
                                       feeHead = fa.fcm_FeeHead,
                                       feeAmount = fa.fcm_amount
                                   }).FirstOrDefault();

            if (feeAmountRecord != null)
            {
                return Json(new
                {
                    id = feeAmountRecord.fcmId,
                    feeAmountRecord.category,
                    feeAmountRecord.classID,
                    feeAmountRecord.feeHead,
                    feeAmountRecord.feeAmount
                });
            }

            // If not found by fcm_id, treat id as fac_id (fee head) and return feeHead info (no amount record yet)
            var feeHeadInfo = (from f in mDB.feeAndCharges
                               where f.fac_id == id
                               select new
                               {
                                   id = 0, // indicates no fcm record exists
                                   category = f.fac_category,
                                   classID = (long?)null,
                                   feeHead = f.fac_id,
                                   feeAmount = 0
                               }).FirstOrDefault();

            if (feeHeadInfo == null)
            {
                return Json(new { success = false, message = "Fee head not found" });
            }

            return Json(feeHeadInfo);
        }
        [HttpGet]
        public IActionResult GetClassesSetup()
        {
            var classes = mDB.Classes
                .Select(c => new { Id = c.Id, Description = c.Description, SubjectWiseTeaching = c.SubjectWiseTeaching })
                .ToList();

            return Json(classes);
        }
        [HttpGet]
        public IActionResult GetLatestStaff()
        {
            // Assuming 'Staffs' is your DbSet<Staff> and 'Id' is primary key
            var latestStaff = mDB.schoolStaff
                                      .OrderByDescending(s => s.stf_id) // or s.CreatedDate if exists
                                      .FirstOrDefault();

            if (latestStaff != null)
            {
                return Json(new
                {
                    success = true,
                    stf_id = latestStaff.stf_id,
                    stf_name = latestStaff.stf_name,
                    stf_staffnumber = latestStaff.stf_staffId
                });
            }
            else
            {
                return Json(new { success = false, message = "No staff found" });
            }
        }
        [HttpGet]
        public JsonResult GetFeeHeadsC()
        {
            long schoolID = 1;
            var fees = (from f in mDB.feeAndCharges
                        join fa in mDB.feeAndChargesAmounts
                            on f.fac_id equals fa.fcm_FeeHead into feeJoin
                        from fa in feeJoin.DefaultIfEmpty()
                        where f.fac_schid == schoolID
                              && f.fac_category == "Admission Charges"
                        select new
                        {
                            facId = f.fac_id,               // fee head id
                            fcmId = fa != null ? fa.fcm_id : 0,  // amount record id (0 if not set)
                            feeHead = f.fac_feeHead,
                            feeAmount = fa != null ? fa.fcm_amount : 0
                        }).ToList();

            return Json(fees);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateChallan(AdminDashboardViewModel model)
        {
            var challan = model.GeneratedChallan;

            if (challan == null)
                return BadRequest("Invalid challan data");

            var applicant = mDB.Applicants.FirstOrDefault(x => x.Id == challan.app_id);
            if (applicant == null)
                return BadRequest("Applicant not found");

            var school = mDB.schools.FirstOrDefault(x => x.sch_id == applicant.SchoolId);
            if (school == null)
                return BadRequest("School not found");

            using var transaction = mDB.Database.BeginTransaction();

            try
            {
                // 1️⃣ Save challan heads
                SaveFeeHeads(challan, school.sch_id);

                // 2️⃣ Save main challan
                var admissionChallan = SaveAdmissionChallan(challan, applicant, school);

                // 3️⃣ Save challan details
                SaveAdmissionChallanDetails(challan, admissionChallan, school.sch_id);
                var printDto = BuildGeneratedChallanDTO(admissionChallan,applicant,school);
                ViewBag.PrintDTO = printDto;
                transaction.Commit();

                return Json(new { success = true,data=printDto });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        private void SaveFeeHeads(GeneratedChallanViewModel model, long schoolId)
        {
            var challanHeads = model.FeeHeads.Select(fee => new ChallanHeads
            {
                chh_schid = (int)schoolId,
                chh_Appid = model.app_id,
                chh_name = fee.Key,
                chh_amount = fee.Value
            }).ToList();

            mDB.ChallanHeads.AddRange(challanHeads);
            mDB.SaveChanges();
        }
        private AdmissionChallan SaveAdmissionChallan(GeneratedChallanViewModel model,Applicant applicant,Schools school)
        {
            var challan = new AdmissionChallan
            {
                acf_schid = school.sch_id,
                acf_appid = applicant.Id,
                acf_dateofissue = DateTime.Now,
                acf_tobepaidby = model.dueDate,
                acf_expirydate = model.dueDate.AddDays(model.fineDate),
                acf_preparedby = 1, // TODO: Logged-in staff
                acf_totalamount = model.FeeHeads.Sum(x => x.Value),
                acf_fineAfterDueDate = model.fineDate,
                acf_amountHeads = model.FeeHeads.Count
            };

            mDB.admissionChallan.Add(challan);
            mDB.SaveChanges(); // PK GENERATED HERE

            return challan;
        }
        private void SaveAdmissionChallanDetails(GeneratedChallanViewModel model,AdmissionChallan challan,long schoolId)
        {
            var details = model.FeeHeads.Select(fee => new AdmissionChallanDetails
            {
                acd_schid = schoolId,
                acd_acfid = challan.acf_id,     // FK from parent
                acd_detialid = fee.FcmId,       // Or FacId (your business logic)
                acd_chargestype = fee.Key,
                acd_amount = fee.Value
            }).ToList();

            mDB.admissionChallanDetails.AddRange(details);
            mDB.SaveChanges();
        }
        private GeneratedChallanPrintDTO BuildGeneratedChallanDTO(AdmissionChallan challan,Applicant applicant,Schools school)
        {
            // Bank details
            var bank = mDB.bankDetails
                .FirstOrDefault(x => x.bnk_schid == school.sch_id);

            // Guardian (pick first)
            var guardian = mDB.ApplicantGuardians
                .FirstOrDefault(x => x.Id == applicant.Id);

            // Fee heads (from DB)
            var feeHeads = mDB.ChallanHeads
                .Where(x =>
                    x.chh_Challanid == challan.acf_id &&
                    x.chh_Appid == applicant.Id)
                .Select(x => new ChallanFeeHeadDTO
                {
                    FeeName = x.chh_name,
                    Amount = x.chh_amount
                })
                .ToList();

            return new GeneratedChallanPrintDTO
            {
                // School
                SchoolName = school.sch_name,
                BankName = bank?.bnk_name,
                BranchLocation = bank?.bnk_branchLocation,
                AccountTitle = bank?.bnk_accounttitle,
                AccountNumber = bank?.bnk_accountnumber,

                // Applicant
                ApplicantName = applicant.FullName,
                GuardianName = guardian?.FullName ?? "N/A",
                RegistrationNo = applicant.Id.ToString(),
                ClassName = applicant.AppliedForClassId?.ToString(),

                // Challan
                DueDate = challan.acf_tobepaidby,
                FineDays = (int)challan.acf_fineAfterDueDate,
                TotalAmount = challan.acf_totalamount,
                FeeHeads = feeHeads
            };
        }


        private void makingViewBags(Applicant applicant, Schools school, ChallanFormViewModel model)
        {
            var bank = mDB.bankDetails.FirstOrDefault(x => x.bnk_schid == school.sch_id);
            var challanDetails = mDB.admissionChallan.FirstOrDefault(x => x.acf_appid == applicant.Id);
            var guardian = mDB.ApplicantGuardians.FirstOrDefault(x => x.ApplicantId == applicant.Id);

            ViewBag.Applicant = applicant;
            ViewBag.School = school;
            ViewBag.Bank = bank;
            ViewBag.ChallanDetails = challanDetails;

            // Send the edited fee heads from the form (dictionary)
            ViewBag.FeeDetails = model.FeeHeads;

            ViewBag.Guardian = guardian;
            ViewBag.Class = mDB.Classes.FirstOrDefault(x => x.Id == applicant.AppliedForClassId);
            ViewBag.FineDate = model.fineDate;
        }
        [HttpGet("loadApplicants")]
        public JsonResult loadApplicants()
        {
            var applicants = mDB.Applicants.Where(x => x.Status == "Completed").ToList();
            return Json(applicants);
        }
        [HttpGet]
        public IActionResult SearchApplicant(long appId = 0, string bForm = null)
        {
            try
            {
                if (appId == 0 && string.IsNullOrEmpty(bForm))
                    return Json(new { success = false, message = "Please provide search criteria" });

                Applicant applicant = null;

                // Search by Application ID
                if (appId != 0)
                {
                    applicant = mDB.Applicants.FirstOrDefault(x => x.Id == appId);
                }
                // Search by B-Form
                else if (!string.IsNullOrEmpty(bForm))
                {
                    applicant = mDB.Applicants.FirstOrDefault(x => x.BFormNumber == bForm);
                }

                if (applicant != null)
                {
                    var guardian = mDB.ApplicantGuardians.FirstOrDefault(x => x.ApplicantId == applicant.Id);
                    var classInfo = mDB.Classes.FirstOrDefault(x => x.Id == applicant.AppliedForClassId);

                    // Return success with all data as anonymous objects
                    return Json(new
                    {
                        success = true,
                        applicant = new
                        {
                            id = applicant.Id,
                            applicationId = applicant.Id,
                            bFormNumber = applicant.BFormNumber,
                            fullName = applicant.FullName,
                            dateOfBirth = applicant.DateOfBirth.ToString(),
                            contactNumber = applicant.contact,
                            email = applicant.emailAddress,
                            status = applicant.Status,
                            appliedForClassId = applicant.AppliedForClassId
                            // Add other properties you need
                        },
                        guardian = guardian != null ? new
                        {
                            id = guardian.Id,
                            fullName = guardian.FullName,
                            contactNumber = guardian.ContactNumber,
                            cnic = guardian.Cnic,
                            occupation = guardian.Occupation
                            // Add other guardian properties you need
                        } : null,
                        classInfo = classInfo != null ? new
                        {
                            id = classInfo.Id,
                            name = classInfo.Prefix,
                            description = classInfo.Description
                            // Add other class properties you need
                        } : null
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Applicant not found" });
                }
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                Console.WriteLine($"Search error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new { success = false, message = "Search failed: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> StaffSetup(AdminDashboardViewModel model)
        {
            if (model?.StaffSetup?.AddStaff == null)
            {
                return Json(new { success = false, message = "Invalid payload." });
            }

            try
            {
                // check existing by staff no
                var existing = await mDB.schoolStaff
                    .FirstOrDefaultAsync(x => x.stf_staffId == model.StaffSetup.AddStaff.stfNo);

                if (existing != null)
                {
                    return Json(new { success = false, message = "Staff with this staff number already exists." });
                }

                var table = new SchoolStaff
                {
                    stf_schid = 1,
                    stf_banid = 3,
                    stf_name = model.StaffSetup.AddStaff.Name,
                    stf_contact = model.StaffSetup.AddStaff.contactNo,
                    stf_address = model.StaffSetup.AddStaff.address,
                    stf_cnicFileName = "cnic",
                    stf_dob = model.StaffSetup.AddStaff.dob,
                    stf_email = model.StaffSetup.AddStaff.email,
                    stf_gender = model.StaffSetup.AddStaff.gender,
                    stf_joiningdate = model.StaffSetup.AddStaff.joiningdate,
                    stf_monthlySalary = model.StaffSetup.AddStaff.salary,
                    stf_lefton = null,
                    stf_pictureFileLink = "//",
                    stf_cnicFileLink = "//",
                    stf_nextOfKin = model.StaffSetup.AddStaff.nextOFKin,
                    stf_ntnNumber = model.StaffSetup.AddStaff.ntnNo,
                    stf_status = model.StaffSetup.AddStaff.Status,
                    stf_staffId = model.StaffSetup.AddStaff.stfNo,
                    stf_teacherorStaff = model.StaffSetup.AddStaff.isTeacher,
                    stf_designation = model.StaffSetup.AddStaff.designation,
                    stf_isactive = true,
                    stf_pictureFileName = "pfp"
                };

                await mDB.schoolStaff.AddAsync(table);
                await mDB.SaveChangesAsync();

                // After SaveChanges, the primary key should be populated
                // Make sure your SchoolStaff entity has stf_id as the primary key
                var createdId = table.stf_id;

                return Json(new
                {
                    success = true,
                    stf_id = createdId,
                    name = table.stf_name, // Make sure this matches the property name
                    message = "Staff created successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving staff: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "An error occurred while saving the staff.",
                    detail = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ClassSetup([FromBody] System.Text.Json.JsonElement jsonData)
        {
            try
            {
                string className = null;
                string prefix = null;
                string category = null;
                string subjectWiseTeaching = null;

                // Extract values directly from JsonElement
                if (jsonData.TryGetProperty("ClassSetup", out var classSetup))
                {
                    if (classSetup.TryGetProperty("ClassSetupModel", out var classSetupModel))
                    {
                        className = classSetupModel.TryGetProperty("className", out var cn) ? cn.GetString() : null;
                        prefix = classSetupModel.TryGetProperty("prefix", out var p) ? p.GetString() : null;
                        category = classSetupModel.TryGetProperty("category", out var cat) ? cat.GetString() : null;
                        subjectWiseTeaching = classSetupModel.TryGetProperty("subjectWiseTeaching", out var swt) ? swt.GetString() : null;
                    }
                }

                if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(subjectWiseTeaching))
                {
                    return Json(new { success = false, message = "Invalid form data - missing required fields" });
                }

                // Check for duplicate prefix
                var existing = await mDB.Classes.FirstOrDefaultAsync(x => x.Prefix == prefix);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Class prefix already exists" });
                }

                // Convert subjectWiseTeaching to bool
                bool subjectWise = subjectWiseTeaching.ToLower() == "true" || subjectWiseTeaching == "1";

                var table = new Classes
                {
                    SchoolId = 1,
                    Category = category,
                    Prefix = prefix,
                    Description = className,
                    SubjectWiseTeaching = subjectWise,
                    Active = true
                };

                await mDB.Classes.AddAsync(table);
                await mDB.SaveChangesAsync();

                return Json(new { success = true, message = "Class added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving class: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddSection([FromBody] System.Text.Json.JsonElement jsonData)
        {
            try
            {
                long classId = 0;
                string prefix = null;
                string description = null;
                string year = null;
                string requiredAssistant = null;

                if (jsonData.TryGetProperty("ClassSetup", out var classSetup))
                {
                    if (classSetup.TryGetProperty("classSectionModel", out var sectionModel))
                    {
                        classId = sectionModel.TryGetProperty("classId", out var cid) ? cid.GetInt64() : 0;
                        prefix = sectionModel.TryGetProperty("prefix", out var p) ? p.GetString() : null;
                        description = sectionModel.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                        year = sectionModel.TryGetProperty("year", out var y) ? y.GetString() : null;
                        requiredAssistant = sectionModel.TryGetProperty("requiredAssistant", out var ra) ? ra.GetString() : null;
                    }
                }

                if (classId <= 0 || string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(year))
                {
                    return Json(new { success = false, message = "Invalid form data - missing required fields" });
                }

                // Check for duplicate prefix
                var existing = await mDB.classSections.FirstOrDefaultAsync(x => x.cse_prefix == prefix);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Section prefix already exists" });
                }

                bool requiresAssistant = requiredAssistant?.ToLower() == "true" || requiredAssistant == "1";

                var table = new ClassSections
                {
                    cse_classId = classId,
                    cse_description = description,
                    cse_prefix = prefix,
                    cse_year = int.TryParse(year, out var parsedYear) ? parsedYear : 0,
                    cse_lastrollNo = 0,
                    cse_schId = 1,
                    requiredAssistant = requiresAssistant
                };

                await mDB.classSections.AddAsync(table);
                await mDB.SaveChangesAsync();

                return Json(new { success = true, message = "Section added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving section: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveStaffEducation([FromBody] StaffEduListModel model)
        {
            if (model == null || model.records == null || !model.records.Any())
                return Json(new { success = false, message = "No records provided." });

            foreach (var rec in model.records)
            {
                var education = new StaffEducation // your EF entity
                {
                    sed_staffid = rec.stfID,
                    sed_degreeName = rec.degreeName,
                    sed_instituteName = rec.instituteName,
                    sed_grades = rec.grade,
                    sed_yearPassing = rec.yearpassing,
                    sed_schid = 1
                };

                mDB.staffEducations.Add(education);
            }

            await mDB.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> AddSubject([FromBody] AdminDashboardViewModel model)
        {
            try
            {
                if (model?.ClassSetup?.AddSubject == null)
                    return Json(new { success = false, message = "No subjects provided." });

                if (model.ClassSetup.AddSubject.classID <= 0)
                    return Json(new { success = false, message = "Invalid class selected." });

                if (model.ClassSetup.AddSubject.subjectID <= 0)
                    return Json(new { success = false, message = "Invalid subject selected." });

                if (model.ClassSetup.AddSubject.year <= 0)
                    return Json(new { success = false, message = "Invalid year provided." });

                // Check for duplicate - MUST include year in the check
                // Double-check to prevent race conditions
                var existing = await mDB.ClassSubjects.FirstOrDefaultAsync(x =>
                    x.csb_classid == model.ClassSetup.AddSubject.classID &&
                    x.csb_subjectID == model.ClassSetup.AddSubject.subjectID &&
                    x.csb_year == model.ClassSetup.AddSubject.year &&
                    x.csb_schid == 1);

                if (existing != null)
                {
                    return Json(new { success = false, message = "This subject is already assigned to this class for the selected year." });
                }

                var table = new ClassSubjects
                {
                    csb_classid = model.ClassSetup.AddSubject.classID,
                    csb_subjectID = model.ClassSetup.AddSubject.subjectID,
                    csb_schid = 1,
                    csb_year = model.ClassSetup.AddSubject.year
                };
                await mDB.AddAsync(table);
                await mDB.SaveChangesAsync();

                // Final check after save to ensure no duplicate was created
                var duplicateCheck = await mDB.ClassSubjects
                    .Where(x => x.csb_classid == model.ClassSetup.AddSubject.classID &&
                                x.csb_subjectID == model.ClassSetup.AddSubject.subjectID &&
                                x.csb_year == model.ClassSetup.AddSubject.year &&
                                x.csb_schid == 1)
                    .CountAsync();

                if (duplicateCheck > 1)
                {
                    // Remove the duplicate if somehow created
                    var duplicates = await mDB.ClassSubjects
                        .Where(x => x.csb_classid == model.ClassSetup.AddSubject.classID &&
                                    x.csb_subjectID == model.ClassSetup.AddSubject.subjectID &&
                                    x.csb_year == model.ClassSetup.AddSubject.year &&
                                    x.csb_schid == 1)
                        .OrderByDescending(x => x.csb_id)
                        .Skip(1)
                        .ToListAsync();

                    if (duplicates.Any())
                    {
                        mDB.ClassSubjects.RemoveRange(duplicates);
                        await mDB.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Subject added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error adding subject: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetClassSubjects()
        {
            try
            {
                var classSubjects = await (from cs in mDB.ClassSubjects
                                           join c in mDB.Classes on cs.csb_classid equals c.Id
                                           join s in mDB.Subjects on cs.csb_subjectID equals s.sub_id
                                           where cs.csb_schid == 1
                                           select new
                                           {
                                               id = cs.csb_id,
                                               classID = cs.csb_classid,
                                               subjectID = cs.csb_subjectID,
                                               subjectName = s.sub_description,
                                               className = c.Description,
                                               year = cs.csb_year
                                           })
                                           .OrderBy(cs => cs.className)
                                           .ThenBy(cs => cs.subjectName)
                                           .ToListAsync();

                return Json(classSubjects);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading class subjects: {ex.Message}" });
            }
        }

        [HttpGet("GetSubjectById/{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            try
            {
                var classSubject = await (from cs in mDB.ClassSubjects
                                          join c in mDB.Classes on cs.csb_classid equals c.Id
                                          join s in mDB.Subjects on cs.csb_subjectID equals s.sub_id
                                          where cs.csb_id == id
                                          select new
                                          {
                                              id = cs.csb_id,
                                              classID = cs.csb_classid,
                                              subjectID = cs.csb_subjectID,
                                              year = cs.csb_year,
                                              subjectName = s.sub_description,
                                              className = c.Description
                                          }).FirstOrDefaultAsync();

                if (classSubject == null)
                {
                    return Json(new { success = false, message = "Subject not found." });
                }

                return Json(new { success = true, data = classSubject });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading subject: {ex.Message}" });
            }
        }

        [HttpPut("UpdateSubject/{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] AdminDashboardViewModel model)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Invalid subject ID." });

                if (model?.ClassSetup?.AddSubject == null)
                    return Json(new { success = false, message = "Invalid data provided." });

                if (model.ClassSetup.AddSubject.classID <= 0)
                    return Json(new { success = false, message = "Invalid class selected." });

                if (model.ClassSetup.AddSubject.subjectID <= 0)
                    return Json(new { success = false, message = "Invalid subject selected." });

                if (model.ClassSetup.AddSubject.year <= 0)
                    return Json(new { success = false, message = "Invalid year provided." });

                var classSubject = await mDB.ClassSubjects.FindAsync(id);
                if (classSubject == null)
                {
                    return Json(new { success = false, message = "Subject not found." });
                }

                // Check for duplicate (excluding current record) - MUST include year
                var existing = await mDB.ClassSubjects.FirstOrDefaultAsync(x =>
                    x.csb_id != id &&
                    x.csb_classid == model.ClassSetup.AddSubject.classID &&
                    x.csb_subjectID == model.ClassSetup.AddSubject.subjectID &&
                    x.csb_year == model.ClassSetup.AddSubject.year &&
                    x.csb_schid == 1);

                if (existing != null)
                {
                    return Json(new { success = false, message = "This subject is already assigned to this class for the selected year." });
                }

                classSubject.csb_classid = model.ClassSetup.AddSubject.classID;
                classSubject.csb_subjectID = model.ClassSetup.AddSubject.subjectID;
                classSubject.csb_year = model.ClassSetup.AddSubject.year;

                await mDB.SaveChangesAsync();

                return Json(new { success = true, message = "Subject updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating subject: {ex.Message}" });
            }
        }

        [HttpDelete("DeleteSubject/{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Invalid subject ID." });

                var classSubject = await mDB.ClassSubjects.FindAsync(id);
                if (classSubject == null)
                {
                    return Json(new { success = false, message = "Subject not found." });
                }

                mDB.ClassSubjects.Remove(classSubject);
                await mDB.SaveChangesAsync();

                return Json(new { success = true, message = "Subject deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting subject: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var staff = mDB.schoolStaff.Select(s => new
            {
                s.stf_name,
                s.stf_gender,
                s.stf_staffId,
                s.stf_designation,
                s.stf_contact,
                s.stf_teacherorStaff
            }).ToList();
            return Json(staff);
        }
        // In your AdminController or ClassSetupController
        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            try
            {
                var subjects = await mDB.Subjects
                    .Select(s => new
                    {
                        id = s.sub_id,      // ✅ SAFE
                        name = s.sub_description
                    })
                    .OrderBy(s => s.name)
                    .ToListAsync();

                return Ok(subjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading subjects: {ex.Message}");
            }
        }

        [HttpGet("GetFeeHeads/{classID}")]
        public JsonResult GetFeeHeads(long classID)
        {
            long schoolID = 1;
            var fees = (from f in mDB.feeAndCharges
                        join fa in mDB.feeAndChargesAmounts
                            on f.fac_id equals fa.fcm_FeeHead into feeJoin
                        from fa in feeJoin.DefaultIfEmpty()
                        where f.fac_schid == schoolID
                              && f.fac_category == "Class Fee Charges"
                              && fa.fcm_clsID == classID
                        select new
                        {
                            facId = f.fac_id,                    // Fee head id
                            fcmId = fa != null ? fa.fcm_id : 0,  // Amount record id (0 if missing)
                            feeHead = f.fac_feeHead,
                            feeAmount = fa != null ? fa.fcm_amount : 0
                        }).ToList();

            return Json(fees);

        }
        [HttpPost("ChallanForm")]
        public IActionResult ChallanForm(ChallanFormViewModel model)
        {
            if (model.app_idSearch != 0)
            {
                var applicant = mDB.Applicants.FirstOrDefault(x => x.Id == model.app_idSearch);
                if (applicant != null)
                {
                    ViewBag.Applicant = applicant;
                    ViewBag.Guardian = mDB.ApplicantGuardians.FirstOrDefault(x => x.ApplicantId == applicant.Id);
                    ViewBag.Class = mDB.Classes.FirstOrDefault(x => x.Id == applicant.AppliedForClassId);
                }
            }
            if (!string.IsNullOrEmpty(model.app_BFORMNO))
            {
                var applicant = mDB.Applicants.FirstOrDefault(x => x.BFormNumber == model.app_BFORMNO);
                if (applicant != null)
                {
                    ViewBag.Applicant = applicant;
                    ViewBag.Guardian = mDB.ApplicantGuardians.FirstOrDefault(x => x.ApplicantId == applicant.Id);
                    ViewBag.Class = mDB.Classes.FirstOrDefault(x => x.Id == applicant.AppliedForClassId);
                }
            }
            return View();
        }
        [HttpGet]
        public IActionResult GetClasses()
        {
            var classes = mDB.Classes
                .Select(c => new { c.Id, c.Description })
                .ToList();

            return Json(classes);
        }
        [HttpDelete]
        public JsonResult DeleteFee(long id)
        {
            var fee = mDB.feeAndChargesAmounts.FirstOrDefault(x => x.fcm_id == id);
            if (fee != null)
            {
                mDB.feeAndChargesAmounts.Remove(fee);
                mDB.SaveChanges();
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> CreateFeeHead([FromBody] FeeSetupViewModel model)
        {
            model.schoolID = 1;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await SaveDataInDatabase(model);
            return Ok(new { message = "Fee created successfully" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetFeeHeadsAmount(AdminDashboardViewModel model)
        {
            // Clear ModelState for other models we're not using
            var keysToRemove = ModelState.Keys
                .Where(k => !k.StartsWith("ClassFee.", StringComparison.OrdinalIgnoreCase) &&
                           !k.StartsWith("__RequestVerificationToken", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            // ensure school id
            if (model?.ClassFee != null)
            {
                model.ClassFee.schoolID = 1;
            }
            else
            {
                return Json(new { success = false, message = "ClassFee data is required", errors = new[] { new { Field = "ClassFee", Errors = new[] { "ClassFee data is missing" } } } });
            }

            // Basic server-side validation depending on category:
            var validationErrors = new List<object>();

            if (string.IsNullOrWhiteSpace(model.ClassFee.Category))
            {
                validationErrors.Add(new { Field = "ClassFee.Category", Errors = new[] { "Category is required." } });
            }

            if (model.ClassFee.Category == "Class Fee Charges")
            {
                if (model.ClassFee.classID == null || model.ClassFee.classID <= 0)
                {
                    validationErrors.Add(new { Field = "ClassFee.classID", Errors = new[] { "Class is required for Class Fee Charges." } });
                }
            }

            if (model.ClassFee.feeHead == null || model.ClassFee.feeHead <= 0)
            {
                validationErrors.Add(new { Field = "ClassFee.feeHead", Errors = new[] { "Fee head is required." } });
            }

            if (model.ClassFee.amount == null || model.ClassFee.amount < 0)
            {
                validationErrors.Add(new { Field = "ClassFee.amount", Errors = new[] { "Amount is required and must be >= 0." } });
            }

            if (validationErrors.Count > 0 || !ModelState.IsValid)
            {
                // Combine custom validation errors with ModelState errors (only ClassFee related)
                var modelStateErrors = ModelState
                    .Where(x => x.Value.Errors.Count > 0 && x.Key.StartsWith("ClassFee.", StringComparison.OrdinalIgnoreCase))
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToList();

                var allErrors = validationErrors.Concat(modelStateErrors).ToList();

                return Json(new { success = false, message = "Validation failed", errors = allErrors });
            }

            try
            {
                // If editId provided -> update existing fcm record; otherwise create new
                if (model.ClassFee.editId.HasValue && model.ClassFee.editId.Value > 0)
                {
                    var existing = mDB.feeAndChargesAmounts.FirstOrDefault(x => x.fcm_id == model.ClassFee.editId.Value);
                    if (existing != null)
                    {
                        existing.fcm_clsID = model.ClassFee.Category == "Class Fee Charges" ? model.ClassFee.classID : null;
                        existing.fcm_FeeHead = model.ClassFee.feeHead.Value;
                        existing.fcm_amount = model.ClassFee.amount.Value;
                        mDB.SaveChanges();
                        return Json(new { success = true, message = "Fee updated successfully" });
                    }
                    else
                    {
                        // If editId provided but not found, create a new record instead
                        FeeAndChargesAmount fee = new FeeAndChargesAmount
                        {
                            fcm_clsID = model.ClassFee.Category == "Class Fee Charges" ? model.ClassFee.classID : null,
                            fcm_FeeHead = model.ClassFee.feeHead.Value,
                            fcm_amount = model.ClassFee.amount.Value
                        };
                        mDB.feeAndChargesAmounts.Add(fee);
                        mDB.SaveChanges();
                        return Json(new { success = true, message = "Fee created successfully" });
                    }
                }
                else
                {
                    FeeAndChargesAmount fee;
                    fee = mDB.feeAndChargesAmounts.FirstOrDefault(x => x.fcm_FeeHead == model.ClassFee.feeHead.Value && x.fcm_clsID == model.ClassFee.classID);
                    // Insert new
                    if (fee == null)
                    {
                        fee = new FeeAndChargesAmount();
                        fee.fcm_clsID = model.ClassFee.Category == "Class Fee Charges" ? model.ClassFee.classID : null;
                        fee.fcm_FeeHead = model.ClassFee.feeHead.Value;
                        fee.fcm_amount = model.ClassFee.amount.Value;
                        mDB.feeAndChargesAmounts.Add(fee);
                        mDB.SaveChanges();
                        return Json(new { success = true, message = "Fee created successfully" });
                    }
                    else
                    {
                        // Update existing record
                        fee.fcm_clsID = model.ClassFee.Category == "Class Fee Charges" ? model.ClassFee.classID : null;
                        fee.fcm_amount = model.ClassFee.amount.Value;
                        mDB.SaveChanges();
                        return Json(new { success = true, message = "Fee updated successfully" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving fee: " + ex.Message });
            }
        }

        private async Task SaveDataInDatabase(FeeSetupViewModel model)
        {
            var existing = await mDB.feeAndCharges
                .FirstOrDefaultAsync(x => x.fac_schid == model.schoolID &&
                                          x.fac_feeHead.Trim().ToLower() == model.feeHEAD.Trim().ToLower() && model.Category == x.fac_category);

            if (existing == null)
            {
                var table = new Domain.Entities.FeeAndCharges
                {
                    fac_schid = model.schoolID,
                    fac_feeHead = model.feeHEAD,
                    fac_category = model.Category
                };

                await mDB.feeAndCharges.AddAsync(table);
                await mDB.SaveChangesAsync();
            }
        }

        // PUT: Update an existing FeeHead
        [HttpPut]
        public async Task<IActionResult> UpdateFeeHead(long id, [FromBody] FeeSetupViewModel model)
        {
            var existing = await mDB.feeAndCharges.FindAsync(id);
            if (existing == null)
            {
                return NotFound("Fee record not found");
            }

            existing.fac_feeHead = model.feeHEAD;
            existing.fac_category = model.Category;
            existing.fac_schid = model.schoolID;

            mDB.feeAndCharges.Update(existing);
            await mDB.SaveChangesAsync();

            return Ok(new { message = "Fee updated successfully" });
        }

        // DELETE: Delete a FeeHead
        [HttpDelete]
        public async Task<IActionResult> DeleteFeeHead(long id)
        {
            var fee = await mDB.feeAndCharges.FindAsync(id);
            if (fee == null)
            {
                return NotFound("Fee record not found");
            }

            try
            {
                // 1. Delete children first
                var relatedAmounts = mDB.feeAndChargesAmounts
                                         .Where(a => a.fcm_FeeHead == id)
                                         .ToList();

                if (relatedAmounts.Any())
                {
                    mDB.feeAndChargesAmounts.RemoveRange(relatedAmounts);
                    await mDB.SaveChangesAsync(); // <-- commit child deletions first
                }

                // 2. Now delete parent
                mDB.feeAndCharges.Remove(fee);
                await mDB.SaveChangesAsync();

                return Ok(new { message = "Fee deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Delete failed", error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
