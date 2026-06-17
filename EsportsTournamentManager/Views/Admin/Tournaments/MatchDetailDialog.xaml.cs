using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.Admin.Tournaments
{
    public partial class MatchDetailDialog : Window
    {
        private readonly int _matchId;
        private readonly TournamentService _tournamentService;
        private Match _match;
        private List<MatchMap> _maps;

        public MatchDetailDialog(int matchId)
        {
            InitializeComponent();
            _matchId = matchId;
            _tournamentService = new TournamentService();
            _maps = new List<MatchMap>();

            LoadMatchData();
        }

        private void LoadMatchData()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _match = db.Matches
                        .Include(m => m.Team1.Players)
                        .Include(m => m.Team2.Players)
                        .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats.Select(ps => ps.Player)))
                        .FirstOrDefault(m => m.MatchId == _matchId);
                }

                if (_match == null)
                {
                    MessageBox.Show("Không tìm thấy trận đấu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // 1. Populate Team Names & Match Format
                TxtTeam1Name.Text = _match.Team1 != null ? _match.Team1.TeamName : "Chờ đội thắng...";
                TxtTeam2Name.Text = _match.Team2 != null ? _match.Team2.TeamName : "Chờ đội thắng...";
                TxtMatchFormat.Text = $"THỂ THỨC: {_match.MatchFormat}";

                // 2. Set Status ComboBox
                SetComboBoxSelectedContent(CboStatus, _match.Status);

                // 3. Disable inputs if teams are not decided
                bool hasTeams = _match.Team1Id.HasValue && _match.Team2Id.HasValue;
                CboStatus.IsEnabled = hasTeams;

                if (!hasTeams)
                {
                    TxtTeam1Score.Text = "0";
                    TxtTeam2Score.Text = "0";
                    TxtMatchMvp.Text = "Chưa xác định";
                    return;
                }

                // 4. Initialize Maps based on MatchFormat (BO1 = 1, BO3 = 3, BO5 = 5)
                int maxMaps = 1;
                if (_match.MatchFormat == "BO3") maxMaps = 3;
                else if (_match.MatchFormat == "BO5") maxMaps = 5;

                for (int i = 1; i <= maxMaps; i++)
                {
                    var existingMap = _match.MatchMaps.FirstOrDefault(mm => mm.MapNumber == i);
                    if (existingMap != null)
                    {
                        // Fill in missing players if any
                        EnsureAllPlayersHaveStats(existingMap);
                        _maps.Add(existingMap);
                    }
                    else
                    {
                        _maps.Add(CreateEmptyMap(i));
                    }
                }

                // 5. Render dynamic TabItems for Maps
                RenderMapTabs();

                // 6. Update summary scores and match MVP
                UpdateMatchScoresFromMaps();
                UpdateOverallMatchMvpDisplay();

                // 7. Manage Rollback button visibility
                BtnRollback.Visibility = _match.Status == "Completed" ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin trận đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void EnsureAllPlayersHaveStats(MatchMap map)
        {
            var existingPlayerIds = map.PlayerStats.Select(ps => ps.PlayerId).ToHashSet();

            if (_match.Team1 != null)
            {
                foreach (var p in _match.Team1.Players)
                {
                    if (!existingPlayerIds.Contains(p.PlayerId))
                    {
                        map.PlayerStats.Add(new PlayerStat { PlayerId = p.PlayerId, Player = p, MatchMap = map });
                    }
                }
            }

            if (_match.Team2 != null)
            {
                foreach (var p in _match.Team2.Players)
                {
                    if (!existingPlayerIds.Contains(p.PlayerId))
                    {
                        map.PlayerStats.Add(new PlayerStat { PlayerId = p.PlayerId, Player = p, MatchMap = map });
                    }
                }
            }
        }

        private MatchMap CreateEmptyMap(int mapNumber)
        {
            var map = new MatchMap
            {
                MatchId = _matchId,
                MapNumber = mapNumber,
                SelectedMapName = $"Ván {mapNumber}",
                Team1RoundScore = 0,
                Team2RoundScore = 0,
                PlayerStats = new List<PlayerStat>()
            };

            EnsureAllPlayersHaveStats(map);
            return map;
        }

        private void RenderMapTabs()
        {
            TabMaps.Items.Clear();
            foreach (var map in _maps)
            {
                var tabItem = new TabItem
                {
                    Header = $"Ván {map.MapNumber}"
                };
                tabItem.Content = CreateMapTabContent(map);
                TabMaps.Items.Add(tabItem);
            }
        }

        private UIElement CreateMapTabContent(MatchMap map)
        {
            var mainGrid = new Grid { Margin = new Thickness(10) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Top Panel (Map name, map score, MVP map text)
            var topPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

            topPanel.Children.Add(new TextBlock { Text = "Tên ván: ", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
            var txtMapName = new TextBox
            {
                Text = map.SelectedMapName,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                Padding = new Thickness(5),
                Width = 120,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };
            txtMapName.TextChanged += (s, e) => map.SelectedMapName = txtMapName.Text;
            topPanel.Children.Add(txtMapName);

            // MVP Map TextBlock
            var mvpMapText = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Margin = new Thickness(20, 0, 0, 0)
            };

            // Team 1 score
            topPanel.Children.Add(new TextBlock { Text = $"{_match.Team1?.Acronym} Điểm: ", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
            var txtT1Score = new TextBox
            {
                Text = map.Team1RoundScore.ToString(),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                Padding = new Thickness(5),
                Width = 40,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            txtT1Score.TextChanged += (s, e) => {
                int score;
                if (int.TryParse(txtT1Score.Text, out score))
                {
                    map.Team1RoundScore = score;
                    UpdateMatchScoresFromMaps();
                    UpdateMapMvpDisplay(map, mvpMapText);
                    UpdateOverallMatchMvpDisplay();
                }
            };
            topPanel.Children.Add(txtT1Score);

            // Team 2 score
            topPanel.Children.Add(new TextBlock { Text = $"{_match.Team2?.Acronym} Điểm: ", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
            var txtT2Score = new TextBox
            {
                Text = map.Team2RoundScore.ToString(),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                Padding = new Thickness(5),
                Width = 40,
                Height = 28,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            txtT2Score.TextChanged += (s, e) => {
                int score;
                if (int.TryParse(txtT2Score.Text, out score))
                {
                    map.Team2RoundScore = score;
                    UpdateMatchScoresFromMaps();
                    UpdateMapMvpDisplay(map, mvpMapText);
                    UpdateOverallMatchMvpDisplay();
                }
            };
            topPanel.Children.Add(txtT2Score);
            topPanel.Children.Add(mvpMapText);

            // DataGrids Panel (Grid of 2 columns)
            var splitGrid = new Grid();
            splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var t1Stats = map.PlayerStats.Where(ps => ps.Player.TeamId == _match.Team1Id).ToList();
            var t2Stats = map.PlayerStats.Where(ps => ps.Player.TeamId == _match.Team2Id).ToList();

            var grid1 = CreatePlayerStatsDataGrid(t1Stats, map, mvpMapText);
            grid1.Margin = new Thickness(0, 0, 8, 0);

            var grid2 = CreatePlayerStatsDataGrid(t2Stats, map, mvpMapText);
            grid2.Margin = new Thickness(8, 0, 0, 0);

            Grid.SetColumn(grid1, 0);
            Grid.SetColumn(grid2, 1);
            splitGrid.Children.Add(grid1);
            splitGrid.Children.Add(grid2);

            Grid.SetRow(topPanel, 0);
            Grid.SetRow(splitGrid, 1);
            mainGrid.Children.Add(topPanel);
            mainGrid.Children.Add(splitGrid);

            // Initial calculation
            UpdateMapMvpDisplay(map, mvpMapText);

            return mainGrid;
        }

        private DataGrid CreatePlayerStatsDataGrid(List<PlayerStat> source, MatchMap map, TextBlock mvpMapText)
        {
            var grid = new DataGrid
            {
                Style = FindResource("StatDataGrid") as Style,
                ItemsSource = source
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Tuyển thủ", Binding = new Binding("Player.InGameName"), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            grid.Columns.Add(new DataGridTextColumn { Header = "K", Binding = new Binding("Kills"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "D", Binding = new Binding("Deaths"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "A", Binding = new Binding("Assists"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Sát thương", Binding = new Binding("DamageDealt"), Width = 70 });
            grid.Columns.Add(new DataGridTextColumn { Header = "CS", Binding = new Binding("CreepScore"), Width = 45 });
            grid.Columns.Add(new DataGridTextColumn { Header = "PTS", Binding = new Binding("PerformancePoints") { StringFormat = "0.0" }, IsReadOnly = true, FontWeight = FontWeights.Bold, Width = 45 });

            grid.CellEditEnding += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    grid.CommitEdit(DataGridEditingUnit.Row, true);
                    grid.Items.Refresh();
                    UpdateMapMvpDisplay(map, mvpMapText);
                    UpdateOverallMatchMvpDisplay();
                }), System.Windows.Threading.DispatcherPriority.Background);
            };

            return grid;
        }

        private void UpdateMapMvpDisplay(MatchMap map, TextBlock textBlock)
        {
            if (map.PlayerStats == null || map.PlayerStats.Count == 0)
            {
                textBlock.Text = "★ MVP Ván: Chưa có";
                return;
            }

            int? winningTeamId = null;
            int? losingTeamId = null;

            if (map.Team1RoundScore > map.Team2RoundScore)
            {
                winningTeamId = _match.Team1Id;
                losingTeamId = _match.Team2Id;
            }
            else if (map.Team2RoundScore > map.Team1RoundScore)
            {
                winningTeamId = _match.Team2Id;
                losingTeamId = _match.Team1Id;
            }

            if (!winningTeamId.HasValue || !losingTeamId.HasValue)
            {
                textBlock.Inlines.Clear();
                textBlock.Text = "★ MVP Ván: Chưa xác định";
                return;
            }

            var winnerMvp = map.PlayerStats
                .Where(ps => ps.Player.TeamId == winningTeamId.Value)
                .OrderByDescending(ps => ps.PerformancePoints)
                .FirstOrDefault();

            var loserMvp = map.PlayerStats
                .Where(ps => ps.Player.TeamId == losingTeamId.Value)
                .OrderByDescending(ps => ps.PerformancePoints)
                .FirstOrDefault();

            textBlock.Inlines.Clear();

            if (winnerMvp != null && winnerMvp.PerformancePoints > 0)
            {
                textBlock.Inlines.Add(new System.Windows.Documents.Run("★ MVP Thắng: ") { Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")), FontWeight = FontWeights.Bold });
                textBlock.Inlines.Add(new System.Windows.Documents.Run($"{winnerMvp.Player.InGameName} ({winnerMvp.PerformancePoints:0.0}) ") { Foreground = Brushes.White, FontWeight = FontWeights.Bold });
            }
            else
            {
                textBlock.Inlines.Add(new System.Windows.Documents.Run("★ MVP Thắng: Chưa có ") { Foreground = Brushes.Gray });
            }

            textBlock.Inlines.Add(new System.Windows.Documents.Run(" | ") { Foreground = Brushes.Gray });

            if (loserMvp != null && loserMvp.PerformancePoints > 0)
            {
                textBlock.Inlines.Add(new System.Windows.Documents.Run("★ MVP Thua: ") { Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), FontWeight = FontWeights.Bold });
                textBlock.Inlines.Add(new System.Windows.Documents.Run($"{loserMvp.Player.InGameName} ({loserMvp.PerformancePoints:0.0})") { Foreground = Brushes.White, FontWeight = FontWeights.Bold });
            }
            else
            {
                textBlock.Inlines.Add(new System.Windows.Documents.Run("★ MVP Thua: Chưa có") { Foreground = Brushes.Gray });
            }
        }

        private void UpdateOverallMatchMvpDisplay()
        {
            var allStats = new List<PlayerStat>();
            foreach (var map in _maps)
            {
                if (map.Team1RoundScore > 0 || map.Team2RoundScore > 0 || map.PlayerStats.Any(ps => ps.Kills > 0 || ps.Deaths > 0 || ps.Assists > 0))
                {
                    allStats.AddRange(map.PlayerStats);
                }
            }

            if (allStats.Count == 0)
            {
                TxtMatchMvp.Text = "Chưa xác định";
                return;
            }

            var mvp = allStats.GroupBy(ps => ps.PlayerId)
                .Select(g => new {
                    PlayerId = g.Key,
                    InGameName = g.First().Player.InGameName,
                    AverageScore = g.Average(ps => ps.PerformancePoints)
                })
                .OrderByDescending(x => x.AverageScore)
                .FirstOrDefault();

            if (mvp != null && mvp.AverageScore > 0)
            {
                TxtMatchMvp.Text = $"{mvp.InGameName} (PTS TB: {mvp.AverageScore:0.0})";
            }
            else
            {
                TxtMatchMvp.Text = "Chưa xác định";
            }
        }

        private void UpdateMatchScoresFromMaps()
        {
            int t1Wins = 0;
            int t2Wins = 0;
            foreach (var map in _maps)
            {
                if (map.Team1RoundScore > map.Team2RoundScore)
                    t1Wins++;
                else if (map.Team2RoundScore > map.Team1RoundScore)
                    t2Wins++;
            }
            TxtTeam1Score.Text = t1Wins.ToString();
            TxtTeam2Score.Text = t2Wins.ToString();
        }

        private void SetComboBoxSelectedContent(System.Windows.Controls.ComboBox comboBox, string value)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CboStatus_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Just placeholder, status is read on save
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_match.Team1Id.HasValue || !_match.Team2Id.HasValue)
            {
                Close();
                return;
            }

            var selectedStatusItem = CboStatus.SelectedItem as System.Windows.Controls.ComboBoxItem;
            string status = selectedStatusItem.Content.ToString();

            // Calculate match wins
            int t1Wins = 0;
            int t2Wins = 0;
            foreach (var map in _maps)
            {
                if (map.Team1RoundScore > map.Team2RoundScore)
                    t1Wins++;
                else if (map.Team2RoundScore > map.Team1RoundScore)
                    t2Wins++;
            }

            if (status == "Completed" && t1Wins == t2Wins)
            {
                MessageBox.Show("Khi hoàn thành trận đấu, tỉ số hai đội không được bằng nhau (phải có đội thắng cuộc). Vui lòng nhập điểm các ván đấu chính xác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Filter maps that were actually played
            // A map is considered played if either scores are non-zero, or some stats were entered
            var playedMaps = _maps.Where(m => m.Team1RoundScore > 0 || m.Team2RoundScore > 0 || m.PlayerStats.Any(ps => ps.Kills > 0 || ps.Deaths > 0 || ps.Assists > 0)).ToList();

            if (playedMaps.Count == 0 && status == "Completed")
            {
                MessageBox.Show("Vui lòng nhập tỉ số và chỉ số cho ít nhất một ván đấu trước khi hoàn thành trận đấu.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _tournamentService.SaveMatchPerformance(_matchId, playedMaps, status);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật trận đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRollback_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Bạn có chắc chắn muốn HỦY kết quả của trận đấu này? Toàn bộ đội tuyển thắng cuộc ở các vòng đấu sau liên quan tới trận đấu này sẽ bị thu hồi và xóa dữ liệu.", "Cảnh báo thu hồi nhánh đấu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    _tournamentService.RollbackMatchResult(_matchId);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi thu hồi kết quả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TabMaps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
