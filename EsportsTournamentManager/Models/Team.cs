using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public string Acronym { get; set; }

        public string LogoPath { get; set; }

        public string Coach { get; set; }

        public string Region { get; set; }

        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
    }
}
