using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management_System.Areas.Admin.Models
{
    public class ChallanFormViewModel
    {
        public long app_idSearch { get; set; } = 0;

        public string app_BFORMNO { get; set; } = "";

        [Required(ErrorMessage = "Application ID is required.")]
        public long app_id { get; set; }

        [Required(ErrorMessage = "Fine Date is required.")]
        public int fineDate { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime dueDate { get; set; }

        [Required(ErrorMessage = "Challan Type is required.")]
        public string challanType { get; set; }

        public Dictionary<string, decimal> FeeHeads { get; set; } = new Dictionary<string, decimal>();

        public decimal TotalAmount
        {
            get
            {
                return FeeHeads?.Values.Sum() ?? 0;
            }
        }
    }
}
