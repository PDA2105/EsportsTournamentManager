using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.Admin.Teams
{
    public partial class PlayerDialog : Window
    {
        public Player Player { get; private set; }

        public PlayerDialog(Player player = null)
        {
            InitializeComponent();

            if (player != null)
            {
                // Edit mode
                Player = player;
                TxtTitle.Text = "Sửa Tuyển Thủ";
                TxtInGameName.Text = player.InGameName;
                TxtRealName.Text = player.RealName;
                TxtAvatarPath.Text = player.AvatarPath;
                ChkIsActive.IsChecked = player.IsActive;

                // Select position in ComboBox
                foreach (ComboBoxItem item in CboPosition.Items)
                {
                    if (item.Content.ToString() == player.Position)
                    {
                        CboPosition.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                // Add mode
                Player = new Player();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string inGameName = TxtInGameName.Text.Trim();
            string realName = TxtRealName.Text.Trim();

            if (string.IsNullOrWhiteSpace(inGameName) || string.IsNullOrWhiteSpace(realName))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin bắt buộc (*)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Player.InGameName = inGameName;
            Player.RealName = realName;
            
            if (CboPosition.SelectedItem is ComboBoxItem selectedItem)
            {
                Player.Position = selectedItem.Content.ToString();
            }
            else
            {
                Player.Position = null;
            }

            Player.AvatarPath = string.IsNullOrWhiteSpace(TxtAvatarPath.Text) ? null : TxtAvatarPath.Text.Trim();
            Player.IsActive = ChkIsActive.IsChecked ?? true;

            DialogResult = true;
            Close();
        }
    }
}
