namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Loại cổng thanh toán
    /// Tương tự như GatewayEnum trong code mẫu Java
    /// </summary>
    public enum PaymentGateway
    {
        /// <summary>
        /// Cổng thanh toán PayOS
        /// </summary>
        PayOS = 0,

        /// <summary>
        /// Cổng thanh toán VNPay (dự phòng)
        /// </summary>
        VNPay = 1,

        /// <summary>
        /// Cổng thanh toán MoMo (dự phòng)
        /// </summary>
        MoMo = 2
    }
}
