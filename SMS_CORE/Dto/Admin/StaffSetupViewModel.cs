using Domain.Dto;

namespace School_Management_System.Areas.Admin.Models
{
    public class StaffSetupViewModel
    {
        public AddStaffDto AddStaff { get; set; }=new AddStaffDto();
        public StaffEdu staffEdu { get; set; } = new StaffEdu();
    }
}
