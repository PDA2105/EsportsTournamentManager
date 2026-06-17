using System;
using System.Windows;
using System.Windows.Input;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.Admin.Teams
{
    public partial class TeamDialog : Window
    {
        public Team Team { get; private set; }

        public TeamDialog(Team team = null)
        {
            InitializeComponent();

            if (team != null)
            {
                // Edit mode
                Team = team;
                TxtTitle.Text = "Sửa Đội Tuyển";
                TxtTeamName.Text = team.TeamName;
                TxtAcronym.Text = team.Acronym;
                TxtCoach.Text = team.Coach;
                TxtLogoPath.Text = team.LogoPath;
            }
            else
            {
                // Add mode
                Team = new Team();
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
            string teamName = TxtTeamName.Text.Trim();
            string acronym = TxtAcronym.Text.Trim();

            if (string.IsNullOrWhiteSpace(teamName) || string.IsNullOrWhiteSpace(acronym))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin bắt buộc (*)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Team.TeamName = teamName;
            Team.Acronym = acronym;
            Team.Coach = string.IsNullOrWhiteSpace(TxtCoach.Text) ? null : TxtCoach.Text.Trim();
            Team.LogoPath = string.IsNullOrWhiteSpace(TxtLogoPath.Text) ? null : TxtLogoPath.Text.Trim();

            if (Team.TeamId == 0)
            {
                Team.CreatedDate = DateTime.Now;
            }

            DialogResult = true;
            Close();
        }
    }
}
