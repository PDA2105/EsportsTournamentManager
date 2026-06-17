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

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
