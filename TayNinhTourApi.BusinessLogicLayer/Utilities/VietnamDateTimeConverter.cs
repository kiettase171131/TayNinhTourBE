using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// JSON converter for DateTime to handle Vietnam timezone (UTC+7)
    /// </summary>
    public class VietnamDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return default;
            }

            // Try to parse the datetime string with timezone awareness
            if (DateTimeOffset.TryParse(dateTimeString, out var dateTimeOffset))
            {
                // Convert to Vietnam time
                var vietnamTime = dateTimeOffset.ToOffset(TimeSpan.FromHours(7)).DateTime;
                return DateTime.SpecifyKind(vietnamTime, DateTimeKind.Unspecified);
            }

            // Fallback to regular DateTime parsing
            if (DateTime.TryParse(dateTimeString, out var dateTime))
            {
                // If the datetime is already UTC, convert to Vietnam time
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    return VietnamTimeZoneUtility.ConvertUtcToVietnam(dateTime);
                }

                // If unspecified, assume it's Vietnam time
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            }

            throw new JsonException($"Unable to parse DateTime from '{dateTimeString}'");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert to Vietnam time and send as Vietnam time with timezone offset
            var vietnamTime = VietnamTimeZoneUtility.ToVietnamTime(value);

            // Format as ISO string with Vietnam timezone offset (+07:00)
            var vietnamIsoString = vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "+07:00";

            writer.WriteStringValue(vietnamIsoString);
        }
    }

    /// <summary>
    /// JSON converter for nullable DateTime to handle Vietnam timezone (UTC+7)
    /// </summary>
    public class VietnamNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            // Try to parse the datetime string with timezone awareness
            if (DateTimeOffset.TryParse(dateTimeString, out var dateTimeOffset))
            {
                // Convert to Vietnam time
                var vietnamTime = dateTimeOffset.ToOffset(TimeSpan.FromHours(7)).DateTime;
                return DateTime.SpecifyKind(vietnamTime, DateTimeKind.Unspecified);
            }

            // Fallback to regular DateTime parsing
            if (DateTime.TryParse(dateTimeString, out var dateTime))
            {
                // If the datetime is already UTC, convert to Vietnam time
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    return VietnamTimeZoneUtility.ConvertUtcToVietnam(dateTime);
                }

                // If unspecified, assume it's Vietnam time
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            }

            throw new JsonException($"Unable to parse DateTime from '{dateTimeString}'");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Convert to Vietnam time and send as Vietnam time with timezone offset
                var vietnamTime = VietnamTimeZoneUtility.ToVietnamTime(value.Value);

                // Format as ISO string with Vietnam timezone offset (+07:00)
                var vietnamIsoString = vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "+07:00";

                writer.WriteStringValue(vietnamIsoString);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
