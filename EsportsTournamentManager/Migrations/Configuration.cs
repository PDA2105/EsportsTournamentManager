namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EsportsTournamentManager.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EsportsTournamentManager.Data.AppDbContext context)
        {
            EsportsTournamentManager.Data.DatabaseSeeder.Seed(context);
        }
    }
}
