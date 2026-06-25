namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAvatarPath : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Players", "AvatarPath");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Players", "AvatarPath", c => c.String());
        }
    }
}
