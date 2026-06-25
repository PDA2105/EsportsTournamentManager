using System;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp dịch vụ quản lý xác thực người dùng (Đăng nhập, Đăng xuất, Đăng ký và Khởi tạo tài khoản).
    /// </summary>
    public class AuthService
    {
        private static readonly object _lock = new object();
        private static AuthService _instance;

        /// <summary>
        /// Thể hiện duy nhất (Singleton Instance) của lớp AuthService.
        /// </summary>
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

        /// <summary>
        /// Thông tin người dùng hiện tại đang đăng nhập hệ thống.
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// Constructor khởi tạo AuthService và tự động seed tài khoản mặc định.
        /// </summary>
        public AuthService()
        {
            SeedInitialUsers();
        }

        /// <summary>
        /// Khởi tạo dữ liệu người dùng mặc định (Admin/User) khi ứng dụng chạy lần đầu.
        /// </summary>
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

        /// <summary>
        /// Đăng nhập tài khoản bằng tên người dùng và mật khẩu.
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu chưa mã hóa</param>
        /// <returns>True nếu đăng nhập thành công, ngược lại False</returns>
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

        /// <summary>
        /// Đăng xuất người dùng hiện tại ra khỏi hệ thống.
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu chưa mã hóa</param>
        /// <param name="fullName">Họ và tên đầy đủ</param>
        /// <param name="role">Vai trò (Admin/User)</param>
        /// <returns>True nếu đăng ký thành công, False nếu tài khoản đã tồn tại</returns>
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
