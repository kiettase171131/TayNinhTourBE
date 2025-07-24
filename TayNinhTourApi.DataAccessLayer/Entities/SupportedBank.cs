using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Danh s�ch c�c ng�n h�ng ph? bi?n ? Vi?t Nam (d�ng cho FE hi?n th? l?a ch?n)
    /// </summary>
    public enum SupportedBank
    {
        Vietcombank,
        VietinBank,
        BIDV,
        Techcombank,
        Sacombank,
        ACB,
        MBBank,
        TPBank,
        VPBank,
        SHB,
        HDBank,
        VIB,
        Eximbank,
        SeABank,
        OCB,
        MSB,
        SCB,
        DongABank,
        LienVietPostBank,
        ABBANK,
        PVcomBank,
        NamABank,
        BacABank,
        Saigonbank,
        VietBank,
        Kienlongbank,
        PGBank,
        OceanBank,
        CoopBank,
        /// <summary>
        /// Ng�n h�ng kh�c - cho ph�p user nh?p t�n ng�n h�ng t? do
        /// </summary>
        Other = 999
    }
}
