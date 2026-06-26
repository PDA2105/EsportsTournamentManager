using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User.Players
{
    public partial class UserPlayersView : UserControl
    {
        private readonly PlayerService _playerService = new PlayerService();
        private List<PlayerStatsRow> _playersList;

        public event EventHandler<int> PlayerClicked;

        public UserPlayersView()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            try
            {
                var stats = _playerService.GetAllPlayersStats();
                _playersList = stats.Select(s => new PlayerStatsRow
                {
                    PlayerId = s.PlayerId,
                    Name = s.Name,
                    RealName = s.RealName,
                    TeamName = s.TeamName,
                    Kills = s.Kills,
                    Deaths = s.Deaths,
                    Assists = s.Assists,
                    Damage = s.Damage,
                    Creep = s.Creep,
                    MatchesPlayed = s.MatchesPlayed
                }).ToList();

                RenderPlayersGrid(_playersList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải danh sách tuyển thủ: {ex.Message}");
            }
        }

        private void RenderPlayersGrid(List<PlayerStatsRow> list)
        {
            GridPlayers.ItemsSource = list;
        }

        private void TxtSearchPlayer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_playersList == null) return;
            string q = TxtSearchPlayer.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(q))
            {
                RenderPlayersGrid(_playersList);
            }
            else
            {
                var filtered = _playersList.Where(p => 
                    p.Name.ToLower().Contains(q) || 
                    (p.RealName != null && p.RealName.ToLower().Contains(q)) || 
                    (p.TeamName != null && p.TeamName.ToLower().Contains(q))
                ).ToList();
                RenderPlayersGrid(filtered);
            }
        }

        private void PlayerName_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag is int playerId)
            {
                PlayerClicked?.Invoke(this, playerId);
            }
        }
    }

    public class PlayerStatsRow
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

        public string KillsDisplay => $"{Kills:0.0}";
        public string DeathsDisplay => $"{Deaths:0.0}";
        public string AssistsDisplay => $"{Assists:0.0}";
        public string DamageDisplay => $"{Damage:N0}";
        public string CreepDisplay => $"{Creep:0.0}";
    }
}
