using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;
using E_commerce_pubg_api.WebApi.Services;

namespace E_commerce_pubg_api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách tất cả sản phẩm",
            Description = "Endpoint này trả về danh sách tất cả sản phẩm, sắp xếp theo thời gian tạo mới nhất",
            OperationId = "GetProducts",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(typeof(ProductsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Fetching all products");
                var products = await _context.Products
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return Ok(new ProductsResponse
                {
                    Success = true,
                    Data = products,
                    Count = products.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products");
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while fetching products",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Upload ảnh cho sản phẩm
        /// </summary>
        [HttpPost("{id}/upload-image")]
        [SwaggerOperation(
            Summary = "Upload ảnh cho sản phẩm",
            Description = "Upload và cập nhật ảnh cho sản phẩm lên Cloudinary. Hỗ trợ định dạng: jpg, jpeg, png, gif. Kích thước tối đa: 5MB",
            OperationId = "UploadProductImage",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Success = false,
                        Message = "No file was uploaded"
                    });
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(product.ImageUrl);
                }

                // Upload new image to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file);
                
                // Update product with new image URL
                product.ImageUrl = imageUrl;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ProductResponse
                {
                    Success = true,
                    Data = product
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading image for product {ProductId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while uploading the image",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một sản phẩm
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Lấy thông tin chi tiết sản phẩm",
            Description = "Endpoint này trả về thông tin chi tiết của một sản phẩm dựa theo ID",
            OperationId = "GetProduct",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ProductResponse
                {
                    Success = true,
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching product {ProductId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while fetching the product",
                    Error = ex.Message
                });
            }
        }
    }

    public class ProductsResponse
    {
        public bool Success { get; set; }
        public IEnumerable<Product> Data { get; set; }
        public int Count { get; set; }
    }

    public class ProductResponse
    {
        public bool Success { get; set; }
        public Product Data { get; set; }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}