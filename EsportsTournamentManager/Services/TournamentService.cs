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

                    // Save MVP to the first map if provided
                    if (mvpPlayerId.HasValue)
                    {
                        var map = db.MatchMaps.FirstOrDefault(mm => mm.MatchId == matchId && mm.MapNumber == 1);
                        if (map == null)
                        {
                            map = new MatchMap
                            {
                                MatchId = matchId,
                                MapNumber = 1,
                                SelectedMapName = "Map 1",
                                Team1RoundScore = team1Score,
                                Team2RoundScore = team2Score,
                                MVPlayerId = mvpPlayerId
                            };
                            db.MatchMaps.Add(map);
                        }
                        else
                        {
                            map.MVPlayerId = mvpPlayerId;
                            db.Entry(map).State = EntityState.Modified;
                        }
                    }

                    // Check if tournament is completed (Single Elimination: final round match complete)
                    if (match.Tournament.Format == "SingleElimination" && !match.NextMatchId.HasValue)
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
    }
}
