using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp dịch vụ quản lý việc tạo nhánh đấu (bracket) và xử lý logic tiến/lùi (advance/rollback) các trận đấu.
    /// </summary>
    public class BracketService
    {
        /// <summary>
        /// Tạo nhánh đấu loại trực tiếp (Single Elimination) cho giải đấu.
        /// Sinh ra các trận đấu từ vòng chung kết ngược về vòng đầu tiên và gán ngẫu nhiên các đội vào Vòng 1.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="tournament">Đối tượng giải đấu cần tạo nhánh</param>
        public void GenerateSingleEliminationBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            int N = teams.Count;
            int numRounds = (int)Math.Log(N, 2);

            // Cấu trúc lưu trữ các trận đấu trong bộ nhớ theo từng vòng (bắt đầu từ index 1)
            List<Match>[] matchesByRound = new List<Match>[numRounds + 1];
            for (int r = 1; r <= numRounds; r++)
            {
                matchesByRound[r] = new List<Match>();
            }

            // Tạo các trận đấu từ vòng chung kết (R) ngược về vòng 1 để thiết lập liên kết NextMatch
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

            // Xáo trộn ngẫu nhiên danh sách đội tuyển và bắt cặp cho Vòng 1
            var rng = new Random();
            var shuffledTeams = teams.OrderBy(t => rng.Next()).ToList();
            for (int i = 0; i < matchesByRound[1].Count; i++)
            {
                var match = matchesByRound[1][i];
                match.Team1Id = shuffledTeams[2 * i].TeamId;
                match.Team2Id = shuffledTeams[2 * i + 1].TeamId;
            }

            // Lưu tất cả trận đấu đã tạo vào cơ sở dữ liệu
            for (int r = numRounds; r >= 1; r--)
            {
                foreach (var match in matchesByRound[r])
                {
                    db.Matches.Add(match);
                }
            }
        }

        /// <summary>
        /// Tạo lịch thi đấu theo thể thức vòng tròn tính điểm (Round Robin).
        /// Sử dụng thuật toán xoay vòng (Circle Method) để bắt cặp đối đầu giữa các đội tuyển.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="tournament">Đối tượng giải đấu cần tạo lịch</param>
        public void GenerateRoundRobinBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            var teamList = teams.ToList();

            // Nếu số lượng đội lẻ, thêm một đội giả (null) đại diện cho lượt nghỉ (bye)
            bool hasBye = teamList.Count % 2 != 0;
            if (hasBye)
            {
                teamList.Add(null);
            }

            int numTeams = teamList.Count;
            int numRounds = numTeams - 1;
            int matchesPerRound = numTeams / 2;
            var rng = new Random();
            // Xáo trộn ngẫu nhiên thứ tự các đội tuyển (đội giả lập null luôn được xếp cuối cùng để chuẩn hóa)
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

                    // Bỏ qua trận đấu nếu một trong hai đội tuyển là lượt nghỉ (null)
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

        /// <summary>
        /// Tạo nhánh đấu thắng thua (Double Elimination) cho giải đấu (chỉ hỗ trợ quy mô 4 hoặc 8 đội).
        /// Xây dựng trước toàn bộ cấu trúc sơ đồ các trận đấu của nhánh Thắng (Winner), nhánh Thua (Loser) và Chung kết Tổng.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="tournament">Đối tượng giải đấu cần tạo nhánh</param>
        public void GenerateDoubleEliminationBracket(AppDbContext db, Tournament tournament)
        {
            var teams = tournament.TournamentTeams.Select(tt => tt.Team).ToList();
            int maxTeams = teams.Count;

            var rng = new Random();
            var shuffledTeams = teams.OrderBy(t => rng.Next()).ToList();

            if (maxTeams == 4)
            {
                // Nhánh Thắng - Vòng 1
                var w1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Winner", Team1Id = shuffledTeams[0].TeamId, Team2Id = shuffledTeams[1].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Winner", Team1Id = shuffledTeams[2].TeamId, Team2Id = shuffledTeams[3].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                db.Matches.Add(w1);
                db.Matches.Add(w2);
                db.SaveChanges(); // Lưu trước để lấy ID tự tăng từ cơ sở dữ liệu

                // Nhánh Thắng - Vòng 2 (Chung kết nhánh Thắng)
                var w3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(w3);
                db.SaveChanges();

                w1.NextMatchId = w3.MatchId;
                w2.NextMatchId = w3.MatchId;

                // Nhánh Thua - Vòng 1
                var l1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(l1);
                db.SaveChanges();

                // Nhánh Thua - Vòng 2 (Chung kết nhánh Thua)
                var l2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(l2);
                db.SaveChanges();

                l1.NextMatchId = l2.MatchId;

                // Chung Kết Tổng (Grand Final)
                var gf = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(3), MatchFormat = "BO5" };
                db.Matches.Add(gf);
                db.SaveChanges();

                w3.NextMatchId = gf.MatchId;
                l2.NextMatchId = gf.MatchId;

                db.SaveChanges();
            }
            else if (maxTeams == 8)
            {
                // Nhánh Thắng - Vòng 1
                var w1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Winner", Team1Id = shuffledTeams[0].TeamId, Team2Id = shuffledTeams[1].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Winner", Team1Id = shuffledTeams[2].TeamId, Team2Id = shuffledTeams[3].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 3, BracketBranch = "Winner", Team1Id = shuffledTeams[4].TeamId, Team2Id = shuffledTeams[5].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                var w4 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 4, BracketBranch = "Winner", Team1Id = shuffledTeams[6].TeamId, Team2Id = shuffledTeams[7].TeamId, ScheduledTime = tournament.StartDate, MatchFormat = "BO3" };
                db.Matches.Add(w1); db.Matches.Add(w2); db.Matches.Add(w3); db.Matches.Add(w4);
                db.SaveChanges();

                // Nhánh Thắng - Vòng 2
                var w5 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                var w6 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 2, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(w5); db.Matches.Add(w6);
                db.SaveChanges();

                w1.NextMatchId = w5.MatchId; w2.NextMatchId = w5.MatchId;
                w3.NextMatchId = w6.MatchId; w4.NextMatchId = w6.MatchId;

                // Nhánh Thắng - Vòng 3 (Chung kết nhánh Thắng)
                var w7 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(w7);
                db.SaveChanges();

                w5.NextMatchId = w7.MatchId; w6.NextMatchId = w7.MatchId;

                // Nhánh Thua - Vòng 1
                var l1 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                var l2 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 1, MatchOrder = 2, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(1), MatchFormat = "BO3" };
                db.Matches.Add(l1); db.Matches.Add(l2);
                db.SaveChanges();

                // Nhánh Thua - Vòng 2
                var l3 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                var l4 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 2, MatchOrder = 2, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(2), MatchFormat = "BO3" };
                db.Matches.Add(l3); db.Matches.Add(l4);
                db.SaveChanges();

                l1.NextMatchId = l3.MatchId;
                l2.NextMatchId = l4.MatchId;

                // Nhánh Thua - Vòng 3
                var l5 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 3, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(3), MatchFormat = "BO3" };
                db.Matches.Add(l5);
                db.SaveChanges();

                l3.NextMatchId = l5.MatchId; l4.NextMatchId = l5.MatchId;

                // Nhánh Thua - Vòng 4 (Chung kết nhánh Thua)
                var l6 = new Match { TournamentId = tournament.TournamentId, RoundNumber = 4, MatchOrder = 1, BracketBranch = "Loser", ScheduledTime = tournament.StartDate.AddDays(4), MatchFormat = "BO3" };
                db.Matches.Add(l6);
                db.SaveChanges();

                l5.NextMatchId = l6.MatchId;

                // Chung Kết Tổng (Grand Final)
                var gf = new Match { TournamentId = tournament.TournamentId, RoundNumber = 4, MatchOrder = 1, BracketBranch = "Winner", ScheduledTime = tournament.StartDate.AddDays(5), MatchFormat = "BO5" };
                db.Matches.Add(gf);
                db.SaveChanges();

                w7.NextMatchId = gf.MatchId;
                l6.NextMatchId = gf.MatchId;

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Tìm trận đấu đích ở nhánh Thua dựa trên trận đấu hiện tại ở nhánh Thắng để sẵn sàng chuyển đội thua xuống.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="currentMatch">Trận đấu hiện tại ở nhánh thắng</param>
        /// <returns>Trận đấu nhánh thua tương ứng sẽ tiếp nhận đội thua cuộc</returns>
        public Match FindLoserDestinationMatch(AppDbContext db, Match currentMatch)
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

        /// <summary>
        /// Thiết lập ID đội tuyển thua cuộc vào vị trí Team 1 hoặc Team 2 của trận đấu đích tương ứng ở nhánh thua.
        /// </summary>
        /// <param name="targetMatch">Trận đấu nhánh thua cần điền đội</param>
        /// <param name="sourceMatch">Trận đấu nhánh thắng nguồn vừa kết thúc</param>
        /// <param name="loserId">ID đội tuyển thua cuộc</param>
        public void SetLoserInMatch(Match targetMatch, Match sourceMatch, int loserId)
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

        /// <summary>
        /// Thu hồi (Rollback) dữ liệu và kết quả của trận đấu nhánh thua nếu trận đấu nhánh thắng trước đó có thay đổi kết quả.
        /// Xóa bản đồ (maps) và đệ quy thu hồi các vòng sau nếu cần.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="loserMatch">Trận đấu nhánh thua bị ảnh hưởng</param>
        /// <param name="loserIdToRemove">ID đội tuyển nhánh thua cần rút lại</param>
        public void RollbackLoserMatch(AppDbContext db, Match loserMatch, int loserIdToRemove)
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

        /// <summary>
        /// Thu hồi (Rollback) đệ quy kết quả của các trận đấu kế tiếp ở vòng sau khi đội thắng ở vòng trước bị thay đổi.
        /// </summary>
        /// <param name="db">Context kết nối cơ sở dữ liệu</param>
        /// <param name="currentMatch">Trận đấu nguồn vừa bị rollback</param>
        /// <param name="winnerIdToRemove">ID đội tuyển thắng cuộc bị hủy tiến trình</param>
        public void RollbackNextMatches(AppDbContext db, Match currentMatch, int winnerIdToRemove)
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

                        nextMatch.Team1Score = 0;
                        nextMatch.Team2Score = 0;
                        nextMatch.WinnerTeamId = null;
                        nextMatch.Status = "Scheduled";
                        db.Entry(nextMatch).State = EntityState.Modified;

                        var maps = db.MatchMaps.Where(mm => mm.MatchId == nextMatch.MatchId).ToList();
                        db.MatchMaps.RemoveRange(maps);

                        if (nextWinnerId.HasValue)
                        {
                            RollbackNextMatches(db, nextMatch, nextWinnerId.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lấy số lượng trận đấu tối đa (vòng đấu tối đa) mà một đội tuyển có thể thi đấu trong giải đấu dựa theo thể thức.
        /// Dùng cho việc tính toán phân bổ điểm số MVP của giải đấu.
        /// </summary>
        /// <param name="tournament">Đối tượng giải đấu cần tính toán</param>
        /// <returns>Số trận đấu tối đa có thể tham gia</returns>
        public int GetMaxPossibleMatches(Tournament tournament)
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
                return 6;
            }
            else if (tournament.Format == "RoundRobin")
            {
                return maxTeams - 1;
            }
            return 1;
        }
    }
}
