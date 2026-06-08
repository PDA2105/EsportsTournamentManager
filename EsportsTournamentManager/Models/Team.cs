using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsportsTournamentManager.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public string Coach { get; set; }
    }
}
