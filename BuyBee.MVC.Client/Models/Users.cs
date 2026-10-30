using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Users
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [DisplayName("Full Name")]
        [StringLength(200, ErrorMessage = "Full Name cannot exceed 200 characters.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DisplayName("Password")]
        public string? PasswordHash { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        [DisplayName("Role")]
        public int? RoleId { get; set; }

        [DisplayName("Status")]
        public bool IsActive { get; set; }

        [DisplayName("Created On")]
        public DateTime? CreatedOn { get; set; }

        [DisplayName("Updated On")]
        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
