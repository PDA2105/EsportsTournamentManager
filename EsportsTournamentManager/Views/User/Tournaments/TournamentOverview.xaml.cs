using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User.Tournaments
{
    public partial class TournamentOverview : UserControl
    {
        private Tournament _tournament;
        private readonly TournamentService _tournamentService;

        public event EventHandler BackClicked;
        public event EventHandler<Tournament> ViewBracketClicked;
        public event EventHandler<Team> TeamClicked;
        public event EventHandler<Player> PlayerClicked;
        public event EventHandler<int> MatchClicked;

        public TournamentOverview()
        {
            InitializeComponent();
            _tournamentService = new TournamentService();
        }

        public void LoadTournament(int tournamentId)
        {
            try
            {
                _tournament = _tournamentService.GetTournamentById(tournamentId);
                if (_tournament == null) return;

                TxtTournamentOverviewHeader.Text = $"TỔNG QUAN GIẢI ĐẤU - {_tournament.Name.ToUpper()}";
                TxtInfoGameType.Text = _tournament.GameType;

                string fmt = _tournament.Format;
                if (fmt == "SingleElimination") TxtInfoFormat.Text = "Loại trực tiếp";
                else if (fmt == "DoubleElimination") TxtInfoFormat.Text = "Nhánh thắng thua";
                else TxtInfoFormat.Text = "Vòng tròn tính điểm";

                TxtInfoMaxTeams.Text = $"{_tournament.MaxTeams} Đội tối đa";

                string statusText = "Chờ bắt đầu";
                string statusColor = "#38BDF8";
                if (_tournament.Status == "Active")
                {
                    statusText = "Đang diễn ra";
                    statusColor = "#818CF8";
                }
                else if (_tournament.Status == "Completed")
                {
                    statusText = "Đã kết thúc";
                    statusColor = "#10B981";
                }
                TxtInfoStatus.Text = statusText;
                TxtInfoStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(statusColor));

                // Load overview statistics
                var overviewStats = _tournamentService.GetTournamentOverviewStats(tournamentId);
                TxtOverviewAvgKills.Text = $"{overviewStats.AvgKills:0.0}";
                TxtOverviewAvgCS.Text = $"{overviewStats.AvgCS:0.0}";
                TxtOverviewAvgDamage.Text = $"{overviewStats.AvgDamage:0.0}";
                TxtOverviewTopTeam.Text = $"{overviewStats.TopTeamName} ({overviewStats.TopTeamWinRateDisplay})";

                // Load champion team
                var championTeam = GetTournamentChampion(_tournament);
                if (championTeam != null)
                {
                    TxtChampionName.Content = championTeam.TeamName;
                    TxtChampionName.Tag = championTeam;
                    PanelTournamentChampion.Visibility = Visibility.Visible;
                    TxtChampionNotDecided.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PanelTournamentChampion.Visibility = Visibility.Collapsed;
                    TxtChampionNotDecided.Visibility = Visibility.Visible;
                }

                // Load MVP player
                double avgMvp;
                var mvp = _tournamentService.GetTournamentMvp(tournamentId, out avgMvp);
                if (mvp != null)
                {
                    TxtMvpName.Content = mvp.InGameName;
                    TxtMvpName.Tag = mvp;
                    TxtMvpScore.Text = $"Điểm TB: {avgMvp:0.0}";
                    PanelTournamentMvp.Visibility = Visibility.Visible;
                    TxtMvpNotDecided.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PanelTournamentMvp.Visibility = Visibility.Collapsed;
                    TxtMvpNotDecided.Visibility = Visibility.Visible;
                }

                // Bind assigned teams list
                ListShowTeams.ItemsSource = _tournament.TournamentTeams.Select(tt => tt.Team).ToList();

                // Bind matches list
                ListTournamentMatches.ItemsSource = _tournament.Matches.OrderBy(m => m.RoundNumber).ThenBy(m => m.MatchOrder).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị tổng quan giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Team GetTournamentChampion(Tournament tournament)
        {
            if (tournament == null || tournament.Status != "Completed") return null;

            if (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination")
            {
                var grandFinal = tournament.Matches.FirstOrDefault(m => !m.NextMatchId.HasValue);
                if (grandFinal != null && grandFinal.Status == "Completed")
                {
                    return grandFinal.WinnerTeam;
                }
            }
            else if (tournament.Format == "RoundRobin")
            {
                var teamWins = new Dictionary<int, int>();
                foreach (var match in tournament.Matches)
                {
                    if (match.Status == "Completed" && match.WinnerTeamId.HasValue)
                    {
                        int winnerId = match.WinnerTeamId.Value;
                        if (!teamWins.ContainsKey(winnerId))
                            teamWins[winnerId] = 0;
                        teamWins[winnerId]++;
                    }
                }
                if (teamWins.Count > 0)
                {
                    var championTeamId = teamWins.OrderByDescending(kv => kv.Value).First().Key;
                    return tournament.TournamentTeams
                        .Select(tt => tt.Team)
                        .FirstOrDefault(t => t != null && t.TeamId == championTeamId);
                }
            }
            return null;
        }

        private void BtnBackToHome_Click(object sender, RoutedEventArgs e)
        {
            BackClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnViewBracket_Click(object sender, RoutedEventArgs e)
        {
            if (_tournament != null)
            {
                ViewBracketClicked?.Invoke(this, _tournament);
            }
        }

        private void TeamLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var team = button?.Tag as Team;
            if (team != null)
            {
                TeamClicked?.Invoke(this, team);
            }
        }

        private void PlayerLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var player = button?.Tag as Player;
            if (player != null)
            {
                PlayerClicked?.Invoke(this, player);
            }
        }

        private void MatchRow_MouseDown(object sender, MouseButtonEventArgs e)
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
    }
}
