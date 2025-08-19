namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.AI
{
    /// <summary>
    /// Model ch?a th�ng tin tour th?c t? t? database cho AI chatbot t? v?n
    /// ??m b?o AI ch? t? v?n tours c� th?t v?i gi� th?t t? TourOperation
    /// </summary>
    public class AITourInfo
    {
        /// <summary>
        /// ID c?a TourTemplate
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// T�n tour
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// M� t? tour
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gi� tour TH?C T? t? TourOperation (kh�ng ph?i hardcode)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// ?i?m kh?i h�nh
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
        /// S? l??ng kh�ch t?i ?a
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// S? gh? c�n tr?ng (t?ng t? c�c slots c� s?n)
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// C�c ?i?m n?i b?t c?a tour
        /// </summary>
        public List<string> Highlights { get; set; } = new List<string>();

        /// <summary>
        /// Danh s�ch c�c ng�y c� tour (t? TourSlots available)
        /// </summary>
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();

        /// <summary>
        /// T�n c�ng ty tour
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Tour c� ???c ph�p booking kh�ng (PUBLIC status)
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Helper property: Ng�y tour g?n nh?t
        /// </summary>
        public DateTime? NextAvailableDate => AvailableDates.OrderBy(d => d).FirstOrDefault();

        /// <summary>
        /// Helper property: S? ng�y tour c� s?n
        /// </summary>
        public int AvailableDaysCount => AvailableDates.Count;

        /// <summary>
        /// Helper property: Format gi� cho display
        /// </summary>
        public string FormattedPrice => $"{Price:N0} VN?";

        /// <summary>
        /// Helper property: T�m t?t ng?n g?n ?? AI s? d?ng
        /// </summary>
        public string Summary => $"{Title} - {FormattedPrice} - {AvailableSlots} ch? tr?ng - {AvailableDaysCount} ng�y c� s?n";
    }
}