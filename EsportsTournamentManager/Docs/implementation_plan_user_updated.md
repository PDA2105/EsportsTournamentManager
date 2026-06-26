# Kế hoạch thiết kế UserMain mở rộng theo phong cách GOL.GG (Home Dashboard – Tournament Overview – Bracket – Team/Player Detail)

Phương án này **giữ lại khoảng 80% kiến trúc** trong kế hoạch hiện tại (đăng nhập → danh sách giải đấu dạng thẻ → sơ đồ nhánh đấu Canvas → popup chi tiết trận đấu) vì rất phù hợp với WPF, đồng thời **bổ sung 20%** lấy cảm hứng từ GOL.GG: thêm Dashboard thống kê, nâng cấp Tournament Card, chèn thêm Tournament Overview trước khi vào Bracket, làm giàu Match Detail, và mở thêm Team Detail / Player Detail khi nhấp vào tên đội/tuyển thủ.

Vẫn giữ nguyên triết lý tích hợp trực tiếp mọi panel vào `UserMain` (chuyển đổi hiển thị/ẩn giữa các Panel) thay vì tách thành nhiều UserControl/Window riêng, để giữ dự án gọn gàng và tối ưu hiệu năng chuyển trang.

---

## Ý kiến từ người dùng cần xác nhận (User Review Required)

> [!IMPORTANT]
> **Luồng giao diện đơn trang mới (Single-Page User Flow mở rộng):**
> 1. **Header phía trên (Top Header Bar):** Giữ nguyên như kế hoạch cũ — Logo bên trái, thông tin khách xem và nút **Đăng xuất** màu đỏ nổi bật bên phải, trải dài toàn bộ chiều rộng cửa sổ.
> 2. **Khu vực nội dung chính (Main Content Area)** gồm 5 Panel chuyển đổi qua lại:
>    - **PanelHomeDashboard (Hiển thị mặc định):** Thay cho `PanelTournamentList` cũ. Phía trên là dải thẻ thống kê nhanh (Tổng số giải đấu / Tổng số đội / Tổng số trận / Trận sắp diễn ra), kế đến là khu **Live Match**, **Top Team**, **Top Player**, rồi mới tới thanh tìm kiếm và **lưới thẻ giải đấu (Tournament Cards)** đã được nâng cấp.
>    - **PanelTournamentOverview (Mới):** Khi khách bấm vào một thẻ giải đấu (nút "Xem chi tiết"), hệ thống **không vào Bracket ngay** mà hiển thị trang tổng quan: tên giải, trạng thái, giải thưởng, số đội, ngày bắt đầu/kết thúc, thể thức, kèm Top Team / Trận gần đây / Thống kê nổi bật của giải. Có nút **"Xem nhánh đấu"** để đi tiếp.
>    - **PanelTournamentBracket (Giữ nguyên vai trò cũ):** Sơ đồ nhánh đấu Canvas, chỉ khác là được mở từ `PanelTournamentOverview` thay vì trực tiếp từ thẻ giải đấu. Có nút **"← Quay lại tổng quan giải đấu"**.
>    - **PanelTeamDetail (Mới):** Mở khi nhấp vào tên Đội ở popup trận đấu hoặc ở Tournament Overview — hiển thị Logo, Tên, HLV, Tỉ lệ thắng, Danh sách Player, Trận gần đây, Thống kê đội.
>    - **PanelPlayerDetail (Mới):** Mở khi nhấp vào tên Tuyển thủ ở popup trận đấu hoặc ở Team Detail — hiển thị Avatar, Vai trò (Role), Quốc gia, Tuổi, KDA trung bình, Champion Pool, Trận gần đây.
>    - Mỗi Panel con (Overview, Bracket, Team Detail, Player Detail) đều có nút "Quay lại" để lùi về đúng một bước trong luồng (không nhảy thẳng về Home), theo đúng thứ tự: **Home → Tournament Overview → Bracket → Team/Player Detail**.
> 3. **Popup thống kê trận đấu (`UserMatchDetailDialog`) nâng cấp:** Vẫn mở khi nhấp vào trận đấu trên sơ đồ nhánh, nhưng giờ tổ chức theo dạng Tab giống GOL.GG (Scoreboard → Player Statistics → Objectives → MVP), và **tên Team/Player trong popup có thể nhấp được** để mở `PanelTeamDetail` / `PanelPlayerDetail` tương ứng (popup tự đóng khi điều hướng).

---

## Các thay đổi đề xuất (Proposed Changes)

### 1. Tái cấu trúc Home Dashboard (thay cho Tournament List cũ)

