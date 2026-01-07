using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Application
{
    public class AddSiblingViewModel
    {
        public long ApplicantId { get; set; }
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int ClassText { get; set; }

        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$",
            ErrorMessage = "Invalid B-Form format (e.g., 35202-1234567-8).")]
        public string? BForm { get; set; }
    }
}
