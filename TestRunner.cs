using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager
{
    class TestRunner
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("STARTING DOUBLE ELIMINATION AUTOMATED TESTS...");
            Console.WriteLine("==================================================");

            try
            {
                // Ensure database is initialized and seeded
                using (var db = new AppDbContext())
                {
                    db.Database.Initialize(force: true);
                    DatabaseSeeder.Seed(db);
                    Console.WriteLine("Database initialized and seeded.");
                }

                Test4TeamsDoubleElimination();
                Test8TeamsDoubleElimination();
                TestPlayerStatsAndAutomaticMvp();

                Console.WriteLine("\n==================================================");
                Console.WriteLine("ALL DOUBLE ELIMINATION TESTS PASSED SUCCESSFULLY!");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine("TEST FAILED WITH EXCEPTION:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==================================================");
                Environment.Exit(1);
            }
        }

        static void Test4TeamsDoubleElimination()
        {
            Console.WriteLine("\n--- Testing 4-Team Double Elimination ---");

            var service = new TournamentService();
            int tourId;

            // 1. Create tournament
            using (var db = new AppDbContext())
            {
                var adminUser = db.Users.First(u => u.Role == "Admin");
                var tour = new Tournament
                {
                    Name = "Test 4 Teams Double Elimination",
                    GameType = "LoL",
                    Format = "DoubleElimination",
                    MaxTeams = 4,
                    StartDate = DateTime.Today,
                    Status = "Pending",
                    CreatedByUserId = adminUser.UserId
                };
                db.Tournaments.Add(tour);
                db.SaveChanges();
                tourId = tour.TournamentId;

                // Assign 4 teams
                var teams = db.Teams.Take(4).ToList();
                foreach (var t in teams)
                {
                    db.TournamentTeams.Add(new TournamentTeam { TournamentId = tourId, TeamId = t.TeamId });
                }
                db.SaveChanges();
            }

            // 2. Start Tournament & check matches
            service.StartTournament(tourId);

            using (var db = new AppDbContext())
            {
                var matches = db.Matches.Where(m => m.TournamentId == tourId).ToList();
                // 4 teams double elimination has 6 matches:
                // Winner: 2 Semifinals (Round 1), 1 Final (Round 2), 1 Grand Final (Round 3)
                // Loser: 1 Semifinal (Round 1), 1 Final (Round 2)
                Assert(matches.Count == 6, $"Should have 6 matches, got {matches.Count}");

                var w1 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                var w3 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l1 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var l2 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var gf = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 3 && m.MatchOrder == 1);

                Assert(w1.Team1Id.HasValue && w1.Team2Id.HasValue, "W1 should have teams");
                Assert(w2.Team1Id.HasValue && w2.Team2Id.HasValue, "W2 should have teams");
                Assert(!w3.Team1Id.HasValue && !w3.Team2Id.HasValue, "W3 should not have teams yet");
                Assert(!l1.Team1Id.HasValue && !l1.Team2Id.HasValue, "L1 should not have teams yet");

                Assert(w1.NextMatchId == w3.MatchId, "W1 NextMatchId should point to W3");
                Assert(w2.NextMatchId == w3.MatchId, "W2 NextMatchId should point to W3");
                Assert(l1.NextMatchId == l2.MatchId, "L1 NextMatchId should point to L2");
                Assert(w3.NextMatchId == gf.MatchId, "W3 NextMatchId should point to GF");
                Assert(l2.NextMatchId == gf.MatchId, "L2 NextMatchId should point to GF");

                Console.WriteLine("Generation and linking verified.");
            }

            // 3. Complete Winner Round 1
            int w1WinnerId, w1LoserId, w2WinnerId, w2LoserId;
            using (var db = new AppDbContext())
            {
                var matches = db.Matches.Where(m => m.TournamentId == tourId).ToList();
                var w1 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);

                w1WinnerId = w1.Team1Id.Value;
                w1LoserId = w1.Team2Id.Value;
                w2WinnerId = w2.Team1Id.Value;
                w2LoserId = w2.Team2Id.Value;
            }

            service.UpdateMatchResult(
                dbMatchesId("Winner", 1, 1, tourId), 2, 1, "Completed"
            );
            service.UpdateMatchResult(
                dbMatchesId("Winner", 1, 2, tourId), 2, 1, "Completed"
            );

            // Verify W3 and L1
            using (var db = new AppDbContext())
            {
                var matches = db.Matches.Where(m => m.TournamentId == tourId).ToList();
                var w3 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l1 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);

                Assert(w3.Team1Id == w1WinnerId, "Winner of W1 should go to W3 Team 1");
                Assert(w3.Team2Id == w2WinnerId, "Winner of W2 should go to W3 Team 2");
                Assert(l1.Team1Id == w1LoserId, "Loser of W1 should go to L1 Team 1");
                Assert(l1.Team2Id == w2LoserId, "Loser of W2 should go to L1 Team 2");

                Console.WriteLine("Winner and Loser advancement from Round 1 verified.");
            }

            // 4. Test Rollback W1
            service.RollbackMatchResult(dbMatchesId("Winner", 1, 1, tourId));

            using (var db = new AppDbContext())
            {
                var matches = db.Matches.Where(m => m.TournamentId == tourId).ToList();
                var w3 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l1 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);

                Assert(!w3.Team1Id.HasValue, "W3 Team 1 should be cleared after W1 rollback");
                Assert(!l1.Team1Id.HasValue, "L1 Team 1 should be cleared after W1 rollback");

                Console.WriteLine("Rollback of W1 verified.");
            }
        }

        static void Test8TeamsDoubleElimination()
        {
            Console.WriteLine("\n--- Testing 8-Team Double Elimination ---");

            var service = new TournamentService();
            int tourId;

            // 1. Create tournament
            using (var db = new AppDbContext())
            {
                var adminUser = db.Users.First(u => u.Role == "Admin");
                var tour = new Tournament
                {
                    Name = "Test 8 Teams Double Elimination",
                    GameType = "LoL",
                    Format = "DoubleElimination",
                    MaxTeams = 8,
                    StartDate = DateTime.Today,
                    Status = "Pending",
                    CreatedByUserId = adminUser.UserId
                };
                db.Tournaments.Add(tour);
                db.SaveChanges();
                tourId = tour.TournamentId;

                // Assign 8 teams (if we don't have 8, seed some extra first)
                var teams = db.Teams.ToList();
                if (teams.Count < 8)
                {
                    for (int i = teams.Count + 1; i <= 8; i++)
                    {
                        var t = new Team { TeamName = "Team Extra " + i, Acronym = "TE" + i };
                        db.Teams.Add(t);
                    }
                    db.SaveChanges();
                    teams = db.Teams.ToList();
                }

                foreach (var t in teams.Take(8))
                {
                    db.TournamentTeams.Add(new TournamentTeam { TournamentId = tourId, TeamId = t.TeamId });
                }
                db.SaveChanges();
            }

            // 2. Start Tournament & check matches
            service.StartTournament(tourId);

            using (var db = new AppDbContext())
            {
                var matches = db.Matches.Where(m => m.TournamentId == tourId).ToList();
                // 8 teams double elimination has 14 matches:
                // Winner: 4 in R1, 2 in R2, 1 in R3, 1 GF in R4
                // Loser: 2 in R1, 2 in R2, 1 in R3, 1 in R4
                Assert(matches.Count == 14, $"Should have 14 matches, got {matches.Count}");

                var w1 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                var w3 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 3);
                var w4 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 4);

                var w5 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var w6 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 2);
                var w7 = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 3 && m.MatchOrder == 1);

                var l1 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var l2 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 2);

                var l3 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l4 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 2);

                var l5 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 3 && m.MatchOrder == 1);
                var l6 = matches.First(m => m.BracketBranch == "Loser" && m.RoundNumber == 4 && m.MatchOrder == 1);

                var gf = matches.First(m => m.BracketBranch == "Winner" && m.RoundNumber == 4 && m.MatchOrder == 1);

                Assert(w1.NextMatchId == w5.MatchId, "W1 NextMatchId -> W5");
                Assert(w2.NextMatchId == w5.MatchId, "W2 NextMatchId -> W5");
                Assert(w3.NextMatchId == w6.MatchId, "W3 NextMatchId -> W6");
                Assert(w4.NextMatchId == w6.MatchId, "W4 NextMatchId -> W6");

                Assert(w5.NextMatchId == w7.MatchId, "W5 NextMatchId -> W7");
                Assert(w6.NextMatchId == w7.MatchId, "W6 NextMatchId -> W7");

                Assert(l1.NextMatchId == l3.MatchId, "L1 NextMatchId -> L3");
                Assert(l2.NextMatchId == l4.MatchId, "L2 NextMatchId -> L4");

                Assert(l3.NextMatchId == l5.MatchId, "L3 NextMatchId -> L5");
                Assert(l4.NextMatchId == l5.MatchId, "L4 NextMatchId -> L5");

                Assert(l5.NextMatchId == l6.MatchId, "L5 NextMatchId -> L6");

                Assert(w7.NextMatchId == gf.MatchId, "W7 NextMatchId -> GF");
                Assert(l6.NextMatchId == gf.MatchId, "L6 NextMatchId -> GF");

                Console.WriteLine("8-Team Generation and linking verified.");
            }

            // 3. Complete Winner Round 1
            int w1WinnerId, w1LoserId, w2WinnerId, w2LoserId;
            using (var db = new AppDbContext())
            {
                var w1 = db.Matches.First(m => m.TournamentId == tourId && m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = db.Matches.First(m => m.TournamentId == tourId && m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                w1WinnerId = w1.Team1Id.Value;
                w1LoserId = w1.Team2Id.Value;
                w2WinnerId = w2.Team1Id.Value;
                w2LoserId = w2.Team2Id.Value;
            }

            service.UpdateMatchResult(dbMatchesId("Winner", 1, 1, tourId), 2, 0, "Completed");
            service.UpdateMatchResult(dbMatchesId("Winner", 1, 2, tourId), 2, 0, "Completed");

            using (var db = new AppDbContext())
            {
                var w5 = db.Matches.First(m => m.TournamentId == tourId && m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l1 = db.Matches.First(m => m.TournamentId == tourId && m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);

                Assert(w5.Team1Id == w1WinnerId, "Winner of W1 -> W5 Team 1");
                Assert(w5.Team2Id == w2WinnerId, "Winner of W2 -> W5 Team 2");
                Assert(l1.Team1Id == w1LoserId, "Loser of W1 -> L1 Team 1");
                Assert(l1.Team2Id == w2LoserId, "Loser of W2 -> L1 Team 2");

                Console.WriteLine("Winner and Loser advancement from Round 1 verified.");
            }
        }

        static int dbMatchesId(string branch, int round, int order, int tourId)
        {
            using (var db = new AppDbContext())
            {
                return db.Matches.First(m => 
                    m.TournamentId == tourId && 
                    m.BracketBranch == branch && 
                    m.RoundNumber == round && 
                    m.MatchOrder == order).MatchId;
            }
        }

        static void TestPlayerStatsAndAutomaticMvp()
        {
            Console.WriteLine("\n--- Testing Player Stats and Automatic MVP ---");

            var service = new TournamentService();
            int tourId;

            // Create tournament
            using (var db = new AppDbContext())
            {
                var adminUser = db.Users.First(u => u.Role == "Admin");
                var tour = new Tournament
                {
                    Name = "Test MVP Tournament",
                    GameType = "LoL",
                    Format = "SingleElimination",
                    MaxTeams = 4,
                    StartDate = DateTime.Today,
                    Status = "Pending",
                    CreatedByUserId = adminUser.UserId
                };
                db.Tournaments.Add(tour);
                db.SaveChanges();
                tourId = tour.TournamentId;

                var teams = db.Teams.Take(4).ToList();
                foreach (var t in teams)
                {
                    db.TournamentTeams.Add(new TournamentTeam { TournamentId = tourId, TeamId = t.TeamId });
                }
                db.SaveChanges();
            }

            service.StartTournament(tourId);

            int matchId;
            int player1Id, player2Id;
            using (var db = new AppDbContext())
            {
                var match = db.Matches.First(m => m.TournamentId == tourId && m.RoundNumber == 1 && m.MatchOrder == 1);
                matchId = match.MatchId;

                // Load players for Team 1 and Team 2
                var team1Players = db.Players.Where(p => p.TeamId == match.Team1Id).ToList();
                var team2Players = db.Players.Where(p => p.TeamId == match.Team2Id).ToList();

                player1Id = team1Players[0].PlayerId;
                player2Id = team2Players[0].PlayerId;
            }

            // Create map 1: Team 1 wins (13-11)
            // Player 1 (Team 1): 82 PTS (Winner MVP)
            // Player 2 (Team 2): 14 PTS (Loser MVP)
            var map1 = new MatchMap
            {
                MapNumber = 1,
                SelectedMapName = "Map 1 Test",
                Team1RoundScore = 13,
                Team2RoundScore = 11,
                PlayerStats = new List<PlayerStat>
                {
                    new PlayerStat { PlayerId = player1Id, Kills = 10, Deaths = 2, Assists = 5, DamageDealt = 25000, CreepScore = 200 },
                    new PlayerStat { PlayerId = player2Id, Kills = 2, Deaths = 8, Assists = 1, DamageDealt = 8000, CreepScore = 100 }
                }
            };

            // Create map 2: Team 1 wins (13-5)
            // Player 1 (Team 1): 37 PTS (Winner MVP)
            // Player 2 (Team 2): 177 PTS (Loser MVP & Match MVP candidate)
            var map2 = new MatchMap
            {
                MapNumber = 2,
                SelectedMapName = "Map 2 Test",
                Team1RoundScore = 13,
                Team2RoundScore = 5,
                PlayerStats = new List<PlayerStat>
                {
                    new PlayerStat { PlayerId = player1Id, Kills = 5, Deaths = 4, Assists = 2, DamageDealt = 12000, CreepScore = 120 },
                    new PlayerStat { PlayerId = player2Id, Kills = 30, Deaths = 2, Assists = 10, DamageDealt = 40000, CreepScore = 300 }
                }
            };

            var mapsInput = new List<MatchMap> { map1, map2 };
            service.SaveMatchPerformance(matchId, mapsInput, "Completed");

            // Verify MVP and scores
            using (var db = new AppDbContext())
            {
                var savedMatch = db.Matches
                    .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats))
                    .First(m => m.MatchId == matchId);

                Assert(savedMatch.Team1Score == 2 && savedMatch.Team2Score == 0, $"Match score should be 2-0, got {savedMatch.Team1Score}-{savedMatch.Team2Score}");
                Assert(savedMatch.Status == "Completed", "Match status should be Completed");

                var savedMap1 = savedMatch.MatchMaps.First(m => m.MapNumber == 1);
                Assert(savedMap1.MVPlayerId == player1Id, "Map 1 Winner MVP should be Player 1");

                var stat1Map1 = savedMap1.PlayerStats.First(ps => ps.PlayerId == player1Id);
                Assert(stat1Map1.IsMvpOfMap, "Player 1 IsMvpOfMap should be true on Map 1");

                var stat2Map1 = savedMap1.PlayerStats.First(ps => ps.PlayerId == player2Id);
                Assert(stat2Map1.IsMvpOfMap, "Player 2 IsMvpOfMap should be true (Loser MVP) on Map 1");

                var savedMap2 = savedMatch.MatchMaps.First(m => m.MapNumber == 2);
                Assert(savedMap2.MVPlayerId == player1Id, "Map 2 Winner MVP should be Player 1");

                var stat1Map2 = savedMap2.PlayerStats.First(ps => ps.PlayerId == player1Id);
                Assert(stat1Map2.IsMvpOfMap, "Player 1 IsMvpOfMap should be true on Map 2");

                var stat2Map2 = savedMap2.PlayerStats.First(ps => ps.PlayerId == player2Id);
                Assert(stat2Map2.IsMvpOfMap, "Player 2 IsMvpOfMap should be true (Loser MVP) on Map 2");

                // Player 1 Match Average = (82.0 + 37.0) / 2 = 59.5
                // Player 2 Match Average = (14.0 + 177.0) / 2 = 95.5
                // So Player 2 (losing team) should be Match MVP!
                double matchMvpAvg;
                var matchMvp = service.GetMatchMvp(matchId, out matchMvpAvg);
                Assert(matchMvp != null && matchMvp.PlayerId == player2Id, $"Match MVP should be Player 2, got player ID {(matchMvp != null ? matchMvp.PlayerId : -1)}");
                Assert(matchMvpAvg == 95.5, $"Match MVP average score should be 95.5, got {matchMvpAvg}");

                // Tournament MVP Score = Total Match Average / maxPossibleMatches
                // Since there is only 1 match played by both:
                // Player 1 Tournament Score = 59.5 / 2 = 29.75
                // Player 2 Tournament Score = 95.5 / 2 = 47.75
                // Tournament MVP should be Player 2!
                double tourMvpAvg;
                var tourMvp = service.GetTournamentMvp(tourId, out tourMvpAvg);
                Assert(tourMvp != null && tourMvp.PlayerId == player2Id, $"Tournament MVP should be Player 2, got player ID {(tourMvp != null ? tourMvp.PlayerId : -1)}");
                Assert(tourMvpAvg == 47.75, $"Tournament MVP average score should be 47.75, got {tourMvpAvg}");

                Console.WriteLine("Player Stats and Automatic MVP verified successfully!");
            }
        }

        static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("ASSERTION FAILED: " + message);
            }
        }
    }
}
