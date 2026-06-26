using System;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // Dịch vụ quản lý xác thực người dùng (Đăng nhập, Đăng xuất, Đăng ký và Khởi tạo tài khoản)
    public class AuthService
    {
        private static readonly object _lock = new object();
        private static AuthService _instance;

        // Singleton Instance của AuthService
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

        // Thông tin người dùng hiện tại đang đăng nhập
        public User CurrentUser { get; private set; }

        // Constructor khởi tạo AuthService và seed tài khoản mặc định
        public AuthService()
        {
            SeedInitialUsers();
        }

        // Khởi tạo dữ liệu người dùng mặc định (Admin/User) khi chạy lần đầu
        public void SeedInitialUsers()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    DatabaseSeeder.Seed(db);
                }
            }
            catch
            {
                // Bỏ qua lỗi kết nối database trong quá trình khởi tạo tĩnh ban đầu
            }
        }

        // Đăng nhập tài khoản bằng tên đăng nhập và mật khẩu
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

        // Đăng xuất người dùng hiện tại
        public void Logout()
        {
            CurrentUser = null;
        }

        // Đăng ký tài khoản người dùng mới vào DB
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
