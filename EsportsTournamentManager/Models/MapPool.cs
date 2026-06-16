using System;

namespace EsportsTournamentManager.Models
{
    public class MapPool
    {
        public int MapPoolId { get; set; }

        public int TournamentId { get; set; }

        public string MapName { get; set; }

        public virtual Tournament Tournament { get; set; }
    }
}
