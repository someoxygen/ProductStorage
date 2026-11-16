using System.ComponentModel.DataAnnotations;

namespace ProductStorageAPI.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Amount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; }
    }
}
