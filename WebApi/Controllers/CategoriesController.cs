using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Application.Validators;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;

namespace E_commerce_pubg_api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;
        private readonly CreateCategoryDtoValidator _createValidator;
        private readonly UpdateCategoryDtoValidator _updateValidator;

        public CategoriesController(
            ApplicationDbContext context,
            ILogger<CategoriesController> logger,
            CreateCategoryDtoValidator createValidator,
            UpdateCategoryDtoValidator updateValidator)
        {
            _context = context;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            try
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
                return Ok(new { 
                    Message = "Lấy danh sách danh mục thành công",
                    Data = categories 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục");
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi lấy danh sách danh mục" });
            }
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            try
            {
                _logger.LogInformation("Đang lấy thông tin danh mục có ID: {Id}", id);

                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

                var categoryDto = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                _logger.LogInformation("Đã lấy thành công thông tin danh mục có ID: {Id}", id);
                return Ok(new { 
                    Message = "Lấy thông tin danh mục thành công",
                    Data = categoryDto 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin danh mục có ID: {Id}", id);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi lấy thông tin danh mục" });
            }
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory(CreateCategoryDTO createCategoryDto)
        {
            try
            {
                _logger.LogInformation("Đang tạo danh mục mới với tên: {Name}", createCategoryDto.Name);

                var validationResult = await _createValidator.ValidateAsync(createCategoryDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
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
                return CreatedAtAction(
                    nameof(GetCategory), 
                    new { id = category.Id }, 
                    new { 
                        Message = "Tạo danh mục thành công",
                        Data = categoryDto 
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo danh mục mới");
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi tạo danh mục mới" });
            }
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDTO updateCategoryDto)
        {
            try
            {
                _logger.LogInformation("Đang cập nhật danh mục có ID: {Id}", id);

                var validationResult = await _updateValidator.ValidateAsync(updateCategoryDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                }

                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

                category.Name = updateCategoryDto.Name;
                category.Description = updateCategoryDto.Description;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật thành công danh mục có ID: {Id}", id);
                return Ok(new { Message = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật danh mục có ID: {Id}", id);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi cập nhật danh mục" });
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Đang xóa danh mục có ID: {Id}", id);

                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Không tìm thấy danh mục có ID: {Id}", id);
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa thành công danh mục có ID: {Id}", id);
                return Ok(new { Message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa danh mục có ID: {Id}", id);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi xóa danh mục" });
            }
        }
    }
}