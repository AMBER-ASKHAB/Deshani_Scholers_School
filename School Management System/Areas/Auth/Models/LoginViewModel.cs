using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Areas.Auth.Models
{
    public class LoginViewModel
    {
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email.")]
            [StringLength(150, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 150 characters.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(200, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        
    }
}
