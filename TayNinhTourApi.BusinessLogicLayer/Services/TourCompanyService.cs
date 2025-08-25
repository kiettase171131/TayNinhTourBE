using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Common.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;


namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class TourCompanyService : BaseService, ITourCompanyService
    {
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TourCompanyService(IUnitOfWork unitOfWork, IMapper mapper,IHostingEnvironment hostingEnvironment,IHttpContextAccessor httpContextAccessor) : base(mapper, unitOfWork)
        {
            _env = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseGetTourDto> GetTourByIdAsync(Guid id)
        {
            var include = new string[] { "CreatedBy", "UpdatedBy", nameof(Tour.Images) };

            var predicate = PredicateBuilder.New<Tour>(x => !x.IsDeleted);

            // Find the branch by id
            var tour = await _unitOfWork.TourRepository.GetByIdAsync(id, include);

            if (tour == null || tour.IsDeleted)
            {
                return new ResponseGetTourDto
                {
                    StatusCode = 404,
                    Message = "Không tìm thấy chi nhánh này",
                };
            }

            return new ResponseGetTourDto
            {
                StatusCode = 200,
                Data = _mapper.Map<TourDto>(tour)
            };
        }

        public async Task<ResponseGetToursDto> GetToursAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var include = new string[] { "CreatedBy", "UpdatedBy", nameof(Tour.Images) };

            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create a predicate for filtering
            var predicate = PredicateBuilder.New<Tour>(x => !x.IsDeleted);

            // Check if textSearch is null or empty
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x => (x.Title != null && x.Title.Contains(textSearch, StringComparison.OrdinalIgnoreCase)));
            }

            // Check if status is null or empty
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status);
            }

            // Get tours from repository
            var tours = await _unitOfWork.TourRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);

            var totalTours = tours.Count();
            var totalPages = (int)Math.Ceiling((double)totalTours / pageSizeValue);

            return new ResponseGetToursDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<TourDto>>(tours),
                TotalRecord = totalTours,
                TotalPages = totalPages,
            };
        }

        public async Task<BaseResposeDto> UpdateTourAsync(RequestUpdateTourDto request, Guid id, Guid updatedById)
        {
            var include = new string[] { nameof(Tour.Images) };

            // Find tour by id
            var existingTour = await _unitOfWork.TourRepository.GetByIdAsync(id, include);

            if (existingTour == null || existingTour.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Tour not found"
                };
            }

            // Update tour
            existingTour.Title = request.Title ?? existingTour.Title;
            existingTour.Description = request.Description ?? existingTour.Description;
            existingTour.Price = request.Price ?? existingTour.Price;
            existingTour.MaxGuests = request.MaxGuests ?? existingTour.MaxGuests;
            existingTour.TourType = request.TourType ?? existingTour.TourType;
            existingTour.IsActive = request.IsActive ?? existingTour.IsActive;

            existingTour.IsApproved = false;
            existingTour.Status = (byte)TourStatusEnum.Pending;

            // Assign images if provided
            if (request.Images != null && request.Images.Any())
            {
                var images = new List<Image>();
                // Validate image URLs
                foreach (var imageUrl in request.Images)
                {
                    // Find the image by URL
                    var existingImage = await _unitOfWork.ImageRepository.GetAllAsync((x => x.Url.Equals(imageUrl) && !x.IsDeleted));
                    if (existingImage == null || !existingImage.Any())
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "Invalid image URL provided"
                        };
                    }
                    images.Add(existingImage.FirstOrDefault()!);
                }

                // Clear existing images and assign new ones
                existingTour.Images = images;
            }

            // Save changes to database
            await _unitOfWork.TourRepository.Update(existingTour);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Cập nhật Tour thành công"
            };
        }

        public async Task<BaseResposeDto> CreateTourAsync(RequestCreateTourCmsDto request, Guid createdBy)
        {
            // Mapping the request to Tour entity
            var tour = _mapper.Map<Tour>(request);

            // Set default values
            tour.Status = (byte)TourStatusEnum.Pending;
            tour.CreatedById = createdBy;
            tour.IsApproved = false;

            // Assign images if provided
            if (request.Images != null && request.Images.Any())
            {
                var images = new List<Image>();
                // Validate image URLs
                foreach (var imageUrl in request.Images)
                {

                    // Find the image by URL
                    var existingImage = await _unitOfWork.ImageRepository.GetAllAsync((x => x.Url.Equals(imageUrl) && !x.IsDeleted));
                    if (existingImage == null || !existingImage.Any())
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "Invalid image URL provided"
                        };
                    }

                    images.Add(existingImage.FirstOrDefault()!);
                }

                tour.Images = images;
            }


            // Get user by id
            var user = await _unitOfWork.UserRepository.GetByIdAsync(createdBy);
            if (user == null || user.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Set the created by user
            tour.CreatedBy = user;

            // Add the tour to DB
            await _unitOfWork.TourRepository.AddAsync(tour);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 201,
                Message = "Tạo Tour thành công",
            };
        }

        public async Task<BaseResposeDto> DeleteTourAsync(Guid id)
        {
            // Find tour by id
            var tour = await _unitOfWork.TourRepository.GetByIdAsync(id);

            if (tour == null || tour.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Tour not found"
                };
            }

            // Delete tour
            tour.IsDeleted = true;
            tour.DeletedAt = DateTime.UtcNow;

            // Save changes to database
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Xóa tour thành công"
            };
        }

        public async Task<BaseResposeDto> ActivatePublicTourDetailsAsync(Guid tourDetailsId, Guid userId)
        {
            try
            {
                // 1. Get TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // 2. Check if TourDetails is in WaitToPublic status
                if (tourDetails.Status != DataAccessLayer.Enums.TourDetailsStatus.WaitToPublic)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"TourDetails phải ở trạng thái WaitToPublic để có thể kích hoạt public. Trạng thái hiện tại: {tourDetails.Status}",
                        success = false
                    };
                }

                // 3. Check permission - only tour company who created this TourDetails can activate
                if (tourDetails.CreatedById != userId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền kích hoạt public cho TourDetails này",
                        success = false
                    };
                }

                // 4. Update status to Public
                tourDetails.Status = DataAccessLayer.Enums.TourDetailsStatus.Public;
                tourDetails.UpdatedAt = DateTime.UtcNow;
                tourDetails.UpdatedById = userId;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã kích hoạt public cho TourDetails thành công. Khách hàng có thể booking tour này.",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi kích hoạt public: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> GetIncidentsAsync(Guid userId, int pageIndex, int pageSize, string? severity, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // Tìm tour company của user hiện tại
                var tourCompany = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (tourCompany == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin công ty tour",
                        success = false
                    };
                }

                // Build predicate để filter incidents
                var predicate = PredicateBuilder.New<TourIncident>(x => !x.IsDeleted);

                // Filter theo tour operations của company này
                predicate = predicate.And(x => x.TourOperation.TourDetails.TourTemplate.CreatedById == userId);

                // Filter theo severity nếu có
                if (!string.IsNullOrEmpty(severity))
                {
                    predicate = predicate.And(x => x.Severity == severity);
                }

                // Filter theo status nếu có
                if (!string.IsNullOrEmpty(status))
                {
                    predicate = predicate.And(x => x.Status == status);
                }

                // Filter theo date range nếu có
                if (fromDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate.And(x => x.CreatedAt <= toDate.Value.AddDays(1));
                }

                var include = new string[] {
                    "TourOperation.TourDetails.TourTemplate",
                    "ReportedByGuide"
                };

                var incidents = await _unitOfWork.TourIncidentRepository.GenericGetPaginationAsync(
                    pageIndex, pageSize, predicate, include);

                var incidentDtos = incidents.Select(incident => new
                {
                    Id = incident.Id,
                    Title = incident.Title,
                    Description = incident.Description,
                    Severity = incident.Severity,
                    Status = incident.Status,
                    CreatedAt = incident.CreatedAt,
                    TourName = incident.TourOperation?.TourDetails?.TourTemplate?.Title,
                    TourDate = incident.TourOperation?.CreatedAt, // Sử dụng CreatedAt thay vì TourDate
                    GuideName = incident.ReportedByGuide?.FullName,
                    GuidePhone = incident.ReportedByGuide?.PhoneNumber
                }).ToList();

                var totalCount = incidents.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách incidents thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi lấy danh sách incidents: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> GetActiveToursAsync(Guid userId)
        {
            try
            {
                // Build predicate để lấy tours đang hoạt động
                var predicate = PredicateBuilder.New<TourSlot>(x => !x.IsDeleted);

                // Filter theo tour company
                predicate = predicate.And(x => x.TourDetails != null && x.TourDetails.TourTemplate.CreatedById == userId);

                // Filter theo status (InProgress hoặc Available trong tương lai gần)
                var today = DateTime.Today;
                var futureLimit = today.AddDays(30); // Lấy tours trong 30 ngày tới

                predicate = predicate.And(x =>
                    (x.Status == TourSlotStatus.InProgress) ||
                    (x.Status == TourSlotStatus.Available && x.TourDate >= DateOnly.FromDateTime(today) && x.TourDate <= DateOnly.FromDateTime(futureLimit)));

                var include = new string[] {
                    "TourDetails.TourTemplate"
                };

                var activeSlots = await _unitOfWork.TourSlotRepository.GetAllAsync(predicate, include);

                var activeTourDtos = activeSlots.ToList().Select(slot => new
                {
                    TourSlotId = slot.Id,
                    TourName = slot.TourDetails?.TourTemplate?.Title,
                    StartDate = slot.TourDate,
                    EndDate = slot.TourDate, // TourSlot chỉ có TourDate, không có StartDate/EndDate riêng
                    Status = slot.Status.ToString(),
                    CurrentBookings = slot.CurrentBookings,
                    MaxGuests = slot.MaxGuests,
                    HasOperations = slot.TourDetails?.TourOperation != null
                }).ToList();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách tours đang hoạt động thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi lấy danh sách tours đang hoạt động: {ex.Message}",
                    success = false
                };
            }
        }
        public async Task<BaseResposeDto> UpdateTourCompanyAsync(CurrentUserObject currentUser, UpdateTourCompanyDto dto)
        {
            var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(currentUser.Id);

            if (tourCompany == null || tourCompany.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Tour company not found"
                };
            }

            if (!string.IsNullOrWhiteSpace(dto.CompanyName))
                tourCompany.CompanyName = dto.CompanyName;

            tourCompany.Description = dto.Description;
            tourCompany.Address = dto.Address;
            tourCompany.Website = dto.Website;

            // ✅ Upload file BusinessLicense nếu có
            if (dto.BusinessLicense != null && dto.BusinessLicense.Length > 0)
            {
                const long MaxSize = 5 * 1024 * 1024;
                var allowedExts = new[] { ".pdf", ".docx", ".doc", ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(dto.BusinessLicense.FileName).ToLowerInvariant();

                if (dto.BusinessLicense.Length > MaxSize)
                    return new BaseResposeDto { StatusCode = 400, Message = "File too large. Max 10MB." };

                if (!allowedExts.Contains(ext))
                    return new BaseResposeDto { StatusCode = 400, Message = "Invalid file type." };

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "files", "business-licenses");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"license_{Guid.NewGuid()}{ext}";
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.BusinessLicense.CopyToAsync(stream);

                var request = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var relativePath = Path.Combine("files", "business-licenses", fileName).Replace("\\", "/");

                tourCompany.BusinessLicense = $"{baseUrl}/{relativePath}";
            }

            tourCompany.UpdatedAt = DateTime.UtcNow;
            tourCompany.UpdatedById = currentUser.Id;

            await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                success = true,
                Message = "Tour company updated successfully"
            };
        }

        public async Task<ApiResponse<string>> UpdateTourCompanyLogoAsync(UpdateTourCompanyLogoDto dto, CurrentUserObject currentUser)
        {
            var logoFile = dto.Logo;

            if (logoFile == null || logoFile.Length == 0)
                return ApiResponse<string>.Error(400, "No file uploaded.");

            const long MaxFileSize = 5 * 1024 * 1024;
            if (logoFile.Length > MaxFileSize)
                return ApiResponse<string>.Error(400, "File too large. Max 5MB.");

            var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
                return ApiResponse<string>.Error(400, "Invalid file type. Only .png, .jpg, .jpeg, .webp allowed.");

            var tourCompany = await _unitOfWork.TourCompanyRepository.GetByUserIdAsync(currentUser.Id);
            if (tourCompany == null)
                return ApiResponse<string>.NotFound("You are not associated with any tour company.");

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "images", "tourcompany-logos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"tc_logo_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await logoFile.CopyToAsync(stream);

            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var relativePath = Path.Combine("images", "tourcompany-logos", fileName).Replace("\\", "/");
            var fullUrl = $"{baseUrl}/{relativePath}";

            tourCompany.LogoUrl = fullUrl;
            tourCompany.UpdatedAt = DateTime.UtcNow;
            tourCompany.UpdatedById = currentUser.Id;

            await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.Success(fullUrl, "Tour company logo updated successfully.");
        }


    }
}
