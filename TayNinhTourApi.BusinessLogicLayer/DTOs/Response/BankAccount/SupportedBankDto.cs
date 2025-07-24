namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount
{
    /// <summary>
    /// DTO cho th�ng tin ng�n h�ng h? tr?
    /// </summary>
    public class SupportedBankDto
    {
        /// <summary>
        /// M� ng�n h�ng (enum value)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// T�n ng�n h�ng
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// T�n hi?n th? ??y ?? c?a ng�n h�ng
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// T�n vi?t t?t c?a ng�n h�ng
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Logo URL (n?u c�)
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// C� ?ang ho?t ??ng kh�ng
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}