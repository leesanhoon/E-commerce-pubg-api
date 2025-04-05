namespace E_commerce_pubg_api.Application.DTOs
{
    public class CloudinaryUploadResult
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? PublicId { get; set; }
        public string? Error { get; set; }
    }
}