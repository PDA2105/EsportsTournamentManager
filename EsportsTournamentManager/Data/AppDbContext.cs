using System.Data.Entity;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("EsportsTournamentDb")
        {
        }

        public DbSet<Team> Teams { get; set; }

        public DbSet<Player> Players { get; set; }
    }
}