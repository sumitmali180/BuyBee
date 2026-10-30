using EMart.Models;

namespace EMart.ViewModels
{
    public class OrderConfirmationVM
    {
        public Orders Order { get; set; } = new();

        public Addresses? Address { get; set; }
    }
}
