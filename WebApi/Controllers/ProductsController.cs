using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Application.Interfaces;
using E_commerce_pubg_api.Application.Validators;
using System.Transactions;

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
        private readonly CreateProductDtoValidator _validator;

        public ProductsController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<ProductsController> logger,
            CreateProductDtoValidator validator)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _validator = validator;
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
                var validationResult = await _validator.ValidateAsync(createProductDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Get category
                var category = await _context.Categories.FindAsync(createProductDto.CategoryId);
                if (category == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Error = "Danh mục không tồn tại"
                    });
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    StockQuantity = createProductDto.StockQuantity,
                    CategoryId = createProductDto.CategoryId,
                    Category = category,
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
                    CategoryId = product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description
                    },
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
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var response = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                        Description = p.Category.Description
                    },
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
                    .Include(p => p.Category)
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
                    CategoryId = product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name,
                        Description = product.Category.Description
                    },
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

        /// <summary>
        /// Xóa sản phẩm và tất cả hình ảnh liên quan
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Xóa sản phẩm",
            Description = "Xóa sản phẩm và tất cả hình ảnh liên quan trên Cloudinary",
            OperationId = "DeleteProduct",
            Tags = new[] { "Products" }
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        private async Task<IActionResult> DeleteProductInternal(Guid id)
        {
            // Get product with images first (outside transaction)
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

            // Delete images from Cloudinary (outside transaction)
            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(image.PublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(image.PublicId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image {PublicId} from Cloudinary", image.PublicId);
                        // Continue with deletion even if Cloudinary delete fails
                    }
                }
            }

            // Database operations within transaction
            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            _logger.LogInformation("Product {ProductId} deleted successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                return await DeleteProductInternal(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the product",
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