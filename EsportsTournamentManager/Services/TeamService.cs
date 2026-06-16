using System.Collections.Generic;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Repositories;

namespace EsportsTournamentManager.Services
{
    public class TeamService
    {
        private readonly TeamRepository _repo;

        public TeamService()
        {
            _repo = new TeamRepository();
        }

        public List<Team> GetAllTeams()
        {
            return _repo.GetAll();
        }

        public Team GetTeamById(int teamId)
        {
            return _repo.GetById(teamId);
        }

        public void AddTeam(Team team)
        {
            _repo.Add(team);
        }

        public void UpdateTeam(Team team)
        {
            _repo.Update(team);
        }

        public void DeleteTeam(int teamId)
        {
            _repo.Delete(teamId);
        }
    }
}