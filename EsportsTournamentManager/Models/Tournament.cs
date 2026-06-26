using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class Tournament
    {
        public int TournamentId { get; set; }

        public string Name { get; set; }

        public string GameType { get; set; } // "LoL", "Valorant", "CS2"

        public string Format { get; set; } // "SingleElimination", "DoubleElimination", "RoundRobin"

        public int MaxTeams { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; } = "Pending"; // "Pending", "Active", "Completed"

        public int CreatedByUserId { get; set; }

        public virtual User CreatedByUser { get; set; }

        public virtual ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
        public virtual ICollection<PrizePool> PrizePools { get; set; } = new List<PrizePool>();
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
