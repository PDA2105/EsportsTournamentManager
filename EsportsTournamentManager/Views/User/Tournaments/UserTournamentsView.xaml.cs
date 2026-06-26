using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User.Tournaments
{
    public partial class UserTournamentsView : UserControl
    {
        private readonly TournamentService _tournamentService = new TournamentService();
        private List<Tournament> _tournamentsList;

        public event EventHandler<int> TournamentClicked;

        public UserTournamentsView()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            try
            {
                _tournamentsList = _tournamentService.GetAllTournaments();
                RenderTournamentsGrid(_tournamentsList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải danh sách giải đấu: {ex.Message}");
            }
        }

        private void RenderTournamentsGrid(List<Tournament> list)
        {
            var gridData = list.Select(t => {
                int numberOfGames = t.Matches != null ? t.Matches.Count(m => m.Status == "Completed") : 0;

                DateTime firstDate = t.StartDate;
                if (t.Matches != null && t.Matches.Any())
                {
                    firstDate = t.Matches.Min(m => m.ScheduledTime);
                }
                string firstGame = firstDate.ToString("yyyy-MM-dd");

                DateTime lastDate = t.EndDate ?? t.StartDate.AddDays(7);
                if (t.Matches != null && t.Matches.Any())
                {
                    lastDate = t.Matches.Max(m => m.ScheduledTime);
                }
                string lastGame = lastDate.ToString("yyyy-MM-dd");

                return new {
                    TournamentId = t.TournamentId,
                    Name = t.Name,
                    NumberOfGames = numberOfGames,
                    FirstGame = firstGame,
                    LastGame = lastGame
                };
            }).ToList();

            GridTournaments.ItemsSource = gridData;
        }

        private void TxtSearchTournament_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_tournamentsList == null) return;
            string q = TxtSearchTournament.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(q))
            {
                RenderTournamentsGrid(_tournamentsList);
            }
            else
            {
                var filtered = _tournamentsList.Where(t => t.Name.ToLower().Contains(q) || t.GameType.ToLower().Contains(q)).ToList();
                RenderTournamentsGrid(filtered);
            }
        }

        private void TournamentName_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag is int tournamentId)
            {
                TournamentClicked?.Invoke(this, tournamentId);
            }
        }
    }
}
