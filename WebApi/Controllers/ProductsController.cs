using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using E_commerce_pubg_api.Domain.Entities;
using E_commerce_pubg_api.Infrastructure.Persistence;

namespace E_commerce_pubg_api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        /// <returns>Danh sách sản phẩm</returns>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
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
        /// Lấy thông tin chi tiết của một sản phẩm
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <returns>Thông tin chi tiết sản phẩm</returns>
        /// <response code="200">Trả về thông tin sản phẩm</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi server khi xử lý yêu cầu</response>
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