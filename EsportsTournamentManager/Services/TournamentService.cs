using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // Dịch vụ quản lý các nghiệp vụ liên quan đến giải đấu (Tournament)
    public class TournamentService
    {
        // Lấy danh sách tất cả các giải đấu kèm người tạo và danh sách đội tham gia
        public List<Tournament> GetAllTournaments()
        {
            using (var db = new AppDbContext())
            {
                return db.Tournaments
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.TournamentTeams.Select(tt => tt.Team))
                    .Include(t => t.Matches.Select(m => m.MatchMaps))
                    .ToList();
            }
        }

        // Lấy chi tiết thông tin một giải đấu theo ID
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

        // Thêm mới một giải đấu vào cơ sở dữ liệu
        public void AddTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Tournaments.Add(tournament);
                db.SaveChanges();
            }
        }

        // Cập nhật thông tin chi tiết của một giải đấu đã tồn tại
        public void UpdateTournament(Tournament tournament)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(tournament).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        // Xóa giải đấu ra khỏi hệ thống theo ID
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

        // Lưu danh sách các đội tuyển được phân bổ vào giải đấu
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

        // Bắt đầu giải đấu, xác minh số đội tuyển và sinh nhánh đấu tự động
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
    }
}
