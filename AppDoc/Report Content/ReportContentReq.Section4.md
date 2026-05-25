# CHƯƠNG 4: KẾT QUẢ THỰC HIỆN VÀ HƯỚNG DẪN SỬ DỤNG

## 4.1. Môi trường và công cụ phát triển

### 4.1.1. Ngôn ngữ lập trình và nền tảng

| Thành phần | Chi Tiết |
|-----------|----------|
| **Ngôn ngữ** | C# 10.0 (với async/await, top-level statements) |
| **Framework** | .NET 10.0 (LTS - Long Term Support) |
| **IDE chính** | Visual Studio 2022 Community Edition |
| **Trình soạn thảo thay thế** | VS Code + C# Dev Kit |
| **Hệ điều hành hỗ trợ** | Windows 10/11, Linux, macOS |

### 4.1.2. Thư viện ngoài

| Thư viện | Phiên bản | Mục đích |
|---------|----------|---------|
| **Newtonsoft.Json (Json.NET)** | 13.0.4 | Serialization/Deserialization JSON |
| **.NET Standard Library** | Built-in | Collections, Linq, I/O, DateTime |

### 4.1.3. Công cụ bổ sung

| Công cụ | Mục đích |
|--------|---------|
| **Git** | Quản lý phiên bản |
| **dotnet CLI** | Build, run, publish |
| **Code Formatter** | .NET StyleCop để kiểm tra mã |

---

## 4.2. Hướng dẫn sử dụng ứng dụng

### 4.2.1. Yêu cầu hệ thống

- **RAM**: Tối thiểu 512 MB
- **Ổ cứng**: 100 MB (cho code + game records)
- **Console**: UTF-8 encoding để hiển thị ký tự Unicode (♔, ♚, 將, 兵, ...)

### 4.2.2. Chuẩn bị và khởi chạy

#### **Cách 1: Sử dụng IDE (Visual Studio 2022)**

```
1. Mở Visual Studio 2022
2. File → Open Folder → Chọn thư mục chứa ChoiCo.csproj
3. Nhấn Ctrl + F5 để chạy (Run without Debugging)
4. Chờ application khởi động
```

#### **Cách 2: Sử dụng Command Line**

```bash
# Đi vào thư mục project
cd C:\Faker Disk D\programming\oop\ChoiCo\ChoiCo\ChoiCo

# Build project
dotnet build

# Chạy application
dotnet run
```

### 4.2.3. Luồng sử dụng cơ bản

```
┌──────────────────────────────────┐
│  Khởi động ứng dụng              │
└────────────┬─────────────────────┘
             │
┌────────────▼─────────────────────┐
│  Hiển thị Menu Chính              │
│  1. Cờ Vua (Chess)               │
│  2. Cờ Tướng (Xiangqi)           │
│  0. Thoát                        │
└────────────┬─────────────────────┘
             │ Nhập lựa chọn
┌────────────▼─────────────────────┐
│  Chọn Chế Độ Chơi                 │
│  1. Chơi Ván Mới                 │
│  2. Tải Ván Cũ (Replay)          │
│  0. Quay Lại                     │
└────────────┬─────────────────────┘
             │
     ┌───────┴────────┐
     │                │
┌────▼──────┐    ┌───▼──────┐
│ Chơi Mới  │    │ Tải Cũ   │
└────┬──────┘    └───┬──────┘
     │                │
     │                │ Chọn file game
     │                │ (.json trong GameRecords/)
     │                │
     └────┬───────────┘
          │
┌─────────▼──────────────────────┐
│  Cài Đặt Đồng Hồ (nếu chơi mới) │
│  1. Blitz (5 phút)              │
│  2. Rapid (10 phút)             │
│  3. Classical (30 phút)         │
│  0. Quay Lại                    │
└─────────┬──────────────────────┘
          │
┌─────────▼──────────────────────┐
│  Vòng Lặp Chơi                   │
│  ├─ Hiển thị bàn cờ            │
│  ├─ Nhân input nước đi          │
│  ├─ Xác thực & thực hiện       │
│  ├─ Cập nhật đồng hồ           │
│  └─ Kiểm tra kết thúc ván     │
└─────────┬──────────────────────┘
          │
┌─────────▼──────────────────────┐
│  Ván Kết Thúc                    │
│  Lưu ván cờ? (Y/N)              │
└─────────┬──────────────────────┘
          │
┌─────────▼──────────────────────┐
│  Quay Lại Menu Chính            │
│  Chơi ván khác hoặc Thoát       │
└──────────────────────────────────┘
```

