using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace E_commerce_pubg_api.WebApi.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [MaxLength(3, ErrorMessage = "Maximum 3 images allowed")]
        public List<IFormFile> Images { get; set; }
    }

    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; }
        public List<ProductImageDto> Images { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
    }

    public class CloudinaryUploadResult
    {
        public bool Success { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
        public string Error { get; set; }
    }
}