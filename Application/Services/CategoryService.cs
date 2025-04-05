using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Application.Interfaces;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;
using E_commerce_pubg_api.Application.Validators;
using E_commerce_pubg_api.Application.Exceptions;

namespace E_commerce_pubg_api.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;
        private readonly CreateCategoryDtoValidator _createValidator;
        private readonly UpdateCategoryDtoValidator _updateValidator;

        public CategoryService(
            ApplicationDbContext context,
            ILogger<CategoryService> logger,
            CreateCategoryDtoValidator createValidator, 
            UpdateCategoryDtoValidator updateValidator)
        {
            _context = context;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategories()
        {
            _logger.LogInformation("Đang lấy danh sách các danh mục");
            
            var categories = await _context.Categories
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            _logger.LogInformation("Đã lấy thành công {Count} danh mục", categories.Count);
            return categories;
        }

        public async Task<CategoryDTO> GetCategoryById(int id)
        {
            _logger.LogInformation("Đang lấy thông tin danh mục có ID: {Id}", id);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                return null;
            }

            var categoryDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            _logger.LogInformation("Đã lấy thành công thông tin danh mục có ID: {Id}", id);
            return categoryDto;
        }

        public async Task<CategoryDTO> CreateCategory(CreateCategoryDTO createCategoryDto)
        {
            _logger.LogInformation("Đang tạo danh mục mới với tên: {Name}", createCategoryDto.Name);

            var validationResult = await _createValidator.ValidateAsync(createCategoryDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Dữ liệu không hợp lệ khi tạo danh mục");
                throw new ValidationException(validationResult.Errors);
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            _logger.LogInformation("Đã tạo thành công danh mục mới với ID: {Id}", category.Id);
            return categoryDto;
        }

        public async Task<bool> UpdateCategory(int id, UpdateCategoryDTO updateCategoryDto)
        {
            _logger.LogInformation("Đang cập nhật danh mục có ID: {Id}", id);

            var validationResult = await _updateValidator.ValidateAsync(updateCategoryDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Dữ liệu không hợp lệ khi cập nhật danh mục");
                throw new ValidationException(validationResult.Errors);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                return false;
            }

            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã cập nhật thành công danh mục có ID: {Id}", id);
            return true;
        }

        public async Task<bool> DeleteCategory(int id)
        {
            _logger.LogInformation("Đang xóa danh mục có ID: {Id}", id);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã xóa thành công danh mục có ID: {Id}", id);
            return true;
        }
    }
}