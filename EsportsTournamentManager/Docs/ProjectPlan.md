# PROJECT PLAN & TIMELINE (4 WEEKS): ESPORTSTOURNAMENTMANAGER (WPF)

## 1. TỔNG QUAN DỰ ÁN (PROJECT OVERVIEW)
* **Tên dự án:** EsportsTournamentManager
* **Công nghệ sử dụng:** C# (.NET 8), WPF (Windows Presentation Foundation), MVVM Pattern (CommunityToolkit.Mvvm), Entity Framework Core + SQLite.
* **Mục tiêu:** Xây dựng một ứng dụng desktop chuyên nghiệp, toàn diện giúp quản lý, vận hành và theo dõi các giải đấu Esports (LoL, Valorant, CS2, v.v.). Hệ thống được thiết kế theo mô hình phân lớp chuẩn chỉ, giải quyết bài toán từ quản trị hệ thống, phân quyền, quản lý danh mục dữ liệu đến lõi thuật toán xếp lịch và trình chiếu truyền thông.

---

## 2. KIẾN TRÚC HỆ THỐNG & SƠ ĐỒ MODULE ĐẦY ĐỦ (FULL MODULE DECOMPOSITION)

Hệ thống được cấu trúc thành 5 phân hệ (Module) lớn để đảm bảo tính khép kín, bảo mật và khả năng vận hành thực tế:

### Phân hệ 1: Hệ thống & Bảo mật (System & Security)
* **Đăng nhập / Đăng ký (Authentication):** Xác thực người dùng, mã hóa mật khẩu bằng thuật toán băm (Hashing) trước khi lưu DB.
* **Phân quyền tài khoản (Authorization):** Phân chia 3 vai trò rõ rệt:
    * *Admin/Ban tổ chức (Organizer):* Toàn quyền quản trị hệ thống, tạo giải, cấu hình tham số.
    * *Trọng tài (Referee):* Chỉ có quyền truy cập các trận đấu được phân công để ghi biên bản và cập nhật tỷ số.
    * *Khán giả (Viewer/Guest):* Chỉ xem dữ liệu (Sơ đồ nhánh, bảng xếp hạng, MVP) mà không cần đăng nhập.

### Phân hệ 2: Quản lý Danh mục (Master Data Management)
* **Quản lý Đội tuyển (Team Management):** CRUD thông tin đội tuyển, logo, ngày thành lập, câu lạc bộ chủ quản.
* **Quản lý Tuyển thủ (Player Management):** Quản lý hồ sơ tuyển thủ (Họ tên, In-game ID, vị trí thi đấu, chỉ số KDA phong độ).
* **Quản lý Địa điểm & Thiết bị (Venue & Slot Management):** Quản lý các phòng máy/sân khấu thi đấu (Stage A, Stage B, khu vực PC từ 01-20) nhằm tối ưu tài nguyên phần cứng.

### Phân hệ 3: Lõi Vận hành Giải đấu (Tournament Operations - Core Business)
* **Cấu hình Giải đấu (Tournament Configuration):** Thiết lập tên giải, bộ môn (LoL, Valorant, CS2), cơ cấu giải thưởng (Prize Pool split), danh sách bản đồ thi đấu (Map Pool).
* **Thuật toán Sinh nhánh đấu (Smart Bracket Generator):**
    * Tự động sinh lịch thi đấu theo các thể thức: Loại trực tiếp (Single Elimination), Nhánh thắng nhánh thua (Double Elimination), Vòng tròn tính điểm (Round Robin).
    * Tích hợp cơ chế xếp hạt giống (Smart Seeding) để né các đội mạnh gặp nhau sớm và tính năng hạn chế trùng lặp câu lạc bộ ở vòng đầu (Anti-Clash).
* **Module Trọng tài (Referee Execution Tool):** Giao diện cập nhật tỉ số theo từng Map (Ví dụ: BO3), ghi nhận chỉ số MVP trận đấu hoặc xử phạt vi phạm (Thẻ cảnh cáo, xử thua). Tính năng **Rollback** (Hủy kết quả sai để nhập lại) giúp đảm bảo tính nhất quán dữ liệu mà không làm hỏng cấu trúc nhánh đấu.

### Phân hệ 4: Báo cáo & Thống kê (Analytics & Leaderboard)
* **Bảng xếp hạng động (Live Leaderboard):** Tự động tính toán điểm số, hiệu số map thắng/thua, chỉ số phụ để cập nhật bảng xếp hạng theo thời gian thực (real-time).
* **Thống kê chuyên sâu (Advanced Analytics):** Sử dụng biểu đồ trực quan hóa phong độ các đội, tỷ lệ thắng của các Map/Tướng, danh sách Top MVP của giải đấu.

