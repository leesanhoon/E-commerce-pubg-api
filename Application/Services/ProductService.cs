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
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;
        private readonly CreateProductDtoValidator _validator;

        public ProductService(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<ProductService> logger,
            CreateProductDtoValidator validator)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _validator = validator;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Đang lấy danh sách tất cả sản phẩm");
                
                var products = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                if (!products.Any())
                {
                    _logger.LogInformation("Không có sản phẩm nào trong hệ thống");
                    return new List<ProductResponseDto>();
                }

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
                }).ToList();

                _logger.LogInformation("Đã lấy thành công {Count} sản phẩm", products.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                throw;
            }
        }

        public async Task<ProductResponseDto> GetProductById(Guid id)
        {
            try
            {
                _logger.LogInformation("Đang lấy thông tin sản phẩm có ID: {Id}", id);

                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm có ID: {Id}", id);
                    return null;
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

                _logger.LogInformation("Đã lấy thành công thông tin sản phẩm có ID: {Id}", id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sản phẩm có ID: {Id}", id);
                throw;
            }
        }

        public async Task<ProductResponseDto> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                _logger.LogInformation("Đang tạo sản phẩm mới với tên: {Name}", createProductDto.Name);

                var validationResult = await _validator.ValidateAsync(createProductDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Dữ liệu không hợp lệ khi tạo sản phẩm");
                    throw new ValidationException(validationResult.Errors);
                }

                var category = await _context.Categories.FindAsync(createProductDto.CategoryId);
                if (category == null)
                {
                    var failure = new FluentValidation.Results.ValidationFailure("CategoryId", "Danh mục không tồn tại");
                    throw new ValidationException(new[] { failure });
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
                    var uploadResults = await _cloudinaryService.UploadImagesAsync(createProductDto.Images);
                    
                    var failedUploads = uploadResults.Where(r => !r.Success).ToList();
                    if (failedUploads.Any())
                    {
                        var failure = new FluentValidation.Results.ValidationFailure("Images",
                            string.Join(", ", failedUploads.Select(f => f.Error)));
                        throw new ValidationException(new[] { failure });
                    }

                    product.Images = uploadResults.Select((r, index) => new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = r.ImageUrl,
                        PublicId = r.PublicId,
                        IsMain = index == 0,
                        ProductId = product.Id
                    }).ToList();
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo thành công sản phẩm mới với ID: {Id}", product.Id);

                return await GetProductById(product.Id);
            }
            catch (Exception ex) when (!(ex is ValidationException))
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm mới");
                throw;
            }
        }

        public async Task<bool> DeleteProduct(Guid id)
        {
            try
            {
                _logger.LogInformation("Đang xóa sản phẩm có ID: {Id}", id);

                var product = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm có ID: {Id}", id);
                    return false;
                }

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
                            _logger.LogWarning(ex, "Lỗi khi xóa ảnh {PublicId} từ Cloudinary", image.PublicId);
                        }
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa thành công sản phẩm có ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm có ID: {Id}", id);
                throw;
            }
        }
    }
}