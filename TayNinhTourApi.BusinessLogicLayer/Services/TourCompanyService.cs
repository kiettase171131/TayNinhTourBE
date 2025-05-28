using AutoMapper;
using LinqKit;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Common.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class TourCompanyService : BaseService, ITourCompanyService
    {
        public TourCompanyService(IUnitOfWork unitOfWork, IMapper mapper) : base(mapper, unitOfWork)
        {
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
    }
}
