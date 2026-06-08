using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsportsTournamentManager.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        public string Nickname { get; set; }

        public string FullName { get; set; }

        public int TeamId { get; set; }

        public virtual Team Team { get; set; }
    }
}
