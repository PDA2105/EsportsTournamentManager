using System.Collections.Generic;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Repositories;

namespace EsportsTournamentManager.Services
{
    public class PlayerService
    {
        private readonly PlayerRepository _repo;

        public PlayerService()
        {
            _repo = new PlayerRepository();
        }

        public List<Player> GetPlayersByTeam(int teamId)
        {
            return _repo.GetPlayersByTeam(teamId);
        }

        public Player GetPlayerById(int playerId)
        {
            return _repo.GetById(playerId);
        }

        public void AddPlayer(Player player)
        {
            _repo.Add(player);
        }

        public void UpdatePlayer(Player player)
        {
            _repo.Update(player);
        }

        public void DeletePlayer(int playerId)
        {
            _repo.Delete(playerId);
        }
    }
}
