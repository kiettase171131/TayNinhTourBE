namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Enum ??nh ngh?a các lo?i chat AI
    /// </summary>
    public enum AIChatType
    {
        /// <summary>
        /// Chat v? tour - h?i thông tin v? tours, ??t tour, giá c?, l?ch trình
        /// AI s? truy c?p database ?? cung c?p thông tin tour th?c t?
        /// </summary>
        Tour = 1,

        /// <summary>
        /// Chat v? s?n ph?m - t? v?n mua s?m, tìm ki?m s?n ph?m theo nhu c?u
        /// AI s? truy c?p database s?n ph?m ?? t? v?n và g?i ý
        /// </summary>
        Product = 2,

        /// <summary>
        /// Chat v? thông tin Tây Ninh - l?ch s?, v?n hóa, ??a ?i?m, ?m th?c
        /// AI s? cung c?p thông tin chung v? Tây Ninh, t? ch?i tr? l?i câu h?i không liên quan
        /// </summary>
        TayNinh = 3
    }
}