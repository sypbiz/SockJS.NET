using System;
using System.Linq;

namespace syp.biz.SockJS.NET.Common.Utils
{
    public static class Random
    {
        private const string RANDOM_STRING_CHARS = "abcdefghijklmnopqrstuvwxyz012345";

        public static string GetString(int length)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1");

            var buffer = new byte[length];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(buffer);
            return string.Join(string.Empty, buffer.Select(b => RANDOM_STRING_CHARS.Substring(b % length, 1)));
        }

        public static long GetNumber(long max) => (long) Math.Floor(new System.Random(DateTime.UtcNow.Millisecond).NextDouble() * max);

        public static string GetNumberString(long max)
        {
            var t = (max - 1).ToString().Length;
            var num = GetNumber(max);
            return num.ToString().PadLeft(t, '0');
        }
    }
}
