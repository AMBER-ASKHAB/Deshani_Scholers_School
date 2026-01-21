using System.ComponentModel.DataAnnotations;
using Domain.Dto.Application.Validation;
using Microsoft.AspNetCore.Http;

namespace Domain.Dto.Application
{
    public class CreateGuardianViewModel
    {
        [Required, StringLength(100)]
        public string GuardName { get; set; } = null;

        public string GuardGender { get; set; } = null;

        public string GuardRelation { get; set; } = null;

        [Required, StringLength(180)]
        public string Address { get; set; } = null;

        [Required, Phone, StringLength(20)]
        public string Contact { get; set; } = null;

        [Required]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$",
           ErrorMessage = "Invalid CNIC format (e.g., 35202-1234567-8).")]
        public string CNIC { get; set; } = null;

        [Required, EmailAddress]
        public string Email { get; set; } = null;

        [AllowedExtensions(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile CnicFront { get; set; } = null;

        [AllowedExtensions(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile CnicBack { get; set; } = null;

        [Required, StringLength(60)]
        public string Occupation { get; set; } = null;


    }
}
