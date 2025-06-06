using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpTicket;
using TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface ISupportTicketService
    {
        Task<ResponseSpTicketDto> CreateTicketAsync(RequestCreateTicketDto request, CurrentUserObject currentUserObject);
        Task<IEnumerable<SupportTicketDto>> GetTicketsForUserAsync(Guid userId);
        Task<IEnumerable<SupportTicketDto>> GetTicketsForAdminAsync(Guid adminId);
        Task<SupportTicketDto?> GetTicketDetailsAsync(Guid ticketId);
        Task<BaseResposeDto> ReplyAsync(Guid ticketId, Guid replierId, string comment);
        //Task<BaseResposeDto> ChangeStatusAsync(Guid ticketId, TicketStatus newStatus);
        Task<BaseResposeDto> DeleteTicketAsync(Guid ticketId, Guid requestorId);
    }
}
