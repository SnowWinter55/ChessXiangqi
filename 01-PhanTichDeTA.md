# 01 - Phân Tích Đề Tài Dự Án ChoiCo

## 📖 Tổng Quan Dự Án

**ChoiCo** là một ứng dụng trò chơi cờ vua và cờ tướng được xây dựng theo nguyên tắc lập trình hướng đối tượng (OOP).

### 🎯 Mục Tiêu Chính

1. **Cung cấp hai trò chơi cờ tổng hợp:**
   - Cờ vua (Chess): Bàn cờ 8×8, 6 loại quân cờ
   - Cờ tướng (Xiangqi): Bàn cờ 10×9, 7 loại quân cờ, có cung điện và sông

2. **Hỗ trợ chơi offline:**
   - Hai người chơi trên cùng một máy tính
   - Giao diện console thân thiện

3. **Quản lý thời gian:**
   - Đồng hồ chơi cho mỗi người chơi
   - Hỗ trợ các chế độ: Blitz, Rapid, Classical

4. **Lưu và quay lại ván đấu:**
   - Lưu biên bản ván cờ sang file JSON
   - Tải lại ván cũ để xem lại hoặc phân tích
   - Hỗ trợ nhánh phân tích (Branching)

---

## 🎮 Các Tính Năng Chính

### Tính năng chơi game
- ✅ Khởi tạo bàn cờ theo tiêu chuẩn
- ✅ Nhập nước đi bằng tọa độ hoặc ký hiệu SAN (ví dụ: e4, Nf3)
- ✅ Validation nước đi đầy đủ
- ✅ Xử lý những nước đặc biệt:
  - **Cờ vua**: Nhập thành, chí tỉ, phong cấp
  - **Cờ tướng**: Điều kiện cung điện, sông, chặn quân

### Tính năng lịch sử
- ✅ Undo/Redo nước đi
- ✅ Xem lại toàn bộ nước đi
- ✅ Quản lý nhánh phân tích

### Tính năng đồng hồ
- ✅ Đồng hồ theo nước đi
- ✅ Cảnh báo hết giờ
- ✅ Hỗ trợ increment thời gian

### Tính năng lưu/tải
- ✅ Lưu biên bản ván cờ sang file
- ✅ Tải ván cũ để xem lại
- ✅ Hiển thị danh sách ván cờ đã lưu

---

## 📊 Các Thành Phần Chính

### Core (Lõi)
- **IBoard / ChessBoard / XiangqiBoard**: Biểu diễn bàn cờ
- **IPiece & các lớp Piece**: Biểu diễn quân cờ
- **Move & Position**: Biểu diễn nước đi
- **IMoveValidator & các lớp Validator**: Kiểm tra hợp lệ nước đi
- **GameState**: Trạng thái ván cờ

### Module quản lý
- **GameClock**: Quản lý thời gian chơi
- **HistoryManager**: Quản lý undo/redo
- **BranchTracker**: Quản lý cây nước đi (branching)
- **GameSaver / NotationExporter / NotationImporter**: Lưu/tải game
- **MatchRecord**: Biên bản toàn bộ ván cờ
- **SanFormatter**: Chuyển đổi SAN notation

### UI (Giao diện)
- **AppController**: Bộ điều khiển chính của game
- **BoardRenderer**: Hiển thị bàn cờ trên console
- **InputHandler**: Xử lý input từ người chơi
- **GameReplayUI**: Giao diện xem lại ván cờ

---

## 🔄 Ba Luồng Chính

### 🎮 Luồng 1: Chơi Cờ Vua
1. Chọn loại game → Cờ Vua
2. Chọn chế độ chơi → Chơi Mới
3. Chọn cài đặt đồng hồ
4. Khởi tạo bàn cờ 8×8
5. **Vòng lặp chơi:**
   - Hiển thị bàn cờ
   - Nhận input nước đi
   - Xác thực nước đi
   - Cập nhật bàn cờ
   - Kiểm tra kết thúc ván
6. Lưu ván cờ nếu muốn

### 🀄 Luồng 2: Chơi Cờ Tướng
- **Giống Luồng 1** nhưng:
  - Bàn cờ 10×9 (có cung điện và sông)
  - Quân cờ khác (Tướng, Sĩ, Voi, Mã, Xe, Pháo, Tốt)
  - Quy tắc kiểm tra khác

