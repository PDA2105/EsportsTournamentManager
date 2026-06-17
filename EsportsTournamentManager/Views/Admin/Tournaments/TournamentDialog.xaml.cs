using System;
using System.Windows;
using System.Windows.Input;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.Admin.Tournaments
{
    public partial class TournamentDialog : Window
    {
        public Tournament Tournament { get; private set; }

        public TournamentDialog(Tournament tournament = null)
        {
            InitializeComponent();

            if (tournament != null)
            {
                // Edit mode
                Tournament = tournament;
                TxtTitle.Text = "Sửa Giải Đấu";
                TxtName.Text = tournament.Name;
                
                // Set ComboBox values
                SetComboBoxSelectedContent(CboGameType, tournament.GameType);
                SetComboBoxSelectedContent(CboFormat, tournament.Format);
                SetComboBoxSelectedContent(CboMaxTeams, tournament.MaxTeams.ToString());

                DpStartDate.SelectedDate = tournament.StartDate;

                // Disable format and max teams change if already started/completed
                if (tournament.Status != "Pending")
                {
                    CboFormat.IsEnabled = false;
                    CboMaxTeams.IsEnabled = false;
                }
            }
            else
            {
                // Add mode
                Tournament = new Tournament();
                DpStartDate.SelectedDate = DateTime.Today;
            }
        }

        private void SetComboBoxSelectedContent(System.Windows.Controls.ComboBox comboBox, string value)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    item.IsSelected = true;
                    break;
                }
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
            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng điền tên giải đấu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày khởi tranh.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedGameItem = CboGameType.SelectedItem as System.Windows.Controls.ComboBoxItem;
            var selectedFormatItem = CboFormat.SelectedItem as System.Windows.Controls.ComboBoxItem;
            var selectedMaxTeamsItem = CboMaxTeams.SelectedItem as System.Windows.Controls.ComboBoxItem;

            Tournament.Name = name;
            Tournament.GameType = selectedGameItem.Content.ToString();
            Tournament.Format = selectedFormatItem.Content.ToString();
            Tournament.MaxTeams = int.Parse(selectedMaxTeamsItem.Content.ToString());
            Tournament.StartDate = DpStartDate.SelectedDate.Value;

            DialogResult = true;
            Close();
        }
    }
}
