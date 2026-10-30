using System.ComponentModel;

namespace EMart.Models
{
    public class ProductImages
    {
        public int ImageId { get; set; }

        public int ProductId { get; set; }

        [DisplayName("Image")]
        public string? ImagePath { get; set; }
    }
}