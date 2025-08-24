using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Account;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpTicket;
using TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;


namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _ticketRepo;
        private readonly ISupportTicketCommentRepository _commentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SupportTicketService(
            ISupportTicketRepository ticketRepo,
            ISupportTicketCommentRepository commentRepo, IUserRepository userRepo, IHostingEnvironment env,
        IHttpContextAccessor httpContextAccessor
            )
        {
            _ticketRepo = ticketRepo;
            _commentRepo = commentRepo;
            _userRepo = userRepo;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseSpTicketDto> CreateTicketAsync(RequestCreateTicketDto request, CurrentUserObject currentUserObject)
        {
            // Lấy danh sách admin từ repo mới
            var admins = await _userRepo.ListAdminsAsync();
            if (!admins.Any())
            {
                return new ResponseSpTicketDto
                {
                    StatusCode = 404,
                    Message = "No admin found"
                };
            }
                

            // random 1 admin
            var randomAdmin = admins.OrderBy(_ => Guid.NewGuid()).First();

            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                UserId = currentUserObject.Id,
                AdminId = randomAdmin.Id,
                Title = request.Title,
                Content = request.Content,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow,
                CreatedById = currentUserObject.Id
            };
            var uploadedUrls = new List<string>();
            // 3. Xử lý upload file - Cho phép nhiều loại file hơn
            if (request.Files != null && request.Files.Any())
            {
                const long MaxFileSize = 10 * 1024 * 1024; // Tăng lên 10 MB
                // Cho phép cả hình ảnh và documents
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp", ".pdf", ".doc", ".docx", ".txt" };

                // Đường dẫn gốc để lưu file
                var webRoot = _env.WebRootPath
                              ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads", "tickets", ticket.Id.ToString());

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
                        return new ResponseSpTicketDto
                        {
                            StatusCode = 400,
                            Message = $"File '{file.FileName}' quá lớn. Kích thước tối đa là {MaxFileSize / (1024 * 1024)} MB."
                        };

                    // 3.2 Kiểm tra định dạng
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                        return new ResponseSpTicketDto
                        {
                            StatusCode = 400,
                            Message = $"Định dạng file '{ext}' không được hỗ trợ. Các định dạng được phép: {string.Join(", ", allowedExts)}"
                        };

                    // 3.3 Validate tên file
                    if (string.IsNullOrWhiteSpace(file.FileName) || file.FileName.Length > 255)
                    {
                        return new ResponseSpTicketDto
                        {
                            StatusCode = 400,
                            Message = "Tên file không hợp lệ hoặc quá dài"
                        };
                    }

                    // 3.4 Tạo tên file an toàn
                    var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var safeFileName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars()));
                    
                    // Giới hạn độ dài tên file
                    if (safeFileName.Length > 50)
                    {
                        safeFileName = safeFileName.Substring(0, 50);
                    }
                    
                    var filename = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}_{safeFileName}{ext}";
                    var filePath = Path.Combine(uploadsFolder, filename);

                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        return new ResponseSpTicketDto
                        {
                            StatusCode = 500,
                            Message = $"Lỗi khi lưu file '{file.FileName}': {ex.Message}"
                        };
                    }

                    // 3.5 Sinh URL public
                    var fileUrl = $"{baseUrl}/uploads/tickets/{ticket.Id}/{filename}";
                    uploadedUrls.Add(fileUrl);

                    // 3.6 Thêm vào danh sách ảnh của ticket
                    ticket.Images.Add(new SupportTicketImage
                    {
                        Id = Guid.NewGuid(),
                        SupportTicketId = ticket.Id,
                        Url = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = currentUserObject.Id
                    });
                }
            }

            // 4. Lưu xuống DB
            await _ticketRepo.AddAsync(ticket);
            await _ticketRepo.SaveChangesAsync();

            return new ResponseSpTicketDto
            {
                StatusCode = 200,
                Message = "Ticket created successfully",
                SupportTicketId = ticket.Id,
                ImageUrls = uploadedUrls
            };
        }



        public async Task<IEnumerable<SupportTicketDto>> GetTicketsForUserAsync(Guid userid)
        {
            var entities = await _ticketRepo.ListByUserAsync(userid);
            if (entities == null || !entities.Any())
            {
                return new SupportTicketDto[]
                {
                    new SupportTicketDto
                    {
                        StatusCode = 200,
                        Message = "No tickets found for this user."
                    }
                };
            }

            var dtos = entities
                .Where(t => !t.IsDeleted)
                .Select(t => new SupportTicketDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserEmail = t.User.Email,
                Title = t.Title,
                Content = t.Content,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                AdminId = t.AdminId,

                Images = t.Images.Select(i => new SupportTicketImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList(),

                Comments = t.Comments.Select(c => new SupportTicketCommentDto
                {
                    Id = c.Id,
                    CreatedById = c.CreatedById,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt
                }).ToList()
            });

            return dtos;
        }


        public async Task<IEnumerable<SupportTicketDto>> GetTicketsForAdminAsync(Guid adminId)
        {
            var entities = await _ticketRepo.ListByAdminAsync(adminId);
            if (entities == null || !entities.Any())
            {
                return new SupportTicketDto[]
                {
                    new SupportTicketDto
                    {
                        StatusCode = 200,
                        Message = "No tickets found for this admin."
                    }
                };
            }

            var dtos = entities
                .Where(t => !t.IsDeleted)
                .Select(t => new SupportTicketDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserEmail = t.User.Email,
                Title = t.Title,
                Content = t.Content,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                AdminId = t.AdminId,

                Images = t.Images.Select(i => new SupportTicketImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList(),

                Comments = t.Comments.Select(c => new SupportTicketCommentDto
                {
                    Id = c.Id,
                    CreatedById = c.CreatedById,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt
                }).ToList()
            });

            return dtos;
        }

        public async Task<SupportTicketDto?> GetTicketDetailsAsync(Guid ticketId)
        {
            var entity = await _ticketRepo.GetDetail(ticketId);
            if (entity == null)
            {
                return new SupportTicketDto
                {
                    StatusCode = 200,
                    Message = "Support ticket not found"
                };
            }

            var dto = new SupportTicketDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserEmail = entity.User.Email,
                AdminId = entity.AdminId,
                Title = entity.Title,
                Content = entity.Content,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                Images = entity.Images.Select(i => new SupportTicketImageDto
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList(),
                Comments = entity.Comments.Select(c => new SupportTicketCommentDto
                {
                    Id = c.Id,
                    CreatedById = c.CreatedById,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return dto;
        }
        

        public async Task<BaseResposeDto> ReplyAsync(Guid ticketId, Guid replierId, string comment)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null || ticket.IsDeleted)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Support ticket not found"
                };
            }
            if (ticket.Status != 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 404,
                    Message = "Cannot reply this ticket, it's closed"
                };
            }
            var reply = new SupportTicketComment
            {
                Id = Guid.NewGuid(),
                SupportTicketId = ticketId,
                CreatedById = replierId,
                CommentText = comment,
                CreatedAt = DateTime.UtcNow,

                //CreatedById = replierId
            };
            ticket.Status = TicketStatus.Closed;
            await _commentRepo.AddAsync(reply);
            await _commentRepo.SaveChangesAsync();
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Send successful"
            };
        }

        //public async Task<BaseResposeDto> ChangeStatusAsync(Guid ticketId, TicketStatus newStatus)
        //{
        //    var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        //    if (ticket == null || ticket.IsDeleted)
        //    {
        //        return new BaseResposeDto
        //        {
        //            StatusCode = 404,
        //            Message = "Support ticket not found"
        //        };
        //    }
        //    ticket.Status = newStatus;
        //    ticket.UpdatedAt = DateTime.UtcNow;
        //    await _ticketRepo.SaveChangesAsync();
        //    return new BaseResposeDto
        //    {
        //        StatusCode = 200,
        //        Message = "Status update successful"
        //    };
        //}

        public async Task<BaseResposeDto> DeleteTicketAsync(Guid ticketId, Guid requestorId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket.UserId != requestorId)
            {
                return new BaseResposeDto
                {
                    StatusCode = 403,
                    Message = "You do not have permission to delete this ticket"
                };
            }
                
            // soft-delete
            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepo.SaveChangesAsync();
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Ticket deleted successfully"
            };
        }
    }

}
