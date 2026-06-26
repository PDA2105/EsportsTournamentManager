using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;
using EsportsTournamentManager.Services;

namespace EsportsTournamentManager.Views.User
{
    public partial class UserMain : UserControl
    {
        public event EventHandler LogoutRequested;

        private readonly TournamentService _tournamentService = new TournamentService();
        private readonly TeamService _teamService = new TeamService();
        private readonly PlayerService _playerService = new PlayerService();

        private Tournament _currentTournament;

        // Navigation History Stack
        private readonly Stack<UIElement> _navigationHistory = new Stack<UIElement>();
        private UIElement _currentPanel;

        public UserMain()
        {
            InitializeComponent();

            // Hook up events of the new Dashboard & Tournaments UserControls
            PanelDashboard.MatchClicked += (s, matchId) => OpenMatchDetailDialog(matchId);
            PanelDashboard.TeamClicked += (s, teamId) => LoadTeamDetail(teamId);
            PanelDashboard.PlayerClicked += (s, playerId) => LoadPlayerDetail(playerId);
            PanelDashboard.TournamentClicked += (s, tournamentId) => LoadTournamentOverview(tournamentId);
            PanelDashboard.ViewAllTournamentsRequested += (s, ev) => BtnMenuTournaments_Click(s, null);
            PanelDashboard.ViewAllTeamsRequested += (s, ev) => BtnMenuTeams_Click(s, null);

            PanelTournaments.TournamentClicked += (s, tournamentId) => LoadTournamentOverview(tournamentId);

            PanelPlayers.PlayerClicked += (s, playerId) => LoadPlayerDetail(playerId);

            PanelTeamsUser.TeamClicked += (s, teamId) => LoadTeamDetail(teamId);

            // Hook up events of the new TournamentOverview UserControl
            PanelTournamentOverview.BackClicked += (s, e) => BtnBackToHome_Click(s, null);
            PanelTournamentOverview.ViewBracketClicked += (s, tour) => PanelTournamentOverview_ViewBracketClicked(s, tour);
            PanelTournamentOverview.TeamClicked += (s, team) => LoadTeamDetail(team.TeamId);
            PanelTournamentOverview.PlayerClicked += (s, player) => LoadPlayerDetail(player.PlayerId);
            PanelTournamentOverview.MatchClicked += (s, matchId) => OpenMatchDetailDialog(matchId);
        }

        private void PanelTournamentOverview_ViewBracketClicked(object sender, Tournament tournament)
        {
            _currentTournament = tournament;
            TxtTournamentBracketHeader.Text = $"SƠ ĐỒ NHÁNH ĐẤU & LỊCH THI ĐẤU - {_currentTournament.Name.ToUpper()}";
            RenderBracketVisuals(_currentTournament);
            ShowPanel(PanelTournamentBracket);
        }

        public void SetUser(string fullName)
        {
            TxtUserHeaderUserName.Text = fullName;
            TxtUserHeaderUserRole.Text = "Người xem";

            // Initialize to Home Dashboard
            _navigationHistory.Clear();
            _currentPanel = PanelDashboard;
            PanelDashboard.LoadData();
            ShowPanel(PanelDashboard, false);
            SwitchActiveMenu(BtnMenuHome);
        }

        private void ShowPanel(UIElement panelToShow, bool saveToHistory = true)
        {
            if (saveToHistory && _currentPanel != null && _currentPanel != panelToShow)
            {
                _navigationHistory.Push(_currentPanel);
            }

            var panels = new UIElement[]
            {
                PanelDashboard,
                PanelTournaments,
                PanelTournamentOverview,
                PanelTournamentBracket,
                PanelTeamDetail,
                PanelPlayerDetail,
                PanelPlayers,
                PanelTeamsUser
            };

            foreach (var p in panels)
            {
                p.Visibility = (p == panelToShow) ? Visibility.Visible : Visibility.Collapsed;
            }

            _currentPanel = panelToShow;
        }

