namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AI
{
    /// <summary>
    /// Model ch?a thông tin tour th?c t? t? database cho AI chatbot t? v?n
    /// ??m b?o AI ch? t? v?n tours có th?t v?i giá th?t t? TourOperation
    /// </summary>
    public class AITourInfo
    {
        /// <summary>
        /// ID c?a TourTemplate
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên tour
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô t? tour
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Giá tour TH?C T? t? TourOperation (không ph?i hardcode)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// ?i?m kh?i hành
        /// </summary>
        public string StartLocation { get; set; } = string.Empty;

        /// <summary>
        /// ?i?m ??n
        /// </summary>
        public string EndLocation { get; set; } = string.Empty;

        /// <summary>
        /// Lo?i tour (Tour Danh Lam Th?ng C?nh, Tour Khu Vui Ch?i)
        /// </summary>
        public string TourType { get; set; } = string.Empty;

        /// <summary>
        /// S? l??ng khách t?i ?a
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// S? gh? còn tr?ng (t?ng t? các slots có s?n)
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Các ?i?m n?i b?t c?a tour
        /// </summary>
        public List<string> Highlights { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách các ngày có tour (t? TourSlots available)
        /// </summary>
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();

        /// <summary>
        /// Tên công ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Tour có ???c phép booking không (PUBLIC status)
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Helper property: Ngày tour g?n nh?t
        /// </summary>
        public DateTime? NextAvailableDate => AvailableDates.OrderBy(d => d).FirstOrDefault();

        /// <summary>
        /// Helper property: S? ngày tour có s?n
        /// </summary>
        public int AvailableDaysCount => AvailableDates.Count;

        /// <summary>
        /// Helper property: Format giá cho display
        /// </summary>
        public string FormattedPrice => $"{Price:N0} VN?";

        /// <summary>
        /// Helper property: Tóm t?t ng?n g?n ?? AI s? d?ng
        /// </summary>
        public string Summary => $"{Title} - {FormattedPrice} - {AvailableSlots} ch? tr?ng - {AvailableDaysCount} ngày có s?n";
    }
}