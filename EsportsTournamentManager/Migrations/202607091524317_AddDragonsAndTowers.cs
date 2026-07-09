namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDragonsAndTowers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MatchMaps", "Team1DragonsKilled", c => c.Int(nullable: false));
            AddColumn("dbo.MatchMaps", "Team2DragonsKilled", c => c.Int(nullable: false));
            AddColumn("dbo.MatchMaps", "Team1TowersDestroyed", c => c.Int(nullable: false));
            AddColumn("dbo.MatchMaps", "Team2TowersDestroyed", c => c.Int(nullable: false));
            AddColumn("dbo.Players", "Nationality", c => c.String());
            AddColumn("dbo.Players", "DateOfBirth", c => c.DateTime());
            AddColumn("dbo.Players", "ImagePath", c => c.String());
            AddColumn("dbo.Teams", "Region", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teams", "Region");
            DropColumn("dbo.Players", "ImagePath");
            DropColumn("dbo.Players", "DateOfBirth");
            DropColumn("dbo.Players", "Nationality");
            DropColumn("dbo.MatchMaps", "Team2TowersDestroyed");
            DropColumn("dbo.MatchMaps", "Team1TowersDestroyed");
            DropColumn("dbo.MatchMaps", "Team2DragonsKilled");
            DropColumn("dbo.MatchMaps", "Team1DragonsKilled");
        }
    }
}
