using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace School_Management_System.Areas.Admin.Models
{
    public class FeeSetupViewModel
    {
        [Required]
        public long schoolID { get; set; }
        [Required]
        public string feeHEAD { get; set; }
        [Required]
        public string Category { get; set; }

        public List<SelectListItem> Schools { get; set; } = new();
    }
}
