namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Enum ??nh ngh?a c�c lo?i chat AI
    /// </summary>
    public enum AIChatType
    {
        /// <summary>
        /// Chat v? tour - h?i th�ng tin v? tours, ??t tour, gi� c?, l?ch tr�nh
        /// AI s? truy c?p database ?? cung c?p th�ng tin tour th?c t?
        /// </summary>
        Tour = 1,

        /// <summary>
        /// Chat v? s?n ph?m - t? v?n mua s?m, t�m ki?m s?n ph?m theo nhu c?u
        /// AI s? truy c?p database s?n ph?m ?? t? v?n v� g?i �
        /// </summary>
        Product = 2,

        /// <summary>
        /// Chat v? th�ng tin T�y Ninh - l?ch s?, v?n h�a, ??a ?i?m, ?m th?c
        /// AI s? cung c?p th�ng tin chung v? T�y Ninh, t? ch?i tr? l?i c�u h?i kh�ng li�n quan
        /// </summary>
        TayNinh = 3
    }
}