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
            // 3. Xử lý upload file tương tự avatar
            if (request.Files != null && request.Files.Any())
            {
                const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".webp" };

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
                            Message = $"File too large. Max size is {MaxFileSize / (1024 * 1024)} MB."
                        };

                    // 3.2 Kiểm tra định dạng
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExts.Contains(ext))
                        return new ResponseSpTicketDto
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
                    var fileUrl = $"{baseUrl}/uploads/tickets/{ticket.Id}/{filename}";
                    uploadedUrls.Add(fileUrl);


                    // 3.5 Thêm vào danh sách ảnh của ticket
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



        public async Task<IEnumerable<SupportTicket>> GetTicketsForUserAsync(Guid userid)
        {

            var tickets = await _ticketRepo.ListByUserAsync(userid);
            return tickets
                .Where(t => t != null && !t.IsDeleted);
        }


        public async Task<IEnumerable<SupportTicket>> GetTicketsForAdminAsync(Guid adminId)
        {
            var tickets = await _ticketRepo.ListByAdminAsync(adminId);
            return tickets
                .Where(t => t != null && !t.IsDeleted);
        }

        public async Task<SupportTicket?> GetTicketDetailsAsync(Guid ticketId)
        {
            var tickets = await _ticketRepo.GetWithCommentsAsync(ticketId);
            if (tickets == null || tickets.IsDeleted)
            {
                throw new KeyNotFoundException("Support ticket not found");
            }
            return tickets;
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
            var reply = new SupportTicketComment
            {
                Id = Guid.NewGuid(),
                SupportTicketId = ticketId,
                CreatedById = replierId,
                CommentText = comment,
                CreatedAt = DateTime.UtcNow,
                //CreatedById = replierId
            };
            await _commentRepo.AddAsync(reply);
            await _commentRepo.SaveChangesAsync();
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Send successful"
            };
        }

        public async Task<BaseResposeDto> ChangeStatusAsync(Guid ticketId, TicketStatus newStatus)
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
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepo.SaveChangesAsync();
            return new BaseResposeDto
            {
                StatusCode = 200,
                Message = "Status update successful"
            };
        }

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
