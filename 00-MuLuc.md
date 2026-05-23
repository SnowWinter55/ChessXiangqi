# ChoiCo - Mục Lục Tài Liệu

## Hướng dẫn sử dụng các file tài liệu

Tài liệu dự án ChoiCo được chia thành các file nhỏ, dễ theo dõi và tìm kiếm:

### 📋 **Mục lục chính**

1. **[01-PhânTíchĐềTài.md](01-PhânTíchĐềTài.md)**
   - Giới thiệu chung về dự án ChoiCo
   - Mục tiêu dự án
   - Các tính năng chính

2. **[02-CấuTrúcDựÁn.md](02-CấuTrúcDựÁn.md)**
   - Kiến trúc tổng thể
   - Các thành phần Core (IBoard, IPiece, Move, IMoveValidator, ...)
   - Các thành phần quản lý (GameClock, HistoryManager, BranchTracker, GameSaver, ...)
   - Các thành phần UI (AppController, BoardRenderer, InputHandler, ...)

3. **[03-TriểnKhaiLogic.md](03-TriểnKhaiLogic.md)**
   - Chi tiết từng thành phần chính
   - Các phương thức quan trọng
   - Cách các thành phần tương tác

4. **[04-LuồngChọnGame.md](04-LuồngChọnGame.md)**
   - Luồng menu chọn loại game (Cờ vua/Cờ tướng)
   - Luồng chọn chế độ chơi (Chơi mới/Tải game cũ)
   - Luồng chọn cài đặt đồng hồ

5. **[05-LuồngChơiCờVua.md](05-LuồngChơiCờVua.md)**
   - Khởi tạo bàn cờ vua
   - Vòng lặp chơi chính
   - Xử lý input người chơi
   - Phân tích nước đi
   - Kết thúc ván cờ vua

6. **[06-LuồngChơiCờTướng.md](06-LuồngChơiCờTướng.md)**
   - Khởi tạo bàn cờ tướng
   - Vòng lặp chơi (tương tự cờ vua)
   - Các quy tắc riêng biệt của cờ tướng
   - Kết thúc ván cờ tướng

7. **[07-XửLýLogicTrongVăn.md](07-XửLýLogicTrongVăn.md)**
   - Xác thực nước đi (validation)
   - Quản lý thời gian (GameClock)
   - Quản lý lịch sử nước đi (Undo/Redo)
   - Quản lý nhánh (Branching)
   - Kiểm tra kết thúc ván (Checkmate, Stalemate, ...)

8. **[08-LuồngReplay.md](08-LuồngReplay.md)**
   - Tải game từ file
   - Điều hướng xem lại ván cờ
   - Các lệnh replay
   - Xuất vị trí bàn cờ

9. **[09-LuồngHậuKỳ.md](09-LuồngHậuKỳ.md)**
   - Lưu biên bản ván cờ
   - Định dạng file lưu
   - Quay lại menu chính
   - Thoát ứng dụng

---

## 🎯 Cách sử dụng tài liệu

### Nếu bạn muốn hiểu **tổng quan về dự án:**
→ Bắt đầu từ **01-PhânTíchĐềTài.md**

### Nếu bạn muốn hiểu **kiến trúc code:**
→ Đọc **02-CấuTrúcDựÁn.md** rồi **03-TriểnKhaiLogic.md**

### Nếu bạn muốn hiểu **luồng chơi game:**
→ Đọc **04-LuồngChọnGame.md** → **05-LuồngChơiCờVua.md** (hoặc **06-LuồngChơiCờTướng.md**)

### Nếu bạn muốn hiểu **các logic xử lý trong ván:**
→ Đọc **07-XửLýLogicTrongVăn.md**

### Nếu bạn muốn hiểu **luồng replay:**
→ Đọc **08-LuồngReplay.md**

### Nếu bạn muốn hiểu **luồng kết thúc ván:**
→ Đọc **09-LuồngHậuKỳ.md**

---

## 📊 Sơ đồ luồng chính

```
Chạy ứng dụng
    ↓
┌─ 04-LuồngChọnGame.md ─┐
│  - Chọn loại cờ      │
│  - Chọn chế độ        │
│  - Chọn cài đặt       │
└──────────┬────────────┘
           ↓
     ┌─────────────┐
     │ Chơi mới?   │
     └─┬─────────┬─┘
       │         │
      Có        Không
       │         │
       ↓         ↓
  05/06-       08-
 Luồng      LuồngReplay
 Chơi Cờ       ↓
   ↓         Xem lại ván
 07-          cũ
XửLý        09-LuồngHậuKỳ
Logic         ↓
   ↓       Lưu/Thoát
 Kết
 thúc
   ↓
 09-
Luồng
Hậu
Kỳ
```

---

## 💡 Lưu ý

- File được viết bằng **tiếng Việt**, dễ hiểu cho các lập trình viên Việt
- Mỗi file là **độc lập nhưng liên kết** với các file khác
- Sử dụng **sơ đồ ASCII** để minh họa các luồng
- Đầy đủ **chi tiết từng bước** trong các quy trình
