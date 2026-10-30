using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.ViewModels
{
    public class UserVM
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [DisplayName("Full Name")]
        [StringLength(200, ErrorMessage = "Full Name cannot exceed 200 characters.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [DisplayName("Email")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        [DisplayName("Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        [DisplayName("Role")]
        public int? RoleId { get; set; }

        [DisplayName("Role")]
        public string? RoleName { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        [DisplayName("Created On")]
        public DateTime? CreatedOn { get; set; }

        [DisplayName("Updated On")]
        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
