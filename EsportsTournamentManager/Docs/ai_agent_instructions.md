AI Agent Instructions

Mục đích
- Tài liệu này quy định cách AI-agent (automated coding agent) tương tác với kho mã EsportsTournamentManager.

Quy tắc chung
- Chỉ thực hiện thay đổi khi được yêu cầu rõ ràng bởi con người.
- Thực hiện thay đổi tối thiểu cần thiết để đạt mục tiêu.
- Luôn xây dựng dự án và chạy test (nếu có) trước khi kết thúc một nhiệm vụ.
- Không bao giờ thêm bí mật (password, API keys, certificates) trực tiếp vào mã nguồn hoặc commit.
- Không gọi API bên ngoài hoặc tải xuống nội dung không được phép.

Định dạng commit / nhánh
- Sử dụng nhánh có tiền tố task/ hoặc fix/ (ví dụ: task/add-login, fix/bug-123).
- Commit message ngắn gọn, bắt đầu bằng động từ hiện tại: "Add", "Fix", "Update" kèm mã issue nếu có.

Kiểm thử và build
- Sau thay đổi: chạy "Build Solution" trong Visual Studio hoặc msbuild.
- Nếu có unit tests, chạy qua Test Explorer và đảm bảo tất cả test đều pass.

Phân quyền và PR
- Không tự động merge PR; luôn yêu cầu review từ thành viên con người.
- PR phải bao gồm mô tả ngắn, thay đổi chính, và hướng dẫn kiểm thử.

Xử lý lỗi và báo cáo
- Nếu build/test fail, ghi rõ lỗi và dừng công việc. Thông báo cho người quản lý với logs.

Ghi chú bổ sung
- Tuân thủ chuẩn codebase hiện có (style, naming).
- Không tạo file nhạy cảm trong repository.

Cập nhật tài liệu
- Mọi thay đổi quy trình do AI đề xuất phải được xác nhận bởi con người trước khi áp dụng.