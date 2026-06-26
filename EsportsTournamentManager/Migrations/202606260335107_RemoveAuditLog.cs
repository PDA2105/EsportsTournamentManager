namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAuditLog : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AuditLogs", "Match_MatchId", "dbo.Matches");
            DropForeignKey("dbo.AuditLogs", "UserId", "dbo.Users");
            DropIndex("dbo.AuditLogs", new[] { "UserId" });
            DropIndex("dbo.AuditLogs", new[] { "Match_MatchId" });
            DropTable("dbo.AuditLogs");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        AuditLogId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Action = c.String(),
                        TableName = c.String(),
                        RecordId = c.Int(nullable: false),
                        OldDataSnapshot = c.String(),
                        NewDataSnapshot = c.String(),
                        Timestamp = c.DateTime(nullable: false),
                        Match_MatchId = c.Int(),
                    })
                .PrimaryKey(t => t.AuditLogId);
            
            CreateIndex("dbo.AuditLogs", "Match_MatchId");
            CreateIndex("dbo.AuditLogs", "UserId");
            AddForeignKey("dbo.AuditLogs", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.AuditLogs", "Match_MatchId", "dbo.Matches", "MatchId");
        }
    }
}