        private void BtnNavigateBack_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationHistory.Count > 0)
            {
                var prevPanel = _navigationHistory.Pop();
                ShowPanel(prevPanel, false);
            }
        }

        private void BtnBackToHome_Click(object sender, RoutedEventArgs e)
        {
            _navigationHistory.Clear();
            PanelDashboard.LoadData();
            ShowPanel(PanelDashboard, false);
            SwitchActiveMenu(BtnMenuHome);
        }

        private void BtnMenuHome_Click(object sender, RoutedEventArgs e)
        {
            SwitchActiveMenu(BtnMenuHome);
            _navigationHistory.Clear();
            PanelDashboard.LoadData();
            ShowPanel(PanelDashboard, false);
        }

        private void BtnMenuTournaments_Click(object sender, RoutedEventArgs e)
        {
            SwitchActiveMenu(BtnMenuTournaments);
            _navigationHistory.Clear();
            PanelTournaments.LoadData();
            ShowPanel(PanelTournaments, false);
        }

        private void BtnMenuPlayers_Click(object sender, RoutedEventArgs e)
        {
            SwitchActiveMenu(BtnMenuPlayers);
            _navigationHistory.Clear();
            PanelPlayers.LoadData();
            ShowPanel(PanelPlayers, false);
        }

        private void BtnMenuTeams_Click(object sender, RoutedEventArgs e)
        {
            SwitchActiveMenu(BtnMenuTeams);
            _navigationHistory.Clear();
            PanelTeamsUser.LoadData();
            ShowPanel(PanelTeamsUser, false);
        }

        private void SwitchActiveMenu(Button activeButton)
        {
            Style normalStyle = (Style)FindResource("SidebarButton");
            Style activeStyle = (Style)FindResource("ActiveSidebarButton");

            BtnMenuHome.Style = normalStyle;
            BtnMenuTournaments.Style = normalStyle;
            BtnMenuPlayers.Style = normalStyle;
            BtnMenuTeams.Style = normalStyle;

            if (activeButton != null)
            {
                activeButton.Style = activeStyle;
            }
        }

        private void BtnBackToOverview_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(PanelTournamentOverview, true);
        }

        // ==========================================
        // 2. TOURNAMENT OVERVIEW PANEL
        // ==========================================
        private void LoadTournamentOverview(int tournamentId)
        {
            try
            {
                _currentTournament = _tournamentService.GetTournamentById(tournamentId);
                if (_currentTournament == null) return;

                PanelTournamentOverview.LoadTournament(tournamentId);
                ShowPanel(PanelTournamentOverview);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị tổng quan giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MatchRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var border = sender as Border;
                var match = border?.DataContext as Match;
                if (match != null)
                {
                    OpenMatchDetailDialog(match.MatchId);
                }
            }
        }

        // ==========================================
        // 3. TOURNAMENT BRACKET PANEL (DYNAMIC CANVAS DRAWING)
        // ==========================================
        private void BtnViewBracket_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTournament == null) return;
            TxtTournamentBracketHeader.Text = $"SƠ ĐỒ NHÁNH ĐẤU & LỊCH THI ĐẤU - {_currentTournament.Name.ToUpper()}";
            RenderBracketVisuals(_currentTournament);
            ShowPanel(PanelTournamentBracket);
        }

        private void RenderBracketVisuals(Tournament tournament)
        {
            if (tournament.Status == "Pending")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                CanvasBracket.Children.Clear();
                CanvasBracket.Width = 600;
                CanvasBracket.Height = 400;

                var prompt = new TextBlock
                {
                    Text = "Giải đấu chưa bắt đầu.\nĐang đợi Quản trị viên sinh lịch thi đấu.",
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

            if (tournament.Format == "SingleElimination")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                RenderSingleEliminationTree(tournament);
            }
            else if (tournament.Format == "DoubleElimination")
            {
                CanvasBracket.Visibility = Visibility.Visible;
                ScrollRoundRobin.Visibility = Visibility.Collapsed;
                RenderDoubleEliminationTree(tournament);
            }
            else
            {
                CanvasBracket.Visibility = Visibility.Collapsed;
                ScrollRoundRobin.Visibility = Visibility.Visible;
                RenderRoundRobinList(tournament);
            }
        }

        private string GetRoundName(int round, int maxRounds)
        {
            if (round == maxRounds) return "CHUNG KẾT";
            if (round == maxRounds - 1) return "BÁN KẾT";
            if (round == maxRounds - 2) return "TỨ KẾT";
            return $"VÒNG {round}";
        }

        private void RenderSingleEliminationTree(Tournament tournament)
        {
            CanvasBracket.Children.Clear();

            var matches = tournament.Matches.ToList();
            if (matches.Count == 0) return;

            int N = tournament.TournamentTeams.Count;
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

            var yCoords = new Dictionary<Tuple<int, int>, double>();

            for (int i = 0; i < roundMatches[1].Count; i++)
            {
                double y = i * (cardHeight + spacing) + topPadding;
                yCoords[new Tuple<int, int>(1, i + 1)] = y;
            }

            for (int r = 2; r <= numRounds; r++)
            {
                for (int i = 0; i < roundMatches[r].Count; i++)
                {
                    double y1 = yCoords[new Tuple<int, int>(r - 1, 2 * i + 1)];
                    double y2 = yCoords[new Tuple<int, int>(r - 1, 2 * i + 2)];
                    yCoords[new Tuple<int, int>(r, i + 1)] = (y1 + y2) / 2;
                }
            }

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
                Padding = new Thickness(12, 6, 12, 6),
                DataContext = match
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

        private void RenderRoundRobinList(Tournament tournament)
        {
            PanelRoundRobin.Children.Clear();

            var matches = tournament.Matches.ToList();
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

        private void RenderDoubleEliminationTree(Tournament tournament)
        {
            CanvasBracket.Children.Clear();

            var matches = tournament.Matches.ToList();
            if (matches.Count == 0) return;

            int maxTeams = tournament.TournamentTeams.Count;

            if (maxTeams == 4)
            {
                CanvasBracket.Width = 780;
                CanvasBracket.Height = 520;

                DrawHeader("NHÁNH THẮNG - VÒNG 1", 20, 20);
                DrawHeader("CHUNG KẾT NHÁNH THẮNG", 280, 20);
                DrawHeader("NHÁNH THUA - VÒNG 1", 20, 310);
                DrawHeader("CHUNG KẾT NHÁNH THUA", 280, 310);
                DrawHeader("CHUNG KẾT TỔNG", 540, 120);

                var w1 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var w2 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 1 && m.MatchOrder == 2);
                var w3 = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 2 && m.MatchOrder == 1);

                var l1 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 1 && m.MatchOrder == 1);
                var l2 = matches.FirstOrDefault(m => m.BracketBranch == "Loser" && m.RoundNumber == 2 && m.MatchOrder == 1);

                var gf = matches.FirstOrDefault(m => m.BracketBranch == "Winner" && m.RoundNumber == 3 && m.MatchOrder == 1);

                if (w1 != null) PlaceMatchCard(w1, 20, 60);
                if (w2 != null) PlaceMatchCard(w2, 20, 180);
                if (w3 != null) PlaceMatchCard(w3, 280, 120);
                if (l1 != null) PlaceMatchCard(l1, 20, 360);
                if (l2 != null) PlaceMatchCard(l2, 280, 360);
                if (gf != null) PlaceMatchCard(gf, 540, 240);

                DrawLine(210, 102, 250, 102, w1?.Status == "Completed");
                DrawLine(210, 222, 250, 222, w2?.Status == "Completed");
                DrawLine(250, 102, 250, 222, w1?.Status == "Completed" || w2?.Status == "Completed");
                DrawLine(250, 162, 280, 162, w1?.Status == "Completed" || w2?.Status == "Completed");

                DrawLine(210, 402, 280, 402, l1?.Status == "Completed");

                DrawLine(470, 162, 510, 162, w3?.Status == "Completed");
                DrawLine(510, 162, 510, 282, w3?.Status == "Completed");

                DrawLine(470, 402, 510, 402, l2?.Status == "Completed");
                DrawLine(510, 402, 510, 282, l2?.Status == "Completed");

                DrawLine(510, 282, 540, 282, w3?.Status == "Completed" || l2?.Status == "Completed");
            }
            else if (maxTeams == 8)
            {
                CanvasBracket.Width = 1300;
                CanvasBracket.Height = 820;

                DrawHeader("NHÁNH THẮNG - VÒNG 1", 20, 20);
                DrawHeader("NHÁNH THẮNG - BÁN KẾT", 280, 20);
                DrawHeader("CHUNG KẾT NHÁNH THẮNG", 540, 20);
                DrawHeader("NHÁNH THUA - VÒNG 1", 20, 450);
                DrawHeader("NHÁNH THUA - VÒNG 2", 280, 450);
                DrawHeader("NHÁNH THUA - BÁN KẾT", 540, 450);
                DrawHeader("CHUNG KẾT NHÁNH THUA", 800, 450);
                DrawHeader("CHUNG KẾT TỔNG", 1060, 220);

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

                DrawLine(210, 102, 250, 102, w1?.Status == "Completed");
                DrawLine(210, 202, 250, 202, w2?.Status == "Completed");
                DrawLine(250, 102, 250, 202, w1?.Status == "Completed" || w2?.Status == "Completed");
                DrawLine(250, 152, 280, 152, w1?.Status == "Completed" || w2?.Status == "Completed");

                DrawLine(210, 302, 250, 302, w3?.Status == "Completed");
                DrawLine(210, 402, 250, 402, w4?.Status == "Completed");
                DrawLine(250, 302, 250, 402, w3?.Status == "Completed" || w4?.Status == "Completed");
                DrawLine(250, 352, 280, 352, w3?.Status == "Completed" || w4?.Status == "Completed");

                DrawLine(470, 152, 510, 152, w5?.Status == "Completed");
                DrawLine(470, 352, 510, 352, w6?.Status == "Completed");
                DrawLine(510, 152, 510, 352, w5?.Status == "Completed" || w6?.Status == "Completed");
                DrawLine(510, 252, 540, 252, w5?.Status == "Completed" || w6?.Status == "Completed");

                DrawLine(210, 542, 280, 542, l1?.Status == "Completed");
                DrawLine(210, 662, 280, 662, l2?.Status == "Completed");

                DrawLine(470, 542, 510, 542, l3?.Status == "Completed");
                DrawLine(470, 662, 510, 662, l4?.Status == "Completed");
                DrawLine(510, 542, 510, 662, l3?.Status == "Completed" || l4?.Status == "Completed");
                DrawLine(510, 602, 540, 602, l3?.Status == "Completed" || l4?.Status == "Completed");

                DrawLine(730, 602, 800, 602, l5?.Status == "Completed");

                DrawLine(730, 252, 1020, 252, w7?.Status == "Completed");
                DrawLine(1020, 252, 1020, 427, w7?.Status == "Completed");

                DrawLine(990, 602, 1020, 602, l6?.Status == "Completed");
                DrawLine(1020, 602, 1020, 427, l6?.Status == "Completed");

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
            var dialog = new UserMatchDetailDialog(matchId);
            var owner = Window.GetWindow(this);
            if (owner != null) dialog.Owner = owner;

            dialog.TeamClicked += (s, team) => {
                LoadTeamDetail(team.TeamId);
            };
            dialog.PlayerClicked += (s, player) => {
                LoadPlayerDetail(player.PlayerId);
            };

            dialog.ShowDialog();

            // Refresh the current view data to get updated scores
            if (_currentTournament != null)
            {
                _currentTournament = _tournamentService.GetTournamentById(_currentTournament.TournamentId);
                LoadTournamentOverview(_currentTournament.TournamentId);
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

        // ==========================================
        // 4. TEAM DETAIL PANEL
        // ==========================================
        public void LoadTeamDetail(int teamId)
        {
            try
            {
                var team = _teamService.GetTeamById(teamId);
                if (team == null) return;

                TxtTeamNameTitle.Text = team.TeamName;
                TxtTeamCoach.Text = $"HLV: {team.Coach ?? "Chưa rõ"}";

                var stats = _tournamentService.GetTeamDetailStats(teamId);
                TxtTeamWinRate.Text = stats.WinRateDisplay;
                TxtTeamAvgKills.Text = $"{stats.AvgKills:0.1}";
                TxtTeamAvgDamage.Text = $"{stats.AvgDamage:N0}";
                TxtTeamAvgCS.Text = $"{stats.AvgCS:0.1}";

                // Bind players
                ListTeamPlayers.ItemsSource = _playerService.GetPlayersByTeam(teamId);

                // Bind recent matches
                ListTeamRecentMatches.ItemsSource = stats.RecentMatches;

                ShowPanel(PanelTeamDetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin đội tuyển: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // 5. PLAYER DETAIL PANEL
        // ==========================================
        public void LoadPlayerDetail(int playerId)
        {
            try
            {
                var player = _playerService.GetPlayerById(playerId);
                if (player == null) return;

                TxtPlayerInGameName.Text = player.InGameName;
                TxtPlayerRealName.Text = player.RealName ?? "";

                TxtPlayerNationality.Text = player.Nationality ?? "Chưa rõ";

                if (player.DateOfBirth.HasValue)
                {
                    int age = DateTime.Today.Year - player.DateOfBirth.Value.Year;
                    if (player.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;
                    TxtPlayerAge.Text = $"{age} tuổi";
                }
                else
                {
                    TxtPlayerAge.Text = "Chưa rõ tuổi";
                }

                if (!string.IsNullOrWhiteSpace(player.ImagePath))
                {
                    try
                    {
                        Uri imageUri = null;
                        if (Uri.TryCreate(player.ImagePath, UriKind.Absolute, out imageUri) ||
                            Uri.TryCreate(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, player.ImagePath), UriKind.Absolute, out imageUri))
                        {
                            var bitmap = new System.Windows.Media.Imaging.BitmapImage(imageUri);
                            ImgPlayerAvatar.Source = bitmap;
                            TxtPlayerPlaceholder.Visibility = Visibility.Collapsed;
                            ImgPlayerAvatar.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ImgPlayerAvatar.Source = null;
                            TxtPlayerPlaceholder.Visibility = Visibility.Visible;
                            ImgPlayerAvatar.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch
                    {
                        ImgPlayerAvatar.Source = null;
                        TxtPlayerPlaceholder.Visibility = Visibility.Visible;
                        ImgPlayerAvatar.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ImgPlayerAvatar.Source = null;
                    TxtPlayerPlaceholder.Visibility = Visibility.Visible;
                    ImgPlayerAvatar.Visibility = Visibility.Collapsed;
                }

                if (player.Team != null)
                {
                    TxtPlayerTeam.Content = player.Team.TeamName;
                    TxtPlayerTeam.Tag = player.Team;
                    TxtPlayerTeam.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtPlayerTeam.Visibility = Visibility.Collapsed;
                }

                var stats = _tournamentService.GetPlayerDetailStats(playerId);
                var allPlayerStats = _playerService.GetAllPlayersStats();
                var pStats = allPlayerStats.FirstOrDefault(p => p.PlayerId == playerId);

                if (stats != null)
                {
                    if (pStats != null)
                    {
                        TxtPlayerAvgKDA.Text = $"{pStats.Kills:0.0} / {pStats.Deaths:0.0} / {pStats.Assists:0.0}";
                        TxtPlayerAvgDamage.Text = $"{pStats.Damage:N0}";
                        TxtPlayerAvgCS.Text = $"{pStats.Creep:0.0}";
                        TxtPlayerMatchesPlayed.Text = pStats.MatchesPlayed.ToString();
                    }
                    else
                    {
                        TxtPlayerAvgKDA.Text = $"{stats.AvgKills:0.0} / {stats.AvgDeaths:0.0} / {stats.AvgAssists:0.0}";
                        TxtPlayerAvgDamage.Text = $"{stats.AvgDamage:N0}";
                        TxtPlayerAvgCS.Text = $"{stats.AvgCS:0.0}";
                        TxtPlayerMatchesPlayed.Text = stats.MatchesPlayed.ToString();
                    }
                    TxtPlayerAvgPTS.Text = $"{stats.AvgPTS:0.0}";
                    TxtPlayerPosition.Text = stats.Position ?? "Tự do";
                    TxtPlayerMvpCount.Text = stats.MvpCount.ToString();

                    ListPlayerRecentMatches.ItemsSource = stats.RecentMatches;
                }

                ShowPanel(PanelPlayerDetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin tuyển thủ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // LINK CLICK ROUTING HANDLERS
        // ==========================================
        private void TeamLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag is Team team)
            {
                LoadTeamDetail(team.TeamId);
            }
            else if (btn.DataContext is Team teamCtx)
            {
                LoadTeamDetail(teamCtx.TeamId);
            }
            else if (btn.DataContext != null)
            {
                // Project stats
                var prop = btn.DataContext.GetType().GetProperty("TeamId");
                if (prop != null)
                {
                    int teamId = (int)prop.GetValue(btn.DataContext);
                    LoadTeamDetail(teamId);
                }
            }
        }

        private void PlayerLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag is Player player)
            {
                LoadPlayerDetail(player.PlayerId);
            }
            else if (btn.DataContext is Player playerCtx)
            {
                LoadPlayerDetail(playerCtx.PlayerId);
            }
            else if (btn.DataContext != null)
            {
                var prop = btn.DataContext.GetType().GetProperty("PlayerId");
                if (prop != null)
                {
                    int playerId = (int)prop.GetValue(btn.DataContext);
                    LoadPlayerDetail(playerId);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
