using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for handling file storage operations
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IHostingEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Stores a CV file with enhanced organization and security
        /// </summary>
        public async Task<FileStorageResult> StoreCvFileAsync(IFormFile file, Guid userId)
        {
            try
            {
                // Validate file first
                var validationResult = FileValidationUtility.ValidateCvFile(file);
                if (!validationResult.IsValid)
                {
                    return new FileStorageResult
                    {
                        success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // Create directory structure: uploads/cv/yyyy/mm/userId
                var currentDate = DateTime.UtcNow;
                var relativePath = Path.Combine("uploads", "cv", 
                    currentDate.Year.ToString(), 
                    currentDate.Month.ToString("D2"), 
                    userId.ToString());

                var webRoot = _environment.WebRootPath ?? 
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullDirectoryPath = Path.Combine(webRoot, relativePath);

                // Ensure directory exists
                Directory.CreateDirectory(fullDirectoryPath);

                // Generate safe filename
                var extension = validationResult.Extension!;
                var safeFileName = FileValidationUtility.GenerateSafeFileName(file.FileName, extension);
                var fullFilePath = Path.Combine(fullDirectoryPath, safeFileName);
                var relativeFilePath = Path.Combine(relativePath, safeFileName).Replace("\\", "/");

                // Save file to disk
                using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate access URL
                var accessUrl = GetFileAccessUrl(relativeFilePath);

                _logger.LogInformation("CV file stored successfully: {FilePath} for user {UserId}", 
                    relativeFilePath, userId);

                return new FileStorageResult
                {
                    success = true,
                    FilePath = relativeFilePath,
                    AccessUrl = accessUrl,
                    OriginalFileName = file.FileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing CV file for user {UserId}", userId);
                return new FileStorageResult
                {
                    success = false,
                    ErrorMessage = "An error occurred while storing the file"
                };
            }
        }

        /// <summary>
        /// Deletes a CV file from storage
        /// </summary>
        public async Task<bool> DeleteCvFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return true; // Nothing to delete

                var webRoot = _environment.WebRootPath ?? 
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(webRoot, filePath.Replace("/", "\\"));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("CV file deleted: {FilePath}", filePath);
                    return true;
                }

                return true; // File doesn't exist, consider it deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CV file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Gets the full URL for accessing a stored file
        /// </summary>
        public string GetFileAccessUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return filePath; // Fallback to relative path

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/{filePath}";
        }

        /// <summary>
        /// Checks if a file exists in storage
        /// </summary>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var webRoot = _environment.WebRootPath ?? 
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRoot, filePath.Replace("/", "\\"));

            return File.Exists(fullPath);
        }
    }
}
