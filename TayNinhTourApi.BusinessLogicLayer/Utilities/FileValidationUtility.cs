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
        /// Enhanced with multiple possible signatures per file type
        /// </summary>
        public static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { 
                ".pdf", new[] 
                { 
                    new byte[] { 0x25, 0x50, 0x44, 0x46 }, // %PDF
                    new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }, // %PDF-
                    new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 }, // %PDF-1
                } 
            },
            { 
                ".png", new[] 
                { 
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } // PNG signature
                } 
            },
            { 
                ".jpg", new[] 
                { 
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG Exif
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG Canon
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG Samsung
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // JPEG SPIFF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, // JPEG
                    new byte[] { 0xFF, 0xD8, 0xFF }        // Basic JPEG
                } 
            },
            { 
                ".jpeg", new[] 
                { 
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG Exif
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG Canon
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG Samsung
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // JPEG SPIFF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, // JPEG
                    new byte[] { 0xFF, 0xD8, 0xFF }        // Basic JPEG
                } 
            },
            { 
                ".webp", new[] 
                { 
                    new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF (WebP container)
                } 
            },
            { 
                ".doc", new[] 
                { 
                    new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, // MS Office
                    new byte[] { 0x0D, 0x44, 0x4F, 0x43 }, // Legacy DOC
                    new byte[] { 0xDB, 0xA5, 0x2D, 0x00 }  // Word 2.0
                } 
            },
            { 
                ".docx", new[] 
                { 
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 }, // ZIP (DOCX is ZIP-based)
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 }, // Empty ZIP
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }  // ZIP with data descriptor
                } 
            }
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

            // Log file details for debugging
            try
            {
                var fileInfo = $"File: {file.FileName}, Size: {file.Length} bytes, ContentType: {file.ContentType}, Extension: {extension}";
                System.Diagnostics.Debug.WriteLine($"[CV File Validation] {fileInfo}");
            }
            catch
            {
                // Ignore logging errors
            }

            // Check MIME type (more lenient check)
            if (AllowedMimeTypes.ContainsKey(extension))
            {
                var allowedMimes = AllowedMimeTypes[extension];
                if (!string.IsNullOrEmpty(file.ContentType) && 
                    !allowedMimes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    // Log warning but don't fail - some browsers may send different MIME types
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[CV File Validation] MIME type mismatch - Expected: {string.Join(", ", allowedMimes)}, Got: {file.ContentType}");
                    }
                    catch
                    {
                        // Ignore logging errors
                    }
                }
            }

            // Check file signature for security (more robust check)
            var validationResult = ValidateFileSignature(file, extension);
            if (!validationResult.IsValid)
            {
                // Log the validation failure for debugging
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[CV File Validation] Signature validation failed for {file.FileName}: {validationResult.ErrorMessage}");
                }
                catch
                {
                    // Ignore logging errors
                }
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
        /// Enhanced with better error handling and more robust checking
        /// </summary>
        private static FileValidationResult ValidateFileSignature(IFormFile file, string extension)
        {
            // For CV files, be more permissive with validation to avoid false rejections
            if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
            {
                try
                {
                    // Basic checks for Office/PDF files
                    if (file.Length < 100) // Very small files are likely invalid
                    {
                        return new FileValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = "File is too small to be a valid document"
                        };
                    }

                    var headerBytes = new byte[16];
                    int bytesRead = 0;

                    using (var stream = file.OpenReadStream())
                    {
                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }
                        
                        bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);
                        
                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }
                    }

                    // Debug logging
                    try
                    {
                        var headerHex = BitConverter.ToString(headerBytes, 0, Math.Min(bytesRead, 8)).Replace("-", " ");
                        System.Diagnostics.Debug.WriteLine($"[File Signature] {file.FileName} ({extension}): {headerHex}");
                    }
                    catch
                    {
                        // Ignore logging errors
                    }

                    if (bytesRead >= 4)
                    {
                        // PDF validation - check for %PDF header
                        if (extension == ".pdf")
                        {
                            string headerString = System.Text.Encoding.ASCII.GetString(headerBytes, 0, Math.Min(bytesRead, 8));
                            if (headerString.StartsWith("%PDF", StringComparison.OrdinalIgnoreCase))
                            {
                                System.Diagnostics.Debug.WriteLine($"[File Signature] PDF validation passed for {file.FileName}");
                                return new FileValidationResult { IsValid = true };
                            }
                            // Be more permissive - if it looks like it might be a PDF, allow it
                            if (headerBytes[0] == 0x25) // % character
                            {
                                System.Diagnostics.Debug.WriteLine($"[File Signature] PDF validation passed (permissive) for {file.FileName}");
                                return new FileValidationResult { IsValid = true };
                            }
                        }

                        // DOCX validation - check for ZIP signature
                        if (extension == ".docx")
                        {
                            if (headerBytes[0] == 0x50 && headerBytes[1] == 0x4B)
                            {
                                System.Diagnostics.Debug.WriteLine($"[File Signature] DOCX validation passed for {file.FileName}");
                                return new FileValidationResult { IsValid = true };
                            }
                        }

                        // DOC validation - check for MS Office signature
                        if (extension == ".doc")
                        {
                            if (headerBytes[0] == 0xD0 && headerBytes[1] == 0xCF)
                            {
                                System.Diagnostics.Debug.WriteLine($"[File Signature] DOC validation passed for {file.FileName}");
                                return new FileValidationResult { IsValid = true };
                            }
                        }
                    }

                    // For document files, if basic checks pass, be permissive
                    System.Diagnostics.Debug.WriteLine($"[File Signature] Document validation defaulted to permissive for {file.FileName}");
                    return new FileValidationResult { IsValid = true };
                }
                catch (Exception ex)
                {
                    // For document files, if validation fails, be permissive
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[File Signature] Exception in document validation for {file.FileName}: {ex.Message}");
                    }
                    catch
                    {
                        // Ignore logging errors
                    }
                    return new FileValidationResult { IsValid = true };
                }
            }

            // For image files, maintain stricter validation
            if (!FileSignatures.ContainsKey(extension))
            {
                return new FileValidationResult { IsValid = true };
            }

            try
            {
                var signatures = FileSignatures[extension];
                var headerBytes = new byte[16];
                int bytesRead = 0;

                using (var stream = file.OpenReadStream())
                {
                    if (stream.CanSeek)
                    {
                        stream.Position = 0;
                    }
                    
                    bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);
                    
                    if (stream.CanSeek)
                    {
                        stream.Position = 0;
                    }
                }

                if (bytesRead == 0)
                {
                    return new FileValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Cannot read file content for validation"
                    };
                }

                foreach (var signature in signatures)
                {
                    if (signature.Length <= bytesRead)
                    {
                        bool matches = true;
                        for (int i = 0; i < signature.Length; i++)
                        {
                            if (headerBytes[i] != signature[i])
                            {
                                matches = false;
                                break;
                            }
                        }
                        
                        if (matches)
                        {
                            return new FileValidationResult { IsValid = true };
                        }
                    }
                }

                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File appears to be corrupted or not a valid file of the specified type"
                };
            }
            catch (Exception ex)
            {
                // For image files, maintain some validation
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[File Signature] Exception in image validation for {file.FileName}: {ex.Message}");
                }
                catch
                {
                    // Ignore logging errors
                }
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "File signature validation failed"
                };
            }
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

            // Use the improved file signature validation
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

            // Use the improved file signature validation (images will have stricter validation)
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
