using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Areas.Admin.Models
{
    public class ClassFeeViewModel
    {
        // Will be set server-side (not from user input)
        public long schoolID { get; set; }

        // nullable because not all categories require a class
        public long? classID { get; set; }

        [Required]
        public string Category { get; set; }

        // nullable because sometimes user picks a fee head but amount record doesn't exist yet
        public long? feeHead { get; set; }

        // nullable because when showing table for Admission/Other there might be no amount record
        public int? amount { get; set; }

        // edit id -> id of fee amount record (fcm_id). nullable when creating new entry.
        public long? editId { get; set; }
    }
}
