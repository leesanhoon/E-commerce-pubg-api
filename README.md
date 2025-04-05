# E-commerce PUBG API

REST API cho hệ thống thương mại điện tử PUBG được xây dựng theo Clean Architecture.

## Kiến Trúc

Dự án được tổ chức theo Clean Architecture với các layer rõ ràng:

### Domain Layer
- Chứa các business entities (Product, Category)
- Không phụ thuộc vào bất kỳ layer nào khác
- Đặt trong thư mục `/Domain`

### Application Layer
- Chứa business logic và use cases
- Định nghĩa interfaces cho infrastructure
- Bao gồm:
  - DTOs: Data Transfer Objects
  - Interfaces: Định nghĩa contracts cho services
  - Services: Implement business logic
  - Validators: Validation logic
  - Exceptions: Custom exceptions
- Đặt trong thư mục `/Application`

### Infrastructure Layer
- Implement các interface của Application layer
- Xử lý tương tác với external services (database, cloud storage)
- Bao gồm:
  - Persistence: Database context và configurations
  - External Services: Cloudinary, etc.
- Đặt trong thư mục `/Infrastructure`

### Web API Layer
- Controllers chỉ handle HTTP requests/responses
- Không chứa business logic
- Sử dụng Dependency Injection để tương tác với Application layer
- Đặt trong thư mục `/WebApi`

## Nguyên Tắc Clean Architecture

1. **Dependency Rule**: Các dependencies chỉ đi từ ngoài vào trong (Web API → Application → Domain)
2. **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
3. **Independence**: Domain và Application layers độc lập với infrastructure concerns

## Ví Dụ Flow

Khi có request đến `/api/products`:

1. **ProductsController** (Web API Layer)
   - Nhận HTTP request
   - Validate input nếu cần
   - Gọi đến ProductService
   - Trả về HTTP response

2. **ProductService** (Application Layer)
   - Implement business logic
   - Sử dụng repositories/interfaces để tương tác với data
   - Trả về DTOs

3. **Domain Entities** (Domain Layer)
   - Định nghĩa business rules và relationships
   - Không phụ thuộc vào bất kỳ layer nào khác

4. **Infrastructure** (Infrastructure Layer)
   - Implement các interfaces của Application layer
   - Xử lý tương tác với database và external services

## Service Layer Pattern

Services được tổ chức theo pattern sau:

```csharp
public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllProducts();
    Task<ProductResponseDto> GetProductById(Guid id);
    Task<ProductResponseDto> CreateProduct(CreateProductDto createProductDto);
    Task<bool> DeleteProduct(Guid id);
}

public class ProductService : IProductService
{
    // Implement các methods với business logic
}
```

Controllers chỉ phụ thuộc vào interfaces:

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    // API endpoints
}
```

## Error Handling

- Custom exceptions cho business logic errors
- Consistent error responses qua ErrorResponse class
- Logging chi tiết ở service layer

## Validation

- FluentValidation cho request validation
- Validation logic tập trung trong Validators folder
- Custom ValidationException để xử lý validation errors