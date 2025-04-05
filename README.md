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

## 📂 Cấu trúc thư mục
```
E-commerce-pubg-api/
├── Domain/                # Business entities và logic
│   ├── Entities/         # Domain models
│   ├── Enums/           # Enumerations
│   └── Exceptions/      # Custom exceptions
│
├── Infrastructure/        # Data access và external services
│   ├── Persistence/     # Database related
│   │   ├── Contexts/   
│   │   ├── Repositories/
│   │   └── Migrations/
│   └── ExternalServices/ # Third-party integrations
│
├── WebApi/               # API endpoints và presentation
│   ├── Controllers/     # API controllers
│   ├── Services/        # Application services
│   └── Middleware/      # Custom middleware
│
├── .vscode/             # VS Code configuration
├── docker/              # Docker configuration
└── tests/               # Unit và integration tests
```

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống
- .NET 9.0 SDK
- Docker Desktop
- VS Code với C# extension
- Tài khoản Cloudinary (cho image storage)

### Các bước cài đặt

1. Clone repository:
```bash
git clone https://github.com/your-username/E-commerce-pubg-api.git
cd E-commerce-pubg-api
```

2. Cấu hình Cloudinary:
- Cập nhật `appsettings.json` với thông tin Cloudinary của bạn:
```json
{
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  }
}
```

3. Khởi động ứng dụng:
```bash
docker-compose up
```

4. Truy cập ứng dụng:
- API và Swagger UI: http://localhost:5000
- API Documentation: http://localhost:5000/swagger

## 💻 Phát triển

### Debug với VS Code
1. Mở project trong VS Code
2. Chọn Debug Configuration:
   - `.NET Core Launch (web)`: Debug local
   - `Docker .NET Core Launch`: Debug với Docker
3. Nhấn F5 để bắt đầu debug

### Các lệnh hữu ích
```bash
# Chạy migration
dotnet ef database update

# Build project
dotnet build

# Run tests
dotnet test

# Watch mode
dotnet watch run
```

## 👥 Nhóm phát triển

### Người phát triển chính
- **Nguyễn Văn A**
  - 🌐 Website: [developer.com](https://developer.com)
  - 📧 Email: developer@example.com
  - 💼 LinkedIn: [linkedin.com/developer](https://linkedin.com/developer)

### Đóng góp
Mọi đóng góp cho dự án đều được chào đón. Vui lòng:
1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License
Dự án được phân phối dưới license MIT. Xem `LICENSE` để biết thêm thông tin.

## 📞 Liên hệ
- Email: support@pubg-ecommerce.com
- Website: https://pubg-ecommerce.com
- GitHub: https://github.com/your-username/E-commerce-pubg-api