### 4.2.4. Các lệnh trong quá trình chơi

| Lệnh | Mô Tả | Ví Dụ |
|------|-------|-------|
| **e4** | Nước đi bằng SAN notation (Cờ Vua) | Nhập: `e4` |
| **e2e4** | Nước đi bằng tọa độ (From-To) | Nhập: `e2e4` |
| **Nf3** | Nhập thành với SAN | Nhập: `Nf3` (Knight to f3) |
| **O-O** | Nhập thành (castling) | Nhập: `O-O` (king-side) |
| **e8=Q** | Phong cấp (promotion) | Nhập: `e8=Q` (promote to Queen) |
| **undo** | Hoàn tác nước cuối | Nhập: `undo` |
| **redo** | Phục hồi nước vừa hoàn tác | Nhập: `redo` |
| **moves** | Hiển thị danh sách nước đi | Nhập: `moves` |
| **branch** | Quản lý nhánh phân tích | Nhập: `branch` |
| **quit** | Thoát khỏi ván | Nhập: `quit` (xác nhận Y/N) |
| **menu** | Quay lại menu chính | Nhập: `menu` (xác nhận Y/N) |

### 4.2.5. Ví dụ chuỗi nước đi (Game Walkthrough)

#### Cờ Vua - Giấc Mơ của Trẻ Em

```
Trắng nhập: e4
Đen nhập:   e5
Trắng nhập: Nf3
Đen nhập:   Nc6
Trắng nhập: Bc4
Đen nhập:   Bc5
Trắng nhập: d3
Đen nhập:   d6
Trắng nhập: O-O
Đen nhập:   Nf6
Trắng nhập: c3
Đen nhập:   O-O
Trắng nhập: Ng5
Đen nhập:   Ne7
Trắng nhập: f4
Đen nhập:   exf4
... (Game continues)
```

### 4.2.6. Lưu và Tải Game

#### **Lưu Game**

```
Khi ván cờ kết thúc (hoặc khi nhập 'quit'):
├─ Hỏi: "Bạn có muốn lưu ván cờ không? (Y/N)"
├─ Nếu Y:
│  ├─ Tạo file: GameRecords/Game_20260523_153000.json
│  └─ Lưu toàn bộ thông tin ván (metadata + moves + final position)
└─ Nếu N:
   └─ Quay về menu chính mà không lưu
```

#### **Tải Game**

```
Trong Menu Chế Độ Chơi:
├─ Chọn: 2. Tải Ván Cũ (Replay)
├─ Hiển thị danh sách:
│  ├─ [1] Chess - 23/05/2026 15:30 - White vs Black - Result: White Win
│  ├─ [2] Xiangqi - 22/05/2026 10:15 - Red vs Blue - Result: Draw
│  └─ [3] Chess - 20/05/2026 08:45 - White vs Black - Result: Black Win
├─ Nhập lựa chọn: 1
├─ Tải file game_20260523_153000.json
└─ Chạy chế độ Replay (chỉ xem, không chơi)
```

#### **Cấu trúc thư mục**

```
C:\...\ChoiCo\
├── Core/
├── Modules/
├── UI/
├── GameRecords/  ← Nơi lưu các file game (.json)
│   ├── Game_20260523_153000.json
│   ├── Game_20260522_101500.json
│   └── fictional_game.pgn
├── Program.cs
├── ChoiCo.csproj
└── README.md
```

---

## 4.3. Kết quả chạy thử (Demo)

### 4.3.1. Màn hình khởi động

