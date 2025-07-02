using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/payment-callback")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpPost] 
        public async Task<IActionResult> Callback([FromBody] PayOSCallbackDto payload)
        {
            // Có thể kiểm tra chữ ký bảo mật ở đây nếu cần (hạn chế fake callback)

            // Lấy orderCode (kiểu string/long/Guid tuỳ bạn truyền cho PayOS)
            var orderId = Guid.Parse(payload.orderCode); // hoặc parse kiểu long nếu bạn dùng số
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
                return NotFound("Không tìm thấy đơn hàng");

            // Cập nhật trạng thái theo payload.status
            if (payload.status == "PAID")
            {
                order.Status = OrderStatus.Paid;
            }
            else if (payload.status == "CANCELLED")
            {
                order.Status = OrderStatus.Cancelled;
            }
            else
            {
                order.Status = OrderStatus.Pending;
            }

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }
    }
}
