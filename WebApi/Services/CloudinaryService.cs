using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace E_commerce_pubg_api.WebApi.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

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

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is empty");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("File size exceeds 5MB limit");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    throw new ArgumentException("Invalid file type. Only JPG, PNG and GIF are allowed.");
                }

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "products",
                    // Add transformations if needed
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception(uploadResult.Error.Message);
                }

                _logger.LogInformation("Image uploaded successfully to Cloudinary. Public ID: {PublicId}", uploadResult.PublicId);
                
                // Return the secure URL of the uploaded image
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            try
            {
                // Extract public ID from URL
                var publicId = ExtractPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogWarning("Could not extract public ID from URL: {ImageUrl}", imageUrl);
                    return;
                }

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
                _logger.LogError(ex, "Error deleting image from Cloudinary. URL: {ImageUrl}", imageUrl);
                throw;
            }
        }

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                // Example URL: https://res.cloudinary.com/your-cloud-name/image/upload/v1234567890/products/image.jpg
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                // Find the 'upload' segment and get everything after it
                var uploadIndex = Array.IndexOf(pathSegments, "upload");
                if (uploadIndex >= 0 && uploadIndex < pathSegments.Length - 2)
                {
                    // Skip the version number (v1234567890) and combine the rest
                    var publicId = string.Join("/", pathSegments.Skip(uploadIndex + 2));
                    // Remove the file extension
                    return Path.GetFileNameWithoutExtension(publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting public ID from URL: {ImageUrl}", imageUrl);
            }
            return null;
        }
    }
}