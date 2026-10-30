using EMart.Models;

namespace EMart.ViewModels
{
    public class HomeVM
    {
        public List<Categories> Categories { get; set; } = new();
        public List<Brands> Brands { get; set; } = new ();
        public List<Products> LatestProducts { get; set; } = new();
        public List<Products> FeaturedProducts { get; set; } = new();

        public PagedResult<Products> AllProducts { get; set; } = new();
    }
}
