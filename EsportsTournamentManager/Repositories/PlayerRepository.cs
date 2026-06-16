using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Repositories
{
    public class PlayerRepository
    {
        public List<Player> GetPlayersByTeam(int teamId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Where(p => p.TeamId == teamId).ToList();
            }
        }

        public Player GetById(int playerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Players.Find(playerId);
            }
        }

        public void Add(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Players.Add(player);
                db.SaveChanges();
            }
        }

        public void Update(Player player)
        {
            using (var db = new AppDbContext())
            {
                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Delete(int playerId)
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
