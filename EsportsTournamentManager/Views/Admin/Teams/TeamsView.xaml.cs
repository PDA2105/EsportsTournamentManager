using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.Admin.Teams
{
    public partial class TeamsView : UserControl
    {
        private readonly TeamService _teamService;
        private readonly PlayerService _playerService;

        private List<Team> _allTeams;
        private Team _selectedTeam;

        public TeamsView()
        {
            InitializeComponent();
            _teamService = new TeamService();
            _playerService = new PlayerService();

            Loaded += TeamsView_Loaded;
        }

        private void TeamsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTeams();
        }

        public void LoadTeams()
        {
            try
            {
                _allTeams = _teamService.GetAllTeams();
                FilterTeams();

                // Populate Combo box for players panel
                ComboTeamsFilter.ItemsSource = _allTeams;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách đội tuyển: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterTeams()
        {
            if (_allTeams == null) return;

            string searchText = TxtSearchTeam.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                GridTeams.ItemsSource = _allTeams;
            }
            else
            {
                GridTeams.ItemsSource = _allTeams.Where(t => 
                    (t.TeamName != null && t.TeamName.ToLower().Contains(searchText)) || 
                    (t.Acronym != null && t.Acronym.ToLower().Contains(searchText)) ||
                    (t.Coach != null && t.Coach.ToLower().Contains(searchText))
                ).ToList();
            }
        }

        private void TxtSearchTeam_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTeams();
        }

        private void GridTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTeam = GridTeams.SelectedItem as Team;
        }

        private void BtnViewPlayers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Team team)
            {
                _selectedTeam = team;
                ComboTeamsFilter.SelectedValue = team.TeamId;
                ShowPlayersPanel();
            }
        }

        private void ShowPlayersPanel()
        {
            if (_selectedTeam != null)
            {
                PanelTeams.Visibility = Visibility.Collapsed;
                PanelPlayers.Visibility = Visibility.Visible;

                TxtPlayersHeader.Text = $"TUYỂN THỦ - {_selectedTeam.TeamName.ToUpper()} ({_selectedTeam.Acronym})";
                LoadPlayers();
            }
        }

        private void BtnBackToTeams_Click(object sender, RoutedEventArgs e)
        {
            PanelPlayers.Visibility = Visibility.Collapsed;
            PanelTeams.Visibility = Visibility.Visible;
            LoadTeams();
        }

        private void ComboTeamsFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboTeamsFilter.SelectedItem is Team team)
            {
                _selectedTeam = team;
                TxtPlayersHeader.Text = $"TUYỂN THỦ - {_selectedTeam.TeamName.ToUpper()} ({_selectedTeam.Acronym})";
                LoadPlayers();
            }
        }

        private void LoadPlayers()
        {
            if (_selectedTeam == null) return;

            try
            {
                var players = _playerService.GetPlayersByTeam(_selectedTeam.TeamId);
                GridPlayers.ItemsSource = players;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách tuyển thủ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================================
        // TEAMS CRUD HANDLERS
        // ==============================================
        private void BtnAddTeam_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TeamDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _teamService.AddTeam(dialog.Team);
                    LoadTeams();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm đội tuyển: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEditTeam_Click(object sender, RoutedEventArgs e)
        {
            Team teamToEdit = null;
            if (sender is Button btn && btn.DataContext is Team t)
            {
                teamToEdit = t;
            }
            else
            {
                teamToEdit = GridTeams.SelectedItem as Team;
            }

            if (teamToEdit != null)
            {
                var dialog = new TeamDialog(teamToEdit);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        _teamService.UpdateTeam(dialog.Team);
                        LoadTeams();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi cập nhật đội tuyển: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnDeleteTeam_Click(object sender, RoutedEventArgs e)
        {
            Team teamToDelete = null;
            if (sender is Button btn && btn.DataContext is Team t)
            {
                teamToDelete = t;
            }
            else
            {
                teamToDelete = GridTeams.SelectedItem as Team;
            }

            if (teamToDelete != null)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa đội tuyển '{teamToDelete.TeamName}'? Toàn bộ tuyển thủ thuộc đội tuyển này cũng sẽ bị xóa.", 
                                            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _teamService.DeleteTeam(teamToDelete.TeamId);
                        LoadTeams();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa đội tuyển: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ==============================================
        // PLAYERS CRUD HANDLERS
        // ==============================================
        private void BtnAddPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var dialog = new PlayerDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    dialog.Player.TeamId = _selectedTeam.TeamId;
                    _playerService.AddPlayer(dialog.Player);
                    LoadPlayers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm tuyển thủ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEditPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (GridPlayers.SelectedItem is Player player)
            {
                var dialog = new PlayerDialog(player);
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        _playerService.UpdatePlayer(dialog.Player);
                        LoadPlayers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi cập nhật tuyển thủ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnDeletePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (GridPlayers.SelectedItem is Player player)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tuyển thủ '{player.InGameName}'?", 
                                            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _playerService.DeletePlayer(player.PlayerId);
                        LoadPlayers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa tuyển thủ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
