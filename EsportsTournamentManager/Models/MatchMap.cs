using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class MatchMap
    {
        public int MatchMapId { get; set; }

        public int MatchId { get; set; }

        public int MapNumber { get; set; }

        public string SelectedMapName { get; set; }

        public int Team1RoundScore { get; set; } = 0;

        public int Team2RoundScore { get; set; } = 0;

        public int? DurationSeconds { get; set; }

        public int? MVPlayerId { get; set; }

        public virtual Match Match { get; set; }

        public virtual Player MVPlayer { get; set; }

        public virtual ICollection<PlayerStat> PlayerStats { get; set; } = new List<PlayerStat>();
    }
}