#### [MODIFY] [UserMain.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml)
* Đổi tên/khoanh vùng `PanelTournamentList` cũ thành **`PanelHomeDashboard`**, bổ sung phía trên lưới thẻ giải đấu:
  - Dải **Stat Cards** dạng `ItemsControl`/`UniformGrid` 4 cột: *Tổng giải đấu*, *Tổng đội*, *Tổng tuyển thủ*, *Trận đang diễn ra*.
  - Khối **Live Match** (danh sách trận đang diễn ra, có chấm đỏ "LIVE" nhấp nháy).
  - Khối **Top Team** và **Top Player** dạng carousel/list ngắn (5 mục, có ảnh đại diện + chỉ số nổi bật).
  - Giữ nguyên thanh tìm kiếm và lưới `ItemsControl` + `WrapPanel` cho Tournament Card ở dưới cùng.
* Thêm 4 Panel mới cùng cấp với `PanelTournamentBracket` cũ, tất cả đặt `Visibility="Collapsed"` mặc định:
  - `PanelTournamentOverview`
  - `PanelTeamDetail`
  - `PanelPlayerDetail`
  - (`PanelTournamentBracket` giữ nguyên, chỉ đổi nguồn điều hướng vào/ra)

#### [MODIFY] [UserMain.xaml.cs](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml.cs)
* Thêm Service mới: `StatisticsService` (hoặc tái dùng `TournamentService`/`TeamService`/`PlayerService`/`MatchService` để tổng hợp số liệu cho Dashboard).
* Thêm phương thức tải số liệu tổng quan khi `SetUser()` được gọi:
  ```csharp
  private void LoadHomeDashboard()
  {
      var stats = _tournamentService.GetDashboardSummary();
      TxtTotalTournament.Text = stats.TotalTournament.ToString();
      TxtTotalTeam.Text = stats.TotalTeam.ToString();
      TxtTotalPlayer.Text = stats.TotalPlayer.ToString();
      TxtActiveMatch.Text = stats.ActiveMatch.ToString();

      LiveMatchList.ItemsSource = _matchService.GetLiveMatches();
      TopTeamList.ItemsSource = _teamService.GetTopTeams(5);
      TopPlayerList.ItemsSource = _playerService.GetTopPlayers(5);
  }
  ```
* Quản lý điều hướng giữa các Panel bằng một hàm dùng chung, tránh lặp code ẩn/hiện:
  ```csharp
  private void ShowPanel(UIElement panelToShow)
  {
      foreach (var panel in new UIElement[]
               { PanelHomeDashboard, PanelTournamentOverview,
                 PanelTournamentBracket, PanelTeamDetail, PanelPlayerDetail })
      {
          panel.Visibility = panel == panelToShow ? Visibility.Visible : Visibility.Collapsed;
      }
  }
  ```

---

### 2. Nâng cấp Tournament Card

#### [MODIFY] [UserMain.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml)
* Thẻ giải đấu hiện tại (chỉ có Tên + Button) được nâng cấp thành thẻ kiểu GOL.GG, gồm:
  - Logo/ảnh đại diện giải đấu.
  - Tên giải đấu + nhãn trạng thái (`LIVE` / `Sắp diễn ra` / `Đã kết thúc`) dùng `Border` đổi màu theo trạng thái.
  - Giải thưởng (Prize Pool), số đội tham gia, thể thức (Single/Double Elimination, Round Robin).
  - `ProgressBar` thể hiện tiến độ giải đấu (số trận đã đấu / tổng số trận).
  - Nút **"Xem chi tiết"** thay cho nút "Xem nhánh đấu" cũ — dẫn vào `PanelTournamentOverview` chứ không vào Bracket trực tiếp.
* Cập nhật `DataTemplate` của thẻ giải đấu để bind đầy đủ các trường nói trên từ Model `Tournament` (bổ sung thuộc tính tính toán `Progress`, `StatusLabel` nếu Model chưa có).

---

### 3. Trang tổng quan giải đấu (Tournament Overview) — Mới

#### [NEW PANEL] `PanelTournamentOverview` trong [UserMain.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml)
* Bố cục:
  - Nút "← Quay lại Trang chủ".
  - Khối thông tin chính: Tên giải, Trạng thái, Giải thưởng, Số đội, Ngày bắt đầu/kết thúc, Thể thức.
  - Khối **Top Team** của giải đấu này.
  - Khối **Trận đấu gần đây** (Recent Match) của giải đấu này.
  - Khối **Thống kê nổi bật** (Average Kill, Average Gold, Winrate cao nhất...).
  - Nút lớn **"Xem nhánh đấu →"** ở cuối trang, dẫn sang `PanelTournamentBracket`.

