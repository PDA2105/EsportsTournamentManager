using System;
using System.Linq;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Data.Seeders
{
    public static class UserSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Clean up old referee user if present
            var oldReferee = context.Users.FirstOrDefault(u => u.Username.ToLower() == "referee");
            if (oldReferee != null)
            {
                context.Users.Remove(oldReferee);
            }

            // Gieo (Seed) tài khoản admin nếu chưa có, hoặc cập nhật nếu đã có để đồng bộ mật khẩu mới
            var adminUser = context.Users.FirstOrDefault(u => u.Username.ToLower() == "admin");
            if (adminUser == null)
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.HashPassword("admin123"),
                    FullName = "System Administrator",
                    Role = "Admin"
                });
            }
            else
            {
                adminUser.PasswordHash = PasswordHasher.HashPassword("admin123");
            }

            // Gieo (Seed) tài khoản user thường nếu chưa có, hoặc cập nhật nếu đã có để đồng bộ mật khẩu mới
            var defaultUser = context.Users.FirstOrDefault(u => u.Username.ToLower() == "user");
            if (defaultUser == null)
            {
                context.Users.Add(new User
                {
                    Username = "user",
                    PasswordHash = PasswordHasher.HashPassword("user123"),
                    FullName = "Default User",
                    Role = "User"
                });
            }
            else
            {
                defaultUser.PasswordHash = PasswordHasher.HashPassword("user123");
            }

            context.SaveChanges();
        }
    }
}
