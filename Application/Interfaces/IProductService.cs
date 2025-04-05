using E_commerce_pubg_api.Application.DTOs;

namespace E_commerce_pubg_api.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProducts();
        Task<ProductResponseDto> GetProductById(Guid id);
        Task<ProductResponseDto> CreateProduct(CreateProductDto createProductDto);
        Task<bool> DeleteProduct(Guid id);
    }
}