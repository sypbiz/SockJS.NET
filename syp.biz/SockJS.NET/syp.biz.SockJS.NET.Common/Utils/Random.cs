using System;
using System.Linq;
using System.Text;

namespace syp.biz.SockJS.NET.Common.Utils
{
    public static class Random
    {
        private const string RandomStringChars = "abcdefghijklmnopqrstuvwxyz012345";

        public static string GetString(int length)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1");

            var buffer = new byte[length];
            var charsLength = RandomStringChars.Length;

            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(buffer);

            var chars = buffer
                .Select(b => b % charsLength) // relative position (index) of random byte in RandomStringChars
                .Select(idx => RandomStringChars[idx]); // select char for index

            return new StringBuilder(length).Append(chars).ToString();
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