#### [MODIFY] [UserMain.xaml.cs](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml.cs)
* Khi bấm "Xem chi tiết" trên Tournament Card:
  ```csharp
  private void TournamentCard_ViewDetail_Click(object sender, RoutedEventArgs e)
  {
      var tournament = (Tournament)((Button)sender).DataContext;
      _currentTournament = tournament;
      LoadTournamentOverview(tournament);
      ShowPanel(PanelTournamentOverview);
  }

  private void BtnViewBracket_Click(object sender, RoutedEventArgs e)
  {
      RenderBracketVisuals(_currentTournament);
      ShowPanel(PanelTournamentBracket);
  }
  ```
* Nút "← Quay lại danh sách" trong Bracket cũ giờ trỏ về `PanelTournamentOverview` (`ShowPanel(PanelTournamentOverview)`) thay vì về thẳng Home.

---

### 4. Popup thống kê trận đấu (Match Detail) nâng cấp

#### [MODIFY] [UserMatchDetailDialog.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMatchDetailDialog.xaml)
* Tổ chức lại nội dung popup theo Tab (`TabControl` style tối giản, không viền mặc định của Windows):
  - **Tab "Tổng quan":** Scoreboard lớn kiểu thể thao (Team A **2** — VS — **1** Team B), Badge MVP với icon sao vàng, danh sách map đã đấu kèm tỉ số vòng (`Dust II (13 - 9)`).
  - **Tab "Thống kê tuyển thủ":** Bảng KDA chỉ đọc, bổ sung thêm các cột **Gold**, **Damage**, **CS** (giữ tinh thần "không lưới thô, không chỉnh sửa" như kế hoạch cũ).
  - **Tab "Mục tiêu" (Objectives):** So sánh hai đội bằng thanh ngang (Tower, Dragon/Baron hoặc tương đương) — dùng `ProgressBar` hoặc `Rectangle` vẽ tay để giữ đơn giản.
* Tên Team và tên từng Player trong các bảng/Scoreboard được bọc trong `Hyperlink`/`Button` dạng link (không viền, đổi màu khi hover) để có thể nhấp.

#### [MODIFY] [UserMatchDetailDialog.xaml.cs](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMatchDetailDialog.xaml.cs)
* Bổ sung 2 sự kiện để báo cho `UserMain` biết người dùng muốn xem chi tiết Team/Player, đóng popup và điều hướng:
  ```csharp
  public event EventHandler<Team> TeamNameClicked;
  public event EventHandler<Player> PlayerNameClicked;

  private void TeamName_Click(object sender, RoutedEventArgs e)
  {
      var team = (Team)((FrameworkElement)sender).DataContext;
      TeamNameClicked?.Invoke(this, team);
      this.Close();
  }

  private void PlayerName_Click(object sender, RoutedEventArgs e)
  {
      var player = (Player)((FrameworkElement)sender).DataContext;
      PlayerNameClicked?.Invoke(this, player);
      this.Close();
  }
  ```
* Vẫn giữ nguyên: chỉ có nút "Đóng", không có nút Lưu/Rollback, tự tính Performance Points và bình chọn MVP như kế hoạch cũ.

#### [MODIFY] [UserMain.xaml.cs](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml.cs)
* Khi mở popup, đăng ký lắng nghe 2 sự kiện trên để chuyển sang Panel chi tiết tương ứng:
  ```csharp
  var dialog = new UserMatchDetailDialog(match);
  dialog.TeamNameClicked += (s, team) => { LoadTeamDetail(team); ShowPanel(PanelTeamDetail); };
  dialog.PlayerNameClicked += (s, player) => { LoadPlayerDetail(player); ShowPanel(PanelPlayerDetail); };
  dialog.ShowDialog();
  ```

---

### 5. Trang chi tiết Đội & Tuyển thủ (Team Detail / Player Detail) — Mới

#### [NEW PANEL] `PanelTeamDetail` trong [UserMain.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml)
* Bố cục: Nút "← Quay lại", Logo + Tên + HLV + Tỉ lệ thắng + Giải đấu đang tham gia, danh sách **Players** (mỗi player có thể nhấp để mở `PanelPlayerDetail`), **Trận đấu gần đây**, **Thống kê đội** (Winrate, Average Kill, Average Gold, Average Tower).

#### [NEW PANEL] `PanelPlayerDetail` trong [UserMain.xaml](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml)
* Bố cục: Nút "← Quay lại", Avatar + Role + Quốc gia + Tuổi, khối **KDA trung bình**, **Champion Pool** (dạng chip/tag list), **Trận đấu gần đây**.

