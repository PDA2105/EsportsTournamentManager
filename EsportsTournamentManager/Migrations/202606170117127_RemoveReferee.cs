namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveReferee : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Matches", "RefereeId", "dbo.Users");
            DropIndex("dbo.Matches", new[] { "RefereeId" });
            DropColumn("dbo.Matches", "RefereeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Matches", "RefereeId", c => c.Int());
            CreateIndex("dbo.Matches", "RefereeId");
            AddForeignKey("dbo.Matches", "RefereeId", "dbo.Users", "UserId");
        }
    }
}
