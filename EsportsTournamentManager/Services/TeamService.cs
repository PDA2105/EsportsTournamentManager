using EsportsTournamentManager.Models;
using System.Collections.Generic;

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
}