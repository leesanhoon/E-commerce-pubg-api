using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Application.Interfaces;
using E_commerce_pubg_api.Application.Exceptions;

namespace E_commerce_pubg_api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
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
                var product = await _productService.CreateProduct(createProductDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new 
                { 
                    success = true,
                    message = "Tạo sản phẩm thành công",
                    data = product 
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Error = string.Join(", ", ex.Errors)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm mới");
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi tạo sản phẩm mới",
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
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProducts();
                return Ok(new
                {
                    success = true,
                    message = products.Any() ? "Lấy danh sách sản phẩm thành công" : "Không có sản phẩm nào",
                    data = products,
                    count = products.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm",
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
        public async Task<IActionResult> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductById(id);
                return Ok(new
                {
                    success = true,
                    message = product != null ? "Lấy thông tin sản phẩm thành công" : $"Không tìm thấy sản phẩm có id = {id}",
                    data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sản phẩm có ID: {Id}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lấy thông tin sản phẩm",
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
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var result = await _productService.DeleteProduct(id);
                if (!result)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = $"Không tìm thấy sản phẩm có id = {id}"
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm có ID: {Id}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xóa sản phẩm",
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