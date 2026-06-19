using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            _authService = AuthService.Instance;

            LoginSection.LoginSuccess += LoginSection_LoginSuccess;
            AdminMainSection.LogoutRequested += AdminMainSection_LogoutRequested;
            UserMainSection.LogoutRequested += UserMainSection_LogoutRequested;
        }

        private void LoginSection_LoginSuccess(object sender, EventArgs e)
        {
            var user = _authService.CurrentUser;
            if (user != null)
            {
                LoginSection.Visibility = Visibility.Collapsed;
                if (user.Role == "Admin")
                {
                    AdminMainSection.SetUser(user.FullName);
                    AdminMainSection.Visibility = Visibility.Visible;
                }
                else
                {
                    UserMainSection.SetUser(user.FullName);
                    UserMainSection.Visibility = Visibility.Visible;
                }
            }
        }

        private void AdminMainSection_LogoutRequested(object sender, EventArgs e)
        {
            PerformLogout();
        }

        private void UserMainSection_LogoutRequested(object sender, EventArgs e)
        {
            PerformLogout();
        }

        private void PerformLogout()
        {
            _authService.Logout();
            AdminMainSection.Visibility = Visibility.Collapsed;
            UserMainSection.Visibility = Visibility.Collapsed;
            LoginSection.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && this.WindowState == WindowState.Normal)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
