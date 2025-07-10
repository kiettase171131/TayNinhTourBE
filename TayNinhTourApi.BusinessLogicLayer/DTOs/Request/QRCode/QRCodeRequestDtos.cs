using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.QRCode
{
    /// <summary>
    /// Request DTO for scanning and processing QR code by specialty shop
    /// Combines verification and marking as used in one operation
    /// </summary>
    public class ScanQRCodeRequestDto
    {
        /// <summary>
        /// QR code data string scanned by the shop
        /// </summary>
        [Required(ErrorMessage = "QR code data is required")]
        public string QRCodeData { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for admin to create missing SpecialtyShop record
    /// </summary>
    public class CreateMissingShopRequest
    {
        [Required]
        public Guid UserId { get; set; }
        
        public string? ShopName { get; set; }
        
        public string? Location { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? ShopType { get; set; }
    }
}