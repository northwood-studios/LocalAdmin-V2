using System;
using System.Linq;

namespace LocalAdmin_V2_Net_Core
{
    public static class Randomizer
    {
        private static readonly Random random = new Random();

        public static string RandomString(int length)
        {
            return new string(Enumerable
                .Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}