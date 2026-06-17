using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("EsportsTournamentDb")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentTeam> TournamentTeams { get; set; }
        public DbSet<MapPool> MapPools { get; set; }
        public DbSet<PrizePool> PrizePools { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchMap> MatchMaps { get; set; }
        public DbSet<PlayerStat> PlayerStats { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User unique Username index
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Username") { IsUnique = true }));

            // 2. Team unique TeamName index
            modelBuilder.Entity<Team>()
                .Property(t => t.TeamName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_TeamName") { IsUnique = true }));

            // 3. Player unique InGameName index
            modelBuilder.Entity<Player>()
                .Property(p => p.InGameName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_InGameName") { IsUnique = true }));

            // 4. TournamentTeam composite key & relationships
            modelBuilder.Entity<TournamentTeam>()
                .HasKey(tt => new { tt.TournamentId, tt.TeamId });

            modelBuilder.Entity<TournamentTeam>()
                .HasRequired(tt => tt.Tournament)
                .WithMany(t => t.TournamentTeams)
                .HasForeignKey(tt => tt.TournamentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TournamentTeam>()
                .HasRequired(tt => tt.Team)
                .WithMany(t => t.TournamentTeams)
                .HasForeignKey(tt => tt.TeamId)
                .WillCascadeOnDelete(true);

            // 5. MapPool & PrizePool relationships
            modelBuilder.Entity<MapPool>()
                .HasRequired(mp => mp.Tournament)
                .WithMany(t => t.MapPools)
                .HasForeignKey(mp => mp.TournamentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PrizePool>()
                .HasRequired(pp => pp.Tournament)
                .WithMany(t => t.PrizePools)
                .HasForeignKey(pp => pp.TournamentId)
                .WillCascadeOnDelete(true);

            // 6. Match relationships and composite index
            modelBuilder.Entity<Match>()
                .HasRequired(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Match>()
                .HasOptional(m => m.Team1)
                .WithMany()
                .HasForeignKey(m => m.Team1Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Match>()
                .HasOptional(m => m.Team2)
                .WithMany()
                .HasForeignKey(m => m.Team2Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Match>()
                .HasOptional(m => m.WinnerTeam)
                .WithMany()
                .HasForeignKey(m => m.WinnerTeamId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Match>()
                .HasOptional(m => m.NextMatch)
                .WithMany()
                .HasForeignKey(m => m.NextMatchId)
                .WillCascadeOnDelete(false);

            // Composite Index on Matches (TournamentId, RoundNumber, MatchOrder)
            modelBuilder.Entity<Match>()
                .Property(m => m.TournamentId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Tournament_Round_Order", 1)));

            modelBuilder.Entity<Match>()
                .Property(m => m.RoundNumber)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Tournament_Round_Order", 2)));

            modelBuilder.Entity<Match>()
                .Property(m => m.MatchOrder)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Tournament_Round_Order", 3)));

            // 7. MatchMap relationships
            modelBuilder.Entity<MatchMap>()
                .HasRequired(mm => mm.Match)
                .WithMany(m => m.MatchMaps)
                .HasForeignKey(mm => mm.MatchId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<MatchMap>()
                .HasOptional(mm => mm.MVPlayer)
                .WithMany()
                .HasForeignKey(mm => mm.MVPlayerId)
                .WillCascadeOnDelete(false);

            // 8. PlayerStat relationships
            modelBuilder.Entity<PlayerStat>()
                .HasRequired(ps => ps.MatchMap)
                .WithMany(mm => mm.PlayerStats)
                .HasForeignKey(ps => ps.MatchMapId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PlayerStat>()
                .HasRequired(ps => ps.Player)
                .WithMany(p => p.PlayerStats)
                .HasForeignKey(ps => ps.PlayerId)
                .WillCascadeOnDelete(true);

            // 9. AuditLog relationship
            modelBuilder.Entity<AuditLog>()
                .HasRequired(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .WillCascadeOnDelete(true);
        }
    }
}