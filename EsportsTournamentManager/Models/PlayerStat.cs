using System;

namespace EsportsTournamentManager.Models
{
    public class PlayerStat
    {
        public int PlayerStatId { get; set; }

        public int MatchMapId { get; set; }

        public int PlayerId { get; set; }

        public int Kills { get; set; } = 0;

        public int Deaths { get; set; } = 0;

        public int Assists { get; set; } = 0;

        public int DamageDealt { get; set; } = 0;

        public int CreepScore { get; set; } = 0;

        public bool IsMvpOfMap { get; set; } = false;

        public virtual MatchMap MatchMap { get; set; }

        public virtual Player Player { get; set; }
    }
}
