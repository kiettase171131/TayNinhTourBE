﻿
namespace TayNinhTourApi.BusinessLogicLayer.Common
{
    public static class Constants
    {
        public const int TokenExpiredTime = 1; // in day

        public const string RoleAdminName = "Admin";
        public const string RoleTourCompanyName = "Tour Company";
        public const string RoleUserName = "User";
        public const string RoleTourGuideName = "Tour Guide";
        public const string RoleBloggerName = "Blogger";
        public const string RoleShopName = "Specialty Shop";
        public const string RoleSpecialtyShopName = "Specialty Shop";
        // Regex email chuẩn
        public const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // Regex phone chỉ cho phép 10 số, không chữ, không ký tự đặc biệt
        public const string PhoneNumberRegexPattern = @"^\d{10}$";
        public const int MaxFailedAttempts = 5;

        public const int PageSizeDefault = 10;

        public const int PageIndexDefault = 1;
    }
}