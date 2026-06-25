using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Đội tuyển (Team).
    /// </summary>
    public class TeamService
    {
        /// <summary>
        /// Lấy danh sách tất cả các đội tuyển hiện có trong cơ sở dữ liệu.
        /// </summary>
        /// <returns>Danh sách tất cả đội tuyển</returns>
        public List<Team> GetAllTeams()
        {
            using (var db = new AppDbContext())
            {
                return db.Teams.ToList();
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đội tuyển theo ID.
        /// </summary>
        /// <param name="teamId">ID đội tuyển</param>
        /// <returns>Đối tượng đội tuyển tương ứng hoặc null nếu không tìm thấy</returns>
        public Team GetTeamById(int teamId)
        {
            using (var db = new AppDbContext())
            {
                return db.Teams.Find(teamId);
            }
        }

        /// <summary>
        /// Thêm mới một đội tuyển vào hệ thống.
        /// </summary>
        /// <param name="team">Đối tượng đội tuyển cần thêm</param>
        public void AddTeam(Team team)
        {
            using (var db = new AppDbContext())
            {
                db.Teams.Add(team);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Cập nhật thông tin chi tiết của một đội tuyển đã tồn tại.
        /// </summary>
        /// <param name="team">Đối tượng đội tuyển đã chỉnh sửa</param>
        public void UpdateTeam(Team team)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa một đội tuyển ra khỏi hệ thống theo ID.
        /// </summary>
        /// <param name="teamId">ID đội tuyển cần xóa</param>
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