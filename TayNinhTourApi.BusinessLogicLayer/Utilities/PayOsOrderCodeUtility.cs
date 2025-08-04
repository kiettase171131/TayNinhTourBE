using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class để xử lý PayOS Order Code với prefix TNDT
    /// </summary>
    public static class PayOsOrderCodeUtility
    {
        private const string TNDT_PREFIX = "TNDT";

        /// <summary>
        /// Tạo PayOS Order Code với format TNDT + timestamp + random
        /// </summary>
        /// <returns>PayOS Order Code với format TNDT{timestamp}{random}</returns>
        public static string GeneratePayOsOrderCode()
        {
            var timestamp = VietnamTimeZoneUtility.GetVietnamNow().Ticks.ToString();
            var timestampLast7 = timestamp.Substring(Math.Max(0, timestamp.Length - 7));
            var random = new Random().Next(100, 999);
            return $"{TNDT_PREFIX}{timestampLast7}{random}";
        }

        /// <summary>
        /// Extract numeric part từ PayOS Order Code (loại bỏ TNDT prefix)
        /// </summary>
        /// <param name="payOsOrderCode">PayOS Order Code (có thể có hoặc không có TNDT prefix)</param>
        /// <returns>Numeric part của order code</returns>
        public static long ExtractNumericPart(string payOsOrderCode)
        {
            if (string.IsNullOrWhiteSpace(payOsOrderCode))
                throw new ArgumentException("PayOS Order Code cannot be null or empty");

            var numericPart = payOsOrderCode.StartsWith(TNDT_PREFIX) 
                ? payOsOrderCode.Substring(TNDT_PREFIX.Length) 
                : payOsOrderCode;

            if (!long.TryParse(numericPart, out var result))
                throw new ArgumentException($"Invalid PayOS Order Code format: {payOsOrderCode}");

            return result;
        }

        /// <summary>
        /// Thêm TNDT prefix vào numeric order code
        /// </summary>
        /// <param name="numericOrderCode">Numeric order code</param>
        /// <returns>PayOS Order Code với TNDT prefix</returns>
        public static string AddTndtPrefix(long numericOrderCode)
        {
            return $"{TNDT_PREFIX}{numericOrderCode}";
        }

        /// <summary>
        /// Thêm TNDT prefix vào numeric order code (string)
        /// </summary>
        /// <param name="numericOrderCode">Numeric order code as string</param>
        /// <returns>PayOS Order Code với TNDT prefix</returns>
        public static string AddTndtPrefix(string numericOrderCode)
        {
            if (string.IsNullOrWhiteSpace(numericOrderCode))
                throw new ArgumentException("Numeric order code cannot be null or empty");

            // Nếu đã có TNDT prefix thì return luôn
            if (numericOrderCode.StartsWith(TNDT_PREFIX))
                return numericOrderCode;

            return $"{TNDT_PREFIX}{numericOrderCode}";
        }

        /// <summary>
        /// Kiểm tra xem order code có TNDT prefix không
        /// </summary>
        /// <param name="orderCode">Order code cần kiểm tra</param>
        /// <returns>True nếu có TNDT prefix</returns>
        public static bool HasTndtPrefix(string orderCode)
        {
            return !string.IsNullOrWhiteSpace(orderCode) && orderCode.StartsWith(TNDT_PREFIX);
        }

        /// <summary>
        /// Normalize order code để đảm bảo có TNDT prefix
        /// </summary>
        /// <param name="orderCode">Order code cần normalize</param>
        /// <returns>Order code với TNDT prefix</returns>
        public static string NormalizeOrderCode(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                throw new ArgumentException("Order code cannot be null or empty");

            return HasTndtPrefix(orderCode) ? orderCode : AddTndtPrefix(orderCode);
        }

        /// <summary>
        /// Tạo PayOS Order Code cho legacy compatibility (numeric only)
        /// </summary>
        /// <returns>Numeric order code</returns>
        public static long GenerateNumericOrderCode()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
