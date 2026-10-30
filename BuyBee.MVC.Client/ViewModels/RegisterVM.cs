using System.ComponentModel.DataAnnotations;

namespace EMart.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(200, ErrorMessage = "Full Name cannot exceed 200 characters.")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Password and Confirm Password do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}