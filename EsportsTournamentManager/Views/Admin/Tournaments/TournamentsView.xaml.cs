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
            PanelTournamentDetail.Visibility = Visibility.Collapsed;
            PanelTournaments.Visibility = Visibility.Visible;
            LoadTournaments();
        }

        private void ShowTournamentDetails(int tournamentId)
        {
            try
            {
                _selectedTournament = _tournamentService.GetTournamentById(tournamentId);
                if (_selectedTournament == null) return;

                // 1. Populate UI properties
                TxtTournamentDetailHeader.Text = $"CHI TIẾT GIẢI ĐẤU - {_selectedTournament.Name.ToUpper()}";
                TxtInfoGameType.Text = _selectedTournament.GameType;
                TxtInfoFormat.Text = _selectedTournament.Format == "SingleElimination" ? "Loại trực tiếp" : "Vòng tròn tính điểm";
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

                // 2. Transition panels
                PanelTournaments.Visibility = Visibility.Collapsed;
                PanelTournamentDetail.Visibility = Visibility.Visible;

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

            int N = _selectedTournament.MaxTeams;
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
    }

    public class TeamSelectionItem
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
