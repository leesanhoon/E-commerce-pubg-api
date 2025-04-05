using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;
using E_commerce_pubg_api.WebApi.Services;
using E_commerce_pubg_api.WebApi.DTOs;

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
        /// Tạo sản phẩm mới với hình ảnh
        /// </summary>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo sản phẩm mới",
            Description = "Tạo sản phẩm mới với tối đa 3 hình ảnh. Hỗ trợ định dạng: jpg, jpeg, png. Kích thước tối đa: 5MB/ảnh",
            OperationId = "CreateProduct",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            try
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    StockQuantity = createProductDto.StockQuantity,
                    Category = createProductDto.Category,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (createProductDto.Images != null && createProductDto.Images.Any())
                {
                    // Upload all images to Cloudinary
                    var uploadResults = await _cloudinaryService.UploadImagesAsync(createProductDto.Images);
                    
                    // Check for any failed uploads
                    var failedUploads = uploadResults.Where(r => !r.Success).ToList();
                    if (failedUploads.Any())
                    {
                        return BadRequest(new ErrorResponse
                        {
                            Success = false,
                            Message = "Some images failed to upload",
                            Error = string.Join(", ", failedUploads.Select(f => f.Error))
                        });
                    }

                    // Add successful uploads to product
                    product.Images = uploadResults.Select((r, index) => new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = r.ImageUrl,
                        PublicId = r.PublicId,
                        IsMain = index == 0, // First image is main
                        ProductId = product.Id
                    }).ToList();
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Map to response DTO
                var response = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Category = product.Category,
                    Images = product.Images?.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).ToList(),
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the product",
                    Error = ex.Message
                });
            }
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
        [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Fetching all products");
                var products = await _context.Products
                    .Include(p => p.Images)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var response = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    Images = p.Images?.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).ToList(),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(new
                {
                    success = true,
                    data = response,
                    count = products.Count
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
        /// Lấy thông tin chi tiết của một sản phẩm
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Lấy thông tin chi tiết sản phẩm",
            Description = "Endpoint này trả về thông tin chi tiết của một sản phẩm dựa theo ID",
            OperationId = "GetProduct",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(Guid id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                var response = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Category = product.Category,
                    Images = product.Images?.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).ToList(),
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return Ok(new { success = true, data = response });
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

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}