using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        public int TournamentId { get; set; }

        public int? Team1Id { get; set; }

        public int? Team2Id { get; set; }

        public int MatchOrder { get; set; }

        public int RoundNumber { get; set; }

        public string BracketBranch { get; set; } = "Winner"; // "Winner", "Loser"

        public int? NextMatchId { get; set; }

        public string MatchFormat { get; set; } = "BO3"; // "BO1", "BO3", "BO5"

        public int Team1Score { get; set; } = 0;

        public int Team2Score { get; set; } = 0;

        public int? WinnerTeamId { get; set; }

        public DateTime ScheduledTime { get; set; }

        public string VenueSlot { get; set; }

        public string Status { get; set; } = "Scheduled"; // "Scheduled", "Live", "Completed", "Cancelled"

        public virtual Tournament Tournament { get; set; }

        public virtual Team Team1 { get; set; }

        public virtual Team Team2 { get; set; }

        public virtual Team WinnerTeam { get; set; }

        public virtual Match NextMatch { get; set; }

        public virtual ICollection<MatchMap> MatchMaps { get; set; } = new List<MatchMap>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
