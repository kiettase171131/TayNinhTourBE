using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms.TayNinhTourApi.Business.DTOs.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class CmsService : ICmsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly BcryptUtility _bcryptUtility;
        private readonly IBlogRepository _blogRepository;
        private readonly IBlogCommentRepository _blogCommentRepository;
        private readonly IBlogReactionRepository _blogReactionRepository;
        private readonly ISpecialtyShopRepository _shopRepository;
        private readonly ITourGuideRepository _tourGuideRepository;
        private readonly ITourCompanyRepository _tourCompany;
        private readonly IAdminSettingDiscountRepository _adminSetting;
        public CmsService(IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IBlogRepository repo, IBlogCommentRepository blogCommentRepository, IBlogReactionRepository blogReactionRepository, IRoleRepository roleRepository, BcryptUtility bcryptUtility, ISpecialtyShopRepository shopRepository,ITourGuideRepository tourGuideRepository,IAdminSettingDiscountRepository adminSetting,ITourCompanyRepository tourCompany)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _blogRepository = repo;
            _blogCommentRepository = blogCommentRepository;
            _blogReactionRepository = blogReactionRepository;
            _roleRepository = roleRepository;
            _bcryptUtility = bcryptUtility;
            _shopRepository = shopRepository;
            _tourGuideRepository = tourGuideRepository;
            _adminSetting = adminSetting;
            _tourCompany = tourCompany;
        }

        public async Task<BaseResposeDto> DeleteUserAsync(Guid id)
        {
            // Find user by id
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Delete user
            user.IsDeleted = true;

            // Save changes to database
            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "User deleted successfully",
                success = true
            };
        }

        public async Task<ResponseGetBlogsDto> GetBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var include = new string[] { nameof(Blog.BlogImages) };
            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create a predicate for filtering
            var predicate = PredicateBuilder.New<Blog>(x => !x.IsDeleted);

            // Check if textSearch is null or empty
            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(b =>
             EF.Functions.Like(b.Title, $"%{textSearch}%"));
            }

            // Check if status is null or empty
            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status);
            }

            // Get tours from repository
            var blogs = await _blogRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);
            var blogIds = blogs.Select(b => b.Id).ToList();
            var likeCounts = await _blogReactionRepository.GetLikeCountsAsync(blogIds);
            var dislikeCounts = await _blogReactionRepository.GetDislikeCountsAsync(blogIds);
            var commentCounts = await _blogCommentRepository.GetCommentCountsAsync(blogIds);
            var dtos = _mapper.Map<List<BlogDto>>(blogs);
            dtos.ForEach(dto =>
            {
                dto.TotalLikes = likeCounts.GetValueOrDefault(dto.Id, 0);
                dto.TotalDislikes = dislikeCounts.GetValueOrDefault(dto.Id, 0);
                dto.TotalComments = commentCounts.GetValueOrDefault(dto.Id, 0);
            });
            var totalblogs = blogs.Count();
            var totalPages = (int)Math.Ceiling((double)totalblogs / pageSizeValue);

            return new ResponseGetBlogsDto
            {
                StatusCode = 200,
                Message = "Blog found successfully",
                success = true,
                Data = dtos,
                TotalRecord = totalblogs,
                TotalPages = totalPages,
            };
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
                    StatusCode = 200,
                    Message = "Không tìm thấy chi nhánh này",
                    success = true
                };
            }

            return new ResponseGetTourDto
            {
                StatusCode = 200,
                success = true,
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
                success = true,
                Data = _mapper.Map<List<TourDto>>(tours),
                TotalRecord = totalTours,
                TotalPages = totalPages,
            };
        }

        public async Task<ResponseGetUsersCmsDto> GetUserAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var include = new string[] { nameof(User.Role) };
            // Set page index and page size
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create predicate for filtering
            var predicate = PredicateBuilder.New<User>(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x => x.Name.Contains(textSearch, StringComparison.OrdinalIgnoreCase) || x.Email.Contains(textSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == status.Value);
            }
            predicate = predicate.And(x => x.Role.Name != "Admin");

            // Get users from repository
            var users = await _userRepository.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate,include);

            var totalUsers = users.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSizeValue);

            return new ResponseGetUsersCmsDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<UserCmsDto>>(users),
                TotalRecord = totalUsers,
                TotalPages = totalPages,
            };
        }

        public async Task<ResponseGetUserByIdCmsDto> GetUserByIdAsync(Guid id)
        {
            var include = new string[] { nameof(User.Role) };
            // Find user by id
            var user = await _userRepository.GetByIdAsync(id,include);

            if (user == null || user.IsDeleted)
            {
                return new ResponseGetUserByIdCmsDto
                {
                    StatusCode = 200,
                    Message = "User not found"
                };
            }

            return new ResponseGetUserByIdCmsDto
            {
                StatusCode = 200,
                success = true,
                Data = _mapper.Map<UserCmsDto>(user)
            };
        }
        public async Task<BaseResposeDto> CreateUserAsync(RequestCreateUserDto request)
        {
            // 1. Kiểm tra email trùng
            var existingUser = await _userRepository.GetFirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser != null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Email đã tồn tại"
                };
            }

            // 2. Kiểm tra role hợp lệ (không được là Admin)
            var role = await _roleRepository.GetByIdAsync(request.RoleId);
            if (role == null || role.IsDeleted || !role.IsActive || role.Name == Constants.RoleAdminName)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Role không hợp lệ"
                };
            }

            // 3. Tạo user mới
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _bcryptUtility.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true,
                IsVerified = true,  // Nếu muốn user CMS mặc định là verified
                CreatedAt = DateTime.UtcNow,
                CreatedById = Guid.Empty,  // hoặc gán userId đang login nếu cần
                Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png"
            };

            await _userRepository.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 201,
                Message = "Tạo user thành công",
                success = true
            };
        }


        public async Task<BaseResposeDto> UpdateBlogAsync(RequestUpdateBlogCmsDto request, Guid id, Guid updatedById)
        {
            var blog = await _blogRepository.GetByIdAsync(id);

            if (blog == null || blog.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }
            if (blog.Status == (byte)BlogStatus.Accepted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Blog Accepted"
                };
            }
            if (blog.Status == (byte)BlogStatus.Rejected)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Blog Rejected"
                };
            }
            // Update 
            blog.Status = request.Status ?? blog.Status;
            blog.CommentOfAdmin = request.CommentOfAdmin ?? blog.CommentOfAdmin;
            blog.UpdatedById = updatedById;
            // Get user by id
            var user = await _userRepository.GetByIdAsync(updatedById);
            if (user == null || user.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Set the updated by user
            blog.UpdatedById = updatedById;

            // Save changes to database
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Blog updated successfully",
                success = true
            };
        }

        public async Task<BaseResposeDto> UpdateTourAsync(RequestUpdateTourCmsDto request, Guid id, Guid updatedById)
        {

            // Find tour by id
            var existingTour = await _unitOfWork.TourRepository.GetByIdAsync(id);

            if (existingTour == null || existingTour.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Tour not found"
                };
            }

            // Check if the tour is already approved
            if (existingTour.IsApproved)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Tour đã được phê duyệt"
                };
            }

            // Update tour
            existingTour.Status = request.Status ?? existingTour.Status;
            existingTour.CommentApproved = request.CommentApproved ?? existingTour.CommentApproved;

            existingTour.IsApproved = true;
            existingTour.UpdatedById = updatedById;

            // Get user by id
            var user = await _userRepository.GetByIdAsync(updatedById);
            if (user == null || user.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // Set the updated by user
            existingTour.UpdatedBy = user;

            // Save changes to database
            await _unitOfWork.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Cập nhật Tour thành công",
                success = true
            };
        }

        public async Task<BaseResposeDto> UpdateUserAsync(RequestUpdateUserCmsDto request, Guid id)
        {
            // Load user cùng tất cả navigation cần thiết
            var existingUser = await _userRepository.GetUserWithAllNavigationsAsync(id);
            if (existingUser == null)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            bool isDeactivating = existingUser.IsActive && request.IsActive == false;

            existingUser.Name = request.Name ?? existingUser.Name;
            existingUser.PhoneNumber = request.PhoneNumber ?? existingUser.PhoneNumber;
            existingUser.Avatar = request.Avatar ?? existingUser.Avatar;
            existingUser.IsActive = request.IsActive ?? existingUser.IsActive;

            if (isDeactivating)
            {
                // Soft delete các entity 1-1
                if (existingUser.SpecialtyShop != null)
                    existingUser.SpecialtyShop.IsDeleted = true;
                if (existingUser.TourGuide != null)
                    existingUser.TourGuide.IsDeleted = true;
                if (existingUser.TourCompany != null)
                    existingUser.TourCompany.IsDeleted = true;

                // Soft delete các collection
                foreach (var item in existingUser.ToursCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.ToursUpdated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourSlotsCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourSlotsUpdated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.Blogs)
                    item.IsDeleted = true;
                foreach (var item in existingUser.BlogReactions)
                    item.IsDeleted = true;
                foreach (var item in existingUser.BlogComments)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TicketsCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TicketsAssigned)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TicketComments)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourOperationsAsGuide)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourOperationsCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourOperationsUpdated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.ApprovedTourGuides)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourTemplatesCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourTemplatesUpdated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourDetailsCreated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourDetailsUpdated)
                    item.IsDeleted = true;
                foreach (var item in existingUser.BankAccounts)
                    item.IsDeleted = true;
                foreach (var item in existingUser.WithdrawalRequests)
                    item.IsDeleted = true;
                foreach (var item in existingUser.TourBookingRefunds)
                    item.IsDeleted = true;
            }

            await _userRepository.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "User updated successfully",
                success = true
            };
        }


        public async Task<ResponseGetSpecialtyShopsDto> GetSpecialtyShopsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? isActive)
        {
            var include = new string[] { nameof(SpecialtyShop.User) };

            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            var predicate = PredicateBuilder.New<SpecialtyShop>(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x =>
                    x.ShopName.Contains(textSearch, StringComparison.OrdinalIgnoreCase) ||
                    x.RepresentativeName.Contains(textSearch, StringComparison.OrdinalIgnoreCase) ||
                    x.Email.Contains(textSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (isActive.HasValue)
            {
                predicate = predicate.And(x => x.IsShopActive == isActive.Value);
            }

            // Lấy danh sách SpecialtyShop với phân trang
            var shops = await _shopRepository.GenericGetPaginationAsync(
                pageIndexValue,
                pageSizeValue,
                predicate,
                include
            );

            var totalShops = shops.Count();
            var totalPages = (int)Math.Ceiling((double)totalShops / pageSizeValue);

            return new ResponseGetSpecialtyShopsDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<SpecialtyShopCmsDto>>(shops),
                TotalRecord = totalShops,
                TotalPages = totalPages
            };
        }
        public async Task<ResponseGetTourGuidesDto> GetTourGuidesAsync(int? pageIndex,int? pageSize,string? textSearch,bool? isAvailable, bool? isActive)
        {
            // Bao gồm navigation property User (nếu cần)
            var include = new string[] { nameof(TourGuide.User) };

            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Khởi tạo predicate mặc định: không lấy tour guide đã bị xoá (nếu có IsDeleted)
            var predicate = PredicateBuilder.New<TourGuide>(x => true);

            // Search theo FullName (nếu có textSearch)
            if (!string.IsNullOrEmpty(textSearch))
            {
                var search = textSearch.ToLower();
                predicate = predicate.And(x => x.FullName.ToLower().Contains(search));
            }
            if (isActive.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == isActive.Value);
            }
            // Lọc trạng thái IsAvailable nếu có
            if (isAvailable.HasValue)
            {
                predicate = predicate.And(x => x.IsAvailable == isAvailable.Value);
            }

            // Lấy danh sách TourGuide phân trang
            var tourGuides = await _tourGuideRepository.GenericGetPaginationAsync(
                pageIndexValue,
                pageSizeValue,
                predicate,
                include
            );

            var totalGuides = tourGuides.Count();
            var totalPages = (int)Math.Ceiling((double)totalGuides / pageSizeValue);

            return new ResponseGetTourGuidesDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<TourGuideCmsDto>>(tourGuides),
                TotalRecord = totalGuides,
                TotalPages = totalPages
            };
        }
        public async Task<ResponseGetTourCompaniesDto> GetTourCompaniesAsync(int? pageIndex,int? pageSize,string? textSearch,bool? isActive)
        {
            var include = new string[] { nameof(TourCompany.User) };

            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            var predicate = PredicateBuilder.New<TourCompany>(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(textSearch))
            {
                predicate = predicate.And(x =>
                    x.CompanyName.Contains(textSearch, StringComparison.OrdinalIgnoreCase) ||
                    x.Description!.Contains(textSearch, StringComparison.OrdinalIgnoreCase) ||
                    x.Address!.Contains(textSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (isActive.HasValue)
            {
                predicate = predicate.And(x => x.IsActive == isActive.Value);
            }

            // Lấy danh sách TourCompany với phân trang
            var companies = await _tourCompany.GenericGetPaginationAsync(
                pageIndexValue,
                pageSizeValue,
                predicate,
                include
            );

            var totalCompanies = companies.Count();
            var totalPages = (int)Math.Ceiling((double)totalCompanies / pageSizeValue);

            return new ResponseGetTourCompaniesDto
            {
                StatusCode = 200,
                Data = _mapper.Map<List<TourCompanyCmsDto>>(companies),
                TotalRecord = totalCompanies,
                TotalPages = totalPages
            };
        }

        public async Task<decimal> GetTourDiscountPercentAsync()
        => await _adminSetting.GetTourDiscountPercentAsync();

        public async Task UpdateTourDiscountPercentAsync(decimal newPercent)
            => await _adminSetting.UpdateTourDiscountPercentAsync(newPercent);
    }
}
