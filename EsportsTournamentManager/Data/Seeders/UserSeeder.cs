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

            // Seed admin if not exists
            if (!context.Users.Any(u => u.Username.ToLower() == "admin"))
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.HashPassword("admin123"),
                    FullName = "System Administrator",
                    Role = "Admin"
                });
            }

            // Seed user if not exists
            if (!context.Users.Any(u => u.Username.ToLower() == "user"))
            {
                context.Users.Add(new User
                {
                    Username = "user",
                    PasswordHash = PasswordHasher.HashPassword("user123"),
                    FullName = "Default User",
                    Role = "User"
                });
            }

            context.SaveChanges();
        }
    }
}
