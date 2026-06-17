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
            var tour1 = context.Tournaments.FirstOrDefault(t => t.Name == "LCK Mùa Hè 2026");
            if (tour1 == null)
            {
                tour1 = new Tournament
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
            SeedMapPoolsAndPrizePools(context, tour1.TournamentId, 200000000m); // 200M VND

            // 2. Seed Tournament 2: LCK Mùa Xuân 2026 (Active)
            var tour2 = context.Tournaments.FirstOrDefault(t => t.Name == "LCK Mùa Xuân 2026");
            if (tour2 == null)
            {
                tour2 = new Tournament
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

                // Seed MatchMaps and PlayerStats for m1
                CreateMatchMap(context, m1, 1, "Summoner's Rift", 1, 0, "Faker");
                CreateMatchMap(context, m1, 2, "Summoner's Rift", 0, 1, "ShowMaker");
                CreateMatchMap(context, m1, 3, "Summoner's Rift", 1, 0, "Gumayusi");
            }
            else
            {
                // In case it already exists but we want to make sure matches are there
                var m1 = context.Matches.FirstOrDefault(m => m.TournamentId == tour2.TournamentId && m.RoundNumber == 1 && m.MatchOrder == 1);
                if (m1 != null)
                {
                    CreateMatchMap(context, m1, 1, "Summoner's Rift", 1, 0, "Faker");
                    CreateMatchMap(context, m1, 2, "Summoner's Rift", 0, 1, "ShowMaker");
                    CreateMatchMap(context, m1, 3, "Summoner's Rift", 1, 0, "Gumayusi");
                }
            }
            SeedMapPoolsAndPrizePools(context, tour2.TournamentId, 500000000m); // 500M VND

            // 3. Seed Tournament 3: LCK Chung Kết Thế Giới 2025 (Completed)
            var tour3 = context.Tournaments.FirstOrDefault(t => t.Name == "LCK Chung Kết Thế Giới 2025");
            if (tour3 == null)
            {
                tour3 = new Tournament
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

                // Seed MatchMaps and PlayerStats for tour3 matches
                // m1 (T1 vs DK, 2-0)
                CreateMatchMap(context, m1, 1, "Summoner's Rift", 1, 0, "Oner");
                CreateMatchMap(context, m1, 2, "Summoner's Rift", 1, 0, "Zeus");

                // m2 (GEN vs HLE, 2-1)
                CreateMatchMap(context, m2, 1, "Summoner's Rift", 1, 0, "Chovy");
                CreateMatchMap(context, m2, 2, "Summoner's Rift", 0, 1, "Zeka");
                CreateMatchMap(context, m2, 3, "Summoner's Rift", 1, 0, "Peyz");

                // finalMatch (T1 vs GEN, 3-2)
                CreateMatchMap(context, finalMatch, 1, "Summoner's Rift", 1, 0, "Faker");
                CreateMatchMap(context, finalMatch, 2, "Summoner's Rift", 0, 1, "Canyon");
                CreateMatchMap(context, finalMatch, 3, "Summoner's Rift", 1, 0, "Keria");
                CreateMatchMap(context, finalMatch, 4, "Summoner's Rift", 0, 1, "Kiin");
                CreateMatchMap(context, finalMatch, 5, "Summoner's Rift", 1, 0, "Gumayusi");
            }
            else
            {
                // In case it already exists but we want to make sure matches are populated with maps and stats
                var m1 = context.Matches.FirstOrDefault(m => m.TournamentId == tour3.TournamentId && m.RoundNumber == 1 && m.MatchOrder == 1);
                if (m1 != null)
                {
                    CreateMatchMap(context, m1, 1, "Summoner's Rift", 1, 0, "Oner");
                    CreateMatchMap(context, m1, 2, "Summoner's Rift", 1, 0, "Zeus");
                }
                var m2 = context.Matches.FirstOrDefault(m => m.TournamentId == tour3.TournamentId && m.RoundNumber == 1 && m.MatchOrder == 2);
                if (m2 != null)
                {
                    CreateMatchMap(context, m2, 1, "Summoner's Rift", 1, 0, "Chovy");
                    CreateMatchMap(context, m2, 2, "Summoner's Rift", 0, 1, "Zeka");
                    CreateMatchMap(context, m2, 3, "Summoner's Rift", 1, 0, "Peyz");
                }
                var finalMatch = context.Matches.FirstOrDefault(m => m.TournamentId == tour3.TournamentId && m.RoundNumber == 2 && m.MatchOrder == 1);
                if (finalMatch != null)
                {
                    CreateMatchMap(context, finalMatch, 1, "Summoner's Rift", 1, 0, "Faker");
                    CreateMatchMap(context, finalMatch, 2, "Summoner's Rift", 0, 1, "Canyon");
                    CreateMatchMap(context, finalMatch, 3, "Summoner's Rift", 1, 0, "Keria");
                    CreateMatchMap(context, finalMatch, 4, "Summoner's Rift", 0, 1, "Kiin");
                    CreateMatchMap(context, finalMatch, 5, "Summoner's Rift", 1, 0, "Gumayusi");
                }
            }
            SeedMapPoolsAndPrizePools(context, tour3.TournamentId, 1000000000m); // 1B VND
        }

        private static void SeedMapPoolsAndPrizePools(AppDbContext context, int tournamentId, decimal totalPrize)
        {
            if (!context.MapPools.Any(mp => mp.TournamentId == tournamentId))
            {
                var maps = new[] { "Summoner's Rift", "Howling Abyss", "The Crystal Scar", "Twisted Treeline" };
                foreach (var m in maps)
                {
                    context.MapPools.Add(new MapPool { TournamentId = tournamentId, MapName = m });
                }
            }

            if (!context.PrizePools.Any(pp => pp.TournamentId == tournamentId))
            {
                context.PrizePools.Add(new PrizePool { TournamentId = tournamentId, RankPlace = 1, PrizeAmount = totalPrize * 0.5m, OtherRewards = "Cúp vô địch + Huy chương Vàng" });
                context.PrizePools.Add(new PrizePool { TournamentId = tournamentId, RankPlace = 2, PrizeAmount = totalPrize * 0.3m, OtherRewards = "Huy chương Bạc" });
                context.PrizePools.Add(new PrizePool { TournamentId = tournamentId, RankPlace = 3, PrizeAmount = totalPrize * 0.1m, OtherRewards = "Huy chương Đồng" });
                context.PrizePools.Add(new PrizePool { TournamentId = tournamentId, RankPlace = 4, PrizeAmount = totalPrize * 0.1m, OtherRewards = "Huy chương Đồng" });
            }
            context.SaveChanges();
        }

        private static void CreateMatchMap(AppDbContext context, Match match, int mapNumber, string mapName, int team1RoundScore, int team2RoundScore, string mvpInGameName)
        {
            // Check if already exists to avoid duplicates
            if (context.MatchMaps.Any(mm => mm.MatchId == match.MatchId && mm.MapNumber == mapNumber))
                return;

            // Load players for both teams
            var team1Players = context.Players.Where(p => p.TeamId == match.Team1Id).ToList();
            var team2Players = context.Players.Where(p => p.TeamId == match.Team2Id).ToList();

            Player mvpPlayer = null;
            if (!string.IsNullOrEmpty(mvpInGameName))
            {
                mvpPlayer = team1Players.FirstOrDefault(p => p.InGameName == mvpInGameName)
                         ?? team2Players.FirstOrDefault(p => p.InGameName == mvpInGameName);
            }

            var map = new MatchMap
            {
                MatchId = match.MatchId,
                MapNumber = mapNumber,
                SelectedMapName = mapName,
                Team1RoundScore = team1RoundScore,
                Team2RoundScore = team2RoundScore,
                DurationSeconds = 1800 + (new Random(match.MatchId + mapNumber).Next(0, 600)), // 30-40 mins
                MVPlayerId = mvpPlayer?.PlayerId
            };

            context.MatchMaps.Add(map);
            context.SaveChanges(); // Get map ID

            var rng = new Random(map.MatchMapId);

            // Create player stats for Team 1
            foreach (var player in team1Players)
            {
                bool isMvp = mvpPlayer != null && player.PlayerId == mvpPlayer.PlayerId;
                context.PlayerStats.Add(new PlayerStat
                {
                    MatchMapId = map.MatchMapId,
                    PlayerId = player.PlayerId,
                    Kills = isMvp ? rng.Next(5, 12) : rng.Next(1, 8),
                    Deaths = isMvp ? rng.Next(0, 3) : rng.Next(1, 6),
                    Assists = isMvp ? rng.Next(8, 15) : rng.Next(2, 12),
                    DamageDealt = rng.Next(10000, 30000),
                    CreepScore = player.Position == "Support" ? rng.Next(20, 60) : rng.Next(180, 320),
                    IsMvpOfMap = isMvp
                });
            }

            // Create player stats for Team 2
            foreach (var player in team2Players)
            {
                bool isMvp = mvpPlayer != null && player.PlayerId == mvpPlayer.PlayerId;
                context.PlayerStats.Add(new PlayerStat
                {
                    MatchMapId = map.MatchMapId,
                    PlayerId = player.PlayerId,
                    Kills = isMvp ? rng.Next(5, 12) : rng.Next(0, 5),
                    Deaths = isMvp ? rng.Next(0, 3) : rng.Next(2, 8),
                    Assists = isMvp ? rng.Next(8, 15) : rng.Next(1, 10),
                    DamageDealt = rng.Next(8000, 25000),
                    CreepScore = player.Position == "Support" ? rng.Next(20, 60) : rng.Next(160, 300),
                    IsMvpOfMap = isMvp
                });
            }
            context.SaveChanges();
        }
    }
}
