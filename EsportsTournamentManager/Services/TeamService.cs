using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Services
{
    // DTO chứa thông số thống kê của đội tuyển
    public class TeamStatsDto
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public int MatchesPlayed { get; set; }
        public double WinRate { get; set; }
        public double AvgKills { get; set; }
        public double AvgDeaths { get; set; }
        public double AvgAssists { get; set; }
        public double AvgDamage { get; set; }
        public double AvgCreep { get; set; }
    }

    // Dịch vụ quản lý các thao tác CRUD và truy vấn thông tin Đội tuyển (Team)
    public class TeamService
    {
        // Lấy danh sách tất cả các đội tuyển trong hệ thống kèm theo thống kê
        public List<TeamStatsDto> GetAllTeamsStats()
        {
            using (var db = new AppDbContext())
            {
                var teams = db.Teams.ToList();
                var result = new List<TeamStatsDto>();

                foreach (var t in teams)
                {
                    int teamId = t.TeamId;

                    // Get completed matches for this team
                    var matches = db.Matches
                        .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats.Select(ps => ps.Player)))
                        .Where(m => (m.Team1Id == teamId || m.Team2Id == teamId) && m.Status == "Completed")
                        .ToList();

                    int played = matches.Count;
                    int wins = matches.Count(m => m.WinnerTeamId == teamId);
                    double winRate = played > 0 ? (double)wins / played * 100 : 0.0;

                    var maps = matches.SelectMany(m => m.MatchMaps).ToList();
                    int mapCount = maps.Count;

                    double totalKills = 0;
                    double totalDeaths = 0;
                    double totalAssists = 0;
                    double totalDamage = 0;
                    double totalCreep = 0;

                    foreach (var map in maps)
                    {
                        var teamPlayerStats = map.PlayerStats.Where(ps => ps.Player != null && ps.Player.TeamId == teamId).ToList();
                        totalKills += teamPlayerStats.Sum(ps => ps.Kills);
                        totalDeaths += teamPlayerStats.Sum(ps => ps.Deaths);
                        totalAssists += teamPlayerStats.Sum(ps => ps.Assists);
                        totalDamage += teamPlayerStats.Sum(ps => ps.DamageDealt);
                        totalCreep += teamPlayerStats.Sum(ps => ps.CreepScore);
                    }

                    result.Add(new TeamStatsDto
                    {
                        TeamId = teamId,
                        Name = t.TeamName,
                        Region = t.Region ?? "Chưa rõ",
                        MatchesPlayed = played,
                        WinRate = winRate,
                        AvgKills = mapCount > 0 ? totalKills / mapCount : 0.0,
                        AvgDeaths = mapCount > 0 ? totalDeaths / mapCount : 0.0,
                        AvgAssists = mapCount > 0 ? totalAssists / mapCount : 0.0,
                        AvgDamage = mapCount > 0 ? totalDamage / mapCount : 0.0,
                        AvgCreep = mapCount > 0 ? totalCreep / mapCount : 0.0
                    });
                }

                return result.OrderBy(r => r.Name).ToList();
            }
        }

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