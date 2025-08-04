using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IPayOsService
    {
        // === LEGACY METHODS (Deprecated - Use Enhanced methods instead) ===
        // [Obsolete("Use CreatePaymentLinkAsync instead. This method will be removed in future versions.")]
        // Task<string?> CreatePaymentUrlAsync(decimal amount, string orderCode, string baseUrl);

        // [Obsolete("Use CreatePaymentLinkAsync instead. This method will be removed in future versions.")]
        // Task<string?> CreateTourBookingPaymentUrlAsync(decimal amount, string orderCode, string baseUrl);

        Task<OrderStatus> GetOrderPaymentStatusAsync(string orderCode);

        // === ENHANCED METHODS (Recommended for new implementations) ===
        Task<PaymentTransaction> CreatePaymentLinkAsync(CreatePaymentRequestDto request);
        Task<PaymentTransaction> RetryPaymentAsync(RetryPaymentRequestDto request);
        Task<PaymentTransaction> HandlePayOsWebhookAsync(object webhookBody);
        Task<List<PaymentTransactionResponseDto>> GetTransactionsByOrderIdAsync(Guid orderId);
        Task<List<PaymentTransactionResponseDto>> GetTransactionsByTourBookingIdAsync(Guid tourBookingId);
        Task<CancelPaymentResponseDto> CancelAllPendingTransactionsAsync(CancelPaymentRequestDto request);
        Task<string> ConfirmWebhookAsync(string webhookUrl);

        // Payment type detection methods
        Task<string> GetPaymentTypeAsync(Guid transactionId);
        Task<List<PaymentTransactionResponseDto>> GetProductPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10);
        Task<List<PaymentTransactionResponseDto>> GetTourBookingPaymentTransactionsAsync(int pageIndex = 0, int pageSize = 10);

        // Additional methods for enhanced payment controller
        Task<object?> GetTransactionByOrderCodeAsync(string orderCode);
        Task<object> ProcessWebhookCallbackAsync(string orderCode, string status);
    }
}
