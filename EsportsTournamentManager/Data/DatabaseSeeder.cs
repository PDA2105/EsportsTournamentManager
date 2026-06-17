using System;
using System.Linq;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(AppDbContext context)
        {
            Seeders.UserSeeder.Seed(context);
            Seeders.TeamSeeder.Seed(context);
        }
    }
}
