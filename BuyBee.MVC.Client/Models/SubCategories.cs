using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class SubCategories
    {
        public int? SubCategoryId { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        [DisplayName("SubCategory Name")]
        public string? SubCategoryName { get; set; }
        
        public bool IsActive { get; set; }

        public string? CategoryName { get; set; }
    }
}
