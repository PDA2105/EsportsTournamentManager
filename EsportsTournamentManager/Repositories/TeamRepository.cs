using System.Collections.Generic;
using System.Linq;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

public class TeamRepository
{
    public List<Team> GetAll()
    {
        using (var db = new AppDbContext())
        {
            return db.Teams.ToList();
        }
    }

    public void Add(Team team)
    {
        using (var db = new AppDbContext())
        {
            db.Teams.Add(team);
            db.SaveChanges();
        }
    }
}