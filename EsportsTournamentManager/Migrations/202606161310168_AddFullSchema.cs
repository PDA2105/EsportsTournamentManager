namespace EsportsTournamentManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFullSchema : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.AuditLogId)
                .ForeignKey("dbo.Matches", t => t.Match_MatchId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.Match_MatchId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.String(),
                        FullName = c.String(),
                        Role = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.Username, unique: true);
            
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        MatchId = c.Int(nullable: false, identity: true),
                        TournamentId = c.Int(nullable: false),
                        Team1Id = c.Int(),
                        Team2Id = c.Int(),
                        MatchOrder = c.Int(nullable: false),
                        RoundNumber = c.Int(nullable: false),
                        BracketBranch = c.String(),
                        NextMatchId = c.Int(),
                        MatchFormat = c.String(),
                        Team1Score = c.Int(nullable: false),
                        Team2Score = c.Int(nullable: false),
                        WinnerTeamId = c.Int(),
                        ScheduledTime = c.DateTime(nullable: false),
                        VenueSlot = c.String(),
                        RefereeId = c.Int(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.MatchId)
                .ForeignKey("dbo.Matches", t => t.NextMatchId)
                .ForeignKey("dbo.Users", t => t.RefereeId)
                .ForeignKey("dbo.Teams", t => t.Team1Id)
                .ForeignKey("dbo.Teams", t => t.Team2Id)
                .ForeignKey("dbo.Tournaments", t => t.TournamentId, cascadeDelete: true)
                .ForeignKey("dbo.Teams", t => t.WinnerTeamId)
                .Index(t => new { t.TournamentId, t.RoundNumber, t.MatchOrder }, name: "IX_Tournament_Round_Order")
                .Index(t => t.Team1Id)
                .Index(t => t.Team2Id)
                .Index(t => t.NextMatchId)
                .Index(t => t.WinnerTeamId)
                .Index(t => t.RefereeId);
            
            CreateTable(
                "dbo.MatchMaps",
                c => new
                    {
                        MatchMapId = c.Int(nullable: false, identity: true),
                        MatchId = c.Int(nullable: false),
                        MapNumber = c.Int(nullable: false),
                        SelectedMapName = c.String(),
                        Team1RoundScore = c.Int(nullable: false),
                        Team2RoundScore = c.Int(nullable: false),
                        DurationSeconds = c.Int(),
                        MVPlayerId = c.Int(),
                    })
                .PrimaryKey(t => t.MatchMapId)
                .ForeignKey("dbo.Matches", t => t.MatchId, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.MVPlayerId)
                .Index(t => t.MatchId)
                .Index(t => t.MVPlayerId);
            
            CreateTable(
                "dbo.PlayerStats",
                c => new
                    {
                        PlayerStatId = c.Int(nullable: false, identity: true),
                        MatchMapId = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                        Kills = c.Int(nullable: false),
                        Deaths = c.Int(nullable: false),
                        Assists = c.Int(nullable: false),
                        DamageDealt = c.Int(nullable: false),
                        CreepScore = c.Int(nullable: false),
                        IsMvpOfMap = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PlayerStatId)
                .ForeignKey("dbo.MatchMaps", t => t.MatchMapId, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.PlayerId, cascadeDelete: true)
                .Index(t => t.MatchMapId)
                .Index(t => t.PlayerId);
            
            CreateTable(
                "dbo.TournamentTeams",
                c => new
                    {
                        TournamentId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                        SeedNumber = c.Int(),
                        RegisteredAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.TournamentId, t.TeamId })
                .ForeignKey("dbo.Teams", t => t.TeamId, cascadeDelete: true)
                .ForeignKey("dbo.Tournaments", t => t.TournamentId, cascadeDelete: true)
                .Index(t => t.TournamentId)
                .Index(t => t.TeamId);
            
            CreateTable(
                "dbo.Tournaments",
                c => new
                    {
                        TournamentId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        GameType = c.String(),
                        Format = c.String(),
                        MaxTeams = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        CreatedByUserId = c.Int(nullable: false),
                        CreatedByUser_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.TournamentId)
                .ForeignKey("dbo.Users", t => t.CreatedByUser_UserId)
                .Index(t => t.CreatedByUser_UserId);
            
            CreateTable(
                "dbo.MapPools",
                c => new
                    {
                        MapPoolId = c.Int(nullable: false, identity: true),
                        TournamentId = c.Int(nullable: false),
                        MapName = c.String(),
                    })
                .PrimaryKey(t => t.MapPoolId)
                .ForeignKey("dbo.Tournaments", t => t.TournamentId, cascadeDelete: true)
                .Index(t => t.TournamentId);
            
            CreateTable(
                "dbo.PrizePools",
                c => new
                    {
                        PrizePoolId = c.Int(nullable: false, identity: true),
                        TournamentId = c.Int(nullable: false),
                        RankPlace = c.Int(nullable: false),
                        PrizeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OtherRewards = c.String(),
                    })
                .PrimaryKey(t => t.PrizePoolId)
                .ForeignKey("dbo.Tournaments", t => t.TournamentId, cascadeDelete: true)
                .Index(t => t.TournamentId);
            
            AddColumn("dbo.Players", "RealName", c => c.String());
            AddColumn("dbo.Players", "InGameName", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Players", "Position", c => c.String());
            AddColumn("dbo.Players", "AvatarPath", c => c.String());
            AddColumn("dbo.Players", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Teams", "TeamName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Teams", "Acronym", c => c.String());
            AddColumn("dbo.Teams", "LogoPath", c => c.String());
            AddColumn("dbo.Teams", "CreatedDate", c => c.DateTime());
            CreateIndex("dbo.Players", "InGameName", unique: true);
            CreateIndex("dbo.Teams", "TeamName", unique: true);
            DropColumn("dbo.Players", "Nickname");
            DropColumn("dbo.Players", "FullName");
            DropColumn("dbo.Teams", "Name");
            DropColumn("dbo.Teams", "Country");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Teams", "Country", c => c.String());
            AddColumn("dbo.Teams", "Name", c => c.String());
            AddColumn("dbo.Players", "FullName", c => c.String());
            AddColumn("dbo.Players", "Nickname", c => c.String());
            DropForeignKey("dbo.AuditLogs", "UserId", "dbo.Users");
            DropForeignKey("dbo.Matches", "WinnerTeamId", "dbo.Teams");
            DropForeignKey("dbo.Matches", "TournamentId", "dbo.Tournaments");
            DropForeignKey("dbo.Matches", "Team2Id", "dbo.Teams");
            DropForeignKey("dbo.Matches", "Team1Id", "dbo.Teams");
            DropForeignKey("dbo.Matches", "RefereeId", "dbo.Users");
            DropForeignKey("dbo.Matches", "NextMatchId", "dbo.Matches");
            DropForeignKey("dbo.MatchMaps", "MVPlayerId", "dbo.Players");
            DropForeignKey("dbo.TournamentTeams", "TournamentId", "dbo.Tournaments");
            DropForeignKey("dbo.PrizePools", "TournamentId", "dbo.Tournaments");
            DropForeignKey("dbo.MapPools", "TournamentId", "dbo.Tournaments");
            DropForeignKey("dbo.Tournaments", "CreatedByUser_UserId", "dbo.Users");
            DropForeignKey("dbo.TournamentTeams", "TeamId", "dbo.Teams");
            DropForeignKey("dbo.PlayerStats", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.PlayerStats", "MatchMapId", "dbo.MatchMaps");
            DropForeignKey("dbo.MatchMaps", "MatchId", "dbo.Matches");
            DropForeignKey("dbo.AuditLogs", "Match_MatchId", "dbo.Matches");
            DropIndex("dbo.PrizePools", new[] { "TournamentId" });
            DropIndex("dbo.MapPools", new[] { "TournamentId" });
            DropIndex("dbo.Tournaments", new[] { "CreatedByUser_UserId" });
            DropIndex("dbo.TournamentTeams", new[] { "TeamId" });
            DropIndex("dbo.TournamentTeams", new[] { "TournamentId" });
            DropIndex("dbo.Teams", new[] { "TeamName" });
            DropIndex("dbo.PlayerStats", new[] { "PlayerId" });
            DropIndex("dbo.PlayerStats", new[] { "MatchMapId" });
            DropIndex("dbo.Players", new[] { "InGameName" });
            DropIndex("dbo.MatchMaps", new[] { "MVPlayerId" });
            DropIndex("dbo.MatchMaps", new[] { "MatchId" });
            DropIndex("dbo.Matches", new[] { "RefereeId" });
            DropIndex("dbo.Matches", new[] { "WinnerTeamId" });
            DropIndex("dbo.Matches", new[] { "NextMatchId" });
            DropIndex("dbo.Matches", new[] { "Team2Id" });
            DropIndex("dbo.Matches", new[] { "Team1Id" });
            DropIndex("dbo.Matches", "IX_Tournament_Round_Order");
            DropIndex("dbo.Users", new[] { "Username" });
            DropIndex("dbo.AuditLogs", new[] { "Match_MatchId" });
            DropIndex("dbo.AuditLogs", new[] { "UserId" });
            DropColumn("dbo.Teams", "CreatedDate");
            DropColumn("dbo.Teams", "LogoPath");
            DropColumn("dbo.Teams", "Acronym");
            DropColumn("dbo.Teams", "TeamName");
            DropColumn("dbo.Players", "IsActive");
            DropColumn("dbo.Players", "AvatarPath");
            DropColumn("dbo.Players", "Position");
            DropColumn("dbo.Players", "InGameName");
            DropColumn("dbo.Players", "RealName");
            DropTable("dbo.PrizePools");
            DropTable("dbo.MapPools");
            DropTable("dbo.Tournaments");
            DropTable("dbo.TournamentTeams");
            DropTable("dbo.PlayerStats");
            DropTable("dbo.MatchMaps");
            DropTable("dbo.Matches");
            DropTable("dbo.Users");
            DropTable("dbo.AuditLogs");
        }
    }
}
