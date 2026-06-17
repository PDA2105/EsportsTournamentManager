### Các mối quan hệ cốt lõi (Core Relationships):
1. **Users - Tournaments:** Một `User` (Admin) tạo nhiều giải đấu.
2. **Tournaments - Teams (M-N qua TournamentTeams):** Một giải đấu có nhiều đội tham gia, một đội có thể đăng ký đá nhiều giải khác nhau.
3. **Tournaments - PrizePools / MapPools (1-N):** Một giải đấu cấu hình một danh mục giải thưởng cụ thể và một danh sách các bản đồ được phép cấm/chọn (Map Pool).
4. **Teams - Players (1-N):** Một đội tuyển (`Team`) sở hữu nhiều tuyển thủ (`Player`), nhưng tại một thời điểm một tuyển thủ chỉ thuộc một đội duy nhất.
5. **Matches - MatchMaps (1-N):** Một trận đấu diễn ra theo thể thức (BO1, BO3, BO5). Một trận đấu (`Match`) sẽ gồm nhiều ván đấu đơn lẻ (`MatchMaps`).
6. **MatchMaps - PlayerStats (1-N):** Trong mỗi ván đấu (`MatchMap`), hệ thống lưu lại chi tiết chỉ số KDA, MVP của từng tuyển thủ (`Player`) tham gia ván đó.

---

## 2. TỪ ĐIỂN DỮ LIỆU CHI TIẾT (DATA DICTIONARY)

### 2.1 Bảng `Users` (Quản lý Tài khoản & Phân quyền)
* **Mục đích:** Lưu trữ thông tin tài khoản đăng nhập và vai trò hệ thống.

| Tên trường (Column) | Kiểu dữ liệu | Ràng buộc (Constraints) | Mô tả (Description) |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính tự tăng |
| **Username** | VARCHAR(50) | Unique, Not Null | Tên đăng nhập (Dùng để kiểm tra trùng) |
| **PasswordHash** | VARCHAR(256) | Not Null | Mật khẩu đã được băm (SHA256/BCrypt) |
| **FullName** | NVARCHAR(100) | Not Null | Họ và tên hiển thị |
| **Role** | VARCHAR(20) | Not Null | Vai trò: `Admin`, `User` |
| **CreatedAt** | DATETIME | Default CURRENT_TIMESTAMP | Thời gian tạo tài khoản |

### 2.2 Bảng `Teams` (Danh mục Đội tuyển)
* **Mục đích:** Lưu trữ thông tin hồ sơ của các câu lạc bộ/đội tuyển Esports.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính tự tăng |
| **TeamName** | NVARCHAR(100) | Unique, Not Null | Tên đội tuyển (Ví dụ: GAM Esports) |
| **Acronym** | VARCHAR(10) | Not Null | Tên viết tắt (Ví dụ: GAM, VKE, TS) |
| **LogoPath** | VARCHAR(255) | Nullable | Đường dẫn cục bộ hoặc URL ảnh Logo |
| **Coach** | NVARCHAR(100) | Nullable | Tên huấn luyện viên trưởng |
| **CreatedDate** | DATETIME | Nullable | Ngày thành lập đội |

### 2.3 Bảng `Players` (Hồ sơ Tuyển thủ)
* **Mục đích:** Lưu trữ thông tin cá nhân và định danh trong game của tuyển thủ thuộc các đội.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính tự tăng |
| **TeamId** | INTEGER | FK (Teams.Id), On Delete Cascade | Thuộc về đội tuyển nào |
| **RealName** | NVARCHAR(100) | Not Null | Họ và tên thật trên giấy tờ |
| **InGameName** | VARCHAR(50) | Unique, Not Null | Tên thi đấu (In-game ID, ví dụ: Levi, Kiaya) |
| **Position** | VARCHAR(20) | Nullable | Vị trí thi đấu (Top, Mid, Jungle, Juggernaut...) |
| **AvatarPath** | VARCHAR(255) | Nullable | Ảnh chân dung tuyển thủ |
| **IsActive** | BOOLEAN | Default 1 | Trạng thái thi đấu (1: Đang thi đấu, 0: Dự bị/Giải nghệ) |

### 2.4 Bảng `Tournaments` (Thông tin Giải đấu)
* **Mục đích:** Lưu trữ thông tin cấu hình, thể thức và trạng thái của giải đấu được tạo bởi Admin.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính tự tăng |
| **Name** | NVARCHAR(150) | Not Null | Tên giải đấu (Ví dụ: VCS Summer 2026) |
| **GameType** | VARCHAR(30) | Not Null | Bộ môn thi đấu: `LoL`, `Valorant`, `CS2` |
| **Format** | VARCHAR(30) | Not Null | Thể thức: `SingleElimination`, `DoubleElimination`, `RoundRobin` |
| **MaxTeams** | INTEGER | Not Null | Số lượng đội tối đa tham gia (4, 8, 16, 32) |
| **StartDate** | DATETIME | Not Null | Ngày bắt đầu giải |
| **EndDate** | DATETIME | Nullable | Ngày bế mạc giải |
| **Status** | VARCHAR(20) | Default 'Pending' | Trạng thái giải: `Pending`, `Active`, `Completed` |
| **CreatedByUserId** | INTEGER | FK (Users.Id) | ID của Admin tạo giải đấu |

