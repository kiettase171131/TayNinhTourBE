using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class cho xử lý images của TourTemplate
    /// </summary>
    public class TourTemplateImageHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int MaxImagesPerTemplate = 10;
        private readonly HashSet<string> _allowedExtensions = new HashSet<string> { ".png", ".jpg", ".jpeg", ".webp" };

        public TourTemplateImageHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Validate image URLs for tour template
        /// </summary>
        public async Task<ResponseValidationDto> ValidateImageUrlsAsync(List<string> imageUrls)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            if (imageUrls == null || !imageUrls.Any())
            {
                return result; // No images is valid
            }

            // Check maximum number of images
            if (imageUrls.Count > MaxImagesPerTemplate)
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.Message = $"Số lượng hình ảnh không được vượt quá {MaxImagesPerTemplate}";
                result.ValidationErrors.Add($"Tối đa {MaxImagesPerTemplate} hình ảnh cho mỗi tour template");
                return result;
            }

            // Check for duplicate URLs
            var duplicates = imageUrls.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicates.Any())
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.Message = "Có hình ảnh trùng lặp";
                result.ValidationErrors.Add($"Hình ảnh trùng lặp: {string.Join(", ", duplicates)}");
                return result;
            }

            // Validate each image URL
            var invalidUrls = new List<string>();
            var notFoundUrls = new List<string>();

            foreach (var imageUrl in imageUrls)
            {
                // Check URL format
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    invalidUrls.Add("URL rỗng");
                    continue;
                }

                // Check file extension
                var extension = Path.GetExtension(imageUrl).ToLower();
                if (!_allowedExtensions.Contains(extension))
                {
                    invalidUrls.Add($"{imageUrl} (định dạng không hỗ trợ)");
                    continue;
                }

                // Check if image exists in database
                var existingImages = await _unitOfWork.ImageRepository.GetAllAsync(x => x.Url.Equals(imageUrl) && !x.IsDeleted);
                if (!existingImages.Any())
                {
                    notFoundUrls.Add(imageUrl);
                }
            }

            // Add validation errors
            if (invalidUrls.Any())
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.ValidationErrors.AddRange(invalidUrls.Select(url => $"URL không hợp lệ: {url}"));
            }

            if (notFoundUrls.Any())
            {
                result.IsValid = false;
                result.StatusCode = 404;
                result.ValidationErrors.AddRange(notFoundUrls.Select(url => $"Không tìm thấy hình ảnh: {url}"));
            }

            if (!result.IsValid)
            {
                result.Message = "Có lỗi với hình ảnh được cung cấp";
            }

            return result;
        }

        /// <summary>
        /// Get images for tour template
        /// </summary>
        public async Task<List<Image>> GetImagesAsync(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
            {
                return new List<Image>();
            }

            var images = new List<Image>();
            foreach (var imageUrl in imageUrls)
            {
                var existingImages = await _unitOfWork.ImageRepository.GetAllAsync(x => x.Url.Equals(imageUrl) && !x.IsDeleted);
                var image = existingImages.FirstOrDefault();
                if (image != null)
                {
                    images.Add(image);
                }
            }

            return images;
        }

        /// <summary>
        /// Update images for tour template
        /// </summary>
        public async Task<ResponseValidationDto> UpdateTourTemplateImagesAsync(TourTemplate template, List<string>? newImageUrls)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            try
            {
                if (newImageUrls != null)
                {
                    // Validate new image URLs
                    var validationResult = await ValidateImageUrlsAsync(newImageUrls);
                    if (!validationResult.IsValid)
                    {
                        return validationResult;
                    }

                    // Get new images
                    var newImages = await GetImagesAsync(newImageUrls);
                    
                    // Update template images
                    template.Images.Clear();
                    foreach (var image in newImages)
                    {
                        template.Images.Add(image);
                    }
                }

                result.Message = "Cập nhật hình ảnh thành công";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.StatusCode = 500;
                result.Message = "Lỗi khi cập nhật hình ảnh";
                result.ValidationErrors.Add($"Chi tiết lỗi: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Copy images from source template to destination template
        /// </summary>
        public async Task<ResponseValidationDto> CopyImagesAsync(TourTemplate sourceTemplate, TourTemplate destinationTemplate)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            try
            {
                if (sourceTemplate.Images != null && sourceTemplate.Images.Any())
                {
                    // Copy all images from source to destination
                    destinationTemplate.Images.Clear();
                    foreach (var image in sourceTemplate.Images)
                    {
                        destinationTemplate.Images.Add(image);
                    }
                }

                result.Message = "Sao chép hình ảnh thành công";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.StatusCode = 500;
                result.Message = "Lỗi khi sao chép hình ảnh";
                result.ValidationErrors.Add($"Chi tiết lỗi: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Clean up unused images (optional - for maintenance)
        /// </summary>
        public async Task<int> CleanupUnusedImagesAsync()
        {
            try
            {
                // Get all images that are not associated with any tour template
                var allImages = await _unitOfWork.ImageRepository.GetAllAsync();
                var usedImageIds = new HashSet<Guid>();

                // Get all tour templates with their images
                var allTemplates = await _unitOfWork.TourTemplateRepository.GetAllAsync();
                foreach (var template in allTemplates)
                {
                    if (template.Images != null)
                    {
                        foreach (var image in template.Images)
                        {
                            usedImageIds.Add(image.Id);
                        }
                    }
                }

                // Find unused images
                var unusedImages = allImages.Where(img => !usedImageIds.Contains(img.Id) && !img.IsDeleted).ToList();

                // Mark unused images as deleted (soft delete)
                foreach (var unusedImage in unusedImages)
                {
                    unusedImage.IsDeleted = true;
                    unusedImage.DeletedAt = DateTime.UtcNow;
                    await _unitOfWork.ImageRepository.Update(unusedImage);
                }

                if (unusedImages.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return unusedImages.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get image statistics for tour template
        /// </summary>
        public async Task<object> GetImageStatisticsAsync(Guid? templateId = null)
        {
            try
            {
                if (templateId.HasValue)
                {
                    // Statistics for specific template
                    var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(templateId.Value, new[] { "Images" });
                    if (template == null)
                    {
                        return new { Error = "Template not found" };
                    }

                    return new
                    {
                        TemplateId = templateId.Value,
                        ImageCount = template.Images?.Count ?? 0,
                        Images = template.Images?.Select(img => new
                        {
                            img.Id,
                            img.Url,
                            img.CreatedAt
                        }).ToList() ?? new List<object>()
                    };
                }
                else
                {
                    // Global statistics
                    var allTemplates = await _unitOfWork.TourTemplateRepository.GetAllAsync();
                    var totalImages = 0;
                    var templatesWithImages = 0;
                    var templatesWithoutImages = 0;

                    foreach (var template in allTemplates.Where(t => !t.IsDeleted))
                    {
                        var imageCount = template.Images?.Count ?? 0;
                        totalImages += imageCount;

                        if (imageCount > 0)
                            templatesWithImages++;
                        else
                            templatesWithoutImages++;
                    }

                    return new
                    {
                        TotalTemplates = allTemplates.Count(t => !t.IsDeleted),
                        TotalImages = totalImages,
                        TemplatesWithImages = templatesWithImages,
                        TemplatesWithoutImages = templatesWithoutImages,
                        AverageImagesPerTemplate = allTemplates.Any() ? (double)totalImages / allTemplates.Count(t => !t.IsDeleted) : 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }
    }
}