### 📺 Luồng 3: Xem Lại Ván Cờ (Replay)
1. Chọn loại game
2. Chọn chế độ chơi → Tải Game Cũ
3. Chọn file ván cờ
4. Tải và xác thực nước đi
5. **Vòng lặp xem lại:**
   - Hiển thị vị trí bàn cờ hiện tại
   - Nhận lệnh điều hướng (Previous/Next/Home/End/GoTo)
   - Cập nhật vị trí
6. Thoát về menu

---

## 🔧 Công Nghệ & Ngôn Ngữ

- **Ngôn ngữ**: C# 10
- **.NET Framework**: .NET 10.0
- **Kiến trúc**: OOP với các Design Patterns
  - Strategy Pattern (IMoveValidator)
  - Command Pattern (MoveCommand)
  - Observer Pattern (GameClock events)
  - Tree Structure (BranchTracker)
- **Lưu trữ**: JSON (Newtonsoft.Json)

---

## 📁 Cấu Trúc Thư Mục

```
ChoiCo/
├── Core/                 # Lõi logic game
│   ├── Enums/           # Color, GameType, PieceType
│   ├── Interfaces/      # IBoard, IPiece, IMoveValidator, ...
│   ├── Models/
│   │   ├── Chess/       # ChessBoard, MoveValidator, Pieces
│   │   ├── Xiangqi/     # XiangqiBoard, MoveValidator, Pieces
│   │   └── Common/      # GameState, Move, Position
│   └── Extension/       # Mở rộng các lớp
├── Modules/             # Module quản lý
│   ├── Clock/           # GameClock, ClockSettings
│   ├── Engine/          # AI (placeholder)
│   ├── Movement/        # HistoryManager, MoveCommand, MoveParser
│   └── Notation/        # GameSaver, Exporter, Importer, BranchTracker
├── UI/                  # Giao diện
│   ├── AppController.cs # Điều khiển chính
│   └── ConsoleUI/       # Giao diện console
├── GameRecords/         # Lưu trữ ván cờ (JSON files)
└── Program.cs           # Điểm vào ứng dụng
```

---

## 🎯 Các Quy Tắc & Định Nghĩa

### Màu sắc (Color)
- `White` / `Red`: Người chơi 1 (đi trước)
- `Black` / `Blue`: Người chơi 2 (đi sau)

### Loại game (GameType)
- `Chess`: Cờ vua
- `Xiangqi`: Cờ tướng

### Kết thúc ván
- **Checkmate / Chiếu hết**: Vua/Tướng bị chiếu và không có nước thoát
- **Stalemate / Hòa cờ**: Người chơi không bị chiếu nhưng không có nước hợp lệ
- **Resignation**: Một người chơi từ bỏ
- **Time Out**: Hết thời gian chơi

---

## 🚀 Luồng Khởi Động Ứng Dụng

```
Program.Main()
    ↓
Vòng lặp chính: While(true)
    ├── Hiện menu chọn loại game (Cờ vua / Cờ tướng / Thoát)
    ├── Tạo Board & Validator tương ứng
    ├── Hiện menu chọn chế độ (Chơi mới / Tải cũ)
    │   ├── Nếu Chơi mới:
    │   │   ├── Chọn cài đặt đồng hồ
    │   │   └── Tạo AppController & chơi game
    │   └── Nếu Tải cũ:
    │       ├── Chọn file game
    │       └── Tạo AppController & replay game
    ├── Xử lý ván cờ (Play/Replay)
    ├── Lưu ván nếu muốn
    └── Quay lại menu chính
```

---

## 💡 Ưu Điểm Thiết Kế

✅ **Tách biệt trách nhiệm (Separation of Concerns)**
- Board logic, Validation, UI, Management tách riêng

✅ **Tái sử dụng code (Code Reuse)**
- Một cấu trúc Board cho cả cờ vua và cờ tướng
- Chia sẻ các component UI

✅ **Dễ mở rộng (Extensibility)**
- Thêm game type mới: Tạo Board & Validator mới
- Thêm feature mới: Tạo Module mới

✅ **Dễ kiểm thử (Testability)**
- Các thành phần độc lập, dễ unit test

---

**Tiếp theo:** Đọc [02-CấuTrúcDựÁn.md](02-CấuTrúcDựÁn.md) để hiểu chi tiết kiến trúc code.
