using System;
using System.Security.Cryptography;
using System.Text;

namespace EsportsTournamentManager.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedBytes = new byte[salt.Length + passwordBytes.Length];
            Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, salt.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string hashedPasswordAndSalt)
        {
            if (string.IsNullOrEmpty(hashedPasswordAndSalt)) return false;

            var parts = hashedPasswordAndSalt.Split(':');
            if (parts.Length != 2) return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combinedBytes = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, combinedBytes, salt.Length, passwordBytes.Length);

                using (var sha256 = SHA256.Create())
                {
                    byte[] actualHash = sha256.ComputeHash(combinedBytes);
                    if (actualHash.Length != expectedHash.Length) return false;
                    for (int i = 0; i < actualHash.Length; i++)
                    {
                        if (actualHash[i] != expectedHash[i]) return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
