using System;
using System.Windows;
using System.Windows.Controls;

namespace EsportsTournamentManager.Views.Admin
{
    public partial class AdminMain : UserControl
    {
        public event EventHandler LogoutRequested;

        public AdminMain()
        {
            InitializeComponent();
        }

        public void SetUser(string fullName)
        {
            TxtHeaderUserName.Text = fullName;
            TxtHeaderUserRole.Text = "Quản trị viên";
            LoadDashboardStatistics();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset to Dashboard when logout
            SwitchActiveMenu(BtnMenuDashboard);
            SwitchSectionVisibility(DashboardSection);
            TxtHeaderTitle.Text = "Bảng điều khiển";

            LogoutRequested?.Invoke(this, EventArgs.Empty);
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
                        TxtHeaderTitle.Text = "Giải đấu & Lịch đấu";
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

        public void LoadDashboardStatistics()
        {
            DashboardSection.LoadStatistics();
        }
    }
}
