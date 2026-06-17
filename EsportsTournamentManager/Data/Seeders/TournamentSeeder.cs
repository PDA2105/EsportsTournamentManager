using System;
using System.Collections.Generic;
using System.Linq;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Data.Seeders
{
    public static class TournamentSeeder
    {
        public static void Seed(AppDbContext context)
        {
            var teams = context.Teams.ToList();
            if (teams.Count < 4) return; // Need all 4 teams seeded first

            var adminUser = context.Users.FirstOrDefault(u => u.Role == "Admin");
            int adminId = adminUser != null ? adminUser.UserId : 1;

            // 1. Seed Tournament 1: LCK Mùa Hè 2026 (Pending)
            if (!context.Tournaments.Any(t => t.Name == "LCK Mùa Hè 2026"))
            {
                var tour1 = new Tournament
                {
                    Name = "LCK Mùa Hè 2026",
                    GameType = "LoL",
                    Format = "SingleElimination",
                    MaxTeams = 4,
                    StartDate = DateTime.Today.AddDays(30),
                    Status = "Pending",
                    CreatedByUserId = adminId
                };
                context.Tournaments.Add(tour1);
                context.SaveChanges();

                // Assign all 4 teams
                foreach (var team in teams)
                {
                    context.TournamentTeams.Add(new TournamentTeam { TournamentId = tour1.TournamentId, TeamId = team.TeamId });
                }
                context.SaveChanges();
            }

            // 2. Seed Tournament 2: LCK Mùa Xuân 2026 (Active)
            if (!context.Tournaments.Any(t => t.Name == "LCK Mùa Xuân 2026"))
            {
                var tour2 = new Tournament
                {
                    Name = "LCK Mùa Xuân 2026",
                    GameType = "LoL",
                    Format = "SingleElimination",
                    MaxTeams = 4,
                    StartDate = DateTime.Today.AddDays(-5),
                    Status = "Active",
                    CreatedByUserId = adminId
                };
                context.Tournaments.Add(tour2);
                context.SaveChanges();

                // Assign teams
                foreach (var team in teams)
                {
                    context.TournamentTeams.Add(new TournamentTeam { TournamentId = tour2.TournamentId, TeamId = team.TeamId });
                }
                context.SaveChanges();

                // Create Matches
                // Round 2 (Final)
                var finalMatch = new Match
                {
                    TournamentId = tour2.TournamentId,
                    RoundNumber = 2,
                    MatchOrder = 1,
                    Status = "Scheduled",
                    ScheduledTime = tour2.StartDate.AddDays(2),
                    MatchFormat = "BO3"
                };
                context.Matches.Add(finalMatch);
                context.SaveChanges();

                // Round 1 (Semifinals)
                var m1 = new Match
                {
                    TournamentId = tour2.TournamentId,
                    RoundNumber = 1,
                    MatchOrder = 1,
                    Team1Id = teams[0].TeamId, // T1
                    Team2Id = teams[3].TeamId, // DK
                    Team1Score = 2,
                    Team2Score = 1,
                    WinnerTeamId = teams[0].TeamId,
                    Status = "Completed",
                    ScheduledTime = tour2.StartDate,
                    MatchFormat = "BO3",
                    NextMatchId = finalMatch.MatchId
                };

                var m2 = new Match
                {
                    TournamentId = tour2.TournamentId,
                    RoundNumber = 1,
                    MatchOrder = 2,
                    Team1Id = teams[1].TeamId, // GEN
                    Team2Id = teams[2].TeamId, // HLE
                    Team1Score = 0,
                    Team2Score = 0,
                    Status = "Scheduled",
                    ScheduledTime = tour2.StartDate.AddDays(1),
                    MatchFormat = "BO3",
                    NextMatchId = finalMatch.MatchId
                };

                context.Matches.Add(m1);
                context.Matches.Add(m2);
                context.SaveChanges();

                // Since m1 is completed and won by T1, set T1 as Team1Id for finalMatch
                finalMatch.Team1Id = teams[0].TeamId;
                context.SaveChanges();
            }

            // 3. Seed Tournament 3: LCK Chung Kết Thế Giới 2025 (Completed)
            if (!context.Tournaments.Any(t => t.Name == "LCK Chung Kết Thế Giới 2025"))
            {
                var tour3 = new Tournament
                {
                    Name = "LCK Chung Kết Thế Giới 2025",
                    GameType = "LoL",
                    Format = "SingleElimination",
                    MaxTeams = 4,
                    StartDate = DateTime.Today.AddDays(-60),
                    EndDate = DateTime.Today.AddDays(-55),
                    Status = "Completed",
                    CreatedByUserId = adminId
                };
                context.Tournaments.Add(tour3);
                context.SaveChanges();

                // Assign teams
                foreach (var team in teams)
                {
                    context.TournamentTeams.Add(new TournamentTeam { TournamentId = tour3.TournamentId, TeamId = team.TeamId });
                }
                context.SaveChanges();

                // Create Matches
                var finalMatch = new Match
                {
                    TournamentId = tour3.TournamentId,
                    RoundNumber = 2,
                    MatchOrder = 1,
                    Team1Id = teams[0].TeamId, // T1
                    Team2Id = teams[1].TeamId, // GEN
                    Team1Score = 3,
                    Team2Score = 2,
                    WinnerTeamId = teams[0].TeamId,
                    Status = "Completed",
                    ScheduledTime = tour3.StartDate.AddDays(5),
                    MatchFormat = "BO5"
                };
                context.Matches.Add(finalMatch);
                context.SaveChanges();

                var m1 = new Match
                {
                    TournamentId = tour3.TournamentId,
                    RoundNumber = 1,
                    MatchOrder = 1,
                    Team1Id = teams[0].TeamId, // T1
                    Team2Id = teams[3].TeamId, // DK
                    Team1Score = 2,
                    Team2Score = 0,
                    WinnerTeamId = teams[0].TeamId,
                    Status = "Completed",
                    ScheduledTime = tour3.StartDate,
                    MatchFormat = "BO3",
                    NextMatchId = finalMatch.MatchId
                };

                var m2 = new Match
                {
                    TournamentId = tour3.TournamentId,
                    RoundNumber = 1,
                    MatchOrder = 2,
                    Team1Id = teams[1].TeamId, // GEN
                    Team2Id = teams[2].TeamId, // HLE
                    Team1Score = 2,
                    Team2Score = 1,
                    WinnerTeamId = teams[1].TeamId,
                    Status = "Completed",
                    ScheduledTime = tour3.StartDate.AddDays(1),
                    MatchFormat = "BO3",
                    NextMatchId = finalMatch.MatchId
                };

                context.Matches.Add(m1);
                context.Matches.Add(m2);
                context.SaveChanges();
            }
        }
    }
}
