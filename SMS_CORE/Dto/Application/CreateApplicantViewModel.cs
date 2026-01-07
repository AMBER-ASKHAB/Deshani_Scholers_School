using Domain.Dto.Application.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Application
{
    public class CreateApplicantViewModel
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? Gender { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public string? MotherTongue { get; set; }
        [Required, StringLength(40)]
        public string? Contact { get; set; }
        
        public string? status { get; set; }

        // ✅ BForm number
        [Required]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$",
            ErrorMessage = "Invalid B-Form format (e.g., 35202-1234567-8).")]
        public string? BFormNumber { get; set; }


        // ✅ File uploads
        [Required(ErrorMessage = "B-Form scan is required.")]
        [AllowedExtensions(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? BFormFile { get; set; }

        [Required(ErrorMessage = "Profile picture is required.")]
        [AllowedExtensions(".jpg", ".jpeg", ".png")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ProfilePic { get; set; }

        public string ProfilePicFilePath { get; set; } = "";
    }

}
