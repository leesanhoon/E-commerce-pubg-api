using Microsoft.AspNetCore.Http;
using E_commerce_pubg_api.Application.DTOs;

namespace E_commerce_pubg_api.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
        Task<List<CloudinaryUploadResult>> UploadImagesAsync(List<IFormFile> files);
        Task DeleteImageAsync(string publicId);
    }
}