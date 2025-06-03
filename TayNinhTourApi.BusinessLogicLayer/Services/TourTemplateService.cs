using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Legacy TourTemplateService implementation - DEPRECATED
    /// Sử dụng EnhancedTourTemplateService thay thế
    /// </summary>
    public class LegacyTourTemplateService : BaseService
    {
        private readonly TourTemplateImageHandler _imageHandler;

        public LegacyTourTemplateService(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        {
            _imageHandler = new TourTemplateImageHandler(unitOfWork);
        }

        public async Task<TourTemplate> CreateTourTemplateAsync(RequestCreateTourTemplateDto request, Guid createdById)
        {
            // Map DTO to entity
            var tourTemplate = _mapper.Map<TourTemplate>(request);

            // Set audit fields
            tourTemplate.Id = Guid.NewGuid();
            tourTemplate.CreatedById = createdById;
            tourTemplate.CreatedAt = DateTime.UtcNow;
            tourTemplate.IsActive = true;
            tourTemplate.IsDeleted = false;

            // Add to repository
            await _unitOfWork.TourTemplateRepository.AddAsync(tourTemplate);
            await _unitOfWork.SaveChangesAsync();

            return tourTemplate;
        }

        public async Task<TourTemplate?> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById)
        {
            var existingTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
            if (existingTemplate == null || existingTemplate.IsDeleted)
            {
                return null;
            }

            // Map updates
            _mapper.Map(request, existingTemplate);

            // Set audit fields
            existingTemplate.UpdatedById = updatedById;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TourTemplateRepository.Update(existingTemplate);
            await _unitOfWork.SaveChangesAsync();

            return existingTemplate;
        }

        public async Task<bool> DeleteTourTemplateAsync(Guid id, Guid deletedById)
        {
            var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
            if (template == null || template.IsDeleted)
            {
                return false;
            }

            // Check if template can be deleted
            var canDelete = await CanDeleteTourTemplateAsync(id);
            if (!canDelete)
            {
                return false;
            }

            // Soft delete
            template.IsDeleted = true;
            template.DeletedAt = DateTime.UtcNow;
            template.UpdatedById = deletedById;
            template.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TourTemplateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<TourTemplate?> GetTourTemplateByIdAsync(Guid id)
        {
            return await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "CreatedBy", "UpdatedBy", "Images" });
        }

        public async Task<TourTemplate?> GetTourTemplateWithDetailsAsync(Guid id)
        {
            return await _unitOfWork.TourTemplateRepository.GetWithDetailsAsync(id);
        }

        public async Task<IEnumerable<TourTemplate>> GetTourTemplatesByCreatedByAsync(Guid createdById, bool includeInactive = false)
        {
            return await _unitOfWork.TourTemplateRepository.GetByCreatedByAsync(createdById, includeInactive);
        }

        public async Task<IEnumerable<TourTemplate>> GetTourTemplatesByTypeAsync(TourTemplateType templateType, bool includeInactive = false)
        {
            return await _unitOfWork.TourTemplateRepository.GetByTemplateTypeAsync(templateType, includeInactive);
        }

        public async Task<IEnumerable<TourTemplate>> SearchTourTemplatesAsync(string keyword, bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<TourTemplate>();
            }

            return await _unitOfWork.TourTemplateRepository.SearchAsync(keyword.Trim(), includeInactive);
        }

        public async Task<ResponseGetTourTemplatesDto> GetTourTemplatesPaginatedAsync(
            int pageIndex,
            int pageSize,
            TourTemplateType? templateType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? startLocation = null,
            bool includeInactive = false)
        {
            var (templates, totalCount) = await _unitOfWork.TourTemplateRepository.GetPaginatedAsync(
                pageIndex, pageSize, templateType, minPrice, maxPrice, startLocation, includeInactive);

            var tourTemplateDtos = _mapper.Map<List<TourTemplateSummaryDto>>(templates);

            return new ResponseGetTourTemplatesDto
            {
                StatusCode = 200,
                Data = tourTemplateDtos,
                TotalRecord = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<IEnumerable<TourTemplate>> GetPopularTourTemplatesAsync(int top = 10)
        {
            return await _unitOfWork.TourTemplateRepository.GetPopularTemplatesAsync(top);
        }

        public async Task<bool> SetTourTemplateActiveStatusAsync(Guid id, bool isActive, Guid updatedById)
        {
            var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
            if (template == null || template.IsDeleted)
            {
                return false;
            }

            template.IsActive = isActive;
            template.UpdatedById = updatedById;
            template.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TourTemplateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<TourTemplate?> CopyTourTemplateAsync(Guid id, string newTitle, Guid createdById)
        {
            var originalTemplate = await _unitOfWork.TourTemplateRepository.GetWithDetailsAsync(id);
            if (originalTemplate == null || originalTemplate.IsDeleted)
            {
                return null;
            }

            // Create new template based on original
            var newTemplate = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = newTitle,
                Description = originalTemplate.Description,
                Price = originalTemplate.Price,
                MaxGuests = originalTemplate.MaxGuests,
                Duration = originalTemplate.Duration,
                TemplateType = originalTemplate.TemplateType,
                ScheduleDays = originalTemplate.ScheduleDays,
                StartLocation = originalTemplate.StartLocation,
                EndLocation = originalTemplate.EndLocation,
                SpecialRequirements = originalTemplate.SpecialRequirements,
                MinGuests = originalTemplate.MinGuests,
                ChildPrice = originalTemplate.ChildPrice,
                ChildMaxAge = originalTemplate.ChildMaxAge,
                Transportation = originalTemplate.Transportation,
                MealsIncluded = originalTemplate.MealsIncluded,
                AccommodationInfo = originalTemplate.AccommodationInfo,
                IncludedServices = originalTemplate.IncludedServices,
                ExcludedServices = originalTemplate.ExcludedServices,
                CancellationPolicy = originalTemplate.CancellationPolicy,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.TourTemplateRepository.AddAsync(newTemplate);
            await _unitOfWork.SaveChangesAsync();

            return newTemplate;
        }

        public async Task<bool> CanDeleteTourTemplateAsync(Guid id)
        {
            // Check if template is being used in any tour slots
            return !await _unitOfWork.TourTemplateRepository.IsTemplateInUseAsync(id);
        }

        public async Task<object> GetTourTemplateStatisticsAsync(Guid? createdById = null)
        {
            // TODO: Implement comprehensive statistics
            // For now, return basic counts
            var allTemplates = createdById.HasValue
                ? await _unitOfWork.TourTemplateRepository.GetByCreatedByAsync(createdById.Value, true)
                : await _unitOfWork.TourTemplateRepository.GetAllAsync();

            var activeCount = allTemplates.Count(t => t.IsActive && !t.IsDeleted);
            var inactiveCount = allTemplates.Count(t => !t.IsActive && !t.IsDeleted);
            var deletedCount = allTemplates.Count(t => t.IsDeleted);

            return new
            {
                TotalTemplates = allTemplates.Count(),
                ActiveTemplates = activeCount,
                InactiveTemplates = inactiveCount,
                DeletedTemplates = deletedCount,
                TemplatesByType = allTemplates
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => t.TemplateType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }
    }
}
