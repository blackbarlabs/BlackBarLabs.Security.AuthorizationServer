using System;

namespace JoshCodes.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToUtcString(this DateTimeOffset dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string ToUtcString(this DateTimeOffset? dateTime)
        {
            if(dateTime.HasValue)
            {
                return dateTime.Value.ToUtcString();
            }
            return null;
        }
    }
}
