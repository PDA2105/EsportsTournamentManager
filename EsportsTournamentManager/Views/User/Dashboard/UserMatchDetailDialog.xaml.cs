using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.User
{
    public partial class UserMatchDetailDialog : Window
    {
        private readonly int _matchId;
        private Match _match;

        public event EventHandler<Team> TeamClicked;
        public event EventHandler<Player> PlayerClicked;

        public UserMatchDetailDialog(int matchId)
        {
            InitializeComponent();
            _matchId = matchId;
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
                        .Include(m => m.WinnerTeam)
                        .Include(m => m.MatchMaps.Select(mm => mm.PlayerStats.Select(ps => ps.Player)))
                        .FirstOrDefault(m => m.MatchId == _matchId);
                }

                if (_match == null)
                {
                    MessageBox.Show("Không tìm thấy trận đấu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // 1. Populate Team Names & Scores
                BtnTeam1Link.Content = _match.Team1 != null ? _match.Team1.TeamName : "Chờ đội tuyển...";
                BtnTeam2Link.Content = _match.Team2 != null ? _match.Team2.TeamName : "Chờ đội tuyển...";
                TxtTeam1Score.Text = _match.Team1Score.ToString();
                TxtTeam2Score.Text = _match.Team2Score.ToString();
                TxtMatchFormat.Text = $"THỂ THỨC: {_match.MatchFormat} | {_match.Status.ToUpper()}";

                // Fade losing team in scoreboard
                if (_match.Status == "Completed" && _match.WinnerTeamId.HasValue)
                {
                    if (_match.WinnerTeamId == _match.Team1Id)
                    {
                        BtnTeam2Link.Opacity = 0.5;
                    }
                    else if (_match.WinnerTeamId == _match.Team2Id)
                    {
                        BtnTeam1Link.Opacity = 0.5;
                    }
                }

                // 2. Fetch Match MVP
                var allStats = _match.MatchMaps.SelectMany(mm => mm.PlayerStats).ToList();
                if (allStats.Any())
                {
                    var mvpGroup = allStats.GroupBy(ps => ps.PlayerId)
                        .Select(g => new
                        {
                            Player = g.First().Player,
                            AveragePTS = g.Average(ps => ps.PerformancePoints)
                        })
                        .OrderByDescending(x => x.AveragePTS)
                        .FirstOrDefault();

                    if (mvpGroup != null && mvpGroup.AveragePTS > 0)
                    {
                        BtnMatchMvpLink.Content = $"{mvpGroup.Player.InGameName} (TB: {mvpGroup.AveragePTS:0.0} PTS)";
                        BtnMatchMvpLink.Tag = mvpGroup.Player;
                    }
                    else
                    {
                        BtnMatchMvpLink.Content = "Chưa có";
                        BtnMatchMvpLink.Tag = null;
                    }
                }
                else
                {
                    BtnMatchMvpLink.Content = "Chưa có";
                    BtnMatchMvpLink.Tag = null;
                }

                // 3. Tab 1: Map List Overview
                ItemsMapsList.ItemsSource = _match.MatchMaps.OrderBy(mm => mm.MapNumber).ToList();

                // 4. Tab 2: Player Stats per Map Tabs
                RenderPlayerStatsTabs();

                // 5. Tab 3: Objectives & Averages Comparison
                RenderObjectivesComparison(allStats);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin trận đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void RenderPlayerStatsTabs()
        {
            TabPlayerStatsMaps.Items.Clear();

            var playedMaps = _match.MatchMaps.OrderBy(mm => mm.MapNumber).ToList();
            if (!playedMaps.Any())
            {
                var emptyTab = new TabItem { Header = "Chưa thi đấu" };
                emptyTab.Content = new TextBlock
                {
                    Text = "Trận đấu chưa diễn ra hoặc chưa cập nhật số liệu ván đấu.",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 13,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                TabPlayerStatsMaps.Items.Add(emptyTab);
                return;
            }

            foreach (var map in playedMaps)
            {
                var tabItem = new TabItem
                {
                    Header = map.SelectedMapName ?? $"Ván {map.MapNumber}"
                };

                // Split grid into two columns for Team 1 and Team 2
                var splitGrid = new Grid { Margin = new Thickness(10) };
                splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                splitGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var t1Stats = map.PlayerStats.Where(ps => ps.Player.TeamId == _match.Team1Id).ToList();
                var t2Stats = map.PlayerStats.Where(ps => ps.Player.TeamId == _match.Team2Id).ToList();

                var grid1 = CreateReadOnlyStatsGrid(t1Stats);
                grid1.Margin = new Thickness(0, 0, 10, 0);

                var grid2 = CreateReadOnlyStatsGrid(t2Stats);
                grid2.Margin = new Thickness(10, 0, 0, 0);

                Grid.SetColumn(grid1, 0);
                Grid.SetColumn(grid2, 1);
                splitGrid.Children.Add(grid1);
                splitGrid.Children.Add(grid2);

                tabItem.Content = splitGrid;
                TabPlayerStatsMaps.Items.Add(tabItem);
            }
        }

        private DataGrid CreateReadOnlyStatsGrid(List<PlayerStat> stats)
        {
            var grid = new DataGrid
            {
                Style = FindResource("StatDataGrid") as Style,
                ItemsSource = stats
            };

            // Player InGameName Template Column (Hyperlink Clickable)
            var colPlayer = new DataGridTemplateColumn
            {
                Header = "Tuyển thủ",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };

            var btnFactory = new FrameworkElementFactory(typeof(Button));
            btnFactory.SetBinding(Button.ContentProperty, new Binding("Player.InGameName"));
            btnFactory.SetValue(Button.StyleProperty, FindResource("HyperlinkButton"));
            btnFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, e) =>
            {
                var button = s as Button;
                var stat = button?.DataContext as PlayerStat;
                if (stat?.Player != null)
                {
                    PlayerClicked?.Invoke(this, stat.Player);
                    Close();
                }
            }));

            colPlayer.CellTemplate = new DataTemplate { VisualTree = btnFactory };

            grid.Columns.Add(colPlayer);
            grid.Columns.Add(new DataGridTextColumn { Header = "K", Binding = new Binding("Kills"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "D", Binding = new Binding("Deaths"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "A", Binding = new Binding("Assists"), Width = 35 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Sát thương", Binding = new Binding("DamageDealt"), Width = 70 });
            grid.Columns.Add(new DataGridTextColumn { Header = "CS", Binding = new Binding("CreepScore"), Width = 45 });
            grid.Columns.Add(new DataGridTextColumn { Header = "PTS", Binding = new Binding("PerformancePoints") { StringFormat = "0.0" }, FontWeight = FontWeights.Bold, Width = 45 });

            return grid;
        }

        private void RenderObjectivesComparison(List<PlayerStat> allStats)
        {
            if (!allStats.Any())
            {
                TxtT1Kills.Text = "0 Kills";
                TxtT2Kills.Text = "0 Kills";
                TxtT1Damage.Text = "0 DMG";
                TxtT2Damage.Text = "0 DMG";
                TxtT1CS.Text = "0 CS";
                TxtT2CS.Text = "0 CS";
                TxtT1Dragons.Text = "0 Rồng";
                TxtT2Dragons.Text = "0 Rồng";
                TxtT1Towers.Text = "0 Trụ";
                TxtT2Towers.Text = "0 Trụ";

                ColT1Kills.Width = new GridLength(1, GridUnitType.Star);
                ColT2Kills.Width = new GridLength(1, GridUnitType.Star);
                ColT1Damage.Width = new GridLength(1, GridUnitType.Star);
                ColT2Damage.Width = new GridLength(1, GridUnitType.Star);
                ColT1CS.Width = new GridLength(1, GridUnitType.Star);
                ColT2CS.Width = new GridLength(1, GridUnitType.Star);
                ColT1Dragons.Width = new GridLength(1, GridUnitType.Star);
                ColT2Dragons.Width = new GridLength(1, GridUnitType.Star);
                ColT1Towers.Width = new GridLength(1, GridUnitType.Star);
                ColT2Towers.Width = new GridLength(1, GridUnitType.Star);
                return;
            }

            int t1Kills = allStats.Where(ps => ps.Player.TeamId == _match.Team1Id).Sum(ps => ps.Kills);
            int t2Kills = allStats.Where(ps => ps.Player.TeamId == _match.Team2Id).Sum(ps => ps.Kills);
            int t1Dmg = allStats.Where(ps => ps.Player.TeamId == _match.Team1Id).Sum(ps => ps.DamageDealt);
            int t2Dmg = allStats.Where(ps => ps.Player.TeamId == _match.Team2Id).Sum(ps => ps.DamageDealt);
            int t1CS = allStats.Where(ps => ps.Player.TeamId == _match.Team1Id).Sum(ps => ps.CreepScore);
            int t2CS = allStats.Where(ps => ps.Player.TeamId == _match.Team2Id).Sum(ps => ps.CreepScore);

            int t1Dragons = _match.MatchMaps.Sum(mm => mm.Team1DragonsKilled);
            int t2Dragons = _match.MatchMaps.Sum(mm => mm.Team2DragonsKilled);
            int t1Towers = _match.MatchMaps.Sum(mm => mm.Team1TowersDestroyed);
            int t2Towers = _match.MatchMaps.Sum(mm => mm.Team2TowersDestroyed);

            TxtT1Kills.Text = $"{t1Kills} Kills";
            TxtT2Kills.Text = $"{t2Kills} Kills";
            TxtT1Damage.Text = $"{t1Dmg:N0} DMG";
            TxtT2Damage.Text = $"{t2Dmg:N0} DMG";
            TxtT1CS.Text = $"{t1CS} CS";
            TxtT2CS.Text = $"{t2CS} CS";
            TxtT1Dragons.Text = $"{t1Dragons} Rồng";
            TxtT2Dragons.Text = $"{t2Dragons} Rồng";
            TxtT1Towers.Text = $"{t1Towers} Trụ";
            TxtT2Towers.Text = $"{t2Towers} Trụ";

            double totalKills = Math.Max(1, t1Kills + t2Kills);
            ColT1Kills.Width = new GridLength(t1Kills / totalKills, GridUnitType.Star);
            ColT2Kills.Width = new GridLength(t2Kills / totalKills, GridUnitType.Star);

            double totalDmg = Math.Max(1, t1Dmg + t2Dmg);
            ColT1Damage.Width = new GridLength(t1Dmg / totalDmg, GridUnitType.Star);
            ColT2Damage.Width = new GridLength(t2Dmg / totalDmg, GridUnitType.Star);

            double totalCS = Math.Max(1, t1CS + t2CS);
            ColT1CS.Width = new GridLength(t1CS / totalCS, GridUnitType.Star);
            ColT2CS.Width = new GridLength(t2CS / totalCS, GridUnitType.Star);

            double totalDragons = Math.Max(1, t1Dragons + t2Dragons);
            ColT1Dragons.Width = new GridLength(t1Dragons / totalDragons, GridUnitType.Star);
            ColT2Dragons.Width = new GridLength(t2Dragons / totalDragons, GridUnitType.Star);

            double totalTowers = Math.Max(1, t1Towers + t2Towers);
            ColT1Towers.Width = new GridLength(t1Towers / totalTowers, GridUnitType.Star);
            ColT2Towers.Width = new GridLength(t2Towers / totalTowers, GridUnitType.Star);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Team1Link_Click(object sender, RoutedEventArgs e)
        {
            if (_match?.Team1 != null)
            {
                TeamClicked?.Invoke(this, _match.Team1);
                Close();
            }
        }

        private void Team2Link_Click(object sender, RoutedEventArgs e)
        {
            if (_match?.Team2 != null)
            {
                TeamClicked?.Invoke(this, _match.Team2);
                Close();
            }
        }

        private void PlayerLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var player = button?.Tag as Player;
            if (player != null)
            {
                PlayerClicked?.Invoke(this, player);
                Close();
            }
        }
    }
}
