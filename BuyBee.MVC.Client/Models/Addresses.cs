using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Addresses
    {
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [DisplayName("Customer")]
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [DisplayName("Address")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string AddressLine1 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [DisplayName("City")]
        [StringLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [DisplayName("State")]
        [StringLength(100)]
        public string State { get; set; }

        [Required(ErrorMessage = "Pincode is required.")]
        [DisplayName("Pincode")]
        [StringLength(20)]
        public string Pincode { get; set; }
    }
}
