using System;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.Auth
{
    public partial class LoginView : UserControl
    {
        private readonly AuthService _authService;

        public event EventHandler LoginSuccess;

        public LoginView()
        {
            InitializeComponent();
            _authService = AuthService.Instance;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text;
            string password = TxtPassword.Password;

            LblLoginError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                LblLoginError.Text = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                LblLoginError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                bool success = _authService.Login(username, password);
                if (success)
                {
                    // Clear form inputs
                    TxtUsername.Text = string.Empty;
                    TxtPassword.Password = string.Empty;

                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    LblLoginError.Text = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                    LblLoginError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LblLoginError.Text = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message;
                LblLoginError.Visibility = Visibility.Visible;
            }
        }
    }
}
