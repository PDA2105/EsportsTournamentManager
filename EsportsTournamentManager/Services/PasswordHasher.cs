using System;
using System.Security.Cryptography;
using System.Text;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp tiện ích mã hóa mật khẩu sử dụng thuật toán SHA256 chuyển sang định dạng chuỗi Hex.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Băm mật khẩu văn bản thường thành chuỗi mã hóa SHA256 Hexadecimal.
        /// </summary>
        /// <param name="password">Mật khẩu chưa mã hóa</param>
        /// <returns>Chuỗi băm SHA256 dạng Hex</returns>
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

        /// <summary>
        /// Xác thực mật khẩu người dùng nhập vào so với mật khẩu đã băm lưu trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="password">Mật khẩu nhập vào cần kiểm tra</param>
        /// <param name="hashedPassword">Mật khẩu đã được băm từ trước</param>
        /// <returns>True nếu trùng khớp, ngược lại False</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword)) 
                return false;

            return string.Equals(HashPassword(password), hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
