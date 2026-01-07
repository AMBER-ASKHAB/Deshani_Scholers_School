using Domain.Dto.Application.Validation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Domain.Dto.Application
{
    public class ApplicantEducationViewModel
    {
        public int Category { get; set; }
        [StringLength(120)]
        public string PrevSchool { get; set; } = "";
        [StringLength(40)]
        public string YearsAttended { get; set; }
        [StringLength(10)]
        public string Grade { get; set; }
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100.")]
        public float? Percentage { get; set; }
        [AllowedExtensionsAttribute(".jpg", ".jpeg", ".png", ".pdf")]
        [MaxFileSize(2 * 1024 * 1024)]
        public  IFormFile PreviousSchoolCertid { get; set; }
        public  IFormFile PreviousSchoolLeavCertid { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Category > 2) // middle/high
            {
                if (string.IsNullOrWhiteSpace(PrevSchool))
                    yield return new ValidationResult("Previous school is required.",
                        new[] { nameof(PrevSchool) });

                if (string.IsNullOrWhiteSpace(YearsAttended))
                    yield return new ValidationResult("Years attended is required.",
                        new[] { nameof(YearsAttended) });

                if (string.IsNullOrWhiteSpace(Grade))
                    yield return new ValidationResult("Grade is required.",
                        new[] { nameof(Grade) });

                if (Percentage is null)
                    yield return new ValidationResult("Percentage is required.",
                        new[] { nameof(Percentage) });
            }
        }
    }
}
