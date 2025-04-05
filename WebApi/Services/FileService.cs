using Microsoft.AspNetCore.Http;

namespace E_commerce_pubg_api.WebApi.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file);
        void DeleteImage(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly string _uploadDirectory;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int MaxFileSizeInMB = 5;

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "wwwroot/uploads/images";
            _logger = logger;

            // Ensure upload directory exists
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            // Validate file size
            if (file.Length > MaxFileSizeInMB * 1024 * 1024)
            {
                throw new ArgumentException($"File size exceeds {MaxFileSizeInMB}MB limit");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadDirectory, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File {FileName} saved successfully", fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {FileName}", fileName);
                throw new Exception("Error saving file", ex);
            }
        }

        public void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var filePath = Path.Combine(_uploadDirectory, fileName);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File {FileName} deleted successfully", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName}", fileName);
                throw new Exception("Error deleting file", ex);
            }
        }
    }
}