using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Attributes
{
    /// <summary>
    /// Custom validation attribute for CV file uploads
    /// </summary>
    public class CvFileValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is IFormFile file)
            {
                var validationResult = FileValidationUtility.ValidateCvFile(file);
                if (!validationResult.IsValid)
                {
                    ErrorMessage = validationResult.ErrorMessage;
                    return false;
                }
                return true;
            }

            // If value is null, let the Required attribute handle it
            if (value == null)
            {
                return true;
            }

            ErrorMessage = "Invalid file type";
            return false;
        }
    }

    /// <summary>
    /// Custom validation attribute for Business License file uploads
    /// </summary>
    public class BusinessLicenseFileValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is IFormFile file)
            {
                var validationResult = FileValidationUtility.ValidateBusinessLicenseFile(file);
                if (!validationResult.IsValid)
                {
                    ErrorMessage = validationResult.ErrorMessage;
                    return false;
                }
                return true;
            }

            // If value is null, return false since BusinessLicenseFile is required
            if (value == null)
            {
                ErrorMessage = "Giấy phép kinh doanh là bắt buộc";
                return false;
            }

            ErrorMessage = "Định dạng file không hợp lệ";
            return false;
        }
    }

    /// <summary>
    /// Custom validation attribute for Logo file uploads
    /// </summary>
    public class LogoFileValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is IFormFile file)
            {
                var validationResult = FileValidationUtility.ValidateLogoFile(file);
                if (!validationResult.IsValid)
                {
                    ErrorMessage = validationResult.ErrorMessage;
                    return false;
                }
                return true;
            }

            // If value is null, return false since Logo is required
            if (value == null)
            {
                ErrorMessage = "Logo cửa hàng là bắt buộc";
                return false;
            }

            ErrorMessage = "Định dạng file không hợp lệ";
            return false;
        }
    }

    /// <summary>
    /// Custom validation attribute for time format (HH:mm)
    /// </summary>
    public class TimeFormatValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true; // Allow null/empty values, use Required attribute for mandatory fields
            }

            var timeString = value.ToString()!;

            // Check if the format matches HH:mm
            if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out _))
            {
                return true;
            }

            ErrorMessage = "Định dạng thời gian không hợp lệ. Vui lòng sử dụng định dạng HH:mm (ví dụ: 08:00)";
            return false;
        }
    }
}
