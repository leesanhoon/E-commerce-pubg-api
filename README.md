# E-commerce PUBG API

## Giới thiệu
API backend cho hệ thống thương mại điện tử bán các vật phẩm trong game PUBG. Dự án được xây dựng bằng ASP.NET Core theo mô hình Layered Architecture.

## Cấu trúc thư mục
```
E-commerce-pubg-api/
├── Application/            # Chứa business logic
│   ├── DTOs/              # Data Transfer Objects
│   ├── Interfaces/        # Interface definitions
│   ├── Services/          # Implementation của business logic
│   └── Mappings/          # Auto mapper profiles
│
├── Domain/                # Business entities, enums, exceptions
│   ├── Entities/         # Domain models
│   ├── Enums/            # Enumerations
│   └── Exceptions/       # Custom exceptions
│
├── Infrastructure/        # External concerns
│   ├── Persistence/      # Database related operations
│   │   ├── Contexts/     # DbContext
│   │   ├── Repositories/ # Repository implementations
│   │   └── Migrations/   # Database migrations
│   └── ExternalServices/ # Third-party service integrations
│
├── WebApi/               # Presentation layer
│   ├── Controllers/      # API endpoints
│   ├── Middlewares/      # Custom middlewares
│   └── Filters/         # Action filters
│
└── Tests/                # Unit tests, integration tests
    ├── UnitTests/
    └── IntegrationTests/
```

## Công nghệ sử dụng
- ASP.NET Core 9.0
- Entity Framework Core
- PostgreSQL
- AutoMapper
- FluentValidation
- JWT Authentication
- Swagger/OpenAPI

## Kiến trúc
Dự án được thiết kế theo mô hình Layered Architecture với 4 layer chính:

1. **Presentation Layer (WebApi)**
   - Xử lý HTTP requests
   - Routing
   - Model validation
   - Authentication/Authorization

2. **Application Layer**
   - Business logic
   - Orchestration
   - Application services
   - DTOs & mapping

3. **Domain Layer**
   - Business entities
   - Domain logic
   - Business rules
   - Domain events

4. **Infrastructure Layer**
   - Database operations
   - External service integrations
   - Logging
   - File system operations

## Cài đặt

### Yêu cầu hệ thống
- .NET 7.0 SDK
- PostgreSQL 14 trở lên
- Visual Studio/VS Code

### Các bước cài đặt
1. Clone repository:
```bash
git clone https://github.com/your-username/E-commerce-pubg-api.git
```

2. Di chuyển vào thư mục dự án:
```bash
cd E-commerce-pubg-api
```

3. Restore packages:
```bash
dotnet restore
```

4. Cấu hình connection string trong appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pubg_ecommerce;Username=your_username;Password=your_password;"
  }
}
```

5. Cập nhật database:
```bash
dotnet ef database update
```

6. Chạy dự án:
```bash
dotnet run
```

API sẽ chạy tại địa chỉ: `https://localhost:5001`

## API Documentation
Swagger UI có sẵn tại: `https://localhost:5001/swagger`