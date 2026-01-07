using Domain.Dto.Application;
using Domain.Entities.Application;

namespace School_Management_System.Areas.Applicants.Models
{
    public class ApplicantViewModel
    {
        public long ApplicantId { get; set; }
        public CreateApplicantViewModel CreateApplicant { get; set; } = new CreateApplicantViewModel();
        public AppliedClassViewModel AppliedClass { get; set; } = new AppliedClassViewModel();
        public AddSiblingViewModel AddSibling { get; set; } = new AddSiblingViewModel();
        public CreateGuardianViewModel CreateGuardian { get; set; } = new CreateGuardianViewModel();
        public List<ClassCategoryViewModel> ClassCategoryList { get; set; } = new List<ClassCategoryViewModel>();
        public ClassCategoryViewModel? ClassCategory { get; set; }=new ClassCategoryViewModel();
        public List<CreateGuardianViewModel> Guardians { get; set; } = new List<CreateGuardianViewModel>();
        public List<AddSiblingViewModel> Siblings { get; set; } = new List<AddSiblingViewModel>();
        public ApplicantEducationViewModel ApplicantEducation { get; set; } = new ApplicantEducationViewModel();

        public bool IsClassSelectionCompleted { get; set; }
        public bool IsPersonalInfoCompleted { get; set; }
        public bool IsEducationInfoCompleted { get; set; }
        public bool IsGuardianSiblingInfoCompleted { get; set; }


    }
}