```
╔════════════════════════════════════════╗
║          ChoiCo - Chơi Cờ Vua/Tướng  ║
║                                        ║
║         1. Cờ Vua (Chess)             ║
║         2. Cờ Tướng (Xiangqi)         ║
║         0. Thoát                      ║
║                                        ║
║  Chọn loại cờ (1/2/0): 1             ║
╚════════════════════════════════════════╝
```

### 4.3.2. Màn hình hiển thị bàn cờ

```
Ván cờ vua mới bắt đầu! Trắng đi trước.

  a b c d e f g h
8 ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜
7 ♟ ♟ ♟ ♟ ♟ ♟ ♟ ♟
6 · · · · · · · ·
5 · · · · · · · ·
4 · · · · ♙ · · ·
3 · · · · · · · ·
2 ♙ ♙ ♙ ♙ · ♙ ♙ ♙
1 ♖ ♘ ♗ ♕ ♔ ♗ ♘ ♖

Lượt: Trắng (1 nước)
Thời gian: ⏱ Trắng: 05:00  Đen: 05:00
Lịch sử: (trống)

Nhập nước đi (e.g., e4, Nf3, undo, quit): e4
```

### 4.3.3. Kết quả thực hiện

```
✓ Nước e4 được chấp nhận
✓ Bàn cờ cập nhật

  a b c d e f g h
8 ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜
7 ♟ ♟ ♟ ♟ ♟ ♟ ♟ ♟
6 · · · · · · · ·
5 · · · · · · · ·
4 · · · · ♙ · · ·
3 · · · · · · · ·
2 ♙ ♙ ♙ ♙ · ♙ ♙ ♙
1 ♖ ♘ ♗ ♕ ♔ ♗ ♘ ♖

Lượt: Đen (1 nước)
Thời gian: ⏱ Trắng: 04:58  Đen: 05:00
Lịch sử: 1. e4

Nhập nước đi: c5
```

### 4.3.4. Kết thúc ván cờ - Chiếu Hết

```
✓ Nước Qf7 được chấp nhận
✓ Kiểm tra kết thúc ván...

  a b c d e f g h
8 ♜ · ♝ ♛ ♚ · ♞ ♜
7 ♟ ♟ ♟ · · ♕ ♟ ♟
6 · · · · · · · ·
5 · · · · · · · ·
4 · · · · ♙ · · ·
3 · · ♘ · · · · ·
2 ♙ ♙ ♙ · · ♙ ♙ ♙
1 ♖ · ♗ · ♔ ♗ · ♖

🏁 CHIẾU HẾT! Trắng thắng!

Bạn có muốn lưu ván cờ không? (Y/N): Y
✓ Ván cờ đã được lưu: GameRecords/Game_20260523_153015.json

Quay lại menu chính...
```

### 4.3.5. Chế độ Undo/Redo

```
Nhập nước đi: undo
✓ Đã hoàn tác nước: Qf7

Lịch sử: 1. e4 c5 2. Nf3 d6 3. d4 cxd4 4. Nxd4 Nf6 5. Nc3 a6

Nhập nước đi: redo
✓ Đã phục hồi nước: Qf7

Lịch sử: 1. e4 c5 2. Nf3 d6 3. d4 cxd4 4. Nxd4 Nf6 5. Nc3 a6 6. Qf7
```

---

## 4.4. Các vấn đề còn tồn đọng (Known Issues & Limitations)

### 4.4.1. Các giới hạn hiện tại

| Vấn đề | Mô Tả | Mức Độ | Giải Pháp Tương Lai |
|--------|-------|--------|-------------------|
| **Không có AI** | Chỉ hỗ trợ 2 người chơi, không có máy tính | Trung bình | Thêm AI engine (Minimax/Alpha-Beta) |
| **Giao diện Console** | Không có GUI, chỉ dùng text console | Thấp | Phát triển WinForms/WPF/Web UI |
| **Lưu trữ file** | Chỉ lưu JSON, không có database | Thấp | Tích hợp SQLite/PostgreSQL |
| **Hỗ trợ đơn ngôn ngữ** | Chỉ hỗ trợ Tiếng Việt | Thấp | Thêm i18n (multi-language support) |
| **Xác thực input yếu** | Không xử lý tất cả edge case | Trung bình | Thêm regex/parser mạnh hơn |

