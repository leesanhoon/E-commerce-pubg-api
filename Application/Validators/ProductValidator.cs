using FluentValidation;
using E_commerce_pubg_api.Application.DTOs;
using E_commerce_pubg_api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace E_commerce_pubg_api.Application.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator(ApplicationDbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
                .MaximumLength(100).WithMessage("Tên sản phẩm không được vượt quá 100 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượng tồn kho không được âm");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Danh mục không hợp lệ")
                .MustAsync(async (categoryId, cancellation) =>
                {
                    return await context.Categories.AnyAsync(c => c.Id == categoryId, cancellation);
                }).WithMessage("Danh mục không tồn tại");

            RuleFor(x => x.Images)
                .Must(x => x == null || x.Count <= 3)
                .WithMessage("Chỉ được phép tải lên tối đa 3 hình ảnh");

            When(x => x.Images != null && x.Images.Any(), () =>
            {
                RuleForEach(x => x.Images)
                    .Must(file =>
                    {
                        if (file.Length > 5 * 1024 * 1024) // 5MB
                            return false;
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        return new[] { ".jpg", ".jpeg", ".png" }.Contains(extension);
                    })
                    .WithMessage("Mỗi file ảnh phải có định dạng jpg, jpeg hoặc png và kích thước không vượt quá 5MB");
            });
        }
    }
}