### Phân hệ 5: Truyền thông & Xuất bản (Media & Integration)
* **Chế độ Trình chiếu (Kiosk/Livestream Mode):** Giao diện full-screen ẩn các tác vụ quản trị, tự động chuyển động (Animation) giữa sơ đồ nhánh đấu và bảng xếp hạng phục vụ ban tổ chức livestream hoặc chiếu lên màn hình lớn.
* **Bộ xuất dữ liệu (Media Export Service):**
    * Chụp ảnh trực tiếp sơ đồ nhánh đấu từ WPF Canvas xuất thành file ảnh độ phân giải cao (`.PNG`/`.JPG`) để đăng mạng xã hội.
    * Xuất báo cáo tổng kết giải đấu, danh sách khen thưởng ra file Excel.

---

## 3. LỘ TRÌNH TRIỂN KHAI 4 TUẦN (4-WEEK TIMELINE)

Để hoàn thiện một hệ thống lớn như trên trong 4 tuần, kế hoạch tập trung hoàn thiện 100% Core Logic và UI chính, các module phụ trợ được thiết kế sẵn cấu trúc DB và cài đặt ở mức cơ bản.

### TUẦN 1: THIẾT KẾ CƠ SỞ DỮ LIỆU, KIẾN TRÚC MVVM & MODULE BẢO MẬT
* **Mục tiêu:** Thiết lập cấu trúc dự án chuẩn, liên kết DB và hoàn thiện phân hệ Đăng nhập/Phân quyền.
* **Chi tiết từng ngày:**
    * **Day 1-2:** Thiết kế lược đồ cơ sở dữ liệu (ERD) toàn vẹn cho tất cả các phân hệ trên SQLite. Thực hiện Code-First Migration bằng Entity Framework Core.
    * **Day 3:** Khởi tạo cấu trúc dự án WPF (.NET 8). Chia thư mục chuẩn MVVM: `Models`, `Views`, `ViewModels`, `Services`, `Converters`, `Resources`. Tích hợp thư viện `CommunityToolkit.Mvvm` và `MaterialDesignThemes`.
    * **Day 4-5:** Xây dựng cửa sổ đăng nhập (`LoginView`), thiết lập cơ chế mã hóa mật khẩu và phân quyền người dùng thông qua `Custom Principal` hoặc `ValueConverter` để ẩn/hiện chức năng tùy theo Role.
    * **Day 6-7:** Tạo khung giao diện chính (`ShellWindow`) với thanh điều hướng bên trái (Navigation Menu), kiểm tra luồng chuyển trang bất đồng bộ (`NavigationService`).

### TUẦN 2: HOÀN THIỆN MODULE DANH MỤC & CẤU HÌNH GIẢI ĐẤU
* **Mục tiêu:** Hoàn thiện giao diện và logic CRUD cho Đội tuyển, Tuyển thủ và khởi tạo Giải đấu.
* **Chi tiết từng ngày:**
    * **Day 8-9:** Thiết kế `TeamsAndPlayersView.xaml`. Sử dụng DataGrid hiển thị danh sách đội, tích hợp bộ lọc tìm kiếm nhanh. Viết cửa sổ Pop-up (Dialog) để thêm/sửa thông tin tuyển thủ, kiểm tra ràng buộc dữ liệu đầu vào.
    * **Day 10-11:** Thiết kế module Quản lý địa điểm/phòng máy (`VenueManagementView`) để xếp vị trí thi đấu cho các đội, tránh trùng lịch.
    * **Day 12-13:** Xây dựng màn hình Tạo giải đấu (`TournamentCreationView.xaml`). Cho phép nhập cấu hình nâng cao: Số lượng đội, thể thức, cơ cấu giải thưởng và lựa chọn danh sách Map thi đấu (Map Pool).
    * **Day 14:** Đồng bộ hóa dữ liệu xuống SQLite thông qua DbContext. Viết Unit Test cho tầng Service để đảm bảo dữ liệu đội và giải đấu được lưu chính xác.

### TUẦN 3: LÕI THUẬT TOÁN BRACKET & GIAO DIỆN SƠ ĐỒ NHÁNH ĐẤU
* **Mục tiêu:** Phát triển thuật toán sinh lịch thi đấu thông minh và vẽ sơ đồ cây trực quan trên WPF.
* **Chi tiết từng ngày:**
    * **Day 15-16 (Thuật toán):** Hiện thực hóa class `BracketGenerator`. Viết thuật toán bắt cặp ngẫu nhiên hoặc xếp hạt giống (Smart Seeding) cho thể thức Loại trực tiếp (Single Elimination) và Vòng tròn (Round Robin). Tự động tạo danh sách các `Match` và lưu vào DB.
    * **Day 17-19 (Giao diện sơ đồ):** Sử dụng các control nâng cao của WPF (`ItemsControl` kết hợp đồ họa `Canvas` hoặc cấu trúc hình cây `Grid`) để tự động vẽ sơ đồ nhánh đấu (Bracket Tree) dựa trên dữ liệu trận đấu thu được từ DB.
    * **Day 20-21 (Cập nhật kết quả):** Thiết kế giao diện `MatchDetailWindow.xaml` dành cho trọng tài cập nhật tỉ số theo từng map đấu, chọn MVP trận. Xử lý logic tự động đẩy đội thắng lên vòng tiếp theo trong DB và phát tín hiệu (`Messenger` của MVVM) để làm mới (Refresh) giao diện sơ đồ nhánh đấu ngay lập tức.

