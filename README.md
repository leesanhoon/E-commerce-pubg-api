# E-commerce PUBG API

REST API cho hệ thống thương mại điện tử PUBG được xây dựng theo Clean Architecture.

## Kiến Trúc

Dự án được tổ chức theo Clean Architecture với các layer độc lập và giao tiếp thông qua interfaces:

### Domain Layer (Core)
- Chứa các business entities (Product, Category)
- Không phụ thuộc vào bất kỳ layer nào khác
- Định nghĩa các domain interfaces nếu cần
- Đặt trong thư mục `/Domain`

### Application Layer
- Chứa business logic và use cases
- Định nghĩa interfaces cho infrastructure
- Dependencies:
  - Chỉ phụ thuộc vào Domain layer
  - Không phụ thuộc vào Infrastructure hay Web API layer
- Bao gồm:
  - DTOs: Data Transfer Objects
  - Interfaces: Contracts cho services
  - Services: Business logic implementations
  - Validators: Validation logic
  - Exceptions: Custom exceptions
- Đặt trong thư mục `/Application`

### Infrastructure Layer
- Implement các interface của Application layer
- Dependencies:
  - Phụ thuộc vào Application layer thông qua interfaces
  - Không phụ thuộc trực tiếp vào Web API layer
- Bao gồm:
  - Persistence: Database implementations
  - External Services: Cloud storage, etc.
- Đặt trong thư mục `/Infrastructure`

### Web API Layer
- Controllers xử lý HTTP requests/responses
- Dependencies:
  - Chỉ phụ thuộc vào interfaces từ Application layer
  - Không phụ thuộc trực tiếp vào Infrastructure hay implementations
- Đặt trong thư mục `/WebApi`

## Interface-based Architecture

### 1. Service Interfaces
Application layer định nghĩa các service interfaces:

```csharp
// Business logic interfaces
public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllCategories();
    Task<CategoryDTO> GetCategoryById(int id);
    Task<CategoryDTO> CreateCategory(CreateCategoryDTO dto);
    Task<bool> UpdateCategory(int id, UpdateCategoryDTO dto);
    Task<bool> DeleteCategory(int id);
}

public interface IProductService 
{
    Task<IEnumerable<ProductResponseDto>> GetAllProducts();
    Task<ProductResponseDto> GetProductById(Guid id);
    Task<ProductResponseDto> CreateProduct(CreateProductDto dto);
    Task<bool> DeleteProduct(Guid id);
}

// Infrastructure interfaces
public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
    Task<IEnumerable<CloudinaryUploadResult>> UploadImagesAsync(IEnumerable<IFormFile> files);
    Task<bool> DeleteImageAsync(string publicId);
}
```

### 2. Đề Xuất Repository Pattern
Để tăng tính loose coupling, nên thêm repository interfaces:

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(object id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public interface ICategoryRepository : IRepository<Category>
{
    // Additional category-specific methods
}

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsWithImagesAsync();
    // Additional product-specific methods
}
```

### 3. Service Implementations
Services sử dụng các interfaces để truy cập infrastructure:

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        ICloudinaryService cloudinaryService,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }
    
    // Implementation methods using injected interfaces
}
```

### 4. Controllers
Controllers chỉ phụ thuộc vào service interfaces:

```csharp
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
    
    // API endpoints using only the interface
}
```

### 5. Dependency Injection
Đăng ký tất cả interfaces và implementations:

```csharp
// Application Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Infrastructure Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

## Flow và Dependencies

1. **Controller Layer**
   - Depends on: IProductService, ICategoryService
   - No direct infrastructure dependencies

2. **Service Layer**
   - Depends on: IRepository<T>, ICloudinaryService
   - Implements: IProductService, ICategoryService
   - No direct database access

3. **Repository Layer**
   - Implements: IRepository<T>
   - Handles: Database operations
   - Uses: DbContext

4. **Infrastructure Layer**
   - Implements: ICloudinaryService
   - Handles: External service integration

## Lợi Ích

1. **Loose Coupling**
   - Mỗi layer chỉ biết về interfaces
   - Dễ dàng thay đổi implementations
   - Giảm thiểu dependencies

2. **Testability**
   - Dễ dàng mock interfaces
   - Unit testing không cần database
   - Có thể test từng layer độc lập

3. **Maintainability**
   - Cấu trúc rõ ràng
   - Dễ thay đổi implementations
   - Separation of concerns rõ ràng

4. **Security**
   - Controllers không thể truy cập trực tiếp database
   - Business logic được bảo vệ trong service layer
   - Validations tập trung

5. **Scalability**
   - Dễ dàng thêm features mới
   - Có thể thay đổi database provider
   - Dễ dàng thêm caching layer