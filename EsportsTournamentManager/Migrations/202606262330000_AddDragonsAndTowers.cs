namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDragonsAndTowers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MatchMaps", "Team1DragonsKilled", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.MatchMaps", "Team2DragonsKilled", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.MatchMaps", "Team1TowersDestroyed", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.MatchMaps", "Team2TowersDestroyed", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MatchMaps", "Team2TowersDestroyed");
            DropColumn("dbo.MatchMaps", "Team1TowersDestroyed");
            DropColumn("dbo.MatchMaps", "Team2DragonsKilled");
            DropColumn("dbo.MatchMaps", "Team1DragonsKilled");
        }
    }
}
