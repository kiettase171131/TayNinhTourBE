namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount
{
    /// <summary>
    /// DTO cho thông tin ngân hàng h? tr?
    /// </summary>
    public class SupportedBankDto
    {
        /// <summary>
        /// Mã ngân hàng (enum value)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tên hi?n th? ??y ?? c?a ngân hàng
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Tên vi?t t?t c?a ngân hàng
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Logo URL (n?u có)
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Có ?ang ho?t ??ng không
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}