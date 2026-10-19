using System;
using System.Security.Cryptography;
using System.Text;

namespace PizzeriaChainApp.Helpers
{
    public static class PasswordHelper
    {
        // Случайная соль: 16 байт -> 32 hex-символа
        public static string GenerateSalt()
        {
            byte[] bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return ToHex(bytes);
        }

        // SHA-256(UTF-8(соль + пароль)) -> 64 hex-символа
        public static string HashPassword(string password, string salt)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
                return ToHex(hash);
            }
        }

        public static bool Verify(string password, string salt, string expectedHash)
        {
            return string.Equals(HashPassword(password, salt), expectedHash,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}