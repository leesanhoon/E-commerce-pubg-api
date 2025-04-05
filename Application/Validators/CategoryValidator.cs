using FluentValidation;
using E_commerce_pubg_api.Application.DTOs;

namespace E_commerce_pubg_api.Application.Validators
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDTO>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên danh mục không được để trống")
                .MaximumLength(200).WithMessage("Tên danh mục không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");
        }
    }

    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDTO> 
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên danh mục không được để trống")
                .MaximumLength(200).WithMessage("Tên danh mục không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");
        }
    }
}