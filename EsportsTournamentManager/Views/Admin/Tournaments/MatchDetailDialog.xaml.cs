using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        public MatchDetailDialog(int matchId)
        {
            InitializeComponent();
            _matchId = matchId;
            _tournamentService = new TournamentService();

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
                        .Include(m => m.MatchMaps.Select(mm => mm.MVPlayer))
                        .FirstOrDefault(m => m.MatchId == _matchId);
                }

                if (_match == null)
                {
                    MessageBox.Show("Không tìm thấy trận đấu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // 1. Populate Team Names
                TxtTeam1Name.Text = _match.Team1 != null ? _match.Team1.TeamName : "Chờ đội thắng...";
                TxtTeam2Name.Text = _match.Team2 != null ? _match.Team2.TeamName : "Chờ đội thắng...";

                // 2. Populate Scores
                TxtTeam1Score.Text = _match.Team1Score.ToString();
                TxtTeam2Score.Text = _match.Team2Score.ToString();

                // 3. Disable inputs if teams are not decided
                bool hasTeams = _match.Team1Id.HasValue && _match.Team2Id.HasValue;
                TxtTeam1Score.IsEnabled = hasTeams;
                TxtTeam2Score.IsEnabled = hasTeams;
                CboStatus.IsEnabled = hasTeams;
                CboMvp.IsEnabled = hasTeams;

                // 4. Set Status ComboBox
                SetComboBoxSelectedContent(CboStatus, _match.Status);

                // 5. Build MVP selection list
                var mvpList = new List<MvpSelectItem>();
                mvpList.Add(new MvpSelectItem { PlayerId = 0, DisplayName = "-- Không chọn --" });

                if (_match.Team1 != null)
                {
                    foreach (var p in _match.Team1.Players)
                    {
                        mvpList.Add(new MvpSelectItem { PlayerId = p.PlayerId, DisplayName = $"[{_match.Team1.Acronym}] {p.InGameName} - {p.RealName}" });
                    }
                }

                if (_match.Team2 != null)
                {
                    foreach (var p in _match.Team2.Players)
                    {
                        mvpList.Add(new MvpSelectItem { PlayerId = p.PlayerId, DisplayName = $"[{_match.Team2.Acronym}] {p.InGameName} - {p.RealName}" });
                    }
                }

                CboMvp.ItemsSource = mvpList;
                CboMvp.DisplayMemberPath = "DisplayName";
                CboMvp.SelectedValuePath = "PlayerId";

                // Check if there's a map MVP already
                int currentMvpId = 0;
                var map = _match.MatchMaps.FirstOrDefault(mm => mm.MapNumber == 1);
                if (map != null && map.MVPlayerId.HasValue)
                {
                    currentMvpId = map.MVPlayerId.Value;
                }
                CboMvp.SelectedValue = currentMvpId;

                // 6. Manage Rollback button visibility
                BtnRollback.Visibility = _match.Status == "Completed" ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin trận đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
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
            var selectedItem = CboStatus.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedItem == null) return;

            string status = selectedItem.Content.ToString();
            // Show MVP selection only for Live or Completed matches
            if (CboMvp != null)
            {
                CboMvp.IsEnabled = (status == "Live" || status == "Completed") && _match.Team1Id.HasValue && _match.Team2Id.HasValue;
            }
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

            int t1Score, t2Score;
            if (!int.TryParse(TxtTeam1Score.Text, out t1Score) || t1Score < 0)
            {
                MessageBox.Show("Tỉ số đội 1 không hợp lệ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtTeam2Score.Text, out t2Score) || t2Score < 0)
            {
                MessageBox.Show("Tỉ số đội 2 không hợp lệ.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStatusItem = CboStatus.SelectedItem as System.Windows.Controls.ComboBoxItem;
            string status = selectedStatusItem.Content.ToString();

            if (status == "Completed" && t1Score == t2Score)
            {
                MessageBox.Show("Khi hoàn thành trận đấu, tỉ số hai đội không được bằng nhau (phải có đội thắng cuộc).", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? mvpId = null;
            if (CboMvp.SelectedValue != null)
            {
                int val = (int)CboMvp.SelectedValue;
                if (val > 0)
                {
                    mvpId = val;
                }
            }

            try
            {
                _tournamentService.UpdateMatchResult(_matchId, t1Score, t2Score, status, mvpId);
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
    }

    public class MvpSelectItem
    {
        public int PlayerId { get; set; }
        public string DisplayName { get; set; }
    }
}
