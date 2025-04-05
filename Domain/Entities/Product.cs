using Domain.Entities;

namespace E_commerce_pubg_api.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<ProductImage> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductImage
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }  // Cloudinary public ID
        public bool IsMain { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}