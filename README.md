# PUBG E-commerce API

## 📝 Giới thiệu
API backend cho hệ thống thương mại điện tử chuyên về mua bán các vật phẩm trong game PUBG. Dự án này được xây dựng với mục đích tạo ra một nền tảng an toàn và tiện lợi cho người chơi PUBG có thể trao đổi, mua bán các vật phẩm trong game.

## ✨ Tính năng chính
- 🛍️ Quản lý sản phẩm (vật phẩm game)
- 🖼️ Upload và quản lý hình ảnh qua Cloudinary CDN
- 🔐 Xác thực và phân quyền người dùng
- 💾 Lưu trữ dữ liệu với PostgreSQL
- 📦 Container hóa với Docker
- 📚 API Documentation với Swagger UI
- 🔍 Tìm kiếm và lọc sản phẩm
- 📊 Logging và monitoring

## 🔧 Công nghệ sử dụng
- **.NET 9.0**: Framework chính để phát triển API
- **PostgreSQL**: Hệ quản trị cơ sở dữ liệu
- **Entity Framework Core**: ORM framework
- **Cloudinary**: CDN cho lưu trữ và quản lý hình ảnh
- **Docker**: Container hóa ứng dụng
- **Swagger/OpenAPI**: API documentation
- **VS Code**: IDE chính để phát triển

## 📂 Cấu trúc thư mục (Clean Architecture)
```
E-commerce-pubg-api/
├── Domain/                      # Enterprise Business Rules
│   ├── Entities/               # Business entities
│   ├── Enums/                 # Enumerations
│   └── Exceptions/            # Domain exceptions
│
├── Application/                 # Application Business Rules
│   ├── Interfaces/            # Service interfaces
│   ├── Services/              # Application services
│   ├── DTOs/                 # Data transfer objects
│   └── Validators/           # Request validators
│
├── Infrastructure/              # Frameworks & Drivers
│   ├── Persistence/           # Database implementation
│   │   ├── Contexts/
│   │   ├── Repositories/
│   │   └── Migrations/
│   └── ExternalServices/      # External service implementations
│       └── Cloudinary/       # Cloudinary integration
│
├── WebApi/                     # Interface Adapters
│   ├── Controllers/           # API controllers
│   └── Middleware/            # Custom middleware
│
├── .vscode/                   # VS Code configuration
├── docker/                    # Docker configuration
└── tests/                     # Unit và integration tests
```

## 🏗️ Clean Architecture

Dự án được tổ chức theo nguyên tắc Clean Architecture với 4 layer chính:

1. **Domain Layer** (Innermost)
   - Chứa business entities và business rules
   - Không phụ thuộc vào bất kỳ layer nào khác
   - Không phụ thuộc vào frameworks hay thư viện bên ngoài

2. **Application Layer**
   - Chứa business logic và orchestration
   - Định nghĩa interfaces cho các services
   - Chứa DTOs và validators
   - Phụ thuộc vào Domain layer

3. **Infrastructure Layer**
   - Implement các interfaces từ Application layer
   - Chứa implementations cho database và external services
   - Phụ thuộc vào Application và Domain layer

4. **Web API Layer** (Outermost)
   - Controllers và Middleware
   - Presentation logic
   - Phụ thuộc vào Application layer

## 🚀 Hướng dẫn cài đặt chi tiết

