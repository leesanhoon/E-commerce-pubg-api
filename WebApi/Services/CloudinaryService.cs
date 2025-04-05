using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using E_commerce_pubg_api.WebApi.DTOs;

namespace E_commerce_pubg_api.WebApi.Services
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
        Task<List<CloudinaryUploadResult>> UploadImagesAsync(List<IFormFile> files);
        Task DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png" };
        private const int MaxFileSizeInMB = 5;
        private const int MaxConcurrentUploads = 3;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing");
            }

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
            _logger = logger;
        }

        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file)
        {
            try
            {
                await ValidateImageFile(file);

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "products",
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {ErrorMessage}", uploadResult.Error.Message);
                    return new CloudinaryUploadResult
                    {
                        Success = false,
                        Error = uploadResult.Error.Message
                    };
                }

                return new CloudinaryUploadResult
                {
                    Success = true,
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return new CloudinaryUploadResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<CloudinaryUploadResult>> UploadImagesAsync(List<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                return new List<CloudinaryUploadResult>();
            }

            // Process uploads in parallel with limited concurrency
            var semaphore = new SemaphoreSlim(MaxConcurrentUploads);
            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await UploadImageAsync(file);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return;
            }

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error != null)
                {
                    throw new Exception(result.Error.Message);
                }

                _logger.LogInformation("Image deleted successfully from Cloudinary. Public ID: {PublicId}", publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary. Public ID: {PublicId}", publicId);
                throw;
            }
        }

        private async Task ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            if (file.Length > MaxFileSizeInMB * 1024 * 1024)
            {
                throw new ArgumentException($"File size exceeds {MaxFileSizeInMB}MB limit");
            }

            if (!_allowedImageTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException($"File type {file.ContentType} is not allowed. Only JPG and PNG are supported.");
            }

            await Task.CompletedTask;
        }
    }
}