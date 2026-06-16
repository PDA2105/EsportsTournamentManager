using System;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    public class AuthService
    {
        private static readonly object _lock = new object();
        private static AuthService _instance;
        public static AuthService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ?? (_instance = new AuthService());
                }
            }
        }

        public User CurrentUser { get; private set; }

        public AuthService()
        {
            SeedInitialUsers();
        }

        public void SeedInitialUsers()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    if (!db.Users.Any())
                    {
                        var admin = new User
                        {
                            Username = "admin",
                            PasswordHash = PasswordHasher.HashPassword("admin123"),
                            FullName = "System Administrator",
                            Role = "Admin"
                        };

                        var referee = new User
                        {
                            Username = "referee",
                            PasswordHash = PasswordHasher.HashPassword("referee123"),
                            FullName = "Default Referee",
                            Role = "Referee"
                        };

                        db.Users.Add(admin);
                        db.Users.Add(referee);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                // Suppress database connection errors on static initialization
            }
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
                if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    CurrentUser = user;
                    return true;
                }
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool Register(string username, string password, string fullName, string role)
        {
            using (var db = new AppDbContext())
            {
                if (db.Users.Any(u => u.Username.ToLower() == username.ToLower()))
                {
                    return false;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = PasswordHasher.HashPassword(password),
                    FullName = fullName,
                    Role = role
                };

                db.Users.Add(newUser);
                db.SaveChanges();
                return true;
            }
        }
    }
}
