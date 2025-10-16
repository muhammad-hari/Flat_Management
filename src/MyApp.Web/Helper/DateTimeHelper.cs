using System;

namespace MyApp.Web.Helper
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo WibTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Jakarta"
            );

        public static DateTime NowWIB => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WibTimeZone);

        public static DateTime ToWIB(DateTime utcDateTime) =>
            TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, WibTimeZone);

        public static DateTime ToUTC(DateTime wibDateTime) =>
            TimeZoneInfo.ConvertTimeToUtc(wibDateTime, WibTimeZone);
    }
}
