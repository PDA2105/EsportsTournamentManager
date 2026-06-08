EsportsTournamentManager

Tổng quan
- EsportsTournamentManager là ứng dụng quản lý giải đấu eSports được triển khai bằng .NET Framework 4.7.2.

Yêu cầu trước
- Windows 10/11
- Visual Studio 2019/2022/2026 (Community/Professional/Enterprise) hoặc MSBuild tương thích
- .NET Framework 4.7.2 Developer Pack (để build project)
- (Tùy chọn) SQL Server / LocalDB nếu dự án sử dụng database

Cấu trúc dự án (thư mục chính)
- /EsportsTournamentManager.sln            - Solution file
- /EsportsTournamentManager/               - Dự án chính (project)
  - /Models/                               - Các model dữ liệu (Team.cs, Player.cs...)
  - /Views/                                - Giao diện (nếu là ứng dụng web/desktop có view)
  - /Controllers/                          - Logic điều khiển (nếu áp dụng)
  - /Data/                                 - Migration, seed, hoặc lớp truy cập dữ liệu
  - /Services/                             - Các dịch vụ nghiệp vụ
  - /Properties/                           - AssemblyInfo, settings
- /tests/ (nếu có)                         - Unit tests

Cách cài đặt và chạy
1. Lấy mã nguồn
   - Clone repository về máy: git clone <repo-url>
2. Mở project
   - Mở EsportsTournamentManager.sln bằng Visual Studio.
3. Cài đặt phụ thuộc
   - Nếu dùng NuGet packages: trong Visual Studio chọn Restore NuGet packages hoặc chạy "nuget restore".
4. Cấu hình database (nếu cần)
   - Chỉnh file cấu hình (app.config / web.config) để trỏ tới connection string thích hợp.
   - Chạy migrations hoặc tạo database nếu có hướng dẫn trong project.
5. Build và chạy
   - Trong Visual Studio: Build -> Build Solution, sau đó Debug -> Start Debugging (F5) hoặc Start Without Debugging (Ctrl+F5).
   - Hoặc dùng MSBuild: msbuild EsportsTournamentManager.sln /p:Configuration=Release

Chạy unit tests
- Mở Test Explorer trong Visual Studio và chạy các test.
- Hoặc dùng công cụ test runner phù hợp nếu project có cấu hình CI.

Ghi chú
- Điều chỉnh các bước cấu hình chi tiết theo README nội bộ hoặc file docs có sẵn trong repo.
- Không thêm secret vào mã nguồn; sử dụng biến môi trường hoặc Azure Key Vault / user secrets.

Liên hệ
- Thông tin liên hệ và hướng dẫn đóng góp: xem CONTRIBUTING.md (nếu có) hoặc liên hệ maintainer của repo.