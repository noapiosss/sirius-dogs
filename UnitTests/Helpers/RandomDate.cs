using System;

namespace UnitTests.Helpers
{
    internal static class RandomDate
    {
        public static DateTime GetRandomDate()
        {
            Random random = new();
            DateTime start = new(1900, 1, 1);
            DateTime end = DateTime.Now;
            return start.AddDays(random.Next((end - start).Days));
        }
    }
}