### Yêu cầu hệ thống
#### Windows
- Windows 10/11
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/download)
- [Git for Windows](https://gitforwindows.org/)

#### macOS
- macOS Catalina (10.15) trở lên
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/download)
- [Git](https://git-scm.com/download/mac)

#### Linux (Ubuntu/Debian)
```bash
# Cài đặt .NET SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install dotnet-sdk-9.0

# Cài đặt Docker
sudo apt install docker.io docker-compose
sudo systemctl enable --now docker

# Cài đặt VS Code
sudo snap install code --classic
```

### Các bước cài đặt chi tiết

1. **Clone repository**:
```bash
git clone https://github.com/your-username/E-commerce-pubg-api.git
cd E-commerce-pubg-api
```

2. **Cài đặt các dependencies**:
```bash
dotnet restore
```

3. **Cấu hình môi trường**:

   a. Tạo file `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=pubg_ecommerce;Username=postgres;Password=your_password"
     },
     "Cloudinary": {
       "CloudName": "your_cloud_name",
       "ApiKey": "your_api_key",
       "ApiSecret": "your_api_secret"
     },
     "JWT": {
       "Secret": "your_jwt_secret_key",
       "Issuer": "your_issuer",
       "Audience": "your_audience"
     }
   }
   ```

   b. Cấu hình Cloudinary:
   - Đăng ký tài khoản tại [Cloudinary](https://cloudinary.com)
   - Lấy thông tin CloudName, ApiKey, ApiSecret từ Dashboard
   - Cập nhật vào file `appsettings.Development.json`

4. **Khởi động Database**:
```bash
docker-compose up -d db
```

5. **Chạy Migrations**:
```bash
dotnet ef database update
```

6. **Khởi động API**:
```bash
# Development mode
dotnet run --environment Development

# hoặc sử dụng Docker
docker-compose up
```

## 💾 Hướng dẫn sử dụng Database

### Kết nối PostgreSQL Database

1. **Sử dụng psql trong container**:
```bash
# Kết nối vào database
docker exec -it e-commerce-pubg-api-db-1 psql -U postgres -d pubg_ecommerce

# Một số lệnh hữu ích trong psql
\dt                 # Liệt kê các bảng
\d table_name       # Xem cấu trúc bảng
\du                 # Liệt kê users
\l                  # Liệt kê databases
\q                  # Thoát psql
```

2. **Sử dụng pgAdmin**:
- Cài đặt [pgAdmin](https://www.pgadmin.org/download/)
- Tạo kết nối mới với thông tin:
  - Host: localhost
  - Port: 5432
  - Database: pubg_ecommerce
  - Username: postgres
  - Password: your_password

### Backup và Restore Database
```bash
# Backup
docker exec -t e-commerce-pubg-api-db-1 pg_dump -U postgres pubg_ecommerce > backup.sql

# Restore
docker exec -i e-commerce-pubg-api-db-1 psql -U postgres -d pubg_ecommerce < backup.sql
```

## 🔍 API Examples

### Authentication
```bash
# Đăng ký tài khoản mới
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test@123"
  }'

# Đăng nhập
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@123"
  }'
```

### Products
```bash
# Lấy danh sách sản phẩm
curl http://localhost:5000/api/products

# Tạo sản phẩm mới (yêu cầu JWT token)
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer your_jwt_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "AWM Dragon",
    "description": "Skin AWM Dragon limited",
    "price": 999.99,
    "category": "Weapons"
  }'
```

## ❗ Xử lý các vấn đề thường gặp

### 1. Docker Issues

#### Container không khởi động
```bash
# Kiểm tra logs
docker-compose logs

# Restart containers
docker-compose down
docker-compose up -d
```

#### Database connection issues
- Kiểm tra PostgreSQL container đang chạy:
```bash
docker ps | grep postgres
```
- Verify connection string trong appsettings.json
- Đảm bảo port 5432 không bị sử dụng bởi PostgreSQL local

### 2. Migration Issues
```bash
# Reset database
dotnet ef database drop
dotnet ef database update

# Tạo migration mới
dotnet ef migrations add MigrationName
```

### 3. Common Runtime Errors

#### CORS errors
- Kiểm tra cấu hình CORS trong Program.cs
- Đảm bảo domain client được allow

#### 503 Service Unavailable
- Kiểm tra container health:
```bash
docker-compose ps
docker-compose logs api
```

## 📈 Monitoring và Logging

### Kiểm tra logs
```bash
# API logs
docker-compose logs api

# Database logs
docker-compose logs db
```

### Health Check
```bash
# Kiểm tra health của API
curl http://localhost:5000/health

# Kiểm tra health của Database
docker exec e-commerce-pubg-api-db-1 pg_isready -U postgres
```

## 🔄 Continuous Integration/Deployment

### GitHub Actions Workflow
```yaml
name: CI/CD Pipeline
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build
```

## 💡 Contributing Guidelines
1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

### Coding Standards
- Tuân thủ C# Coding Conventions
- Sử dụng async/await cho các operations bất đồng bộ
- Viết unit tests cho logic mới
- Sử dụng dependency injection
- Document API endpoints với XML comments

## 📄 License
Dự án được phân phối dưới license MIT. Xem `LICENSE` để biết thêm thông tin.

## 📞 Liên hệ
- Email: support@pubg-ecommerce.com
- Website: https://pubg-ecommerce.com
- GitHub: https://github.com/your-username/E-commerce-pubg-api