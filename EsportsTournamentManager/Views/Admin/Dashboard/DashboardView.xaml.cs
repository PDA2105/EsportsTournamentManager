using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.Admin.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public void LoadStatistics()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // 1. Load card counts
                    int teamsCount = db.Teams.Count();
                    int playersCount = db.Players.Count();
                    int tournamentsCount = db.Tournaments.Count();

                    TxtStatsTeams.Text = teamsCount.ToString();
                    TxtStatsPlayers.Text = playersCount.ToString();
                    TxtStatsTournaments.Text = tournamentsCount.ToString();

                    // 2. Load active tournaments (Live)
                    var activeTournaments = db.Tournaments
                        .Include(t => t.Matches)
                        .Where(t => t.Status == "Active")
                        .ToList()
                        .Select(t => new
                        {
                            t.Name,
                            t.GameType,
                            FormatDisplay = GetFormatDisplay(t.Format),
                            ProgressText = GetProgressText(t.Matches)
                        })
                        .ToList();

                    GridActiveTournaments.ItemsSource = activeTournaments;

                    // 3. Load top 5 players based on average performance points
                    // Phải tải danh sách stats về bộ nhớ trước (ToList) vì PerformancePoints là thuộc tính C# tính toán, không thể dịch sang SQL
                    var allStats = db.PlayerStats
                        .Include(ps => ps.Player.Team)
                        .ToList();

                    var topPlayersData = allStats
                        .GroupBy(ps => ps.PlayerId)
                        .Select(g => new
                        {
                            PlayerId = g.Key,
                            InGameName = g.First().Player.InGameName,
                            TeamName = g.First().Player.Team != null ? g.First().Player.Team.TeamName : "Tự do",
                            AveragePTS = g.Average(ps => ps.PerformancePoints),
                            TotalMatches = g.Count()
                        })
                        .OrderByDescending(x => x.AveragePTS)
                        .Take(5)
                        .ToList();

                    var topPlayers = topPlayersData
                        .Select((x, idx) => new
                        {
                            Rank = idx + 1,
                            x.InGameName,
                            x.TeamName,
                            DisplayPTS = $"{x.AveragePTS:0.0}",
                            DisplayMatches = $"{x.TotalMatches}"
                        })
                        .ToList();

                    GridTopPlayers.ItemsSource = topPlayers;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải thống kê Dashboard: {ex}");
                TxtStatsTeams.Text = "N/A";
                TxtStatsPlayers.Text = "N/A";
                TxtStatsTournaments.Text = "N/A";
                GridActiveTournaments.ItemsSource = null;
                GridTopPlayers.ItemsSource = null;
            }
        }

        private static string GetFormatDisplay(string format)
        {
            switch (format)
            {
                case "SingleElimination": return "Loại Trực Tiếp";
                case "DoubleElimination": return "Nhánh Thắng Nhánh Thua";
                case "RoundRobin": return "Vòng Tròn";
                default: return format;
            }
        }

        private static string GetProgressText(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0) return "0/0";
            int completed = matches.Count(m => m.Status == "Completed");
            return $"{completed}/{matches.Count}";
        }
    }
}
