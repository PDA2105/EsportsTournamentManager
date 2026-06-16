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
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
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
                    // Update header user profile
                    var user = _authService.CurrentUser;
                    TxtHeaderUserName.Text = user.FullName;
                    TxtHeaderUserRole.Text = user.Role == "Admin" ? "Quản trị viên" : "Trọng tài";

                    // Load statistics cards
                    LoadDashboardStatistics();

                    // Switch Grid Panels
                    LoginGrid.Visibility = Visibility.Collapsed;
                    MainGrid.Visibility = Visibility.Visible;

                    // Clear form inputs
                    TxtUsername.Text = string.Empty;
                    TxtPassword.Password = string.Empty;
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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();

            // Reset menu styling
            SwitchActiveMenu(BtnMenuDashboard);
            SwitchSectionVisibility(DashboardSection);
            TxtHeaderTitle.Text = "Bảng điều khiển";

            // Switch Grid Panels back to Login
            MainGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string tag = clickedButton.Tag as string;

                // Highlight active button
                SwitchActiveMenu(clickedButton);

                // Switch visible grid area
                switch (tag)
                {
                    case "Dashboard":
                        SwitchSectionVisibility(DashboardSection);
                        TxtHeaderTitle.Text = "Bảng điều khiển";
                        LoadDashboardStatistics();
                        break;
                    case "Teams":
                        SwitchSectionVisibility(TeamsSection);
                        TxtHeaderTitle.Text = "Đội tuyển & Tuyển thủ";
                        TeamsSection.LoadTeams();
                        break;
                    case "Tournaments":
                        SwitchSectionVisibility(TournamentsSection);
                        TxtHeaderTitle.Text = "Giải đấu & Nhánh đấu";
                        break;
                }
            }
        }

        private void SwitchActiveMenu(Button activeButton)
        {
            Style normalStyle = (Style)FindResource("SidebarButton");
            Style activeStyle = (Style)FindResource("ActiveSidebarButton");

            BtnMenuDashboard.Style = normalStyle;
            BtnMenuTeams.Style = normalStyle;
            BtnMenuTournaments.Style = normalStyle;

            activeButton.Style = activeStyle;
        }

        private void SwitchSectionVisibility(UserControl activeSection)
        {
            DashboardSection.Visibility = Visibility.Collapsed;
            TeamsSection.Visibility = Visibility.Collapsed;
            TournamentsSection.Visibility = Visibility.Collapsed;

            activeSection.Visibility = Visibility.Visible;
        }

        private void LoadDashboardStatistics()
        {
            DashboardSection.LoadStatistics();
        }
    }
}
