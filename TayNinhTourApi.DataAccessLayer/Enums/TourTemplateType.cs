namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa các loại tour template có thể được tạo trong hệ thống
    /// </summary>
    public enum TourTemplateType
    {
        /// <summary>
        /// Tour tiêu chuẩn - gói tour cơ bản với các dịch vụ thông thường
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Tour cao cấp - gói tour với dịch vụ premium và tiện nghi cao cấp
        /// </summary>
        Premium = 2,

        /// <summary>
        /// Tour tùy chỉnh - gói tour được thiết kế riêng theo yêu cầu khách hàng
        /// </summary>
        Custom = 3,

        /// <summary>
        /// Tour nhóm - gói tour dành cho nhóm khách du lịch
        /// </summary>
        Group = 4,

        /// <summary>
        /// Tour riêng tư - gói tour dành cho cá nhân hoặc gia đình
        /// </summary>
        Private = 5,

        /// <summary>
        /// Tour phiêu lưu - gói tour tập trung vào các hoạt động mạo hiểm
        /// </summary>
        Adventure = 6,

        /// <summary>
        /// Tour văn hóa - gói tour tập trung vào khám phá văn hóa địa phương
        /// </summary>
        Cultural = 7,

        /// <summary>
        /// Tour ẩm thực - gói tour tập trung vào trải nghiệm ẩm thực
        /// </summary>
        Culinary = 8,

        /// <summary>
        /// Tour sinh thái - gói tour tập trung vào thiên nhiên và môi trường
        /// </summary>
        Eco = 9,

        /// <summary>
        /// Tour lịch sử - gói tour tập trung vào các di tích lịch sử
        /// </summary>
        Historical = 10
    }
}
