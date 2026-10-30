using EMart.Models;

namespace EMart.ViewModels
{
    public class OrderVM
    {
        public Orders Order { get; set; } = new();

        public List<OrderDetails> OrderDetails { get; set; } = new();

        public Addresses? Address { get; set; }

        public Payments? Payment { get; set; }
    }
}
