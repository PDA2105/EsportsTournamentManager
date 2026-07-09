using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    public class StatisticsService
    {
        // Tìm ra tuyển thủ xuất sắc nhất giải đấu (MVP Tournament)
        public Player GetTournamentMvp(int tournamentId, out double avgScore)
        {
            avgScore = 0;
            using (var db = new AppDbContext())
            {
                var tournament = db.Tournaments.Find(tournamentId);
                if (tournament == null)
                    return null;

                // Tải tất cả trận đấu thuộc giải đấu này
                var tournamentMatches = db.Matches
                    .Where(m => m.TournamentId == tournamentId)
                    .ToList();

                // Tải danh sách ID các đội tham gia giải đấu
                var teamIds = db.TournamentTeams
                    .Where(tt => tt.TournamentId == tournamentId)
                    .Select(tt => tt.TeamId)
                    .ToList();

                // Đếm số trận đấu thực tế mà từng đội đã thi đấu (chỉ tính trận có trạng thái Completed)
                var teamMatchesCount = teamIds.ToDictionary(tid => tid, tid => 0);
                foreach (var match in tournamentMatches)
                {
                    if (match.Status == "Completed")
                    {
                        if (match.Team1Id.HasValue && teamMatchesCount.ContainsKey(match.Team1Id.Value))
                            teamMatchesCount[match.Team1Id.Value]++;
                        if (match.Team2Id.HasValue && teamMatchesCount.ContainsKey(match.Team2Id.Value))
                            teamMatchesCount[match.Team2Id.Value]++;
                    }
                }

                // Xác định số lượng trận đấu nhiều nhất của một đội tuyển trong giải đấu
                int maxMatchesInTournament = teamMatchesCount.Values.Count > 0 ? teamMatchesCount.Values.Max() : 1;
                if (maxMatchesInTournament < 1) maxMatchesInTournament = 1;

                // Tìm trận Chung kết tổng (Grand Final)
                var grandFinal = tournamentMatches.FirstOrDefault(m => 
                    !m.NextMatchId.HasValue && 
                    (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination"));

                var stats = db.PlayerStats
                    .Include(ps => ps.Player.Team)
                    .Where(ps => ps.MatchMap.Match.TournamentId == tournamentId)
                    .ToList();

                if (stats.Count == 0)
                    return null;

                var playerGroup = stats.GroupBy(ps => ps.PlayerId)
                    .Select(g => {
                        var player = g.First().Player;
                        var matchAverages = g.GroupBy(ps => ps.MatchMap.MatchId)
                                             .Select(mg => mg.Average(ps => ps.PerformancePoints))
                                             .ToList();

                        double totalMatchPoints = matchAverages.Sum();
                        
                        // Tính ước số (divisor) cho đội của người chơi này
                        int divisor = maxMatchesInTournament;
                        int playerTeamId = player.TeamId;

                        if (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination")
                        {
                            // Nếu lọt vào trận Chung kết tổng, chia cho số trận thi đấu thực tế
                            bool isFinalist = grandFinal != null && (playerTeamId == grandFinal.Team1Id || playerTeamId == grandFinal.Team2Id);
                            if (isFinalist && teamMatchesCount.TryGetValue(playerTeamId, out int actualMatches))
                            {
                                divisor = actualMatches;
                            }
                        }

                        if (divisor < 1) divisor = 1;

                        double score = totalMatchPoints / divisor;

                        return new {
                            Player = player,
                            CalculatedScore = score
                        };
                    })
                    .OrderByDescending(x => x.CalculatedScore)
                    .FirstOrDefault();

                if (playerGroup != null)
                {
                    avgScore = playerGroup.CalculatedScore;
                    return playerGroup.Player;
                }
            }
            return null;
        }

        public DashboardSummary GetDashboardSummary()
        {
            using (var db = new AppDbContext())
            {
                return new DashboardSummary
                {
                    TotalTournament = db.Tournaments.Count(),
                    TotalTeam = db.Teams.Count(),
                    TotalPlayer = db.Players.Count(),
                    ActiveMatch = db.Matches.Count(m => m.Status == "Live")
                };
            }
        }

        public List<Match> GetLiveMatches()
        {
            using (var db = new AppDbContext())
            {
                return db.Matches
                    .Include(m => m.Team1)
                    .Include(m => m.Team2)
                    .Include(m => m.Tournament)
                    .Where(m => m.Status == "Live")
                    .ToList();
            }
        }

        public List<TeamDashboardStats> GetTopTeams(int count)
        {
            using (var db = new AppDbContext())
            {
                var teams = db.Teams.ToList();
                var completedMatches = db.Matches
                    .Where(m => m.Status == "Completed" && m.WinnerTeamId.HasValue)
                    .ToList();

                var list = new List<TeamDashboardStats>();
                foreach (var t in teams)
                {
                    int played = completedMatches.Count(m => m.Team1Id == t.TeamId || m.Team2Id == t.TeamId);
                    int wins = completedMatches.Count(m => m.WinnerTeamId == t.TeamId);
                    double rate = played > 0 ? (double)wins / played : 0.0;
                    list.Add(new TeamDashboardStats
                    {
                        Team = t,
                        Wins = wins,
                        MatchesPlayed = played,
                        WinRate = rate
                    });
                }

                return list
                    .OrderByDescending(x => x.WinRate)
                    .ThenByDescending(x => x.Wins)
                    .Take(count)
                    .ToList();
            }
        }

        public List<PlayerDashboardStats> GetTopPlayers(int count)
        {
            using (var db = new AppDbContext())
            {
                var allStats = db.PlayerStats
                    .Include(ps => ps.Player.Team)
                    .ToList();

                if (!allStats.Any()) return new List<PlayerDashboardStats>();

                return allStats
                    .GroupBy(ps => ps.PlayerId)
                    .Select(g => new PlayerDashboardStats
                    {
                        Player = g.First().Player,
                        AveragePoints = g.Average(ps => ps.PerformancePoints),
                        MatchesPlayed = g.Select(ps => ps.MatchMap.MatchId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.AveragePoints)
                    .Take(count)
                    .ToList();
            }
        }

        public TournamentStatsSummary GetTournamentOverviewStats(int tournamentId)
        {
            using (var db = new AppDbContext())
            {
                var stats = db.PlayerStats
                    .Where(ps => ps.MatchMap.Match.TournamentId == tournamentId)
                    .ToList();

                double avgKills = stats.Any() ? stats.Average(ps => ps.Kills) * 5.0 : 0.0;
                double avgDamage = stats.Any() ? stats.Average(ps => ps.DamageDealt) : 0.0;
                double avgCS = stats.Any() ? stats.Average(ps => ps.CreepScore) : 0.0;

                var tournamentTeams = db.TournamentTeams
                    .Include(tt => tt.Team)
                    .Where(tt => tt.TournamentId == tournamentId)
                    .ToList();

                var completedMatches = db.Matches
                    .Where(m => m.TournamentId == tournamentId && m.Status == "Completed" && m.WinnerTeamId.HasValue)
                    .ToList();

                string topTeamName = "Chưa có";
                double topTeamWinRate = 0.0;

                foreach (var tt in tournamentTeams)
                {
                    int played = completedMatches.Count(m => m.Team1Id == tt.TeamId || m.Team2Id == tt.TeamId);
                    int wins = completedMatches.Count(m => m.WinnerTeamId == tt.TeamId);
                    double rate = played > 0 ? (double)wins / played : 0.0;
                    if (rate > topTeamWinRate && played > 0)
                    {
                        topTeamWinRate = rate;
                        topTeamName = tt.Team.TeamName;
                    }
                }

                return new TournamentStatsSummary
                {
                    AvgKills = avgKills,
                    AvgDamage = avgDamage,
                    AvgCS = avgCS,
                    TopTeamName = topTeamName,
                    TopTeamWinRate = topTeamWinRate
                };
            }
        }

        public TeamDetailStatsSummary GetTeamDetailStats(int teamId)
        {
            using (var db = new AppDbContext())
            {
                var completedMatches = db.Matches
                    .Include(m => m.Team1)
                    .Include(m => m.Team2)
                    .Include(m => m.Tournament)
                    .Where(m => (m.Team1Id == teamId || m.Team2Id == teamId) && m.Status == "Completed")
                    .ToList();

                int wins = completedMatches.Count(m => m.WinnerTeamId == teamId);
                int played = completedMatches.Count;
                double winRate = played > 0 ? (double)wins / played : 0.0;

                var stats = db.PlayerStats
                    .Where(ps => ps.Player.TeamId == teamId)
                    .ToList();

                double avgKills = stats.Any() ? stats.Average(ps => ps.Kills) : 0.0;
                double avgDamage = stats.Any() ? stats.Average(ps => ps.DamageDealt) : 0.0;
                double avgCS = stats.Any() ? stats.Average(ps => ps.CreepScore) : 0.0;

                var recentMatches = db.Matches
                    .Include(m => m.Team1)
                    .Include(m => m.Team2)
                    .Include(m => m.Tournament)
                    .Where(m => m.Team1Id == teamId || m.Team2Id == teamId)
                    .OrderByDescending(m => m.ScheduledTime)
                    .Take(5)
                    .ToList();

                return new TeamDetailStatsSummary
                {
                    WinRate = winRate,
                    Wins = wins,
                    MatchesPlayed = played,
                    AvgKills = avgKills,
                    AvgDamage = avgDamage,
                    AvgCS = avgCS,
                    RecentMatches = recentMatches
                };
            }
        }

        public PlayerDetailStatsSummary GetPlayerDetailStats(int playerId)
        {
            using (var db = new AppDbContext())
            {
                var player = db.Players
                    .Include(p => p.Team)
                    .FirstOrDefault(p => p.PlayerId == playerId);

                if (player == null) return null;

                var stats = db.PlayerStats
                    .Include(ps => ps.MatchMap)
                    .Where(ps => ps.PlayerId == playerId)
                    .ToList();

                double avgKills = stats.Any() ? stats.Average(ps => ps.Kills) : 0.0;
                double avgDeaths = stats.Any() ? stats.Average(ps => ps.Deaths) : 0.0;
                double avgAssists = stats.Any() ? stats.Average(ps => ps.Assists) : 0.0;
                double avgDamage = stats.Any() ? stats.Average(ps => ps.DamageDealt) : 0.0;
                double avgCS = stats.Any() ? stats.Average(ps => ps.CreepScore) : 0.0;
                double avgPTS = stats.Any() ? stats.Average(ps => ps.PerformancePoints) : 0.0;
                int played = stats.Select(ps => ps.MatchMap.MatchId).Distinct().Count();
                int mvpCount = 0;
                int playerTeamId = player.TeamId;
                var winningMatches = db.Matches
                    .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats))
                    .Where(m => m.Status == "Completed" && m.WinnerTeamId == playerTeamId)
                    .ToList();

                foreach (var match in winningMatches)
                {
                    var allMatchStats = match.MatchMaps
                        .SelectMany(mm => mm.PlayerStats)
                        .ToList();

                    if (allMatchStats.Any())
                    {
                        var matchMvp = allMatchStats.GroupBy(ps => ps.PlayerId)
                            .Select(g => new {
                                PlayerId = g.Key,
                                AvgPoints = g.Average(ps => ps.PerformancePoints)
                            })
                            .OrderByDescending(x => x.AvgPoints)
                            .FirstOrDefault();

                        if (matchMvp != null && matchMvp.PlayerId == playerId)
                        {
                            mvpCount++;
                        }
                    }
                }

                int teamId = player.TeamId;
                var recentMatches = db.Matches
                    .Include(m => m.Team1)
                    .Include(m => m.Team2)
                    .Include(m => m.Tournament)
                    .Where(m => m.Team1Id == teamId || m.Team2Id == teamId)
                    .OrderByDescending(m => m.ScheduledTime)
                    .Take(5)
                    .ToList();

                return new PlayerDetailStatsSummary
                {
                    AvgKills = avgKills,
                    AvgDeaths = avgDeaths,
                    AvgAssists = avgAssists,
                    AvgDamage = avgDamage,
                    AvgCS = avgCS,
                    AvgPTS = avgPTS,
                    MatchesPlayed = played,
                    MvpCount = mvpCount,
                    Position = player.Position,
                    RecentMatches = recentMatches
                };
            }
        }
    }
}
