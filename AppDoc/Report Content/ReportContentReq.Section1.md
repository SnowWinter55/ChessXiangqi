# CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI

## 1.1. Lý do chọn đề tài

Trò chơi cờ vua và cờ tướng là những trò chơi chiến thuật cổ xưa có giá trị giáo dục cao, giúp phát triển tư duy logic và kỹ năng lập kế hoạch. Tuy nhiên, để xây dựng một ứng dụng hỗ trợ chơi hai loại trò chơi này, nhà phát triển phải:

1. **Quản lý các quy tắc phức tạp**: Mỗi loại quân cờ có hành vi di chuyển khác nhau, có các nước đi đặc biệt (ăn tây tạp, nhập thành, phong cấp trong cờ vua; điều kiện cung điện và sông trong cờ tướng).

2. **Xử lý trạng thái trò chơi**: Kiểm tra chiếu, chiếu hết, hòa cờ; quản lý lịch sử nước đi (undo/redo); quản lý thời gian chơi; lưu/tải biên bản ván cờ.

3. **Áp dụng nguyên tắc OOP**: Sử dụng Interface để trừu tượng hóa, kế thừa để tái sử dụng code, đa hình để linh hoạt xử lý các loại quân khác nhau.

Thực hiện đề tài này là cơ hội tốt để **demonstrate các kỹ năng lập trình OOP: design patterns, clean code, và quản lý dự án phần mềm**.

## 1.2. Mục tiêu và phạm vi của đồ án

### Mục tiêu chính:

Xây dựng ứng dụng **ChoiCo** - một nền tảng chơi trò chơi cờ vua và cờ tướng với các tính năng:

1. **Chơi hai loại cờ**: Cờ vua (Chess) 8×8 và cờ tướng (Xiangqi) 10×9
2. **Hỗ trợ chơi offline**: 2 người chơi trên cùng một máy tính
3. **Quản lý thời gian**: Hỗ trợ các chế độ Blitz (5 phút), Rapid (10 phút), Classical (30 phút)
4. **Lịch sử & phân tích**: Undo/Redo, xem danh sách nước đi, quản lý nhánh phân tích
5. **Lưu/tải ván cờ**: Lưu biên bản ván cờ vào file JSON, tải lại để replay hoặc phân tích

### Phạm vi dự án:

- **Giao diện**: Console (text-based), không dùng GUI
- **Lưu trữ dữ liệu**: File JSON lưu tại thư mục `GameRecords/`
- **Cơ sở dữ liệu**: Không sử dụng database thật, chỉ dùng file
- **Máy chủ**: Ứng dụng standalone, không kết nối mạng
- **Loại trò chơi**: Chỉ hỗ trợ chơi người vs người, không có AI
- **Ngôn ngữ**: C# 10.0, chạy trên .NET 10.0
- **Xử lý lỗi**: Xử lý các ngoại lệ cơ bản, không xử lý tất cả các edge case

## 1.3. Yêu cầu chức năng

| STT | Use-Case | Diễn Tả | Ưu Tiên |
|-----|----------|---------|---------|
| 1 | Chọn loại cờ | Người chơi chọn chơi Cờ Vua hoặc Cờ Tướng | Cao |
| 2 | Chọn chế độ chơi | Chọn Chơi Mới hoặc Tải ván cũ (Replay) | Cao |
| 3 | Cài đặt đồng hồ | Chọn chế độ thời gian: Blitz/Rapid/Classical | Cao |
| 4 | Hiển thị bàn cờ | Render bàn cờ trên console với Unicode symbols | Cao |
| 5 | Nhập nước đi | Nhập nước bằng tọa độ (e2e4) hoặc SAN notation (e4, Nf3) | Cao |
| 6 | Xác thực nước đi | Kiểm tra nước đi hợp lệ theo quy tắc cờ | Cao |
| 7 | Xử lý nước đặc biệt | Xử lý nhập thành, ăn tây tạp, phong cấp (Chess); điều kiện cung/sông (Xiangqi) | Cao |
| 8 | Kiểm tra kết thúc ván | Phát hiện chiếu hết, hòa cờ, hết giờ | Cao |
| 9 | Quản lý thời gian | Tính toán, hiển thị, cảnh báo hết giờ | Cao |
| 10 | Undo/Redo | Hoàn tác hoặc phục hồi nước đi | Trung bình |
| 11 | Xem lịch sử | Hiển thị danh sách tất cả nước đi | Trung bình |
| 12 | Quản lý nhánh | Tạo và chuyển đổi giữa các nhánh phân tích | Trung bình |
| 13 | Lưu ván cờ | Lưu biên bản ván cộng vào file JSON | Cao |
| 14 | Tải ván cờ | Tải ván cũ từ file để xem lại hoặc tiếp tục | Cao |
| 15 | Hiển thị danh sách ván | Liệt kê các ván cờ đã lưu | Trung bình |
| 16 | Replay ván cờ | Điều hướng xem lại từng nước đi, branch | Trung bình |
