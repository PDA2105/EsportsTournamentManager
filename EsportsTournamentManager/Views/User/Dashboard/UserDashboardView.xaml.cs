using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User.Dashboard
{
    public partial class UserDashboardView : UserControl
    {
        private readonly TournamentService _tournamentService = new TournamentService();

        public event EventHandler<int> MatchClicked;
        public event EventHandler<int> TeamClicked;
        public event EventHandler<int> PlayerClicked;
        public event EventHandler<int> TournamentClicked;
        public event EventHandler ViewAllTournamentsRequested;
        public event EventHandler ViewAllTeamsRequested;

        public UserDashboardView()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            try
            {
                // Quick stats
                var summary = _tournamentService.GetDashboardSummary();
                TxtTotalTournament.Text = summary.TotalTournament.ToString();
                TxtTotalTeam.Text = summary.TotalTeam.ToString();
                TxtTotalPlayer.Text = summary.TotalPlayer.ToString();
                TxtActiveMatch.Text = summary.ActiveMatch.ToString();

                // Live matches list
                var liveMatches = _tournamentService.GetLiveMatches();
                LiveMatchList.ItemsSource = liveMatches;
                TxtNoLiveMatches.Visibility = liveMatches.Any() ? Visibility.Collapsed : Visibility.Visible;

                // Top Teams & Top Players lists
                GridTopTeams.ItemsSource = _tournamentService.GetTopTeams(5);
                GridTopPlayers.ItemsSource = _tournamentService.GetTopPlayers(5);

                // 5 Newest Tournaments list
                var allTournaments = _tournamentService.GetAllTournaments();
                var newest5 = allTournaments.OrderByDescending(t => t.StartDate).ThenByDescending(t => t.TournamentId).Take(5).ToList();
                RenderNewestTournamentsGrid(newest5);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải Dashboard người dùng: {ex.Message}");
            }
        }

        private void RenderNewestTournamentsGrid(System.Collections.Generic.List<Tournament> list)
        {
            var gridData = list.Select(t => {
                string statusDisplay = "Sắp diễn ra";
                if (t.Status == "Active") statusDisplay = "Đang diễn ra";
                else if (t.Status == "Completed") statusDisplay = "Đã kết thúc";

                return new {
                    TournamentId = t.TournamentId,
                    Name = t.Name,
                    Status = t.Status,
                    StatusDisplay = statusDisplay
                };
            }).ToList();

            GridNewestTournaments.ItemsSource = gridData;
        }

        private void BtnViewAllTournaments_Click(object sender, RoutedEventArgs e)
        {
            ViewAllTournamentsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnViewAllTeams_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ViewAllTeamsRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void TournamentName_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag is int tournamentId)
            {
                TournamentClicked?.Invoke(this, tournamentId);
            }
        }

        private void LiveMatchRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var border = sender as Border;
                var match = border?.DataContext as Match;
                if (match != null)
                {
                    MatchClicked?.Invoke(this, match.MatchId);
                }
            }
        }

        private void TeamLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag is Team team)
            {
                TeamClicked?.Invoke(this, team.TeamId);
            }
            else if (btn.DataContext is Team teamCtx)
            {
                TeamClicked?.Invoke(this, teamCtx.TeamId);
            }
            else if (btn.DataContext != null)
            {
                var prop = btn.DataContext.GetType().GetProperty("TeamId");
                if (prop != null)
                {
                    int teamId = (int)prop.GetValue(btn.DataContext);
                    TeamClicked?.Invoke(this, teamId);
                }
            }
        }

        private void PlayerLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag is Player player)
            {
                PlayerClicked?.Invoke(this, player.PlayerId);
            }
            else if (btn.DataContext is Player playerCtx)
            {
                PlayerClicked?.Invoke(this, playerCtx.PlayerId);
            }
            else if (btn.DataContext != null)
            {
                var prop = btn.DataContext.GetType().GetProperty("PlayerId");
                if (prop != null)
                {
                    int playerId = (int)prop.GetValue(btn.DataContext);
                    PlayerClicked?.Invoke(this, playerId);
                }
            }
        }
    }
}
