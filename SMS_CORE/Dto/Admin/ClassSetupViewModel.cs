using Domain.Dto;

namespace School_Management_System.Areas.Admin.Models
{
    public class ClassSetupViewModel
    {
        public ClassesDto ClassSetupModel { get; set; }= new ClassesDto();
        public ClassSectionDto classSectionModel { get; set; }=new ClassSectionDto();
        public AllocateTeacher allocateTeacher { get; set; }=new AllocateTeacher();
        public AddSubject AddSubject { get; set; } = new AddSubject();
    }
}
