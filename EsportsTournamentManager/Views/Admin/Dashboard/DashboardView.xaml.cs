using System;
using System.Linq;
using System.Windows.Controls;
using EsportsTournamentManager.Data;

namespace EsportsTournamentManager.Views.Admin.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public void LoadStatistics()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    int teamsCount = db.Teams.Count();
                    int playersCount = db.Players.Count();
                    int tournamentsCount = db.Tournaments.Count();

                    TxtStatsTeams.Text = teamsCount.ToString();
                    TxtStatsPlayers.Text = playersCount.ToString();
                    TxtStatsTournaments.Text = tournamentsCount.ToString();
                }
            }
            catch
            {
                TxtStatsTeams.Text = "N/A";
                TxtStatsPlayers.Text = "N/A";
                TxtStatsTournaments.Text = "N/A";
            }
        }
    }
}
