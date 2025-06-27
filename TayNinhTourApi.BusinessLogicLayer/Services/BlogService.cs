using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Common.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpTicket;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.UnitOfWork;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class BlogService : IBlogService
    {
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IBlogRepository _repo;
        private readonly IBlogImageRepository _repo2;
        private readonly IBlogCommentRepository _blogCommentRepository;
        private readonly IBlogReactionRepository _blogReactionRepository;

        public BlogService(IMapper mapper, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor,IBlogRepository repo, IBlogImageRepository repo2, IBlogCommentRepository blogCommentRepository, IBlogReactionRepository blogReactionRepository)
        {
            _repo = repo;
            _mapper = mapper;      
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _repo2 = repo2;
            _blogCommentRepository = blogCommentRepository;
            _blogReactionRepository = blogReactionRepository;

        }

        public async Task<ResponseCreateBlogDto> CreateBlogAsync(RequestCreateBlogDto request, CurrentUserObject currentUserObject)
        {
            
            var blog = new Blog
            {
                Id = Guid.NewGuid(),
                UserId = currentUserObject.Id,
                Title = request.Title,
                Content = request.Content,
                AuthorName = currentUserObject.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Status = (byte)BlogStatus.Pending,
                CreatedById = currentUserObject.Id,
            };
            var uploadedUrls = new List<string>();
            // 3. Xử lý upload file tương tự avatar
            if (request.Files != null && request.Files.Any())
            {
                const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };

                // Đường dẫn gốc để lưu file
                var webRoot = _env.WebRootPath
                              ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads", "blogs", blog.Id.ToString());

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Tạo base URL để client truy cập
                var req = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{req.Scheme}://{req.Host.Value}";

                foreach (var file in request.Files)
                {
                    if (file.Length == 0)
                        continue;

                    // 3.1 Kiểm tra kích thước
                    if (file.Length > MaxFileSize)
                        return new ResponseCreateBlogDto
                        {
                            StatusCode = 400,
                            Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                        };

                    // 3.2 Kiểm tra định dạng
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                        return new ResponseCreateBlogDto
                        {
                            StatusCode = 400,
                            Message = "Invalid file type. Only .png, .jpg, .jpeg, .webp are allowed."
                        };

                    // 3.3 Đổi tên và lưu file
                    var filename = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, filename);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    // 3.4 Sinh URL public
                    var fileUrl = $"{baseUrl}/uploads/blogs/{blog.Id}/{filename}";
                    uploadedUrls.Add(fileUrl);


                    // 3.5 Thêm vào danh sách ảnh của ticket
                    blog.BlogImages.Add(new BlogImage
                    {
                        Id = Guid.NewGuid(),
                        BlogId = blog.Id,
                        Url = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUserObject.Id
                    });
                }
            }

            // 4. Lưu xuống DB
            await _repo.AddAsync(blog);
            await _repo.SaveChangesAsync();

            return new ResponseCreateBlogDto
            {
                StatusCode = 200,
                Message = "Blog created successfully",
                IsSuccess = true,
                BlogId = blog.Id,
                ImageUrls = uploadedUrls
            };
        }

        public async Task<BaseResposeDto> DeleteBlogAsync(Guid id)
        {
           
            var blog = await _repo.GetByIdAsync(id);

            if (blog == null || blog.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }

            
            blog.IsDeleted = true;
            blog.DeletedAt = DateTime.UtcNow;

            // Save changes to database
            await _repo.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Blog deleted successfully",
                IsSuccess = true
            };
        }

        public async Task<ResponseGetBlogByIdDto> GetBlogByIdAsync(Guid id, Guid? currentUserId)
        {
            var include = new string[] { nameof(Blog.BlogImages) };

            var predicate = PredicateBuilder.New<Blog>(x => !x.IsDeleted);
            // Find the branch by id
            var blog = await _repo.GetByIdAsync(id,include);

            if (blog == null || blog.IsDeleted)
            {
                return new ResponseGetBlogByIdDto
                {
                    StatusCode = 200,
                    Message = "Blog Not Found",
                };
            }
            var likeCounts = await _blogReactionRepository.GetLikeCountsAsync(new[] { id });
            var dislikeCounts = await _blogReactionRepository.GetDislikeCountsAsync(new[] { id });
            var commentCounts = await _blogCommentRepository.GetCommentCountsAsync(new[] { id });
            bool hasLiked = false;
            if (currentUserId.HasValue)
            {
                var likedIds = await _blogReactionRepository
                                    .GetBlogIdsUserLikedAsync(currentUserId.Value, new[] { id });
                hasLiked = likedIds.Contains(id);
            }


            // 3. Map entity → DTO
            var dto = _mapper.Map<BlogDto>(blog);

            // 4. Gán thêm 3 trường thống kê
            dto.TotalLikes = likeCounts.GetValueOrDefault(id, 0);
            dto.TotalDislikes = dislikeCounts.GetValueOrDefault(id, 0);
            dto.TotalComments = commentCounts.GetValueOrDefault(id, 0);
            dto.HasLiked = hasLiked;
            return new ResponseGetBlogByIdDto
            {
                StatusCode = 200,
                Message = "Blog found successfully",
                IsSuccess = true,
                Data = dto
            };
        }

        public async Task<ResponseGetBlogsDto> GetBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, CurrentUserObject currentUserObject)
        {
            var include = new string[] { nameof(Blog.BlogImages) };
            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create a predicate for filtering
            var predicate = PredicateBuilder.New<Blog>(x => !x.IsDeleted && x.CreatedById == currentUserObject.Id);

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
            var blogs = await _repo.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);
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
                IsSuccess = true,
                Data = dtos,
                TotalRecord = totalblogs,
                TotalPages = totalPages,
            };
        }
        public async Task<ResponseGetBlogsDto> GetAcceptedBlogsAsync(int? pageIndex, int? pageSize, string? textSearch, bool? status, Guid? currentUserId)
        {
            var include = new string[] { nameof(Blog.BlogImages) };
            // Default values for pagination
            var pageIndexValue = pageIndex ?? Constants.PageIndexDefault;
            var pageSizeValue = pageSize ?? Constants.PageSizeDefault;

            // Create a predicate for filtering
            var predicate = PredicateBuilder.New<Blog>(x => !x.IsDeleted);
            predicate = predicate.And(x => x.Status == (byte)BlogStatus.Accepted);

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
            var blogs = await _repo.GenericGetPaginationAsync(pageIndexValue, pageSizeValue, predicate, include);
            var blogIds = blogs.Select(b => b.Id).ToList();
            var likeCounts = await _blogReactionRepository.GetLikeCountsAsync(blogIds);
            var dislikeCounts = await _blogReactionRepository.GetDislikeCountsAsync(blogIds);
            var commentCounts = await _blogCommentRepository.GetCommentCountsAsync(blogIds);
            List<Guid> likedByUser = new();
            if (currentUserId.HasValue)
            {
                likedByUser = await _blogReactionRepository
                                   .GetBlogIdsUserLikedAsync(currentUserId.Value, blogIds);
            }
            var dtos = _mapper.Map<List<BlogDto>>(blogs);
            dtos.ForEach(dto =>
            {
                dto.TotalLikes = likeCounts.GetValueOrDefault(dto.Id, 0);
                dto.TotalDislikes = dislikeCounts.GetValueOrDefault(dto.Id, 0);
                dto.TotalComments = commentCounts.GetValueOrDefault(dto.Id, 0);
                dto.HasLiked = likedByUser.Contains(dto.Id);
            });
            var totalblogs = blogs.Count();
            var totalPages = (int)Math.Ceiling((double)totalblogs / pageSizeValue);

            return new ResponseGetBlogsDto
            {
                StatusCode = 200,
                Message = "Blog found successfully",
                IsSuccess = true,
                Data = dtos,
                TotalRecord = totalblogs,
                TotalPages = totalPages,
            };
        }

        public async Task<BaseResposeDto> UpdateBlogAsync(RequestUpdateBlogDto request, Guid id, CurrentUserObject currentUserObject)
        {
            var include = new string[] { nameof(Blog.BlogImages) };

            var blog = await _repo.GetByIdAsync(id,include);

            if (blog == null || blog.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }
            

            // Update 
            blog.Title = request.Title ?? blog.Title;
            blog.Content = request.Content ?? blog.Content;
            blog.AuthorName = currentUserObject.Name;
            blog.UpdatedAt = DateTime.UtcNow;
            blog.UpdatedById = currentUserObject.Id;
            blog.Status = (byte)BlogStatus.Pending;


            // 4. Xử lý ảnh nếu client gửi Files
            var newUploadedUrls = new List<string>();
            if (request.Files != null && request.Files.Any())
            {
                // 4.1 Lấy danh sách BlogImage hiện tại
                //    (giả sử lazy-loading đã bật hoặc BlogImages đã được Include trước đó)
                var existingImages = blog.BlogImages.ToList();

                // 4.2 Xóa tất cả ảnh cũ: cả file trên disk và entity ở DB
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "blogs", blog.Id.ToString());

                foreach (var oldImage in existingImages)
                {
                    // Tách tên file từ URL
                    var uri = new Uri(oldImage.Url);
                    var oldFileName = Path.GetFileName(uri.LocalPath);
                    var oldFilePath = Path.Combine(folder, oldFileName);

                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);

                    await _repo2.DeleteAsync(oldImage.Id);
                }

                // 4.3 Đảm bảo folder tồn tại
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // 4.4 Lặp qua từng file mới, validate và lưu
                const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };

                var req = _httpContextAccessor.HttpContext!.Request;
                var baseUrl = $"{req.Scheme}://{req.Host.Value}";

                foreach (var file in request.Files)
                {
                    if (file.Length == 0)
                        continue;

                    // 4.4.1 Kiểm tra kích thước
                    if (file.Length > MaxFileSize)
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"File quá lớn. Kích thước tối đa là {MaxFileSize / (1024 * 1024)} MB."
                        };
                    }

                    // 4.4.2 Kiểm tra định dạng
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = "Định dạng file không hợp lệ. Chỉ cho phép .png, .jpg, .jpeg, .webp."
                        };
                    }

                    // 4.4.3 Tạo tên file mới và lưu vào disk
                    var newFileName = $"{Guid.NewGuid()}{ext}";
                    var newFilePath = Path.Combine(folder, newFileName);
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    // 4.4.4 Sinh URL công khai
                    var fileUrl = $"{baseUrl}/uploads/blogs/{blog.Id}/{newFileName}";
                    newUploadedUrls.Add(fileUrl);

                    // 4.4.5 Tạo entity BlogImage mới
                    var blogImage = new BlogImage
                    {
                        Id = Guid.NewGuid(),
                        BlogId = blog.Id,
                        Url = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUserObject.Id,
                        IsActive = true
                    };
                    await _repo2.AddAsync(blogImage);
                }
            }

            // Save changes to database
            await _repo.UpdateAsync(blog);
            await _repo.SaveChangesAsync();

            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Blog updated successfully",
                IsSuccess = true
            };
        }
    }
    
}
