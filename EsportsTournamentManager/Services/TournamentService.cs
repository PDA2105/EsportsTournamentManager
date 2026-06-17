using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    public class TournamentService
    {
        public List<Tournament> GetAllTournaments()
        {
            using (var db = new AppDbContext())
            {
                return db.Tournaments
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.TournamentTeams.Select(tt => tt.Team))
                    .ToList();
            }
        }

        public Tournament GetTournamentById(int tournamentId)
        {
            using (var db = new AppDbContext())
            {
                return db.Tournaments
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.TournamentTeams.Select(tt => tt.Team))
                    .Include(t => t.Matches.Select(m => m.Team1))
                    .Include(t => t.Matches.Select(m => m.Team2))
                    .Include(t => t.Matches.Select(m => m.WinnerTeam))
                    .FirstOrDefault(t => t.TournamentId == tournamentId);
            }
        }

        public void AddTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Tournaments.Add(tournament);
                db.SaveChanges();
            }
        }

        public void UpdateTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void DeleteTournament(int tournamentId)
        {
            using (var db = new AppDbContext())
            {
                var tournament = db.Tournaments.Find(tournamentId);
                if (tournament != null)
                {
                    db.Tournaments.Remove(tournament);
                    db.SaveChanges();
                }
            }
        }

        public void SaveTournamentTeams(int tournamentId, List<int> teamIds)
        {
            using (var db = new AppDbContext())
            {
                // Remove existing mappings
                var existing = db.TournamentTeams.Where(tt => tt.TournamentId == tournamentId).ToList();
                db.TournamentTeams.RemoveRange(existing);

                // Add new mappings
                foreach (var teamId in teamIds)
                {
                    db.TournamentTeams.Add(new TournamentTeam
                    {
                        TournamentId = tournamentId,
                        TeamId = teamId
                    });
                }
                db.SaveChanges();
            }
        }

        public void StartTournament(int tournamentId)
        {
            using (var db = new AppDbContext())
            {
                var tournament = db.Tournaments
                    .Include(t => t.TournamentTeams.Select(tt => tt.Team))
                    .FirstOrDefault(t => t.TournamentId == tournamentId);

                if (tournament == null)
                    throw new Exception("Không tìm thấy giải đấu.");

                if (tournament.Status != "Pending")
                    throw new Exception("Giải đấu đã bắt đầu hoặc hoàn thành.");

                int teamCount = tournament.TournamentTeams.Count;
                if (tournament.Format == "SingleElimination")
                {
                    if (teamCount != 4 && teamCount != 8 && teamCount != 16)
                    {
                        throw new Exception("Thể thức Loại trực tiếp yêu cầu số đội tham gia phải là 4, 8 hoặc 16 đội.");
                    }
                    GenerateSingleEliminationBracket(db, tournament);
                }
                else if (tournament.Format == "DoubleElimination")
                {
                    if (teamCount != 4 && teamCount != 8)
                    {
                        throw new Exception("Thể thức Nhánh thắng nhánh thua yêu cầu số đội tham gia phải là 4 hoặc 8 đội.");
                    }
                    GenerateDoubleEliminationBracket(db, tournament);
                }
                else if (tournament.Format == "RoundRobin")
                {
                    if (teamCount < 2)
                    {
                        throw new Exception("Thể thức Vòng tròn tính điểm yêu cầu tối thiểu 2 đội tham gia.");
                    }
                    GenerateRoundRobinBracket(db, tournament);
                }
                else
                {
                    throw new Exception("Thể thức giải đấu không được hỗ trợ.");
                }

                tournament.MaxTeams = teamCount;
                tournament.Status = "Active";
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private void GenerateSingleEliminationBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            int N = teams.Count;
            int numRounds = (int)Math.Log(N, 2);

            // Structure to hold matches in memory by round (1-indexed)
            List<Match>[] matchesByRound = new List<Match>[numRounds + 1];
            for (int r = 1; r <= numRounds; r++)
            {
                matchesByRound[r] = new List<Match>();
            }

            // Create matches from final round (R) down to 1
            for (int r = numRounds; r >= 1; r--)
            {
                int numMatchesInRound = (int)Math.Pow(2, numRounds - r);
                for (int i = 0; i < numMatchesInRound; i++)
                {
                    var match = new Match
                    {
                        TournamentId = tournament.TournamentId,
                        RoundNumber = r,
                        MatchOrder = i + 1,
                        Status = "Scheduled",
                        ScheduledTime = tournament.StartDate.AddDays(r - 1),
                        MatchFormat = "BO3"
                    };

                    if (r < numRounds)
                    {
                        int nextMatchIndex = i / 2;
                        match.NextMatch = matchesByRound[r + 1][nextMatchIndex];
                    }

                    matchesByRound[r].Add(match);
                }
            }

            // Shuffle and pair teams for Round 1
            var rng = new Random();
            var shuffledTeams = teams.OrderBy(t => rng.Next()).ToList();
            for (int i = 0; i < matchesByRound[1].Count; i++)
            {
                var match = matchesByRound[1][i];
                match.Team1Id = shuffledTeams[2 * i].TeamId;
                match.Team2Id = shuffledTeams[2 * i + 1].TeamId;
            }

            // Save all matches to DB
            for (int r = numRounds; r >= 1; r--)
            {
                foreach (var match in matchesByRound[r])
                {
                    db.Matches.Add(match);
                }
            }
        }

        private void GenerateRoundRobinBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            var teamList = teams.ToList();

            // If odd, add a dummy team for "bye"
            bool hasBye = teamList.Count % 2 != 0;
            if (hasBye)
            {
                teamList.Add(null);
            }

            int numTeams = teamList.Count;
            int numRounds = numTeams - 1;
            int matchesPerRound = numTeams / 2;
            var rng = new Random();
            teamList = teamList.OrderBy(t => t == null ? int.MaxValue : rng.Next()).ToList();

            for (int round = 0; round < numRounds; round++)
            {
                for (int matchIdx = 0; matchIdx < matchesPerRound; matchIdx++)
                {
                    int home = (round + matchIdx) % (numTeams - 1);
                    int away = (numTeams - 1 - matchIdx + round) % (numTeams - 1);

                    if (matchIdx == 0)
                    {
                        away = numTeams - 1;
                    }

                    var t1 = teamList[home];
                    var t2 = teamList[away];

                    if (t1 != null && t2 != null)
                    {
                        var match = new Match
                        {
                            TournamentId = tournament.TournamentId,
                            RoundNumber = round + 1,
                            MatchOrder = matchIdx + 1,
                            Team1Id = t1.TeamId,
                            Team2Id = t2.TeamId,
                            Status = "Scheduled",
                            ScheduledTime = tournament.StartDate.AddDays(round),
                            MatchFormat = "BO3"
                        };
                        db.Matches.Add(match);
                    }
                }
            }
        }

        public void UpdateMatchResult(int matchId, int team1Score, int team2Score, string status, int? mvpPlayerId = null)
        {
            using (var db = new AppDbContext())
            {
                var match = db.Matches
                    .Include(m => m.Tournament)
                    .FirstOrDefault(m => m.MatchId == matchId);

                if (match == null)
                    throw new Exception("Không tìm thấy trận đấu.");

                if (match.Status == "Cancelled")
                    throw new Exception("Trận đấu đã bị hủy bỏ.");

                match.Team1Score = team1Score;
                match.Team2Score = team2Score;
                match.Status = status;

                if (status == "Completed")
                {
                    if (team1Score == team2Score)
                        throw new Exception("Trận đấu không thể có tỉ số hòa khi ở trạng thái hoàn thành.");

                    int winnerId = team1Score > team2Score ? match.Team1Id.Value : match.Team2Id.Value;
                    match.WinnerTeamId = winnerId;

                    // If it is Single Elimination, advance the winner
                    if (match.Tournament.Format == "SingleElimination" && match.NextMatchId.HasValue)
                    {
                        var nextMatch = db.Matches.Find(match.NextMatchId.Value);
                        if (nextMatch != null)
                        {
                            if (match.MatchOrder % 2 != 0)
                            {
                                nextMatch.Team1Id = winnerId;
                            }
                            else
                            {
                                nextMatch.Team2Id = winnerId;
                            }
                            db.Entry(nextMatch).State = EntityState.Modified;
                        }
                    }
                    else if (match.Tournament.Format == "DoubleElimination" && match.NextMatchId.HasValue)
                    {
                        var nextMatch = db.Matches.Find(match.NextMatchId.Value);
                        if (nextMatch != null)
                        {
                            // If next match is the Grand Final
                            if (nextMatch.RoundNumber == (match.Tournament.MaxTeams == 4 ? 3 : 4))
                            {
                                if (match.BracketBranch == "Winner")
                                    nextMatch.Team1Id = winnerId;
                                else
                                    nextMatch.Team2Id = winnerId;
                            }
                            else
                            {
                                if (match.MatchOrder % 2 != 0)
                                    nextMatch.Team1Id = winnerId;
                                else
                                    nextMatch.Team2Id = winnerId;
                            }
                            db.Entry(nextMatch).State = EntityState.Modified;
                        }

                        // Advance the loser to the loser's bracket
                        if (match.BracketBranch == "Winner")
                        {
                            var loserMatch = FindLoserDestinationMatch(db, match);
                            if (loserMatch != null)
                            {
                                int loserId = winnerId == match.Team1Id ? match.Team2Id.Value : match.Team1Id.Value;
                                SetLoserInMatch(loserMatch, match, loserId);
                                db.Entry(loserMatch).State = EntityState.Modified;
                            }
                        }
                    }



                    // Check if tournament is completed (Single / Double Elimination: final round match complete)
                    if ((match.Tournament.Format == "SingleElimination" || match.Tournament.Format == "DoubleElimination") && !match.NextMatchId.HasValue)
                    {
                        var tour = db.Tournaments.Find(match.TournamentId);
                        if (tour != null)
                        {
                            tour.Status = "Completed";
                            tour.EndDate = DateTime.Now;
                            db.Entry(tour).State = EntityState.Modified;
                        }
                    }
                    // For Round Robin: check if all matches are completed
                    else if (match.Tournament.Format == "RoundRobin")
                    {
                        bool allCompleted = !db.Matches.Any(m => m.TournamentId == match.TournamentId && m.MatchId != matchId && m.Status != "Completed");
                        if (allCompleted)
                        {
                            var tour = db.Tournaments.Find(match.TournamentId);
                            if (tour != null)
                            {
                                tour.Status = "Completed";
                                tour.EndDate = DateTime.Now;
                                db.Entry(tour).State = EntityState.Modified;
                            }
                        }
                    }
                }
                else
                {
                    // If moving back to Live/Scheduled, clear winner
                    match.WinnerTeamId = null;
                }

                db.Entry(match).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void RollbackMatchResult(int matchId)
        {
            using (var db = new AppDbContext())
            {
                var match = db.Matches
                    .Include(m => m.Tournament)
                    .FirstOrDefault(m => m.MatchId == matchId);

                if (match == null)
                    throw new Exception("Không tìm thấy trận đấu.");

                if (match.Status != "Completed")
                    return; // Already not completed, no rollback needed

                int? prevWinnerId = match.WinnerTeamId;

                // Reset this match
                match.Team1Score = 0;
                match.Team2Score = 0;
                match.WinnerTeamId = null;
                match.Status = "Scheduled";
                db.Entry(match).State = EntityState.Modified;

                // Remove MVP from maps
                var maps = db.MatchMaps.Where(mm => mm.MatchId == matchId).ToList();
                db.MatchMaps.RemoveRange(maps);

                // Revert tournament status back to Active if it was completed
                if (match.Tournament.Status == "Completed")
                {
                    match.Tournament.Status = "Active";
                    match.Tournament.EndDate = null;
                    db.Entry(match.Tournament).State = EntityState.Modified;
                }

                // Recursively clear winner in subsequent rounds
                if (prevWinnerId.HasValue && match.Tournament.Format == "SingleElimination")
                {
                    RollbackNextMatches(db, match, prevWinnerId.Value);
                }
                else if (prevWinnerId.HasValue && match.Tournament.Format == "DoubleElimination")
                {
                    // Revert Winner advance
                    RollbackNextMatches(db, match, prevWinnerId.Value);

                    // Revert Loser advance (if Winner's bracket match)
                    if (match.BracketBranch == "Winner")
                    {
                        var loserMatch = FindLoserDestinationMatch(db, match);
                        if (loserMatch != null)
                        {
                            int loserId = (match.Team1Id == prevWinnerId) ? match.Team2Id.Value : match.Team1Id.Value;
                            RollbackLoserMatch(db, loserMatch, loserId);
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private void RollbackNextMatches(AppDbContext db, Match currentMatch, int winnerIdToRemove)
        {
            if (currentMatch.NextMatchId.HasValue)
            {
                var nextMatch = db.Matches.Find(currentMatch.NextMatchId.Value);
                if (nextMatch != null)
                {
                    bool affected = false;
                    if (nextMatch.Team1Id == winnerIdToRemove)
                    {
                        nextMatch.Team1Id = null;
                        affected = true;
                    }
                    else if (nextMatch.Team2Id == winnerIdToRemove)
                    {
                        nextMatch.Team2Id = null;
                        affected = true;
                    }

                    if (affected)
                    {
                        int? nextWinnerId = nextMatch.WinnerTeamId;

                        // Reset scores and status
                        nextMatch.Team1Score = 0;
                        nextMatch.Team2Score = 0;
                        nextMatch.WinnerTeamId = null;
                        nextMatch.Status = "Scheduled";
                        db.Entry(nextMatch).State = EntityState.Modified;

                        // Remove maps
                        var maps = db.MatchMaps.Where(mm => mm.MatchId == nextMatch.MatchId).ToList();
                        db.MatchMaps.RemoveRange(maps);

                        // Recursively rollback subsequent matches
                        if (nextWinnerId.HasValue)
                        {
                            RollbackNextMatches(db, nextMatch, nextWinnerId.Value);
                        }
                    }
                }
            }
        }

        private void GenerateDoubleEliminationBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            int maxTeams = teams.Count;

            var rng = new Random();
            var shuffledTeams = teams.OrderBy(t => rng.Next()).ToList();

            if (maxTeams == 4)
            {
                // Winner Round 1
                var w1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Winner", Team1Id = shuffledTeams[0].TeamId, Team2Id = shuffledTeams[1].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Winner", Team1Id = shuffledTeams[2].TeamId, Team2Id = shuffledTeams[3].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                db.Matches.Add(w1);
                db.Matches.Add(w2);
                db.SaveChanges(); // Get IDs

                // Winner Round 2 (Winner Final)
                var w3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(w3);
                db.SaveChanges();

                w1.NextMatchId = w3.MatchId;
                w2.NextMatchId = w3.MatchId;

                // Loser Round 1
                var l1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(l1);
                db.SaveChanges();

                // Loser Round 2 (Loser Final)
                var l2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(l2);
                db.SaveChanges();

                l1.NextMatchId = l2.MatchId;

                // Grand Final
                var gf = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(3), MatchFormat = "BO5" };
                db.Matches.Add(gf);
                db.SaveChanges();

                w3.NextMatchId = gf.MatchId;
                l2.NextMatchId = gf.MatchId;

                db.SaveChanges();
            }
            else if (maxTeams == 8)
            {
                // Winner Round 1
                var w1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Winner", Team1Id = shuffledTeams[0].TeamId, Team2Id = shuffledTeams[1].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Winner", Team1Id = shuffledTeams[2].TeamId, Team2Id = shuffledTeams[3].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 3, BracketBranch = "Winner", Team1Id = shuffledTeams[4].TeamId, Team2Id = shuffledTeams[5].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w4 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 4, BracketBranch = "Winner", Team1Id = shuffledTeams[6].TeamId, Team2Id = shuffledTeams[7].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                db.Matches.Add(w1); db.Matches.Add(w2); db.Matches.Add(w3); db.Matches.Add(w4);
                db.SaveChanges();

                // Winner Round 2
                var w5 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                var w6 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 2, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(w5); db.Matches.Add(w6);
                db.SaveChanges();

                w1.NextMatchId = w5.MatchId; w2.NextMatchId = w5.MatchId;
                w3.NextMatchId = w6.MatchId; w4.NextMatchId = w6.MatchId;

                // Winner Round 3 (Winner Final)
                var w7 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(w7);
                db.SaveChanges();

                w5.NextMatchId = w7.MatchId; w6.NextMatchId = w7.MatchId;

                // Loser Round 1
                var l1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                var l2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(l1); db.Matches.Add(l2);
                db.SaveChanges();

                // Loser Round 2
                var l3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                var l4 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 2, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(l3); db.Matches.Add(l4);
                db.SaveChanges();

                l1.NextMatchId = l3.MatchId;
                l2.NextMatchId = l4.MatchId;

                // Loser Round 3
                var l5 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(3), MatchFormat = "BO3" };
                db.Matches.Add(l5);
                db.SaveChanges();

                l3.NextMatchId = l5.MatchId; l4.NextMatchId = l5.MatchId;

                // Loser Round 4 (Loser Final)
                var l6 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 4, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(4), MatchFormat = "BO3" };
                db.Matches.Add(l6);
                db.SaveChanges();

                l5.NextMatchId = l6.MatchId;

                // Grand Final
                var gf = new Match { TournamentId = tournament.TournamentId, RoundNumber = 4, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(5), MatchFormat = "BO5" };
                db.Matches.Add(gf);
                db.SaveChanges();

                w7.NextMatchId = gf.MatchId;
                l6.NextMatchId = gf.MatchId;

                db.SaveChanges();
            }
        }

        private Match FindLoserDestinationMatch(AppDbContext db, Match currentMatch)
        {
            int maxTeams = currentMatch.Tournament.MaxTeams;
            if (maxTeams == 4)
            {
                if (currentMatch.RoundNumber == 1)
                {
                    return db.Matches.FirstOrDefault(m => 
                        m.TournamentId == currentMatch.TournamentId && 
                        m.BracketBranch == "Loser" && 
                        m.RoundNumber == 1 && 
                        m.MatchOrder == 1);
                }
                else if (currentMatch.RoundNumber == 2)
                {
                    return db.Matches.FirstOrDefault(m => 
                        m.TournamentId == currentMatch.TournamentId && 
                        m.BracketBranch == "Loser" && 
                        m.RoundNumber == 2 && 
                        m.MatchOrder == 1);
                }
            }
            else if (maxTeams == 8)
            {
                if (currentMatch.RoundNumber == 1)
                {
                    int targetOrder = (currentMatch.MatchOrder - 1) / 2 + 1;
                    return db.Matches.FirstOrDefault(m => 
                        m.TournamentId == currentMatch.TournamentId && 
                        m.BracketBranch == "Loser" && 
                        m.RoundNumber == 1 && 
                        m.MatchOrder == targetOrder);
                }
                else if (currentMatch.RoundNumber == 2)
                {
                    int targetOrder = currentMatch.MatchOrder;
                    return db.Matches.FirstOrDefault(m => 
                        m.TournamentId == currentMatch.TournamentId && 
                        m.BracketBranch == "Loser" && 
                        m.RoundNumber == 2 && 
                        m.MatchOrder == targetOrder);
                }
                else if (currentMatch.RoundNumber == 3)
                {
                    return db.Matches.FirstOrDefault(m => 
                        m.TournamentId == currentMatch.TournamentId && 
                        m.BracketBranch == "Loser" && 
                        m.RoundNumber == 4 && 
                        m.MatchOrder == 1);
                }
            }
            return null;
        }

        private void SetLoserInMatch(Match targetMatch, Match sourceMatch, int loserId)
        {
            int maxTeams = sourceMatch.Tournament.MaxTeams;
            if (maxTeams == 4)
            {
                if (sourceMatch.RoundNumber == 1)
                {
                    if (sourceMatch.MatchOrder == 1)
                        targetMatch.Team1Id = loserId;
                    else
                        targetMatch.Team2Id = loserId;
                }
                else if (sourceMatch.RoundNumber == 2)
                {
                    targetMatch.Team2Id = loserId;
                }
            }
            else if (maxTeams == 8)
            {
                if (sourceMatch.RoundNumber == 1)
                {
                    if (sourceMatch.MatchOrder == 1 || sourceMatch.MatchOrder == 3)
                        targetMatch.Team1Id = loserId;
                    else
                        targetMatch.Team2Id = loserId;
                }
                else if (sourceMatch.RoundNumber == 2)
                {
                    targetMatch.Team2Id = loserId;
                }
                else if (sourceMatch.RoundNumber == 3)
                {
                    targetMatch.Team2Id = loserId;
                }
            }
        }

        private void RollbackLoserMatch(AppDbContext db, Match loserMatch, int loserIdToRemove)
        {
            bool affected = false;
            if (loserMatch.Team1Id == loserIdToRemove)
            {
                loserMatch.Team1Id = null;
                affected = true;
            }
            else if (loserMatch.Team2Id == loserIdToRemove)
            {
                loserMatch.Team2Id = null;
                affected = true;
            }

            if (affected)
            {
                int? nextWinnerId = loserMatch.WinnerTeamId;

                loserMatch.Team1Score = 0;
                loserMatch.Team2Score = 0;
                loserMatch.WinnerTeamId = null;
                loserMatch.Status = "Scheduled";
                db.Entry(loserMatch).State = EntityState.Modified;

                var maps = db.MatchMaps.Where(mm => mm.MatchId == loserMatch.MatchId).ToList();
                db.MatchMaps.RemoveRange(maps);

                if (nextWinnerId.HasValue)
                {
                    RollbackNextMatches(db, loserMatch, nextWinnerId.Value);
                }
            }
        }

        private int GetMaxPossibleMatches(Tournament tournament)
        {
            if (tournament == null) return 1;
            int maxTeams = tournament.MaxTeams;
            if (maxTeams <= 1) return 1;

            if (tournament.Format == "SingleElimination")
            {
                return (int)Math.Round(Math.Log(maxTeams, 2));
            }
            else if (tournament.Format == "DoubleElimination")
            {
                if (maxTeams == 4) return 4;
                if (maxTeams == 8) return 6;
                return 6; // default fallback
            }
            else if (tournament.Format == "RoundRobin")
            {
                return maxTeams - 1;
            }
            return 1;
        }

        public Player GetTournamentMvp(int tournamentId, out double avgScore)
        {
            avgScore = 0;
            using (var db = new AppDbContext())
            {
                var tournament = db.Tournaments.Find(tournamentId);
                if (tournament == null)
                    return null;

                int maxPossibleMatches = GetMaxPossibleMatches(tournament);

                var stats = db.PlayerStats
                    .Include(ps => ps.Player.Team)
                    .Where(ps => ps.MatchMap.Match.TournamentId == tournamentId)
                    .ToList();

                if (stats.Count == 0)
                    return null;

                var playerGroup = stats.GroupBy(ps => ps.PlayerId)
                    .Select(g => {
                        var player = g.First().Player;
                        var matchAverages = g.GroupBy(ps => ps.MatchMap.MatchId)
                                             .Select(mg => mg.Average(ps => ps.PerformancePoints))
                                             .ToList();

                        double totalMatchPoints = matchAverages.Sum();
                        double score = totalMatchPoints / maxPossibleMatches;

                        return new {
                            Player = player,
                            CalculatedScore = score
                        };
                    })
                    .OrderByDescending(x => x.CalculatedScore)
                    .FirstOrDefault();

                if (playerGroup != null)
                {
                    avgScore = playerGroup.CalculatedScore;
                    return playerGroup.Player;
                }
            }
            return null;
        }

        public Player GetMatchMvp(int matchId, out double avgScore)
        {
            avgScore = 0;
            using (var db = new AppDbContext())
            {
                var stats = db.PlayerStats
                    .Include(ps => ps.Player.Team)
                    .Where(ps => ps.MatchMap.MatchId == matchId)
                    .ToList();

                if (stats.Count == 0)
                    return null;

                var playerGroup = stats.GroupBy(ps => ps.PlayerId)
                    .Select(g => new {
                        PlayerId = g.Key,
                        Player = g.First().Player,
                        AverageScore = g.Average(ps => ps.PerformancePoints)
                    })
                    .OrderByDescending(x => x.AverageScore)
                    .FirstOrDefault();

                if (playerGroup != null)
                {
                    avgScore = playerGroup.AverageScore;
                    return playerGroup.Player;
                }
            }
            return null;
        }

        public void SaveMatchPerformance(int matchId, List<MatchMap> inputMaps, string status)
        {
            using (var db = new AppDbContext())
            {
                var match = db.Matches
                    .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats))
                    .FirstOrDefault(m => m.MatchId == matchId);

                if (match == null)
                    throw new Exception("Không tìm thấy trận đấu.");

                foreach (var inputMap in inputMaps)
                {
                    var existingMap = match.MatchMaps.FirstOrDefault(mm => mm.MapNumber == inputMap.MapNumber);
                    if (existingMap == null)
                    {
                        existingMap = new MatchMap
                        {
                            MatchId = matchId,
                            MapNumber = inputMap.MapNumber,
                            SelectedMapName = inputMap.SelectedMapName ?? $"Ván {inputMap.MapNumber}",
                            Team1RoundScore = inputMap.Team1RoundScore,
                            Team2RoundScore = inputMap.Team2RoundScore
                        };
                        db.MatchMaps.Add(existingMap);
                        db.SaveChanges(); // Get Map ID
                    }
                    else
                    {
                        existingMap.SelectedMapName = inputMap.SelectedMapName ?? $"Ván {inputMap.MapNumber}";
                        existingMap.Team1RoundScore = inputMap.Team1RoundScore;
                        existingMap.Team2RoundScore = inputMap.Team2RoundScore;
                        db.Entry(existingMap).State = EntityState.Modified;
                    }

                    // Save or update player stats for this map
                    int? winnerMvpId = null;
                    int? loserMvpId = null;
                    double maxWinnerPts = -999999;
                    double maxLoserPts = -999999;

                    int? winningTeamId = null;
                    int? losingTeamId = null;
                    if (existingMap.Team1RoundScore > existingMap.Team2RoundScore)
                    {
                        winningTeamId = match.Team1Id;
                        losingTeamId = match.Team2Id;
                    }
                    else if (existingMap.Team2RoundScore > existingMap.Team1RoundScore)
                    {
                        winningTeamId = match.Team2Id;
                        losingTeamId = match.Team1Id;
                    }

                    // Load all players' TeamIds in memory for quick lookup
                    var playerTeamIds = db.Players
                        .Where(p => p.TeamId == match.Team1Id || p.TeamId == match.Team2Id)
                        .ToDictionary(p => p.PlayerId, p => p.TeamId);

                    foreach (var inputStat in inputMap.PlayerStats)
                    {
                        var existingStat = existingMap.PlayerStats.FirstOrDefault(ps => ps.PlayerId == inputStat.PlayerId);
                        if (existingStat == null)
                        {
                            existingStat = new PlayerStat
                            {
                                MatchMapId = existingMap.MatchMapId,
                                PlayerId = inputStat.PlayerId,
                                Kills = inputStat.Kills,
                                Deaths = inputStat.Deaths,
                                Assists = inputStat.Assists,
                                DamageDealt = inputStat.DamageDealt,
                                CreepScore = inputStat.CreepScore,
                                IsMvpOfMap = false
                            };
                            db.PlayerStats.Add(existingStat);
                        }
                        else
                        {
                            existingStat.Kills = inputStat.Kills;
                            existingStat.Deaths = inputStat.Deaths;
                            existingStat.Assists = inputStat.Assists;
                            existingStat.DamageDealt = inputStat.DamageDealt;
                            existingStat.CreepScore = inputStat.CreepScore;
                            existingStat.IsMvpOfMap = false;
                            db.Entry(existingStat).State = EntityState.Modified;
                        }

                        // Calculate PTS
                        double pts = existingStat.PerformancePoints;
                        
                        playerTeamIds.TryGetValue(inputStat.PlayerId, out int playerTeamId);

                        if (winningTeamId.HasValue && playerTeamId == winningTeamId.Value)
                        {
                            if (pts > maxWinnerPts)
                            {
                                maxWinnerPts = pts;
                                winnerMvpId = existingStat.PlayerId;
                            }
                        }
                        else if (losingTeamId.HasValue && playerTeamId == losingTeamId.Value)
                        {
                            if (pts > maxLoserPts)
                            {
                                maxLoserPts = pts;
                                loserMvpId = existingStat.PlayerId;
                            }
                        }
                    }

                    db.SaveChanges(); // Persist stats so we can find them and set IsMvpOfMap

                    // Set IsMvpOfMap to true for the Winner MVP and Loser MVP
                    var mapStats = db.PlayerStats.Where(ps => ps.MatchMapId == existingMap.MatchMapId).ToList();
                    foreach (var stat in mapStats)
                    {
                        stat.IsMvpOfMap = (stat.PlayerId == winnerMvpId || stat.PlayerId == loserMvpId);
                        db.Entry(stat).State = EntityState.Modified;
                    }

                    existingMap.MVPlayerId = winnerMvpId;
                    db.Entry(existingMap).State = EntityState.Modified;
                }

                db.SaveChanges();
            }

            // Calculate Match Scores from Map Scores
            int calculatedTeam1Score = 0;
            int calculatedTeam2Score = 0;

            using (var db = new AppDbContext())
            {
                var maps = db.MatchMaps.Where(mm => mm.MatchId == matchId).ToList();
                foreach (var map in maps)
                {
                    if (map.Team1RoundScore > map.Team2RoundScore)
                        calculatedTeam1Score++;
                    else if (map.Team2RoundScore > map.Team1RoundScore)
                        calculatedTeam2Score++;
                }
            }

            // Call existing UpdateMatchResult to handle winner advancement
            UpdateMatchResult(matchId, calculatedTeam1Score, calculatedTeam2Score, status, null);
        }
    }
}
