using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class for comprehensive file validation
    /// </summary>
    public static class FileValidationUtility
    {
        /// <summary>
        /// Maximum file size for CV uploads (10MB)
        /// </summary>
        public const long MaxCvFileSize = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Allowed file extensions for CV uploads
        /// </summary>
        public static readonly string[] AllowedCvExtensions = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".webp" };

        /// <summary>
        /// Maximum file size for Business License uploads (5MB)
        /// </summary>
        public const long MaxBusinessLicenseFileSize = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Allowed file extensions for Business License uploads
        /// </summary>
        public static readonly string[] AllowedBusinessLicenseExtensions = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".webp" };

        /// <summary>
        /// Maximum file size for Logo uploads (2MB)
        /// </summary>
        public const long MaxLogoFileSize = 2 * 1024 * 1024; // 2MB

        /// <summary>
        /// Allowed file extensions for Logo uploads (only images)
        /// </summary>
        public static readonly string[] AllowedLogoExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

        /// <summary>
        /// Allowed MIME types for CV uploads
        /// </summary>
        public static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            { ".pdf", new[] { "application/pdf" } },
            { ".doc", new[] { "application/msword" } },
            { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { ".png", new[] { "image/png" } },
            { ".jpg", new[] { "image/jpeg" } },
            { ".jpeg", new[] { "image/jpeg" } },
            { ".webp", new[] { "image/webp" } }
        };

        /// <summary>
        /// File signature (magic numbers) for security validation
        /// </summary>
        public static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } }, // PNG
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } }, // JPEG
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } }, // JPEG
            { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }, // RIFF (WebP container)
            { ".doc", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } }, // MS Office
            { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } } // ZIP (DOCX is ZIP-based)
        };

        /// <summary>
        /// Validates a CV file upload
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <returns>Validation result</returns>
        public static FileValidationResult ValidateCvFile(IFormFile file)
        {
            if (file == null)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File is required"
                };
            }

            // Check file size
            if (file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File is empty"
                };
            }

            if (file.Length > MaxCvFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size exceeds maximum limit of {MaxCvFileSize / (1024 * 1024)}MB"
                };
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedCvExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File type not allowed. Allowed types: {string.Join(", ", AllowedCvExtensions)}"
                };
            }

            // Check MIME type
            if (AllowedMimeTypes.ContainsKey(extension))
            {
                var allowedMimes = AllowedMimeTypes[extension];
                if (!allowedMimes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "File content type does not match file extension"
                    };
                }
            }

            // Check file signature for security
            var validationResult = ValidateFileSignature(file, extension);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            return new FileValidationResult
            {
                IsValid = true,
                Extension = extension,
                ContentType = file.ContentType,
                FileSize = file.Length
            };
        }

        /// <summary>
        /// Validates file signature (magic numbers) for security
        /// </summary>
        private static FileValidationResult ValidateFileSignature(IFormFile file, string extension)
        {
            if (!FileSignatures.ContainsKey(extension))
            {
                // If we don't have signature validation for this type, allow it
                return new FileValidationResult { IsValid = true };
            }

            var signatures = FileSignatures[extension];
            var headerBytes = new byte[8]; // Read first 8 bytes

            using (var stream = file.OpenReadStream())
            {
                stream.Read(headerBytes, 0, headerBytes.Length);
                stream.Position = 0; // Reset stream position
            }

            foreach (var signature in signatures)
            {
                if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                {
                    return new FileValidationResult { IsValid = true };
                }
            }

            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = "File appears to be corrupted or not a valid file of the specified type"
            };
        }

        /// <summary>
        /// Validates a Business License file upload
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <returns>Validation result</returns>
        public static FileValidationResult ValidateBusinessLicenseFile(IFormFile file)
        {
            if (file == null)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Giấy phép kinh doanh là bắt buộc"
                };
            }

            // Check file size
            if (file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File rỗng không được phép"
                };
            }

            if (file.Length > MaxBusinessLicenseFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Kích thước file vượt quá giới hạn {MaxBusinessLicenseFileSize / (1024 * 1024)}MB"
                };
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedBusinessLicenseExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", AllowedBusinessLicenseExtensions)}"
                };
            }

            // Validate file signature for security
            var signatureResult = ValidateFileSignature(file, extension);
            if (!signatureResult.IsValid)
            {
                return signatureResult;
            }

            return new FileValidationResult
            {
                IsValid = true,
                Extension = extension,
                ContentType = file.ContentType,
                FileSize = file.Length
            };
        }

        /// <summary>
        /// Validates a Logo file upload
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <returns>Validation result</returns>
        public static FileValidationResult ValidateLogoFile(IFormFile file)
        {
            if (file == null)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Logo cửa hàng là bắt buộc"
                };
            }

            // Check file size
            if (file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File rỗng không được phép"
                };
            }

            if (file.Length > MaxLogoFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Kích thước file vượt quá giới hạn {MaxLogoFileSize / (1024 * 1024)}MB"
                };
            }

            // Check file extension (only images for logo)
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedLogoExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Định dạng file không được hỗ trợ. Logo chỉ chấp nhận: {string.Join(", ", AllowedLogoExtensions)}"
                };
            }

            // Validate file signature for security
            var signatureResult = ValidateFileSignature(file, extension);
            if (!signatureResult.IsValid)
            {
                return signatureResult;
            }

            return new FileValidationResult
            {
                IsValid = true,
                Extension = extension,
                ContentType = file.ContentType,
                FileSize = file.Length
            };
        }

        /// <summary>
        /// Generates a safe filename for storage
        /// </summary>
        public static string GenerateSafeFileName(string originalFileName, string extension)
        {
            var safeFileName = Path.GetFileNameWithoutExtension(originalFileName);
            safeFileName = string.Join("_", safeFileName.Split(Path.GetInvalidFileNameChars()));

            // Limit filename length
            if (safeFileName.Length > 50)
            {
                safeFileName = safeFileName.Substring(0, 50);
            }

            return $"{Guid.NewGuid()}_{safeFileName}{extension}";
        }
    }

    /// <summary>
    /// Result of file validation
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
    }
}
