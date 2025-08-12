using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;

namespace TayNinhTourApi.BusinessLogicLayer.Validations
{
    /// <summary>
    /// Custom validation attribute để validate danh sách guests trong tour booking
    /// Kiểm tra: guest count khớp với numberOfGuests, email unique, required fields
    /// </summary>
    public class GuestListValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not List<GuestInfoRequest> guests)
            {
                return new ValidationResult("Danh sách khách hàng không hợp lệ");
            }

            // Lấy numberOfGuests từ parent object
            var parentObject = validationContext.ObjectInstance;
            var numberOfGuestsProperty = parentObject?.GetType().GetProperty("NumberOfGuests");
            
            if (numberOfGuestsProperty?.GetValue(parentObject) is not int numberOfGuests)
            {
                return new ValidationResult("Không thể xác định số lượng khách");
            }

            // Validate guest count khớp với numberOfGuests
            if (guests.Count != numberOfGuests)
            {
                return new ValidationResult(
                    $"Số lượng thông tin khách hàng ({guests.Count}) phải khớp với số lượng khách đã chọn ({numberOfGuests})");
            }

            // Validate minimum guests
            if (guests.Count == 0)
            {
                return new ValidationResult("Phải có ít nhất 1 khách hàng");
            }

            // Validate unique emails (case-insensitive)
            var emailGroups = guests
                .Where(g => !string.IsNullOrWhiteSpace(g.GuestEmail))
                .GroupBy(g => g.GuestEmail.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .ToList();

            if (emailGroups.Any())
            {
                var duplicateEmails = emailGroups.Select(g => g.Key);
                return new ValidationResult(
                    $"Email khách hàng phải khác nhau. Email trùng lặp: {string.Join(", ", duplicateEmails)}");
            }

            // Validate required fields cho từng guest
            for (int i = 0; i < guests.Count; i++)
            {
                var guest = guests[i];
                
                if (string.IsNullOrWhiteSpace(guest.GuestName))
                {
                    return new ValidationResult($"Tên khách hàng thứ {i + 1} không được để trống");
                }

                if (string.IsNullOrWhiteSpace(guest.GuestEmail))
                {
                    return new ValidationResult($"Email khách hàng thứ {i + 1} không được để trống");
                }

                // Validate email format
                if (!IsValidEmail(guest.GuestEmail))
                {
                    return new ValidationResult($"Email khách hàng thứ {i + 1} không hợp lệ: {guest.GuestEmail}");
                }

                // Validate guest name length
                if (guest.GuestName.Trim().Length < 2)
                {
                    return new ValidationResult($"Tên khách hàng thứ {i + 1} phải có ít nhất 2 ký tự");
                }

                // Validate phone format (nếu có)
                if (!string.IsNullOrWhiteSpace(guest.GuestPhone) && !IsValidPhone(guest.GuestPhone))
                {
                    return new ValidationResult($"Số điện thoại khách hàng thứ {i + 1} không hợp lệ: {guest.GuestPhone}");
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validate email format using simple regex
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate phone format - chỉ cho phép số và các ký tự đặc biệt cơ bản
        /// </summary>
        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            var cleanPhone = phone.Trim();
            
            // Cho phép số, space, dash, plus, parentheses
            return System.Text.RegularExpressions.Regex.IsMatch(cleanPhone, @"^[\d\s\-\+\(\)]+$") 
                   && cleanPhone.Length >= 8 
                   && cleanPhone.Length <= 20;
        }
    }
}
