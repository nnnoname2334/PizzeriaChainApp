using System;
using System.Security.Cryptography;
using System.Text;

namespace PizzeriaChainApp.Helpers
{
    public static class CaptchaHelper
    {
        // Без похожих символов (0/O, 1/I/l)
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

        public static string Generate(int length = 6)
        {
            var sb = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    uint value = BitConverter.ToUInt32(buffer, 0);
                    sb.Append(Alphabet[(int)(value % (uint)Alphabet.Length)]);
                }
            }
            return sb.ToString();
        }

        public static bool Check(string input, string expected)
        {
            return !string.IsNullOrEmpty(expected) &&
                   string.Equals(input?.Trim(), expected, StringComparison.Ordinal);
        }
    }
}