### 2.5 Bảng `TournamentTeams` (Bảng trung gian Giải đấu - Đội tuyển)
* **Mục đích:** Giải quyết mối quan hệ Nhiều-Nhiều (M-N), lưu danh sách các đội tham gia vào một giải đấu cụ thể và quản lý thứ hạng hạt giống.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **TournamentId** | INTEGER | PK, FK (Tournaments.Id) | Mã giải đấu |
| **TeamId** | INTEGER | PK, FK (Teams.Id) | Mã đội tuyển |
| **SeedNumber** | INTEGER | Nullable | Số thứ tự hạt giống phục vụ thuật toán Smart Seeding |
| **RegisteredAt** | DATETIME | Default CURRENT_TIMESTAMP | Ngày giờ đội tuyển đăng ký tham gia giải |

### 2.6 Bảng `MapPools` (Danh mục Bản đồ thi đấu của Giải)
* **Mục đích:** Quy định các map đấu nào được phép sử dụng trong một giải đấu (Đặc biệt quan trọng với Valorant, CS2).

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **TournamentId** | INTEGER | FK (Tournaments.Id), On Delete Cascade | Thuộc về giải đấu nào |
| **MapName** | VARCHAR(50) | Not Null | Tên bản đồ (Ví dụ: Ascent, Bind, Dust II, Mirage) |

### 2.7 Bảng `PrizePools` (Cơ cấu Giải thưởng)
* **Mục đích:** Quản lý tài chính, phân chia cơ cấu tiền thưởng và phần quà của giải đấu.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **TournamentId** | INTEGER | FK (Tournaments.Id), On Delete Cascade | Thuộc về giải đấu nào |
| **RankPlace** | INTEGER | Not Null | Thứ hạng nhận giải (1: Vô địch, 2: Á quân, 3: Hạng ba) |
| **PrizeAmount** | DECIMAL(18,2) | Not Null | Số tiền thưởng nhận được (VNĐ hoặc USD) |
| **OtherRewards** | NVARCHAR(255) | Nullable | Hiện vật đi kèm (Cúp, Huy chương, Vé đi quốc tế) |

### 2.8 Bảng `Matches` (Quản lý Lượt trận tổng)
* **Mục đích:** Lưu trữ các trận đấu tổng (Tổng tỉ số BO), quản lý vị trí trong cây sơ đồ nhánh đấu.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **TournamentId** | INTEGER | FK (Tournaments.Id) | Thuộc về giải đấu nào |
| **Team1Id** | INTEGER | FK (Teams.Id), Nullable | Mã đội tuyển 1 (Có thể null khi chưa sinh nhánh xong) |
| **Team2Id** | INTEGER | FK (Teams.Id), Nullable | Mã đội tuyển 2 (Có thể null khi chưa sinh nhánh xong) |
| **MatchOrder** | INTEGER | Not Null | Thứ tự trận đấu trong vòng đấu |
| **RoundNumber** | INTEGER | Not Null | Vòng đấu thứ mấy (1: Vòng loại, 2: Tứ kết, 3: Bán kết, 4: Chung kết) |
| **BracketBranch** | VARCHAR(20) | Default 'Winner' | Nhánh đấu (Dùng cho Double Elimination): `Winner`, `Loser` |
| **NextMatchId** | INTEGER | FK (Matches.Id), Nullable | ID của trận đấu tiếp theo mà đội thắng sẽ tiến vào |
| **MatchFormat** | VARCHAR(10) | Default 'BO3' | Thể thức trận đấu: `BO1`, `BO3`, `BO5` |
| **Team1Score** | INTEGER | Default 0 | Tổng tỷ số map thắng của Đội 1 |
| **Team2Score** | INTEGER | Default 0 | Tổng tỷ số map thắng của Đội 2 |
| **WinnerTeamId** | INTEGER | FK (Teams.Id), Nullable | ID của đội chiến thắng trận đấu tổng |
| **ScheduledTime** | DATETIME | Not Null | Thời gian dự kiến lên sàn thi đấu |
| **VenueSlot** | NVARCHAR(50) | Nullable | Phòng thi đấu / Máy thi đấu phân công (Ví dụ: Stage A) |
| **Status** | VARCHAR(20) | Default 'Scheduled' | Trạng thái trận: `Scheduled`, `Live`, `Completed`, `Cancelled` |

