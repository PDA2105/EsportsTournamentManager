using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class DashboardSummary
    {
        public int TotalTournament { get; set; }
        public int TotalTeam { get; set; }
        public int TotalPlayer { get; set; }
        public int ActiveMatch { get; set; }
    }

    public class TeamDashboardStats
    {
        public Team Team { get; set; }
        public int TeamId => Team?.TeamId ?? 0;
        public string TeamName => Team?.TeamName;
        public string LogoPath => Team?.LogoPath;
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public double WinRate { get; set; }
        public string WinRateDisplay => $"{WinRate * 100:0.0}%";
    }

    public class PlayerDashboardStats
    {
        public Player Player { get; set; }
        public int PlayerId => Player?.PlayerId ?? 0;
        public string InGameName => Player?.InGameName;
        public string TeamName => Player?.Team?.TeamName ?? "Tự do";
        public double AveragePoints { get; set; }
        public string PointsDisplay => $"{AveragePoints:0.0}";
        public int MatchesPlayed { get; set; }
    }

    public class TournamentStatsSummary
    {
        public double AvgKills { get; set; }
        public double AvgDamage { get; set; }
        public double AvgCS { get; set; }
        public string TopTeamName { get; set; }
        public double TopTeamWinRate { get; set; }
        public string TopTeamWinRateDisplay => $"{TopTeamWinRate * 100:0.0}%";
    }

    public class TeamDetailStatsSummary
    {
        public double WinRate { get; set; }
        public string WinRateDisplay => $"{WinRate * 100:0.0}%";
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public double AvgKills { get; set; }
        public double AvgDamage { get; set; }
        public double AvgCS { get; set; }
        public List<Match> RecentMatches { get; set; }
    }

    public class PlayerDetailStatsSummary
    {
        public double AvgKills { get; set; }
        public double AvgDeaths { get; set; }
        public double AvgAssists { get; set; }
        public double AvgDamage { get; set; }
        public double AvgCS { get; set; }
        public double AvgPTS { get; set; }
        public int MatchesPlayed { get; set; }
        public int MvpCount { get; set; }
        public string Position { get; set; }
        public List<Match> RecentMatches { get; set; }
    }
}
