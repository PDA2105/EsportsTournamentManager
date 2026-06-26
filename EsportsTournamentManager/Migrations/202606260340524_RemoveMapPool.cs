namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMapPool : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MapPools", "TournamentId", "dbo.Tournaments");
            DropIndex("dbo.MapPools", new[] { "TournamentId" });
            DropTable("dbo.MapPools");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MapPools",
                c => new
                    {
                        MapPoolId = c.Int(nullable: false, identity: true),
                        TournamentId = c.Int(nullable: false),
                        MapName = c.String(),
                    })
                .PrimaryKey(t => t.MapPoolId);
            
            CreateIndex("dbo.MapPools", "TournamentId");
            AddForeignKey("dbo.MapPools", "TournamentId", "dbo.Tournaments", "TournamentId", cascadeDelete: true);
        }
    }
}