### TUẦN 4: DASHBOARD, LIVESTREAM VIEW, XUẤT BÁO CÁO & ĐÓNG GÓI
* **Mục tiêu:** Hoàn thiện trải nghiệm người dùng nâng cao, tối ưu hiệu năng UI và đóng gói bộ cài đặt ứng dụng.
* **Chi tiết từng ngày:**
    * **Day 22 (Dashboard):** Hoàn thiện trang chủ với các thẻ Metric tổng quan và tích hợp biểu đồ `LiveCharts.Wpf` hiển thị top tuyển thủ có chỉ số KDA cao nhất hoặc tỷ lệ thắng của các đội.
    * **Day 23 (Kiosk/Livestream Mode):** Phát triển tính năng xem độc lập dạng Full Screen, tích hợp WPF Storyboard Animations giúp giao diện tự động trượt mượt mà qua lại giữa sơ đồ nhánh đấu và bảng xếp hạng phục vụ ban tổ chức livestream.
    * **Day 24 (Media Export Service):** * Ứng dụng công nghệ `RenderTargetBitmap` để chụp lại toàn bộ vùng vẽ Canvas của sơ đồ nhánh đấu rồi xuất ra file hình ảnh `.PNG`.
        * Tích hợp thư viện `EPPlus` để viết Service xuất bảng xếp hạng và biên bản giải đấu ra file Excel.
    * **Day 25-26 (Tối ưu hóa UI & Bug Fixing):** Áp dụng `VirtualizingStackPanel` để tối ưu các danh sách lớn, kiểm tra toàn bộ ứng dụng bằng cơ chế xử lý bất đồng bộ (`async/await`) để đảm bảo không bị đứng giao diện (Not Responding) khi truy vấn DB nặng. Chạy thử nghiệm luồng khép kín (End-to-End Test).
    * **Day 27-28 (Đóng gói sản phẩm):** Sử dụng *Advanced Installer* hoặc *Inno Setup* để đóng gói ứng dụng thành file `.exe` cài đặt chuyên nghiệp. Viết tài liệu hướng dẫn vận hành hệ thống (`README.md`).

---

## 4. QUẢN LÝ RỦI RO VÀ CHIẾN LƯỢC PHÒNG NGỪA (RISK MANAGEMENT)

1. **Rủi ro quá tải tiến độ do ôm đồm nhiều phân hệ:** Việc cài đặt trọn vẹn cả 5 phân hệ lớn trong vòng 4 tuần là thách thức cực kỳ lớn đối với lập trình viên solo hoặc nhóm nhỏ.
   * *Chiến lược phòng ngừa:* Thiết kế Database đầy đủ tất cả bảng dữ liệu ngay từ tuần 1 để hệ thống không bị lỗi logic (Kín kẽ khi giảng viên kiểm tra DB). Tuy nhiên, về mặt code chức năng, ưu tiên hoàn thiện 100% luồng chạy chính (CRUD Đội -> Sinh nhánh Loại trực tiếp -> Trọng tài chấm điểm -> Vẽ sơ đồ). Các tính năng như phân quyền nâng cao, map pool hay quản lý thiết bị có thể làm ở mức cơ bản hoặc giả lập dữ liệu (Mock Data).
2. **Rủi ro vỡ layout Canvas khi sơ đồ giải đấu quá lớn (Ví dụ: Giải 32 hoặc 64 đội):** Sơ đồ nhánh đấu sẽ vượt quá kích thước màn hình thông thường, gây tràn hoặc mất hiển thị.
   * *Chiến lược phòng ngừa:* Bao bọc toàn bộ khối Canvas sơ đồ nhánh đấu trong một thẻ `<ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto">` để người dùng có thể tự do cuộn chuột theo cả hai chiều ngang dọc, hoặc tích hợp ma trận biến đổi (`MatrixTransform`) để hỗ trợ thao tác kéo thả chuột và phóng to/thu nhỏ (Zoom in/out).
3. **Mất đồng bộ trạng thái giữa các màn hình (State Management Fault):** Khi trọng tài bấm lưu tỷ số ở cửa sổ pop-up, màn hình sơ đồ nhánh đấu hoặc màn hình Livestream bên ngoài không tự động đổi dữ liệu trừ khi tắt đi bật lại.
   * *Chiến lược phòng ngừa:* Tuyệt đối không dùng biến toàn cục (Static variable) hoặc gọi trực tiếp view này từ view kia. Sử dụng tính năng `StronglyDrivenMessenger` hoặc `WeakReferenceMessenger` có sẵn trong thư viện `CommunityToolkit.Mvvm` để gửi thông điệp dạng Publish-Subscribe (Pub/Sub). Khi cửa sổ chấm điểm lưu thành công, nó bắn một thông điệp `MatchUpdatedMessage`, màn hình sơ đồ nhận được sẽ tự động kích hoạt hàm nạp lại dữ liệu bất đồng bộ.