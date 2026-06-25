using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp dịch vụ quản lý các nghiệp vụ liên quan đến giải đấu (Tournament), trận đấu (Match) và tính toán điểm phong độ (MVP).
    /// </summary>
    public class TournamentService
    {
        /// <summary>
        /// Lấy danh sách tất cả các giải đấu trong hệ thống kèm theo thông tin người tạo và danh sách đội tuyển tham gia.
        /// </summary>
        /// <returns>Danh sách các giải đấu</returns>
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

        /// <summary>
        /// Lấy chi tiết thông tin một giải đấu theo ID bao gồm người tạo, các đội tuyển và danh sách trận đấu.
        /// </summary>
        /// <param name="tournamentId">ID của giải đấu</param>
        /// <returns>Đối tượng giải đấu tương ứng hoặc null nếu không tìm thấy</returns>
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

        /// <summary>
        /// Thêm mới một giải đấu vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="tournament">Đối tượng giải đấu cần thêm</param>
        public void AddTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Tournaments.Add(tournament);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Cập nhật thông tin chi tiết của một giải đấu đã tồn tại.
        /// </summary>
        /// <param name="tournament">Đối tượng giải đấu đã sửa đổi</param>
        public void UpdateTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa giải đấu ra khỏi hệ thống theo ID giải đấu.
        /// </summary>
        /// <param name="tournamentId">ID giải đấu cần xóa</param>
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

        /// <summary>
        /// Lưu danh sách các đội tuyển được phân bổ vào giải đấu.
        /// Xóa toàn bộ liên kết đội tuyển cũ và tạo liên kết mới trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="tournamentId">ID giải đấu</param>
        /// <param name="teamIds">Danh sách ID các đội tuyển tham gia</param>
        public void SaveTournamentTeams(int tournamentId, List<int> teamIds)
        {
            using (var db = new AppDbContext())
            {
                // Xóa các ánh xạ giải đấu - đội tuyển hiện tại
                var existing = db.TournamentTeams.Where(tt => tt.TournamentId == tournamentId).ToList();
                db.TournamentTeams.RemoveRange(existing);

                // Thêm các ánh xạ giải đấu - đội tuyển mới
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

        private readonly BracketService _bracketService = new BracketService();

        /// <summary>
        /// Bắt đầu giải đấu, xác minh số lượng đội tuyển và kích hoạt sinh nhánh đấu tự động theo thể thức.
        /// </summary>
        /// <param name="tournamentId">ID giải đấu cần bắt đầu</param>
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
                    _bracketService.GenerateSingleEliminationBracket(db, tournament);
                }
                else if (tournament.Format == "DoubleElimination")
                {
                    if (teamCount != 4 && teamCount != 8)
                    {
                        throw new Exception("Thể thức Nhánh thắng nhánh thua yêu cầu số đội tham gia phải là 4 hoặc 8 đội.");
                    }
                    _bracketService.GenerateDoubleEliminationBracket(db, tournament);
                }
                else if (tournament.Format == "RoundRobin")
                {
                    if (teamCount < 2)
                    {
                        throw new Exception("Thể thức Vòng tròn tính điểm yêu cầu tối thiểu 2 đội tham gia.");
                    }
                    _bracketService.GenerateRoundRobinBracket(db, tournament);
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

        /// <summary>
        /// Cập nhật kết quả tỉ số của một trận đấu, xử lý tiến nhánh cho đội thắng cuộc
        /// hoặc chuyển đội thua cuộc xuống nhánh thua nếu ở thể thức nhánh thắng thua (Double Elimination).
        /// Đồng thời kiểm tra xem giải đấu đã hoàn thành toàn bộ hay chưa để cập nhật trạng thái giải đấu.
        /// </summary>
        /// <param name="matchId">ID trận đấu</param>
        /// <param name="team1Score">Điểm số đội 1</param>
        /// <param name="team2Score">Điểm số đội 2</param>
        /// <param name="status">Trạng thái trận đấu (Scheduled/Live/Completed/Cancelled)</param>
        /// <param name="mvpPlayerId">ID tuyển thủ MVP trận đấu (nếu có)</param>
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

        /// <summary>
        /// Thu hồi (Rollback) kết quả trận đấu đã hoàn thành về trạng thái Scheduled.
        /// Đồng thời xóa dữ liệu thi đấu chi tiết (maps/stats) của trận đó và đệ quy thu hồi kết quả các trận đấu sau bị ảnh hưởng.
        /// </summary>
        /// <param name="matchId">ID trận đấu cần thu hồi</param>
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

        /// <summary>
        /// Tính toán và tìm ra tuyển thủ xuất sắc nhất giải đấu (MVP Tournament) dựa trên điểm số phong độ trung bình.
        /// </summary>
        /// <param name="tournamentId">ID giải đấu</param>
        /// <param name="avgScore">Điểm phong độ trung bình trả ra</param>
        /// <returns>Đối tượng tuyển thủ đạt danh hiệu MVP giải đấu</returns>
        public Player GetTournamentMvp(int tournamentId, out double avgScore)
        {
            avgScore = 0;
            using (var db = new AppDbContext())
            {
                var tournament = db.Tournaments.Find(tournamentId);
                if (tournament == null)
                    return null;

                // Tải tất cả trận đấu thuộc giải đấu này
                var tournamentMatches = db.Matches
                    .Where(m => m.TournamentId == tournamentId)
                    .ToList();

                // Tải danh sách ID các đội tham gia giải đấu
                var teamIds = db.TournamentTeams
                    .Where(tt => tt.TournamentId == tournamentId)
                    .Select(tt => tt.TeamId)
                    .ToList();

                // Đếm số trận đấu thực tế mà từng đội đã thi đấu (chỉ tính trận có trạng thái Completed)
                var teamMatchesCount = teamIds.ToDictionary(tid => tid, tid => 0);
                foreach (var match in tournamentMatches)
                {
                    if (match.Status == "Completed")
                    {
                        if (match.Team1Id.HasValue && teamMatchesCount.ContainsKey(match.Team1Id.Value))
                            teamMatchesCount[match.Team1Id.Value]++;
                        if (match.Team2Id.HasValue && teamMatchesCount.ContainsKey(match.Team2Id.Value))
                            teamMatchesCount[match.Team2Id.Value]++;
                    }
                }

                // Xác định số lượng trận đấu nhiều nhất của một đội tuyển trong giải đấu
                int maxMatchesInTournament = teamMatchesCount.Values.Count > 0 ? teamMatchesCount.Values.Max() : 1;
                if (maxMatchesInTournament < 1) maxMatchesInTournament = 1;

                // Tìm trận Chung kết tổng (Grand Final)
                var grandFinal = tournamentMatches.FirstOrDefault(m => 
                    !m.NextMatchId.HasValue && 
                    (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination"));

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
                        
                        // Tính ước số (divisor) cho đội của người chơi này
                        int divisor = maxMatchesInTournament;
                        int playerTeamId = player.TeamId;

                        if (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination")
                        {
                            // Nếu lọt vào trận Chung kết tổng, chia cho số trận thi đấu thực tế
                            bool isFinalist = grandFinal != null && (playerTeamId == grandFinal.Team1Id || playerTeamId == grandFinal.Team2Id);
                            if (isFinalist && teamMatchesCount.TryGetValue(playerTeamId, out int actualMatches))
                            {
                                divisor = actualMatches;
                            }
                        }

                        if (divisor < 1) divisor = 1;

                        double score = totalMatchPoints / divisor;

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

        /// <summary>
        /// Tính toán và lấy tuyển thủ xuất sắc nhất trận đấu (MVP Match) dựa trên trung bình điểm phong độ các ván chơi.
        /// </summary>
        /// <param name="matchId">ID trận đấu</param>
        /// <param name="avgScore">Điểm phong độ trung bình trong trận</param>
        /// <returns>Tuyển thủ đạt danh hiệu MVP trận đấu</returns>
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

        /// <summary>
        /// Lưu/Cập nhật dữ liệu thống kê của các ván đấu (Maps) và chỉ số tuyển thủ (Player stats),
        /// tự động tính điểm hiệu năng, đánh giá tuyển thủ MVP của từng ván (Winner MVP & Loser MVP)
        /// và cập nhật kết quả tỉ số tổng thể cho trận đấu.
        /// </summary>
        /// <param name="matchId">ID trận đấu cần lưu</param>
        /// <param name="inputMaps">Danh sách các ván đấu chi tiết được điền thông tin</param>
        /// <param name="status">Trạng thái trận đấu</param>
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
                        db.SaveChanges(); // Lưu để lấy Map ID
                    }
                    else
                    {
                        existingMap.SelectedMapName = inputMap.SelectedMapName ?? $"Ván {inputMap.MapNumber}";
                        existingMap.Team1RoundScore = inputMap.Team1RoundScore;
                        existingMap.Team2RoundScore = inputMap.Team2RoundScore;
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
