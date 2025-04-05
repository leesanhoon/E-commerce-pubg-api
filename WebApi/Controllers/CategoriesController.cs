using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Application.Interfaces;
using E_commerce_pubg_api.Application.Exceptions;

namespace E_commerce_pubg_api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategories();
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
                var category = await _categoryService.GetCategoryById(id);
                if (category == null)
                {
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

                return Ok(new { 
                    Message = "Lấy thông tin danh mục thành công",
                    Data = category 
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
                var category = await _categoryService.CreateCategory(createCategoryDto);
                return CreatedAtAction(
                    nameof(GetCategory), 
                    new { id = category.Id }, 
                    new { 
                        Message = "Tạo danh mục thành công",
                        Data = category 
                    }
                );
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { 
                    Message = "Dữ liệu không hợp lệ",
                    Errors = ex.Errors
                });
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
                var result = await _categoryService.UpdateCategory(id, updateCategoryDto);
                if (!result)
                {
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

                return Ok(new { Message = "Cập nhật danh mục thành công" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { 
                    Message = "Dữ liệu không hợp lệ",
                    Errors = ex.Errors
                });
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
                var result = await _categoryService.DeleteCategory(id);
                if (!result)
                {
                    return NotFound(new { Message = $"Không tìm thấy danh mục có id = {id}" });
                }

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