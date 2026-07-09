using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    public class MatchService
    {
        private readonly BracketService _bracketService = new BracketService();

        // Cập nhật kết quả tỉ số trận đấu, xử lý tiến nhánh cho đội thắng và chuyển đội thua xuống nhánh thua
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

                    // Nếu là thể thức Loại trực tiếp (Single Elimination), đưa đội thắng lên vòng tiếp theo
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
                    // Nếu là thể thức Nhánh thắng thua (Double Elimination), đưa đội thắng tiến lên và chuyển đội thua xuống
                    else if (match.Tournament.Format == "DoubleElimination" && match.NextMatchId.HasValue)
                    {
                        var nextMatch = db.Matches.Find(match.NextMatchId.Value);
                        if (nextMatch != null)
                        {
                            // Nếu trận đấu kế tiếp là trận Chung kết tổng (Grand Final)
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

                        // Chuyển đội thua từ nhánh thắng xuống nhánh thua tương ứng
                        if (match.BracketBranch == "Winner")
                        {
                            var loserMatch = _bracketService.FindLoserDestinationMatch(db, match);
                            if (loserMatch != null)
                            {
                                int loserId = winnerId == match.Team1Id ? match.Team2Id.Value : match.Team1Id.Value;
                                _bracketService.SetLoserInMatch(loserMatch, match, loserId);
                                db.Entry(loserMatch).State = EntityState.Modified;
                            }
                        }
                    }

                    // Kiểm tra xem giải đấu đã hoàn tất chưa (dành cho thể thức Loại trực tiếp hoặc Thắng thua: ván chung kết tổng kết thúc)
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
                    // Dành cho thể thức Vòng tròn (Round Robin): kiểm tra xem toàn bộ các trận đã hoàn tất chưa
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
                    // Nếu đưa trận đấu về Scheduled hoặc Live, xóa thông tin đội thắng cuộc
                    match.WinnerTeamId = null;
                }

                db.Entry(match).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        // Thu hồi (Rollback) kết quả trận đấu đã hoàn thành về trạng thái Scheduled
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
                    return; // Trận đấu chưa hoàn thành nên không cần thu hồi kết quả

                int? prevWinnerId = match.WinnerTeamId;

                // Reset thông số của trận đấu hiện tại
                match.Team1Score = 0;
                match.Team2Score = 0;
                match.WinnerTeamId = null;
                match.Status = "Scheduled";
                db.Entry(match).State = EntityState.Modified;

                // Xóa chi tiết ván đấu và điểm số người chơi
                var maps = db.MatchMaps.Where(mm => mm.MatchId == matchId).ToList();
                db.MatchMaps.RemoveRange(maps);

                // Nếu giải đấu đã hoàn thành trước đó, trả trạng thái giải đấu về Active
                if (match.Tournament.Status == "Completed")
                {
                    match.Tournament.Status = "Active";
                    match.Tournament.EndDate = null;
                    db.Entry(match.Tournament).State = EntityState.Modified;
                }

                // Thực hiện đệ quy làm sạch các vòng đấu phía sau
                if (prevWinnerId.HasValue && match.Tournament.Format == "SingleElimination")
                {
                    _bracketService.RollbackNextMatches(db, match, prevWinnerId.Value);
                }
                else if (prevWinnerId.HasValue && match.Tournament.Format == "DoubleElimination")
                {
                    // Thu hồi việc thăng tiến của đội thắng ở nhánh thắng
                    _bracketService.RollbackNextMatches(db, match, prevWinnerId.Value);

                    // Thu hồi việc chuyển đội thua xuống nhánh thua (chỉ thực hiện nếu trận đấu nguồn ở nhánh Thắng)
                    if (match.BracketBranch == "Winner")
                    {
                        var loserMatch = _bracketService.FindLoserDestinationMatch(db, match);
                        if (loserMatch != null)
                        {
                            int loserId = (match.Team1Id == prevWinnerId) ? match.Team2Id.Value : match.Team1Id.Value;
                            _bracketService.RollbackLoserMatch(db, loserMatch, loserId);
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        // Lấy tuyển thủ xuất sắc nhất trận đấu (MVP Match)
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

        // Lưu/Cập nhật dữ liệu thống kê của các ván đấu (Maps) và chỉ số tuyển thủ (Player stats)
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
                            Team2RoundScore = inputMap.Team2RoundScore,
                            Team1DragonsKilled = inputMap.Team1DragonsKilled,
                            Team2DragonsKilled = inputMap.Team2DragonsKilled,
                            Team1TowersDestroyed = inputMap.Team1TowersDestroyed,
                            Team2TowersDestroyed = inputMap.Team2TowersDestroyed
                        };
                        db.MatchMaps.Add(existingMap);
                        db.SaveChanges(); // Lưu để lấy Map ID
                    }
                    else
                    {
                        existingMap.SelectedMapName = inputMap.SelectedMapName ?? $"Ván {inputMap.MapNumber}";
                        existingMap.Team1RoundScore = inputMap.Team1RoundScore;
                        existingMap.Team2RoundScore = inputMap.Team2RoundScore;
                        existingMap.Team1DragonsKilled = inputMap.Team1DragonsKilled;
                        existingMap.Team2DragonsKilled = inputMap.Team2DragonsKilled;
                        existingMap.Team1TowersDestroyed = inputMap.Team1TowersDestroyed;
                        existingMap.Team2TowersDestroyed = inputMap.Team2TowersDestroyed;
                        db.Entry(existingMap).State = EntityState.Modified;
                    }

                    // Lưu hoặc cập nhật số liệu thống kê người chơi cho ván đấu
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

                    // Tải TeamId của tất cả tuyển thủ trong bộ nhớ để tra cứu nhanh
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

                        // Tính điểm Performance Points (PTS) cho tuyển thủ
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

                    db.SaveChanges(); // Lưu để đảm bảo thống kê đã được ghi nhận trước khi gán cờ IsMvpOfMap

                    // Đánh dấu cờ IsMvpOfMap cho Winner MVP và Loser MVP trong ván này
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

            // Tính toán tổng điểm số trận đấu dựa trên kết quả các ván đấu (Maps)
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

            // Gọi hàm cập nhật kết quả trận đấu để tiến hành phân nhánh đấu tiếp theo
            UpdateMatchResult(matchId, calculatedTeam1Score, calculatedTeam2Score, status, null);
        }
    }
}