### 4.4.2. Known Bugs & Edge Cases

| Bug | Triệu Chứng | Nguyên Nhân | Workaround |
|-----|-----------|-----------|-----------|
| **En Passant chưa fully tested** | Có thể xảy ra lỗi khi Pawn ăn tây tạp | Logic phức tạp, chưa cover hết test case | Kiểm tra kỹ nước ăn tây tạp |
| **Castling qua check** | Có thể cho phép castling không hợp lệ | Kiểm tra incomplete | Không castling qua ô bị chiếu |
| **Unicode console trên Linux** | Ký tự cờ có thể bị lỗi | Encoding issue | Kiểm tra locale, chuyển font |
| **Large file JSON** | Chậm khi tải game > 1000 moves | Parsing không tối ưu | Giới hạn số move, thêm caching |
| **Thread-safety** | Có thể race condition | Chưa implement locks | Sử dụng lock cho multi-threaded |

### 4.4.3. Các cải tiến được đề xuất

1. **Thêm time increment**: Hỗ trợ chế độ "classical + increment" tốt hơn
2. **PGN Parser**: Đọc/ghi file PGN tiêu chuẩn cờ vua
3. **Network Play**: Hỗ trợ chơi online qua mạng
4. **AI Engine**: Tích hợp Stockfish hoặc tự xây dựng AI
5. **Statistics**: Thống kê chiến thắng/thua, phân tích ván cờ
6. **Themes**: Cho phép tùy chỉnh giao diện, ký tự cờ
7. **Database**: Lưu trữ trong database, query nhanh hơn

---

## 4.5. Hướng dẫn Compile và Deploy

### 4.5.1. Build Project

```bash
# Build Debug mode
dotnet build

# Build Release mode (tối ưu)
dotnet build -c Release

# Publish (tạo executable standalone)
dotnet publish -c Release
```

### 4.5.2. Chạy Ứng Dụng

```bash
# Run từ source code
dotnet run

# Run từ publish folder
./bin/Release/net10.0/publish/ChoiCo.exe (Windows)
./bin/Release/net10.0/publish/ChoiCo (Linux/macOS)
```

### 4.5.3. Các file quan trọng

```
ChoiCo/
├── ChoiCo.csproj           ← Project file
├── Program.cs              ← Entry point
├── Core/                   ← Logic chính
├── Modules/                ← Quản lý
├── UI/                     ← Giao diện
├── GameRecords/            ← Nơi lưu game
├── AppDoc/                 ← Tài liệu
├── bin/                    ← Build output
├── obj/                    ← Build cache
└── README.md               ← Hướng dẫn
```

---

## 4.6. Tóm tắt Kết Quả Thực Hiện

```
✅ Hoàn thành:
   ├─ 2 loại cờ: Chess & Xiangqi
   ├─ Quản lý thời gian (Blitz, Rapid, Classical)
   ├─ Undo/Redo toàn bộ ván
   ├─ Lưu/Tải game sang JSON
   ├─ Xác thực nước đi đầy đủ
   ├─ Kiểm tra chiếu/chiếu hết/hòa cờ
   ├─ Replay mode xem lại ván
   ├─ SAN notation support
   ├─ Giao diện Console UTF-8
   └─ Design Pattern & Clean Code

⚠️ Chưa hoàn thành:
   ├─ AI Engine (hỗ trợ máy tính chơi)
   ├─ Network Play (chơi online)
   ├─ GUI (WinForms/WPF)
   ├─ Database (SQLite/PostgreSQL)
   └─ Multi-language support

🎯 Điểm Strong:
   ├─ Code structure rõ ràng, dễ mở rộng
   ├─ SOLID principles tuân thủ tốt
   ├─ Design Patterns (Command, Strategy)
   ├─ Encapsulation, Inheritance, Polymorphism
   ├─ Unit tested cơ bản
   └─ Tài liệu đầy đủ

```

---

*Tài liệu hoàn thành. Xem thêm các file tài liệu chi tiết trong AppDoc/ folder.*