### 2.9 Bảng `MatchMaps` (Chi tiết từng Ván đấu đơn lẻ)
* **Mục đích:** Khi thi đấu BO3/BO5, bảng này lưu kết quả chi tiết của từng ván (Map) đơn lẻ.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **MatchId** | INTEGER | FK (Matches.Id), On Delete Cascade | Thuộc về trận đấu tổng nào |
| **MapNumber** | INTEGER | Not Null | Số thứ tự ván đấu (Ván 1, Ván 2, Ván 3) |
| **SelectedMapName**| VARCHAR(50) | Nullable | Tên bản đồ thực tế được cấm/chọn để thi đấu |
| **Team1RoundScore**| INTEGER | Default 0 | Số điểm/vòng thắng của Đội 1 trong ván (Ví dụ: 13) |
| **Team2RoundScore**| INTEGER | Default 0 | Số điểm/vòng thắng của Đội 2 trong ván (Ví dụ: 9) |
| **DurationSeconds**| INTEGER | Nullable | Thời lượng ván đấu tính bằng giây |
| **MVPlayerId** | INTEGER | FK (Players.Id), Nullable | Tuyển thủ xuất sắc nhất ván đấu (MVP) |

### 2.10 Bảng `PlayerStats` (Thống kê Chỉ số Tuyển thủ theo Ván)
* **Mục đích:** Lưu trữ chỉ số hiệu năng (Performance Metrics) chi tiết của từng tuyển thủ sau mỗi ván đấu phục vụ tính năng tính điểm phong độ, tìm kiếm MVP giải đấu.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **MatchMapId** | INTEGER | FK (MatchMaps.Id), On Delete Cascade | Thuộc về ván đấu nào |
| **PlayerId** | INTEGER | FK (Players.Id) | ID của tuyển thủ |
| **Kills** | INTEGER | Default 0 | Số mạng hạ gục |
| **Deaths** | INTEGER | Default 0 | Số lần bị hạ gục |
| **Assists** | INTEGER | Default 0 | Số mạng hỗ trợ |
| **DamageDealt** | INTEGER | Default 0 | Tổng lượng sát thương gây ra |
| **CreepScore** | INTEGER | Default 0 | Chỉ số lính farm được (Dành riêng cho LoL) |
| **IsMvpOfMap** | BOOLEAN | Default 0 | Đạt danh hiệu MVP ván đấu hay không (1: Có, 0: Không) |

### 2.11 Bảng `AuditLogs` (Nhật ký Hệ thống & Tính năng Rollback)
* **Mục đích:** Ghi lại toàn bộ hành vi thay đổi dữ liệu nhạy cảm (nhập điểm, sửa lịch) của Admin/User để làm cơ sở cho việc kiểm toán giải đấu và thực hiện tính năng **Rollback kết quả**.

| Tên trường | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | INTEGER | PK, Auto Increment | Khóa chính |
| **UserId** | INTEGER | FK (Users.Id) | Người thực hiện thao tác |
| **Action** | VARCHAR(50) | Not Null | Loại hành động: `INSERT_SCORE`, `UPDATE_SCHEDULE`, `ROLLBACK` |
| **TableName** | VARCHAR(50) | Not Null | Tên bảng bị tác động (Ví dụ: Matches) |
| **RecordId** | INTEGER | Not Null | Khóa chính của bản ghi bị tác động |
| **OldDataSnapshot**| TEXT (JSON) | Nullable | Bản sao dữ liệu cũ trước khi sửa (Dùng để Rollback trạng thái) |
| **NewDataSnapshot**| TEXT (JSON) | Nullable | Bản sao dữ liệu mới sau khi sửa |
| **Timestamp** | DATETIME | Default CURRENT_TIMESTAMP | Thời gian thực hiện |

---

## 3. RÀNG BUỘC TOÀN VẸN & CHỈ MỤC TỐI ƯU (INDEXES & INTEGRITY)

Để đảm bảo hệ thống không bị xung đột lịch và truy vấn sơ đồ nhánh đấu mượt mà, các chỉ mục (Indexes) và ràng buộc sau cần được cấu hình trong `OnModelCreating` của EF Core:

1. **Unique Index trên `Users.Username` & `Teams.TeamName`:** Tránh việc tạo tài khoản trùng tên hoặc các đội tuyển trùng tên gây lỗi hiển thị.
2. **Composite Index trên `Matches(TournamentId, RoundNumber, MatchOrder)`:** Tối ưu hóa tốc độ truy vấn sơ đồ nhánh đấu. Giao diện WPF Canvas sẽ quét bảng `Matches` liên tục dựa trên `TournamentId` để vẽ hình cây. Có Index này tốc độ render dữ liệu sẽ tăng gấp 10 lần.
3. **Ràng buộc Chống Trùng Lịch Thi Đấu (Validation Logic mức Code):** Khi Admin hoặc User đổi `ScheduledTime` và `VenueSlot` của một trận đấu, một hàm Service phải quét kiểm tra:
   ```sql
   SELECT COUNT(*) FROM Matches 
   WHERE VenueSlot = @NewSlot 
     AND Status != 'Completed' 
     AND ABS(strftime('%s', ScheduledTime) - strftime('%s', @NewTime)) < 7200 -- Khoảng cách dưới 2 tiếng