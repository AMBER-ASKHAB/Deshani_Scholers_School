using Domain.Dto.Admin;

namespace School_Management_System.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public ChallanFormViewModel ChallanForm { get; set; }=new ChallanFormViewModel();
        public ClassFeeViewModel ClassFee { get; set; }= new ClassFeeViewModel();
        public ClassSetupViewModel ClassSetup { get; set; }= new ClassSetupViewModel();
        public FeeSetupViewModel FeeSetup { get; set; }=new FeeSetupViewModel();
        public StaffSetupViewModel StaffSetup { get; set; }=new StaffSetupViewModel();
        public GeneratedChallanViewModel GeneratedChallan { get; set; }=new GeneratedChallanViewModel();
    }
}
