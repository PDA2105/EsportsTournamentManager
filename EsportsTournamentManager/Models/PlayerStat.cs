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

        public double PerformancePoints
        {
            get
            {
                return Kills * 3.0 + Assists * 2.0 - Deaths * 1.5 + DamageDealt / 1000.0 + CreepScore / 10.0;
            }
        }

        public virtual MatchMap MatchMap { get; set; }

        public virtual Player Player { get; set; }
    }
}
