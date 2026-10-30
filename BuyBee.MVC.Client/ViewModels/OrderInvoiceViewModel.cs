using EMart.Models;

namespace EMart.ViewModels
{
    public class OrderInvoiceViewModel
    {
        public int SelectedOrderId { get; set; }

        public List<Orders> Orders { get; set; } = new();
    }
}
