using System;

namespace EsportsTournamentManager.Models
{
    public class PrizePool
    {
        public int PrizePoolId { get; set; }

        public int TournamentId { get; set; }

        public int RankPlace { get; set; }

        public decimal PrizeAmount { get; set; }

        public string OtherRewards { get; set; }

        public virtual Tournament Tournament { get; set; }
    }
}
