using System;
using System.Security.Cryptography;
using System.Text;

namespace EsportsTournamentManager.Services
{
    // Tiện ích mã hóa mật khẩu sử dụng SHA256 Hex
    public static class PasswordHasher
    {
        // Băm mật khẩu thành chuỗi SHA256 Hex
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Xác thực mật khẩu nhập vào so với mật khẩu đã băm
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword)) 
                return false;

            return string.Equals(HashPassword(password), hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
