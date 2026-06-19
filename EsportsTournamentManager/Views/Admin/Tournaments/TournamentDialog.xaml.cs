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
                
                // Populate max teams list based on current format
                PopulateMaxTeamsOptions();
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
                
                // Initialize default format and max teams options
                SetComboBoxSelectedContent(CboFormat, "SingleElimination");
                PopulateMaxTeamsOptions();
                CboMaxTeams.SelectedIndex = 1; // Default to 8 teams
            }
        }

        private void SetComboBoxSelectedContent(System.Windows.Controls.ComboBox comboBox, string value)
        {
            if (comboBox == null || value == null) return;
            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content != null && (item.Content.ToString() == value || (item.Tag != null && item.Tag.ToString() == value)))
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }
 
        private void CboFormat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PopulateMaxTeamsOptions();
        }
 
        private void PopulateMaxTeamsOptions()
        {
            if (CboMaxTeams == null || CboFormat == null) return;
 
            var selectedItem = CboFormat.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedItem == null) return;
 
            string format = selectedItem.Tag?.ToString() ?? selectedItem.Content.ToString();
            
            // Save the currently selected number if possible to restore it later
            string currentSelection = (CboMaxTeams.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
 
            CboMaxTeams.Items.Clear();
 
            if (format == "SingleElimination")
            {
                AddMaxTeamsOption("4");
                AddMaxTeamsOption("8");
                AddMaxTeamsOption("16");
            }
            else if (format == "DoubleElimination")
            {
                AddMaxTeamsOption("4");
                AddMaxTeamsOption("8");
            }
            else if (format == "RoundRobin")
            {
                AddMaxTeamsOption("2");
                AddMaxTeamsOption("4");
                AddMaxTeamsOption("6");
                AddMaxTeamsOption("8");
                AddMaxTeamsOption("10");
                AddMaxTeamsOption("12");
                AddMaxTeamsOption("16");
            }
 
            // Restore previous selection if it's still valid, otherwise select first item
            SetComboBoxSelectedContent(CboMaxTeams, currentSelection);
            if (CboMaxTeams.SelectedIndex == -1 && CboMaxTeams.Items.Count > 0)
            {
                CboMaxTeams.SelectedIndex = 0;
            }
        }
 
        private void AddMaxTeamsOption(string value)
        {
            CboMaxTeams.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = value });
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
            Tournament.Format = selectedFormatItem.Tag?.ToString() ?? selectedFormatItem.Content.ToString();
            Tournament.MaxTeams = int.Parse(selectedMaxTeamsItem.Content.ToString());
            Tournament.StartDate = DpStartDate.SelectedDate.Value;

            DialogResult = true;
            Close();
        }
    }
}
