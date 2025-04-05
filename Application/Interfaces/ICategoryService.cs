using E_commerce_pubg_api.Application.DTOs;

namespace E_commerce_pubg_api.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategories();
        Task<CategoryDTO> GetCategoryById(int id);
        Task<CategoryDTO> CreateCategory(CreateCategoryDTO createCategoryDto);
        Task<bool> UpdateCategory(int id, UpdateCategoryDTO updateCategoryDto);
        Task<bool> DeleteCategory(int id);
    }
}