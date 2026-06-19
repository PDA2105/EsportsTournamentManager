using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.Admin.Tournaments
{
    public partial class TournamentsView : UserControl
    {
        private readonly TournamentService _tournamentService;
        private readonly TeamService _teamService;
        private List<Tournament> _tournamentsList;
        private Tournament _selectedTournament;
        private List<TeamSelectionItem> _teamSelectionList;

        public TournamentsView()
        {
            InitializeComponent();
            _tournamentService = new TournamentService();
            _teamService = new TeamService();
            _teamSelectionList = new List<TeamSelectionItem>();

            LoadTournaments();
        }

        private void LoadTournaments()
        {
            try
            {
                _tournamentsList = _tournamentService.GetAllTournaments();
                GridTournaments.ItemsSource = _tournamentsList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearchTournament_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearchTournament.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(query))
            {
                GridTournaments.ItemsSource = _tournamentsList;
            }
            else
            {
                var filtered = _tournamentsList.Where(t => t.Name.ToLower().Contains(query) || t.GameType.ToLower().Contains(query)).ToList();
                GridTournaments.ItemsSource = filtered;
            }
        }

        private void BtnAddTournament_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TournamentDialog();
            var owner = Window.GetWindow(this);
            if (owner != null) dialog.Owner = owner;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Default user id for creation - we can mock or fetch active user
                    dialog.Tournament.CreatedByUserId = 1; // Seeded Administrator
                    _tournamentService.AddTournament(dialog.Tournament);
                    LoadTournaments();
                    MessageBox.Show("Tạo giải đấu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tạo giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEditTournament_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var tournament = button.DataContext as Tournament;
            if (tournament == null) return;

            var dialog = new TournamentDialog(tournament);
            var owner = Window.GetWindow(this);
            if (owner != null) dialog.Owner = owner;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _tournamentService.UpdateTournament(dialog.Tournament);
                    LoadTournaments();
                    MessageBox.Show("Cập nhật giải đấu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi cập nhật giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDeleteTournament_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var tournament = button.DataContext as Tournament;
            if (tournament == null) return;

            var res = MessageBox.Show($"Bạn có chắc chắn muốn xóa giải đấu '{tournament.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    _tournamentService.DeleteTournament(tournament.TournamentId);
                    LoadTournaments();
                    MessageBox.Show("Xóa giải đấu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var tournament = button.DataContext as Tournament;
            if (tournament == null) return;

            ShowTournamentDetails(tournament.TournamentId);
        }

        private void GridTournaments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Do nothing on row selection, we use explicit action buttons
        }

        private void BtnBackToTournaments_Click(object sender, RoutedEventArgs e)
        {
            PanelTournamentOverview.Visibility = Visibility.Collapsed;
            PanelTournamentBracket.Visibility = Visibility.Collapsed;
            PanelTournaments.Visibility = Visibility.Visible;
            LoadTournaments();
        }

        private void BtnViewBracket_Click(object sender, RoutedEventArgs e)
        {
            PanelTournamentOverview.Visibility = Visibility.Collapsed;
            PanelTournamentBracket.Visibility = Visibility.Visible;
        }

        private void BtnBackToOverview_Click(object sender, RoutedEventArgs e)
        {
            PanelTournamentBracket.Visibility = Visibility.Collapsed;
            PanelTournamentOverview.Visibility = Visibility.Visible;
        }

        private void ShowTournamentDetails(int tournamentId)
        {
            try
            {
                _selectedTournament = _tournamentService.GetTournamentById(tournamentId);
                if (_selectedTournament == null) return;

                // 1. Populate UI properties
                TxtTournamentDetailHeader.Text = $"TỔNG QUAN GIẢI ĐẤU - {_selectedTournament.Name.ToUpper()}";
                TxtTournamentBracketHeader.Text = $"NHÁNH ĐẤU & KẾT QUẢ - {_selectedTournament.Name.ToUpper()}";
                TxtInfoGameType.Text = _selectedTournament.GameType;
                switch (_selectedTournament.Format)
                {
                    case "SingleElimination":
                        TxtInfoFormat.Text = "Loại trực tiếp";
                        break;
                    case "DoubleElimination":
                        TxtInfoFormat.Text = "Nhánh thắng nhánh thua";
                        break;
                    case "RoundRobin":
                        TxtInfoFormat.Text = "Vòng tròn tính điểm";
                        break;
                    default:
                        TxtInfoFormat.Text = _selectedTournament.Format;
                        break;
                }
                TxtInfoMaxTeams.Text = $"{_selectedTournament.MaxTeams} Đội tối đa";

                // Format status string
                string statusText = "Chờ bắt đầu";
                string statusColor = "#38BDF8"; // Sky blue
                if (_selectedTournament.Status == "Active")
                {
                    statusText = "Đang diễn ra";
                    statusColor = "#818CF8"; // Indigo
                }
                else if (_selectedTournament.Status == "Completed")
                {
                    statusText = "Đã kết thúc";
                    statusColor = "#10B981"; // Green
                }
                TxtInfoStatus.Text = statusText;
                TxtInfoStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(statusColor));

                // Load Tournament Champion
                var championTeam = GetTournamentChampion(_selectedTournament);
                if (championTeam != null)
                {
                    TxtChampionName.Text = championTeam.TeamName;
                    PanelTournamentChampion.Visibility = Visibility.Visible;
                    TxtChampionNotDecided.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PanelTournamentChampion.Visibility = Visibility.Collapsed;
                    TxtChampionNotDecided.Visibility = Visibility.Visible;
                }
 
                // Load Tournament MVP
                double avgMvpScore;
                var mvpPlayer = _tournamentService.GetTournamentMvp(_selectedTournament.TournamentId, out avgMvpScore);
                if (mvpPlayer != null)
                {
                    TxtMvpName.Text = mvpPlayer.InGameName;
                    TxtMvpTeam.Text = mvpPlayer.Team != null ? mvpPlayer.Team.TeamName : "Tự do";
                    TxtMvpScore.Text = $"Điểm TB: {avgMvpScore:0.0}";
                    PanelTournamentMvp.Visibility = Visibility.Visible;
                    TxtMvpNotDecided.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PanelTournamentMvp.Visibility = Visibility.Collapsed;
                    TxtMvpNotDecided.Visibility = Visibility.Visible;
                }

                // 2. Transition panels
                if (PanelTournamentOverview.Visibility != Visibility.Visible && PanelTournamentBracket.Visibility != Visibility.Visible)
                {
                    PanelTournaments.Visibility = Visibility.Collapsed;
                    PanelTournamentBracket.Visibility = Visibility.Collapsed;
                    PanelTournamentOverview.Visibility = Visibility.Visible;
                }

                // 3. Render left list (Checkbox selection for Pending, simple list for Active/Completed)
                if (_selectedTournament.Status == "Pending")
                {
                    TxtDynamicListHeader.Text = "CHỌN ĐỘI TUYỂN THAM GIA";
                    ListSelectTeams.Visibility = Visibility.Visible;
                    ListShowTeams.Visibility = Visibility.Collapsed;
                    BtnStartTournament.Visibility = Visibility.Visible;

                    // Load all teams and determine which are checked
                    var allTeams = _teamService.GetAllTeams();
                    var assignedTeamIds = _selectedTournament.TournamentTeams.Select(tt => tt.TeamId).ToHashSet();

                    _teamSelectionList = allTeams.Select(t => new TeamSelectionItem
                    {
                        TeamId = t.TeamId,
                        TeamName = t.TeamName,
                        IsAssigned = assignedTeamIds.Contains(t.TeamId)
                    }).ToList();

                    ListSelectTeams.ItemsSource = _teamSelectionList;
                }
                else
                {
                    TxtDynamicListHeader.Text = "ĐỘI TUYỂN THAM GIA";
                    ListSelectTeams.Visibility = Visibility.Collapsed;
                    ListShowTeams.Visibility = Visibility.Visible;
                    BtnStartTournament.Visibility = Visibility.Collapsed;

                    var assignedTeams = _selectedTournament.TournamentTeams.Select(tt => tt.Team).ToList();
                    ListShowTeams.ItemsSource = assignedTeams;
                }

                // 4. Render Bracket visual representation
                RenderBracketVisuals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị chi tiết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TeamCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Sync selected teams list in DB
            if (_selectedTournament == null || _selectedTournament.Status != "Pending") return;

            var checkedTeamIds = _teamSelectionList
                .Where(t => t.IsAssigned)
                .Select(t => t.TeamId)
                .ToList();

            try
            {
                _tournamentService.SaveTournamentTeams(_selectedTournament.TournamentId, checkedTeamIds);
                // Refresh local tournament object
                _selectedTournament = _tournamentService.GetTournamentById(_selectedTournament.TournamentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu đội tuyển: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStartTournament_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTournament == null) return;

            int count = _teamSelectionList.Count(t => t.IsAssigned);
            if (_selectedTournament.Format == "SingleElimination" && count != 4 && count != 8 && count != 16)
            {
                MessageBox.Show("Vui lòng chọn chính xác 4, 8 hoặc 16 đội tuyển để bắt đầu giải đấu loại trực tiếp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_selectedTournament.Format == "RoundRobin" && count < 2)
            {
                MessageBox.Show("Vui lòng chọn tối thiểu 2 đội tuyển để bắt đầu giải đấu vòng tròn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Bạn có chắc chắn muốn bắt đầu giải đấu? Lịch thi đấu và nhánh đấu sẽ được tự động thiết lập và không thể thay đổi danh sách đội tham gia.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _tournamentService.StartTournament(_selectedTournament.TournamentId);
                    ShowTournamentDetails(_selectedTournament.TournamentId);
                    MessageBox.Show("Giải đấu đã bắt đầu! Sơ đồ nhánh đấu đã được tạo thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi bắt đầu giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenderBracketVisuals()
        {
            if (_selectedTournament.Status == "Pending")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                CanvasBracket.Children.Clear();
                CanvasBracket.Width = 600;
                CanvasBracket.Height = 400;

                var prompt = new TextBlock
                {
                    Text = "Giải đấu chưa bắt đầu.\nNhấn nút 'Bắt đầu Giải đấu' ở cột bên trái để sinh lịch thi đấu.",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 14,
                    FontStyle = FontStyles.Italic,
                    TextAlignment = TextAlignment.Center,
                    Width = 500
                };
                Canvas.SetLeft(prompt, 50);
                Canvas.SetTop(prompt, 150);
                CanvasBracket.Children.Add(prompt);
                return;
            }

            if (_selectedTournament.Format == "SingleElimination")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                RenderSingleEliminationTree();
            }
            else if (_selectedTournament.Format == "DoubleElimination")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                RenderDoubleEliminationTree();
            }
            else
            {
                CanvasBracket.Visibility = Visibility.Collapsed;
                ScrollRoundRobin.Visibility = Visibility.Visible;
                RenderRoundRobinList();
            }
        }

        private string GetRoundName(int round, int maxRounds)
        {
            if (round == maxRounds) return "CHUNG KẾT";
            if (round == maxRounds - 1) return "BÁN KẾT";
            if (round == maxRounds - 2) return "TỨ KẾT";
            return $"VÒNG {round}";
        }

        private void RenderSingleEliminationTree()
        {
            CanvasBracket.Children.Clear();

            var matches = _selectedTournament.Matches.ToList();
            if (matches.Count == 0) return;

            int N = _selectedTournament.TournamentTeams.Count;
            int numRounds = (int)Math.Log(N, 2);

            double colWidth = 260;
            double cardWidth = 190;
            double cardHeight = 84;
            double spacing = 40;
            double topPadding = 40;

            CanvasBracket.Width = numRounds * colWidth + 50;
            CanvasBracket.Height = Math.Max(600, (N / 2) * (cardHeight + spacing) + topPadding * 2);

            var roundMatches = new List<Match>[numRounds + 1];
            for (int r = 1; r <= numRounds; r++)
            {
                roundMatches[r] = matches.Where(m => m.RoundNumber == r).OrderBy(m => m.MatchOrder).ToList();
            }

            // Dict to keep Y coordinates by key: Tuple<round, order>
            var yCoords = new Dictionary<Tuple<int, int>, double>();

            // 1. Calculate Y for Round 1
            for (int i = 0; i < roundMatches[1].Count; i++)
            {
                double y = i * (cardHeight + spacing) + topPadding;
                yCoords[new Tuple<int, int>(1, i + 1)] = y;
            }

            // 2. Calculate Y for subsequent rounds
            for (int r = 2; r <= numRounds; r++)
            {
                for (int i = 0; i < roundMatches[r].Count; i++)
                {
                    double y1 = yCoords[new Tuple<int, int>(r - 1, 2 * i + 1)];
                    double y2 = yCoords[new Tuple<int, int>(r - 1, 2 * i + 2)];
                    yCoords[new Tuple<int, int>(r, i + 1)] = (y1 + y2) / 2;
                }
            }

            // 3. Draw column headers
            for (int r = 1; r <= numRounds; r++)
            {
                double x = (r - 1) * colWidth + 20;
                var headerText = new TextBlock
                {
                    Text = GetRoundName(r, numRounds),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(headerText, x + (cardWidth - 100) / 2);
                Canvas.SetTop(headerText, 10);
                CanvasBracket.Children.Add(headerText);
            }

            // 4. Draw Match Cards and Connector Lines
            for (int r = 1; r <= numRounds; r++)
            {
                double x = (r - 1) * colWidth + 20;
                var rMatches = roundMatches[r];

                for (int i = 0; i < rMatches.Count; i++)
                {
                    var match = rMatches[i];
                    double y = yCoords[new Tuple<int, int>(r, i + 1)];

                    var card = CreateMatchCard(match);
                    Canvas.SetLeft(card, x);
                    Canvas.SetTop(card, y);
                    CanvasBracket.Children.Add(card);

                    // Draw connector line to next round match
                    if (match.NextMatchId.HasValue)
                    {
                        var nextMatch = matches.FirstOrDefault(m => m.MatchId == match.NextMatchId.Value);
                        if (nextMatch != null)
                        {
                            double nextX = r * colWidth + 20;
                            double nextY = yCoords[new Tuple<int, int>(nextMatch.RoundNumber, nextMatch.MatchOrder)];

                            double startX = x + cardWidth;
                            double startY = y + cardHeight / 2;
                            double endX = nextX;
                            double endY = nextY + cardHeight / 2;
                            double midX = (startX + endX) / 2;

                            var polyline = new Polyline
                            {
                                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                                    match.Status == "Completed" ? "#6366F1" : "#334155")),
                                StrokeThickness = match.Status == "Completed" ? 2 : 1.5
                            };

                            polyline.Points.Add(new Point(startX, startY));
                            polyline.Points.Add(new Point(midX, startY));
                            polyline.Points.Add(new Point(midX, endY));
                            polyline.Points.Add(new Point(endX, endY));

                            CanvasBracket.Children.Add(polyline);
                        }
                    }
                }
            }
        }

        private Border CreateMatchCard(Match match)
        {
            var border = new Border
            {
                Width = 190,
                Height = 84,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                    match.Status == "Live" ? "#06B6D4" : 
                    match.Status == "Completed" ? "#475569" : "#334155")),
                BorderThickness = new Thickness(match.Status == "Live" ? 2 : 1),
                CornerRadius = new CornerRadius(8),
                Cursor = Cursors.Hand,
                Padding = new Thickness(12, 6, 12, 6)
            };

            border.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    OpenMatchDetailDialog(match.MatchId);
                }
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Team 1
            var team1Panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var t1Color = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")), Margin = new Thickness(0, 0, 8, 0) };
            var t1Text = new TextBlock
            {
                Text = match.Team1 != null ? match.Team1.TeamName : "Chờ đội thắng...",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(match.Team1 != null ? "#F9FAFB" : "#6B7280")),
                FontSize = 12,
                FontWeight = match.WinnerTeamId == match.Team1Id && match.WinnerTeamId.HasValue ? FontWeights.Bold : FontWeights.Normal,
                MaxWidth = 130,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            team1Panel.Children.Add(t1Color);
            team1Panel.Children.Add(t1Text);
            Grid.SetRow(team1Panel, 0);
            grid.Children.Add(team1Panel);

            var t1Score = new TextBlock
            {
                Text = match.Status == "Scheduled" ? "-" : match.Team1Score.ToString(),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(match.WinnerTeamId == match.Team1Id && match.WinnerTeamId.HasValue ? "#10B981" : "#F9FAFB")),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(t1Score, 0);
            grid.Children.Add(t1Score);

            // Team 2
            var team2Panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var t2Color = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")), Margin = new Thickness(0, 0, 8, 0) };
            var t2Text = new TextBlock
            {
                Text = match.Team2 != null ? match.Team2.TeamName : "Chờ đội thắng...",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(match.Team2 != null ? "#F9FAFB" : "#6B7280")),
                FontSize = 12,
                FontWeight = match.WinnerTeamId == match.Team2Id && match.WinnerTeamId.HasValue ? FontWeights.Bold : FontWeights.Normal,
                MaxWidth = 130,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            team2Panel.Children.Add(t2Color);
            team2Panel.Children.Add(t2Text);
            Grid.SetRow(team2Panel, 1);
            grid.Children.Add(team2Panel);

            var t2Score = new TextBlock
            {
                Text = match.Status == "Scheduled" ? "-" : match.Team2Score.ToString(),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(match.WinnerTeamId == match.Team2Id && match.WinnerTeamId.HasValue ? "#10B981" : "#F9FAFB")),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(t2Score, 1);
            grid.Children.Add(t2Score);

            // Fade losing team
            if (match.Status == "Completed" && match.WinnerTeamId.HasValue)
            {
                if (match.WinnerTeamId == match.Team1Id)
                {
                    team2Panel.Opacity = 0.4;
                    t2Score.Opacity = 0.4;
                }
                else if (match.WinnerTeamId == match.Team2Id)
                {
                    team1Panel.Opacity = 0.4;
                    t1Score.Opacity = 0.4;
                }
            }

            border.Child = grid;
            return border;
        }

        private void RenderRoundRobinList()
        {
            PanelRoundRobin.Children.Clear();

            var matches = _selectedTournament.Matches.ToList();
            if (matches.Count == 0) return;

            var rounds = matches.GroupBy(m => m.RoundNumber).OrderBy(g => g.Key).ToList();

            foreach (var roundGroup in rounds)
            {
                var roundHeader = new TextBlock
                {
                    Text = $"VÒNG ĐẤU {roundGroup.Key}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 15, 0, 10)
                };
                PanelRoundRobin.Children.Add(roundHeader);

                var matchesPanel = new WrapPanel { Orientation = Orientation.Horizontal };
                foreach (var match in roundGroup.OrderBy(m => m.MatchOrder))
                {
                    var card = CreateMatchCard(match);
                    card.Margin = new Thickness(0, 0, 15, 10);
                    matchesPanel.Children.Add(card);
                }
                PanelRoundRobin.Children.Add(matchesPanel);
            }
        }

        private void RenderDoubleEliminationTree()
        {
            CanvasBracket.Children.Clear();

            var matches = _selectedTournament.Matches.ToList();
            if (matches.Count == 0) return;

            int maxTeams = _selectedTournament.TournamentTeams.Count;

            if (maxTeams == 4)
            {
                CanvasBracket.Width = 780;
                CanvasBracket.Height = 520;

                // 1. Draw Column Headers
                DrawHeader("NHÁNH THẮNG - VÒNG 1", 20, 20);
                DrawHeader("CHUNG KẾT NHÁNH THẮNG", 280, 20);
                DrawHeader("NHÁNH THUA - VÒNG 1", 20, 310);
                DrawHeader("CHUNG KẾT NHÁNH THUA", 280, 310);
                DrawHeader("CHUNG KẾT TỔNG", 540, 120);

                // 2. Fetch matches
                var w1 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                var w3 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);

                var l1 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var l2 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 1);

                var gf = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 3 && m.MatchOrder == 1);

                // Place cards
                if (w1 != null) PlaceMatchCard(w1, 20, 60);
                if (w2 != null) PlaceMatchCard(w2, 20, 180);
                if (w3 != null) PlaceMatchCard(w3, 280, 120);
                if (l1 != null) PlaceMatchCard(l1, 20, 360);
                if (l2 != null) PlaceMatchCard(l2, 280, 360);
                if (gf != null) PlaceMatchCard(gf, 540, 240);

                // 3. Draw Connectors
                // W1 & W2 to W3
                DrawLine(210, 102, 250, 102, w1?.Status == "Completed");
                DrawLine(210, 222, 250, 222, w2?.Status == "Completed");
                DrawLine(250, 102, 250, 222, w1?.Status == "Completed" || w2?.Status == "Completed");
                DrawLine(250, 162, 280, 162, w1?.Status == "Completed" || w2?.Status == "Completed");

                // L1 to L2
                DrawLine(210, 402, 280, 402, l1?.Status == "Completed");

                // W3 to GF
                DrawLine(470, 162, 510, 162, w3?.Status == "Completed");
                DrawLine(510, 162, 510, 282, w3?.Status == "Completed");

                // L2 to GF
                DrawLine(470, 402, 510, 402, l2?.Status == "Completed");
                DrawLine(510, 402, 510, 282, l2?.Status == "Completed");

                // Connector into GF card (drawn if either W3 or L2 is completed)
                DrawLine(510, 282, 540, 282, w3?.Status == "Completed" || l2?.Status == "Completed");
            }
            else if (maxTeams == 8)
            {
                CanvasBracket.Width = 1300;
                CanvasBracket.Height = 820;

                // 1. Draw Column Headers
                DrawHeader("NHÁNH THẮNG - VÒNG 1", 20, 20);
                DrawHeader("NHÁNH THẮNG - BÁN KẾT", 280, 20);
                DrawHeader("CHUNG KẾT NHÁNH THẮNG", 540, 20);
                DrawHeader("NHÁNH THUA - VÒNG 1", 20, 450);
                DrawHeader("NHÁNH THUA - VÒNG 2", 280, 450);
                DrawHeader("NHÁNH THUA - BÁN KẾT", 540, 450);
                DrawHeader("CHUNG KẾT NHÁNH THUA", 800, 450);
                DrawHeader("CHUNG KẾT TỔNG", 1060, 220);

                // 2. Fetch matches
                var w1 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                var w3 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 3);
                var w4 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 4);

                var w5 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var w6 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 2);

                var w7 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 3 && m.MatchOrder == 1);

                var l1 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var l2 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 2);

                var l3 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 1);
                var l4 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 2);

                var l5 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 3 && m.MatchOrder == 1);
                var l6 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 4 && m.MatchOrder == 1);

                var gf = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 4 && m.MatchOrder == 1);

                // Place cards
                if (w1 != null) PlaceMatchCard(w1, 20, 60);
                if (w2 != null) PlaceMatchCard(w2, 20, 160);
                if (w3 != null) PlaceMatchCard(w3, 20, 260);
                if (w4 != null) PlaceMatchCard(w4, 20, 360);

                if (w5 != null) PlaceMatchCard(w5, 280, 110);
                if (w6 != null) PlaceMatchCard(w6, 280, 310);

                if (w7 != null) PlaceMatchCard(w7, 540, 210);

                if (l1 != null) PlaceMatchCard(l1, 20, 500);
                if (l2 != null) PlaceMatchCard(l2, 20, 620);

                if (l3 != null) PlaceMatchCard(l3, 280, 500);
                if (l4 != null) PlaceMatchCard(l4, 280, 620);

                if (l5 != null) PlaceMatchCard(l5, 540, 560);
                if (l6 != null) PlaceMatchCard(l6, 800, 560);

                if (gf != null) PlaceMatchCard(gf, 1060, 385);

                // 3. Draw Connectors (Winner Branch)
                // W1 & W2 to W5
                DrawLine(210, 102, 250, 102, w1?.Status == "Completed");
                DrawLine(210, 202, 250, 202, w2?.Status == "Completed");
                DrawLine(250, 102, 250, 202, w1?.Status == "Completed" || w2?.Status == "Completed");
                DrawLine(250, 152, 280, 152, w1?.Status == "Completed" || w2?.Status == "Completed");

                // W3 & W4 to W6
                DrawLine(210, 302, 250, 302, w3?.Status == "Completed");
                DrawLine(210, 402, 250, 402, w4?.Status == "Completed");
                DrawLine(250, 302, 250, 402, w3?.Status == "Completed" || w4?.Status == "Completed");
                DrawLine(250, 352, 280, 352, w3?.Status == "Completed" || w4?.Status == "Completed");

                // W5 & W6 to W7
                DrawLine(470, 152, 510, 152, w5?.Status == "Completed");
                DrawLine(470, 352, 510, 352, w6?.Status == "Completed");
                DrawLine(510, 152, 510, 352, w5?.Status == "Completed" || w6?.Status == "Completed");
                DrawLine(510, 252, 540, 252, w5?.Status == "Completed" || w6?.Status == "Completed");

                // Connectors (Loser Branch)
                // L1 to L3, L2 to L4
                DrawLine(210, 542, 280, 542, l1?.Status == "Completed");
                DrawLine(210, 662, 280, 662, l2?.Status == "Completed");

                // L3 & L4 to L5
                DrawLine(470, 542, 510, 542, l3?.Status == "Completed");
                DrawLine(470, 662, 510, 662, l4?.Status == "Completed");
                DrawLine(510, 542, 510, 662, l3?.Status == "Completed" || l4?.Status == "Completed");
                DrawLine(510, 602, 540, 602, l3?.Status == "Completed" || l4?.Status == "Completed");

                // L5 to L6
                DrawLine(730, 602, 800, 602, l5?.Status == "Completed");

                // W7 to GF
                DrawLine(730, 252, 1020, 252, w7?.Status == "Completed");
                DrawLine(1020, 252, 1020, 427, w7?.Status == "Completed");

                // L6 to GF
                DrawLine(990, 602, 1020, 602, l6?.Status == "Completed");
                DrawLine(1020, 602, 1020, 427, l6?.Status == "Completed");

                // Connector into GF card (drawn if either W7 or L6 is completed)
                DrawLine(1020, 427, 1060, 427, w7?.Status == "Completed" || l6?.Status == "Completed");
            }
        }

        private void DrawHeader(string text, double x, double y)
        {
            var headerText = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 190,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(headerText, x);
            Canvas.SetTop(headerText, y);
            CanvasBracket.Children.Add(headerText);
        }

        private void PlaceMatchCard(Match match, double x, double y)
        {
            var card = CreateMatchCard(match);
            Canvas.SetLeft(card, x);
            Canvas.SetTop(card, y);
            CanvasBracket.Children.Add(card);
        }

        private void DrawLine(double x1, double y1, double x2, double y2, bool active)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(active ? "#6366F1" : "#334155")),
                StrokeThickness = active ? 2 : 1.5
            };
            CanvasBracket.Children.Add(line);
        }

        private void OpenMatchDetailDialog(int matchId)
        {
            var dialog = new MatchDetailDialog(matchId);
            var owner = Window.GetWindow(this);
            if (owner != null) dialog.Owner = owner;

            if (dialog.ShowDialog() == true)
            {
                // Refresh visual tree
                ShowTournamentDetails(_selectedTournament.TournamentId);
            }
        }

        private Team GetTournamentChampion(Tournament tournament)
        {
            if (tournament == null || tournament.Status != "Completed") return null;
 
            if (tournament.Format == "SingleElimination" || tournament.Format == "DoubleElimination")
            {
                var grandFinal = tournament.Matches.FirstOrDefault(m => !m.NextMatchId.HasValue);
                if (grandFinal != null && grandFinal.Status == "Completed")
                {
                    return grandFinal.WinnerTeam;
                }
            }
            else if (tournament.Format == "RoundRobin")
            {
                var teamWins = new Dictionary<int, int>();
                foreach (var match in tournament.Matches)
                {
                    if (match.Status == "Completed" && match.WinnerTeamId.HasValue)
                    {
                        int winnerId = match.WinnerTeamId.Value;
                        if (!teamWins.ContainsKey(winnerId))
                            teamWins[winnerId] = 0;
                        teamWins[winnerId]++;
                    }
                }
                if (teamWins.Count > 0)
                {
                    var championTeamId = teamWins.OrderByDescending(kv => kv.Value).First().Key;
                    return tournament.TournamentTeams
                        .Select(tt => tt.Team)
                        .FirstOrDefault(t => t != null && t.TeamId == championTeamId);
                }
            }
            return null;
        }
    }

    public class TeamSelectionItem
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
