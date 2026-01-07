using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Areas.Auth.Models
{
    public class ResetPasswordViewModel
    {
        public string? Token { get; set; } = "";
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W)(?!.*\s).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, contain upper & lower case letters, a special character, and no spaces.")]
        public string Old_Password { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W)(?!.*\s).{8,}$",
         ErrorMessage = "Password must be at least 8 characters, contain upper & lower case letters, a special character, and no spaces.")]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";

        public string callingContext { get; set; }="";
    }
}
