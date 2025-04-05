using FluentValidation.Results;

namespace E_commerce_pubg_api.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("Một hoặc nhiều lỗi xác thực đã xảy ra.")
        {
            Errors = failures.Select(f => f.ErrorMessage);
        }
    }
}