#### [MODIFY] [UserMain.xaml.cs](file:///e:/EsportsTournamentManager/EsportsTournamentManager/Views/User/UserMain.xaml.cs)
* Thêm `TeamService.GetTeamDetail(teamId)` và `PlayerService.GetPlayerDetail(playerId)` (nếu Service chưa có sẵn, bổ sung phương thức) để đổ dữ liệu cho 2 Panel trên:
  ```csharp
  private void LoadTeamDetail(Team team)
  {
      var detail = _teamService.GetTeamDetail(team.Id);
      TeamDetailContext.DataContext = detail;
  }

  private void LoadPlayerDetail(Player player)
  {
      var detail = _playerService.GetPlayerDetail(player.Id);
      PlayerDetailContext.DataContext = detail;
  }
  ```
* Nút "Quay lại" trên cả hai Panel này điều hướng về Panel mà người dùng đã đến trước đó (Bracket popup hoặc Tournament Overview hoặc Team Detail) — dùng một `Stack<UIElement>` đơn giản (`_navigationHistory`) để lưu lại Panel trước khi `ShowPanel()` được gọi, tránh phải hard-code điểm quay về.

---

### 6. Phân hệ Xác thực & Màn hình đăng nhập

#### [KHÔNG THAY ĐỔI]
* Giữ nguyên toàn bộ phần `AuthService.LoginAsGuest()`, nút "Xem với tư cách Khách" trên `LoginView`, và logic điều hướng trong `MainWindow.xaml.cs` như kế hoạch ban đầu. Khách và User thường sau khi đăng nhập vẫn được đưa vào `UserMain`, chỉ khác là màn hình mặc định bên trong giờ là `PanelHomeDashboard` thay vì `PanelTournamentList`.

---

## Kế hoạch kiểm thử (Verification Plan)

### Kiểm thử thủ công:

1. Chạy ứng dụng, nhấp vào nút **"Xem với tư cách Khách"**.
2. Kiểm tra `UserMain` hiển thị **`PanelHomeDashboard`** mặc định: dải thẻ thống kê (Tổng giải đấu/Tổng đội/Tổng tuyển thủ/Trận đang diễn ra), khối Live Match, Top Team, Top Player hiển thị đúng số liệu từ database.
3. Cuộn xuống kiểm tra lưới Tournament Card: mỗi thẻ hiển thị logo, trạng thái (LIVE/Sắp diễn ra/Đã kết thúc), giải thưởng, số đội, thể thức, thanh tiến độ.
4. Thử tìm kiếm giải đấu để kiểm tra tính năng lọc thẻ vẫn hoạt động đúng.
5. Bấm **"Xem chi tiết"** trên một thẻ giải đấu:
   - `PanelHomeDashboard` ẩn, **`PanelTournamentOverview`** hiện ra đúng thông tin giải đấu đã chọn (tên, trạng thái, giải thưởng, số đội, ngày, thể thức, Top Team, trận gần đây, thống kê).
   - Bấm "← Quay lại Trang chủ" phải về đúng `PanelHomeDashboard`.
6. Từ Tournament Overview, bấm **"Xem nhánh đấu →"**:
   - **`PanelTournamentBracket`** hiện lên chiếm trọn không gian với sơ đồ nhánh Canvas đúng giải đấu đã chọn.
   - Bấm "← Quay lại tổng quan giải đấu" phải về đúng `PanelTournamentOverview` (không nhảy thẳng về Home).
7. Trên sơ đồ nhánh đấu, click vào một trận đấu bất kỳ:
   - Popup `UserMatchDetailDialog` hiển thị đúng 3 Tab: **Tổng quan** (Scoreboard + MVP + map), **Thống kê tuyển thủ** (KDA + Gold + Damage + CS), **Mục tiêu** (so sánh 2 đội).
   - Đảm bảo không có bất kỳ nút nhập liệu/lưu/rollback nào, chỉ có nút "Đóng".
8. Trong popup, nhấp vào **tên một Team**:
   - Popup tự đóng, **`PanelTeamDetail`** hiện ra đúng thông tin đội đã chọn (Logo, HLV, Winrate, danh sách Player, trận gần đây, thống kê đội).
9. Trong `PanelTeamDetail`, nhấp vào **tên một Player** trong danh sách:
   - **`PanelPlayerDetail`** hiện ra đúng thông tin tuyển thủ (Avatar, Role, Quốc gia, Tuổi, KDA trung bình, Champion Pool, trận gần đây).
10. Kiểm tra nút "← Quay lại" ở `PanelPlayerDetail` và `PanelTeamDetail` đưa người dùng về đúng Panel trước đó trong lịch sử điều hướng (không bị quay sai bước).
11. Kiểm tra đăng xuất và đăng nhập lại bằng Admin để đảm bảo giao diện quản trị (`Admin Dashboard`, `Tournament`, `Team`, `Player`, `Match`, `Statistics`) vẫn hoạt động bình thường, không bị ảnh hưởng bởi các thay đổi ở User Portal.
