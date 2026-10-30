using EMart.Models;

namespace EMart.ViewModels
{
    public class MiniCartVM
    {
        public int CartCount { get; set;  }
        public decimal GrandTotal {  get; set; }
        public List<CartItem> Items { get; set; } = new();
    }
}
