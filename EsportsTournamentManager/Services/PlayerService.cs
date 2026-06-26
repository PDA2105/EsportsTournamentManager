using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // DTO chứa thông số thống kê trung bình của tuyển thủ
    public class PlayerStatsDto
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public string TeamName { get; set; }
        public double Kills { get; set; }
        public double Deaths { get; set; }
        public double Assists { get; set; }
        public double Damage { get; set; }
        public double Creep { get; set; }
        public int MatchesPlayed { get; set; }
    }

    // Dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Tuyển thủ (Player)
    public class PlayerService
    {
        // Lấy danh sách tất cả các tuyển thủ kèm thống kê trung bình/trận
        public List<PlayerStatsDto> GetAllPlayersStats()
        {
            using (var db = new AppDbContext())
            {
                var players = db.Players
                    .Include(p => p.Team)
                    .Include(p => p.PlayerStats.Select(ps => ps.MatchMap))
                    .ToList();

                var list = new List<PlayerStatsDto>();
                foreach (var p in players)
                {
                    int maps = p.PlayerStats.Count;
                    if (maps > 0)
                    {
                        // Đếm số trận thực sự (Match), không phải số ván (MatchMap)
                        int matches = p.PlayerStats
                            .Where(ps => ps.MatchMap != null)
                            .Select(ps => ps.MatchMap.MatchId)
                            .Distinct()
                            .Count();

                        list.Add(new PlayerStatsDto
                        {
                            PlayerId = p.PlayerId,
                            Name = p.InGameName,
                            RealName = p.RealName,
                            TeamName = p.Team?.TeamName ?? "Tự do",
                            Kills = p.PlayerStats.Average(ps => ps.Kills),
                            Deaths = p.PlayerStats.Average(ps => ps.Deaths),
                            Assists = p.PlayerStats.Average(ps => ps.Assists),
                            Damage = p.PlayerStats.Average(ps => ps.DamageDealt),
                            Creep = p.PlayerStats.Average(ps => ps.CreepScore),
                            MatchesPlayed = matches
                        });
                    }
                    else
                    {
                        list.Add(new PlayerStatsDto
                        {
                            PlayerId = p.PlayerId,
                            Name = p.InGameName,
                            RealName = p.RealName,
                            TeamName = p.Team?.TeamName ?? "Tự do",
                            Kills = 0,
                            Deaths = 0,
                            Assists = 0,
                            Damage = 0,
                            Creep = 0,
                            MatchesPlayed = 0
                        });
                    }
                }

                // Sắp xếp mặc định theo tên tuyển thủ (A-Z)
                return list.OrderBy(x => x.Name).ToList();
            }
        }

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
                return db.Players
                    .Include(p => p.Team)
                    .FirstOrDefault(p => p.PlayerId == playerId);
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
