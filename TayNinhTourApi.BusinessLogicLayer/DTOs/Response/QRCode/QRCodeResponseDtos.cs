using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.QRCode
{
    /// <summary>
    /// Response DTO for QR code generation
    /// </summary>
    public class GenerateQRCodeResponseDto : BaseResposeDto
    {
        /// <summary>
        /// QR code data string
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// URL to the generated QR code image
        /// </summary>
        public string? QRCodeImageUrl { get; set; }

        /// <summary>
        /// Order information
        /// </summary>
        public OrderDto? Order { get; set; }
    }

    /// <summary>
    /// Response DTO for QR code scanning and processing
    /// Combines verification and marking as used in one operation
    /// </summary>
    public class ScanQRCodeResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Order information from QR code
        /// </summary>
        public OrderDto? Order { get; set; }

        /// <summary>
        /// Customer information
        /// </summary>
        public CustomerInfoDto? Customer { get; set; }

        /// <summary>
        /// Whether QR code was valid and processed successfully
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Whether QR code was already used before this scan
        /// </summary>
        public bool WasAlreadyUsed { get; set; }

        /// <summary>
        /// Products in the order
        /// </summary>
        public List<OrderDetailDto> Products { get; set; } = new();

        /// <summary>
        /// Shop name that processed the QR code
        /// </summary>
        public string? ProcessedByShopName { get; set; }

        /// <summary>
        /// Timestamp when QR code was processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }

    /// <summary>
    /// Response DTO for QR code details
    /// </summary>
    public class QRCodeDetailsResponseDto : BaseResposeDto
    {
        /// <summary>
        /// Order ID
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// QR code data string
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// URL to the QR code image
        /// </summary>
        public string? QRCodeImageUrl { get; set; }

        /// <summary>
        /// Whether QR code has been used
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Timestamp when QR code was used
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Shop that used the QR code
        /// </summary>
        public string? UsedByShopName { get; set; }

        /// <summary>
        /// Order information
        /// </summary>
        public OrderDto? Order { get; set; }
    }

    /// <summary>
    /// Customer information DTO for QR code verification
    /// </summary>
    public class CustomerInfoDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}