using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        public int TeamId { get; set; }

        public string RealName { get; set; }

        public string InGameName { get; set; }

        public string Position { get; set; }

        public string AvatarPath { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Team Team { get; set; }
        public virtual ICollection<PlayerStat> PlayerStats { get; set; } = new List<PlayerStat>();
    }
}
