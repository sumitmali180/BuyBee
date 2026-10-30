using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Roles
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        [DisplayName("Role Name")]
        [StringLength(100, ErrorMessage = "Role Name cannot exceed 100 characters.")]
        public string? RoleName { get; set; }

        [DisplayName("Created On")]
        public DateTime? CreatedOn { get; set; }

        [DisplayName("Updated On")]
        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
