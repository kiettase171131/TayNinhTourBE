
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketsController : ControllerBase
    {
        private readonly ISupportTicketService _service;
        public SupportTicketsController(ISupportTicketService service)
        {
            _service = service;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "ExcludeAdmin")]    
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] RequestCreateTicketDto dto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _service.CreateTicketAsync(dto , currentUserObject);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // GET: api/SupportTickets
        [HttpGet("User")]
        public async Task<IActionResult> ListForUser()
        {
            try
            {
                CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var tickets = await _service.GetTicketsForUserAsync(currentUserObject.Id);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            { 
                return NotFound(new { message = ex.Message });
            }
        }
       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // GET: api/SupportTickets/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
           
           var response = await _service.GetTicketDetailsAsync(id);
            return StatusCode(response.StatusCode, response);

        }
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //// PUT: api/SupportTickets/{id}/status
        //[HttpPut("{id:guid}/status")]
        //public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
        //{
        //   var response =  await _service.ChangeStatusAsync(id, dto.NewStatus);
        //    return StatusCode(response.StatusCode, response);
        //}
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // DELETE: api/SupportTickets/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _service.DeleteTicketAsync(id, currentUserObject.Id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
