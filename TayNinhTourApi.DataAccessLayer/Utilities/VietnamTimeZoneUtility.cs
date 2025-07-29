using System;

namespace TayNinhTourApi.DataAccessLayer.Utilities
{
    /// <summary>
    /// Utility class for handling Vietnam timezone (UTC+7) operations
    /// </summary>
    public static class VietnamTimeZoneUtility
    {
        /// <summary>
        /// Vietnam timezone info (UTC+7)
        /// </summary>
        public static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Get current Vietnam time
        /// </summary>
        /// <returns>Current DateTime in Vietnam timezone</returns>
        public static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        /// <summary>
        /// Convert UTC DateTime to Vietnam time
        /// </summary>
        /// <param name="utcDateTime">UTC DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ConvertUtcToVietnam(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTime must be in UTC", nameof(utcDateTime));
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Convert Vietnam time to UTC
        /// </summary>
        /// <param name="vietnamDateTime">Vietnam DateTime to convert</param>
        /// <returns>DateTime in UTC</returns>
        public static DateTime ConvertVietnamToUtc(DateTime vietnamDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Convert any DateTime to Vietnam time
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ToVietnamTime(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    return ConvertUtcToVietnam(dateTime);
                case DateTimeKind.Local:
                    return TimeZoneInfo.ConvertTime(dateTime, VietnamTimeZone);
                case DateTimeKind.Unspecified:
                    // Assume it's already in Vietnam time if unspecified
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
                default:
                    return dateTime;
            }
        }

        /// <summary>
        /// Get Vietnam time for a specific date
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="hour">Hour (default: 0)</param>
        /// <param name="minute">Minute (default: 0)</param>
        /// <param name="second">Second (default: 0)</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime GetVietnamDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        {
            var dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Check if a date is in the past (Vietnam time)
        /// </summary>
        /// <param name="dateTime">DateTime to check</param>
        /// <returns>True if the date is in the past</returns>
        public static bool IsInPast(DateTime dateTime)
        {
            var vietnamTime = ToVietnamTime(dateTime);
            var now = GetVietnamNow();
            return vietnamTime < now;
        }

        /// <summary>
        /// Get the difference in days between two dates (Vietnam time)
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Number of days difference</returns>
        public static int GetDaysDifference(DateTime fromDate, DateTime toDate)
        {
            var vietnamFromDate = ToVietnamTime(fromDate);
            var vietnamToDate = ToVietnamTime(toDate);
            return (int)(vietnamToDate.Date - vietnamFromDate.Date).TotalDays;
        }

        /// <summary>
        /// Format DateTime to Vietnam timezone string
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="format">Format string (default: "dd/MM/yyyy HH:mm:ss")</param>
        /// <returns>Formatted string in Vietnam timezone</returns>
        public static string FormatVietnamTime(DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            var vietnamTime = ToVietnamTime(dateTime);
            return vietnamTime.ToString(format);
        }

        /// <summary>
        /// Get start of day in Vietnam timezone
        /// </summary>
        /// <param name="dateTime">DateTime to get start of day for</param>
        /// <returns>Start of day in Vietnam timezone</returns>
        public static DateTime GetStartOfDay(DateTime dateTime)
        {
            var vietnamTime = ToVietnamTime(dateTime);
            return new DateTime(vietnamTime.Year, vietnamTime.Month, vietnamTime.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Get end of day in Vietnam timezone
        /// </summary>
        /// <param name="dateTime">DateTime to get end of day for</param>
        /// <returns>End of day in Vietnam timezone</returns>
        public static DateTime GetEndOfDay(DateTime dateTime)
        {
            var vietnamTime = ToVietnamTime(dateTime);
            return new DateTime(vietnamTime.Year, vietnamTime.Month, vietnamTime.Day, 23, 59, 59, 999, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Parse date string in Vietnam timezone
        /// </summary>
        /// <param name="dateString">Date string to parse</param>
        /// <param name="format">Format of the date string</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ParseVietnamTime(string dateString, string format = "dd/MM/yyyy")
        {
            if (DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out var result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
            }
            throw new FormatException($"Unable to parse date string '{dateString}' with format '{format}'");
        }

        /// <summary>
        /// Get Vietnam timezone offset
        /// </summary>
        /// <returns>TimeSpan representing UTC+7</returns>
        public static TimeSpan GetVietnamOffset()
        {
            return VietnamTimeZone.GetUtcOffset(DateTime.UtcNow);
        }

        /// <summary>
        /// Convert DateOnly to DateTime in Vietnam timezone
        /// </summary>
        /// <param name="dateOnly">DateOnly to convert</param>
        /// <param name="timeOnly">TimeOnly to combine (default: midnight)</param>
        /// <returns>DateTime in Vietnam timezone</returns>
        public static DateTime ToVietnamDateTime(DateOnly dateOnly, TimeOnly timeOnly = default)
        {
            var dateTime = dateOnly.ToDateTime(timeOnly);
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        }
    }
}
