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
}
