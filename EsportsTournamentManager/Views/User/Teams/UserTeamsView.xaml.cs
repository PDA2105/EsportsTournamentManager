using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User.Teams
{
    public partial class UserTeamsView : UserControl
    {
        private readonly TeamService _teamService = new TeamService();
        private List<TeamStatsRow> _teamsList;

        public event EventHandler<int> TeamClicked;

        public UserTeamsView()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            try
            {
                var stats = _teamService.GetAllTeamsStats();
                _teamsList = stats.Select(s => new TeamStatsRow
                {
                    TeamId = s.TeamId,
                    Name = s.Name,
                    Region = s.Region,
                    MatchesPlayed = s.MatchesPlayed,
                    WinRate = s.WinRate,
                    AvgKills = s.AvgKills,
                    AvgDeaths = s.AvgDeaths,
                    AvgAssists = s.AvgAssists,
                    AvgDamage = s.AvgDamage,
                    AvgCreep = s.AvgCreep
                }).ToList();

                RenderTeamsGrid(_teamsList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải danh sách đội tuyển: {ex.Message}");
            }
        }

        private void RenderTeamsGrid(List<TeamStatsRow> list)
        {
            GridTeams.ItemsSource = list;
        }

        private void TxtSearchTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_teamsList == null) return;
            string q = TxtSearchTeam.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(q))
            {
                RenderTeamsGrid(_teamsList);
            }
            else
            {
                var filtered = _teamsList.Where(t => 
                    t.Name.ToLower().Contains(q) || 
                    (t.Region != null && t.Region.ToLower().Contains(q))
                ).ToList();
                RenderTeamsGrid(filtered);
            }
        }

        private void TeamName_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag is int teamId)
            {
                TeamClicked?.Invoke(this, teamId);
            }
        }
    }

    public class TeamStatsRow
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public int MatchesPlayed { get; set; }
        public double WinRate { get; set; }
        public double AvgKills { get; set; }
        public double AvgDeaths { get; set; }
        public double AvgAssists { get; set; }
        public double AvgDamage { get; set; }
        public double AvgCreep { get; set; }

        public string WinRateDisplay => $"{WinRate:0.0}%";
        public string AvgKillsDisplay => $"{AvgKills:0.0}";
        public string AvgDeathsDisplay => $"{AvgDeaths:0.0}";
        public string AvgAssistsDisplay => $"{AvgAssists:0.0}";
        public string AvgDamageDisplay => $"{AvgDamage:N0}";
        public string AvgCreepDisplay => $"{AvgCreep:0.0}";
    }
}
