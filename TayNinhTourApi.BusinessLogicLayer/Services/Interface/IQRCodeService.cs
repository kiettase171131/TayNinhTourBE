using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.QRCode;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface for QR code generation and verification
    /// Used for customer pickup verification at specialty shops
    /// </summary>
    public interface IQRCodeService
    {
        /// <summary>
        /// Generate QR code for a paid order
        /// </summary>
        /// <param name="orderId">Order ID to generate QR code for</param>
        /// <returns>Response containing QR code data and image URL</returns>
        Task<GenerateQRCodeResponseDto> GenerateQRCodeAsync(Guid orderId);

        /// <summary>
        /// Scan and process QR code by specialty shop
        /// This method combines verification and marking as used in one action
        /// </summary>
        /// <param name="qrCodeData">QR code data string</param>
        /// <param name="shopId">ID of the specialty shop scanning the code</param>
        /// <returns>Response containing order details and processing result</returns>
        Task<ScanQRCodeResponseDto> ScanAndProcessQRCodeAsync(string qrCodeData, Guid shopId);

        /// <summary>
        /// Get QR code details for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>QR code details</returns>
        Task<QRCodeDetailsResponseDto> GetQRCodeDetailsAsync(Guid orderId);
    }
}