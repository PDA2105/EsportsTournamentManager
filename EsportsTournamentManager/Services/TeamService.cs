using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // Dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Đội tuyển (Team)
    public class TeamService
    {
        // Lấy danh sách tất cả các đội tuyển trong hệ thống
        public List<Team> GetAllTeams()
        {
            using (var db = new AppDbContext())
            {
                return db.Teams.ToList();
            }
        }

        // Lấy thông tin chi tiết đội tuyển theo ID
        public Team GetTeamById(int teamId)
        {
            using (var db = new AppDbContext())
            {
                return db.Teams.Find(teamId);
            }
        }

        // Thêm mới một đội tuyển vào hệ thống
        public void AddTeam(Team team)
        {
            using (var db = new AppDbContext())
            {
                db.Teams.Add(team);
                db.SaveChanges();
            }
        }

        // Cập nhật thông tin chi tiết đội tuyển đã tồn tại
        public void UpdateTeam(Team team)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        // Xóa đội tuyển khỏi hệ thống theo ID
        public void DeleteTeam(int teamId)
        {
            using (var db = new AppDbContext())
            {
                var team = db.Teams.Find(teamId);
                if (team != null)
                {
                    db.Teams.Remove(team);
                    db.SaveChanges();
                }
            }
        }
    }
}