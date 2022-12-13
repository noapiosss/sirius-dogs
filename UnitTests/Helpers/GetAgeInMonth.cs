using System;

namespace UnitTests.Helpers
{
    internal static class GetAgeInMonthHelper
    {
        public static int GetAgeInMonth(DateTime birhDate)
        {
            DateTime now = DateTime.UtcNow;
            return (12 * (now.Year - birhDate.Year)) + now.Month - birhDate.Month;
        }
    }
}