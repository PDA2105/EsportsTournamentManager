using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    /// <summary>
    /// Lớp dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Tuyển thủ (Player).
    /// </summary>
    public class PlayerService
    {
        /// <summary>
        /// Lấy danh sách tất cả các tuyển thủ thuộc về một đội tuyển cụ thể.
        /// </summary>
        /// <param name="teamId">ID đội tuyển</param>
        /// <returns>Danh sách các tuyển thủ</returns>
        public List<Player> GetPlayersByTeam(int teamId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Where(p => p.TeamId == teamId).ToList();
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một tuyển thủ theo ID.
        /// </summary>
        /// <param name="playerId">ID tuyển thủ</param>
        /// <returns>Đối tượng tuyển thủ tương ứng hoặc null nếu không tìm thấy</returns>
        public Player GetPlayerById(int playerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Find(playerId);
            }
        }

        /// <summary>
        /// Thêm mới một tuyển thủ vào hệ thống.
        /// </summary>
        /// <param name="player">Đối tượng tuyển thủ cần thêm</param>
        public void AddPlayer(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Players.Add(player);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Cập nhật thông tin chi tiết của một tuyển thủ đã tồn tại.
        /// </summary>
        /// <param name="player">Đối tượng tuyển thủ đã chỉnh sửa</param>
        public void UpdatePlayer(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa một tuyển thủ ra khỏi hệ thống theo ID.
        /// </summary>
        /// <param name="playerId">ID tuyển thủ cần xóa</param>
        public void DeletePlayer(int playerId)
        {
            using (var db = new AppDbContext())
            {
                var player = db.Players.Find(playerId);
                if (player != null)
                {
                    db.Players.Remove(player);
                    db.SaveChanges();
                }
            }
        }
    }
}
