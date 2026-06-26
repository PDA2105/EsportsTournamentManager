using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // Dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Tuyển thủ (Player)
    public class PlayerService
    {
        // Lấy danh sách tất cả các tuyển thủ của một đội tuyển
        public List<Player> GetPlayersByTeam(int teamId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Where(p => p.TeamId == teamId).ToList();
            }
        }

        // Lấy thông tin chi tiết tuyển thủ theo ID
        public Player GetPlayerById(int playerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Find(playerId);
            }
        }

        // Thêm mới một tuyển thủ vào hệ thống
        public void AddPlayer(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Players.Add(player);
                db.SaveChanges();
            }
        }

        // Cập nhật thông tin chi tiết tuyển thủ đã tồn tại
        public void UpdatePlayer(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        // Xóa tuyển thủ khỏi hệ thống theo ID
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
