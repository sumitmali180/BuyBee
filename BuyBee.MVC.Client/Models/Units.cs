using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Units
    {
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Enter Unit Name")]
        [DisplayName("Unit Name")]
        [StringLength(100)]
        public string? UnitName { get; set; }

        [DisplayName("Short Name")]
        [StringLength(20)]
        public string? UnitShortName { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
