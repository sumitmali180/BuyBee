using EMart.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMart.ViewModels
{
    public class SubCategoryVM: SubCategories
    {
        public List<SelectListItem>? Categories { get; set; }
    }
}
