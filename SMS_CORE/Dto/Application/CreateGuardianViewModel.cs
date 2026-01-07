using System.ComponentModel.DataAnnotations;
using Domain.Dto.Application.Validation;
using Microsoft.AspNetCore.Http;

namespace Domain.Dto.Application
{
    public class CreateGuardianViewModel
    {
        [Required, StringLength(100)]
        public string GuardName { get; set; }

        public string GuardGender { get; set; }
        public string GuardRelation { get; set; }
        [Required, StringLength(180)]
        public string Address { get; set; }
        [Required, Phone, StringLength(20)]
        public string Contact { get; set; }
        [Required]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$",
           ErrorMessage = "Invalid CNIC format (e.g., 35202-1234567-8).")]
        public string CNIC { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [AllowedExtensions(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile CnicFront { get; set; }
        [AllowedExtensions(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile CnicBack { get; set; }
        [Required, StringLength(60)]
        public string Occupation { get; set; }

    }
}
