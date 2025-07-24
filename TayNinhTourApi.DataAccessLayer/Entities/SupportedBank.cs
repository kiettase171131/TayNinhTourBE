using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Danh sách các ngân hàng ph? bi?n ? Vi?t Nam (dùng cho FE hi?n th? l?a ch?n)
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
        /// Ngân hàng khác - cho phép user nh?p tên ngân hàng t? do
        /// </summary>
        Other = 999
    }
}
