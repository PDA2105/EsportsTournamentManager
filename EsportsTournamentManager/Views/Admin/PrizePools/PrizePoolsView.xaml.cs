using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EsportsTournamentManager.Data;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Views.Admin.PrizePools
{
    public partial class PrizePoolsView : UserControl
    {
        private Tournament _selectedTournament;

        public PrizePoolsView()
        {
            InitializeComponent();
        }

        public void LoadTournaments()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var tournaments = db.Tournaments
                        .OrderByDescending(t => t.StartDate)
                        .ToList();

                    CboTournaments.ItemsSource = tournaments;
                    
                    if (tournaments.Count > 0)
                    {
                        CboTournaments.SelectedIndex = 0;
                    }
                    else
                    {
                        PanelTournamentDetails.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách giải đấu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CboTournaments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboTournaments.SelectedItem is Tournament selected)
            {
                LoadPrizePoolDetails(selected.TournamentId);
            }
            else
            {
                PanelTournamentDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadPrizePoolDetails(int tournamentId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    _selectedTournament = db.Tournaments
                        .Include(t => t.PrizePools)
                        .FirstOrDefault(t => t.TournamentId == tournamentId);

                    if (_selectedTournament == null) return;

                    // 1. Render inputs visibility based on status
                    if (_selectedTournament.Status == "Pending")
                    {
                        PanelAddPrize.Visibility = Visibility.Visible;
                        TxtReadOnlyInfo.Visibility = Visibility.Collapsed;
                        ColDeletePrize.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PanelAddPrize.Visibility = Visibility.Collapsed;
                        TxtReadOnlyInfo.Visibility = Visibility.Visible;
                        ColDeletePrize.Visibility = Visibility.Collapsed;
                    }

                    // 2. Load prizes list
                    var prizes = _selectedTournament.PrizePools
                        .OrderBy(p => p.RankPlace)
                        .Select(p => new
                        {
                            p.PrizePoolId,
                            p.RankPlace,
                            RankPlaceDisplay = GetRankPlaceDisplay(p.RankPlace),
                            p.PrizeAmount,
                            DisplayAmount = p.PrizeAmount.ToString("N0") + " VNĐ",
                            p.OtherRewards
                        })
                        .ToList();

                    GridPrizePool.ItemsSource = prizes;
                    PanelTournamentDetails.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải cơ cấu giải thưởng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetRankPlaceDisplay(int rankPlace)
        {
            switch (rankPlace)
            {
                case 1: return "Vô địch";
                case 2: return "Á quân";
                case 3: return "Hạng 3";
                default: return $"Hạng {rankPlace}";
            }
        }

        private void BtnAddPrize_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTournament == null || _selectedTournament.Status != "Pending") return;

            var selectedItem = CboPrizeRank.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            int rankPlace = int.Parse(selectedItem.Tag.ToString());

            if (!decimal.TryParse(TxtPrizeAmount.Text.Trim(), out decimal prizeAmount) || prizeAmount < 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền thưởng hợp lệ (là số dương)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string otherRewards = TxtPrizeOther.Text.Trim();

            try
            {
                using (var db = new AppDbContext())
                {
                    // Check if rank already exists
                    bool exists = db.PrizePools.Any(p => p.TournamentId == _selectedTournament.TournamentId && p.RankPlace == rankPlace);
                    if (exists)
                    {
                        MessageBox.Show("Thứ hạng này đã được cấu hình giải thưởng! Hãy xóa phần thưởng cũ trước khi thêm mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newPrize = new PrizePool
                    {
                        TournamentId = _selectedTournament.TournamentId,
                        RankPlace = rankPlace,
                        PrizeAmount = prizeAmount,
                        OtherRewards = otherRewards
                    };

                    db.PrizePools.Add(newPrize);
                    db.SaveChanges();
                }

                // Reset forms
                TxtPrizeAmount.Text = string.Empty;
                TxtPrizeOther.Text = string.Empty;

                // Reload details
                LoadPrizePoolDetails(_selectedTournament.TournamentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm giải thưởng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeletePrize_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTournament == null || _selectedTournament.Status != "Pending") return;

            var button = sender as Button;
            if (button == null) return;

            // Get selected prize item context
            dynamic selectedItem = button.DataContext;
            if (selectedItem == null) return;

            int prizePoolId = selectedItem.PrizePoolId;

            var result = MessageBox.Show("Bạn có thực sự muốn xóa cơ cấu giải thưởng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    var prize = db.PrizePools.Find(prizePoolId);
                    if (prize != null)
                    {
                        db.PrizePools.Remove(prize);
                        db.SaveChanges();
                    }
                }

                // Reload details
                LoadPrizePoolDetails(_selectedTournament.TournamentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa giải thưởng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
