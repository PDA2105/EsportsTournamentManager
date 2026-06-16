using System;

namespace EsportsTournamentManager.Models
{
    public class TournamentTeam
    {
        public int TournamentId { get; set; }

        public int TeamId { get; set; }

        public int? SeedNumber { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public virtual Tournament Tournament { get; set; }

        public virtual Team Team { get; set; }
    }
}
