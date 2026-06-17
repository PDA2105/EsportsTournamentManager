using System;
using System.Windows;
using System.Windows.Controls;

namespace EsportsTournamentManager.Views.User
{
    public partial class UserMain : UserControl
    {
        public event EventHandler LogoutRequested;

        public UserMain()
        {
            InitializeComponent();
        }

        public void SetUser(string fullName)
        {
            TxtUserHeaderUserName.Text = fullName;
            TxtUserHeaderUserRole.Text = "Người dùng";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
