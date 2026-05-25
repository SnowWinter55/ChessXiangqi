# 02 - Cấu Trúc Dự Án

## 🏗️ Kiến Trúc Tổng Thể

```
┌─────────────────────────────────────────────────┐
│        GIAO DIỆN (UI Layer)                     │
│  AppController → BoardRenderer → InputHandler   │
│  GameReplayUI                                   │
└──────────────┬──────────────────────────────────┘
               │
┌──────────────┴──────────────────────────────────┐
│      QUẢN LÝ (Management Layer)                 │
│  GameClock ← → HistoryManager ← → BranchTracker│
│  GameSaver ← → NotationExporter/Importer       │
│  MatchRecord ← → GameState                     │
└──────────────┬──────────────────────────────────┘
               │
┌──────────────┴──────────────────────────────────┐
│   LÕI GAME (Core/Logic Layer)                  │
│  IBoard (ChessBoard / XiangqiBoard)            │
│  IPiece (King, Queen, Rook, ... / Tướng, ...)│
│  IMoveValidator (ChessMoveValidator / ...)    │
│  Move, Position, GameState, Color, PieceType │
└─────────────────────────────────────────────────┘
```

---

## 📋 Thành Phần Core

### 1️⃣ **IBoard Interface** - Biểu diễn Bàn Cờ
```csharp
Tính chất:
├── Rows (int): Số hàng (8 cho cờ vua, 10 cho cờ tướng)
├── Cols (int): Số cột (8 cho cờ vua, 9 cho cờ tướng)
├── GameType (enum): Chess hoặc Xiangqi
└── LastMove (Move): Nước đi cuối cùng

Phương thức chính:
├── GetPieceAt(Position) → IPiece: Lấy quân tại vị trí
├── SetPieceAt(Position, IPiece): Đặt quân tại vị trí
├── MakeMove(Move): Thực hiện nước đi trên bàn
├── Clone() → IBoard: Tạo bản sao sâu (deep copy)
└── GetAllPieces() → Collection<(Position, IPiece)>: Lấy tất cả quân
```

**Cài đặt cụ thể:**
- `ChessBoard (8×8)`: Bàn cờ vua chuẩn
- `XiangqiBoard (10×9)`: Bàn cờ tướng với cung điện & sông

---

### 2️⃣ **IPiece Interface** - Biểu diễn Quân Cờ
```csharp
Tính chất:
├── Type (PieceType): Loại quân (Pawn, King, Queen, ...)
├── Color (Color): Màu quân (White/Red hoặc Black/Blue)
└── IsPromoted (bool): Chỉ dành cho Pawn khi phong cấp

Phương thức chính:
└── GetValidMoves(IBoard, Position) → List<Position>
    Trả về danh sách vị trí hợp lệ quân có thể di chuyển
```

**Cài đặt cụ thể:**

| Cờ Vua | Cờ Tướng |
|--------|----------|
| King   | General  |
| Queen  | Advisor  |
| Rook   | Chariot  |
| Bishop | Elephant |
| Knight | Horse    |
| Pawn   | Soldier  |
|        | Cannon   |

---

### 3️⃣ **Move Class** - Biểu diễn Nước Đi
```csharp
Tính chất:
├── From (Position): Vị trí xuất phát
├── To (Position): Vị trí đích
├── CapturedPiece (IPiece): Quân bị ăn (nếu có)
├── San (string): Ký hiệu SAN (ví dụ: "e4", "Nf3")
├── IsEnPassant (bool): Ăn tây tạp (chỉ cờ vua)
├── IsCastling (bool): Nhập thành (chỉ cờ vua)
└── PromotionPiece (PieceType?): Quân phong cấp (nếu có)
```

---

### 4️⃣ **IMoveValidator Interface** - Kiểm Tra Nước Đi
```csharp
Phương thức chính:
├── IsValidMove(IBoard, Move, Color) → bool
│   Kiểm tra nước đi có hợp lệ không
│
├── IsCheck(IBoard, Color) → bool
│   Vua/Tướng có bị chiếu không
│
├── IsCheckmate(IBoard, Color) → bool
│   Vua/Tướng có bị chiếu hết không
│
├── IsStalemate(IBoard, Color) → bool
│   Hòa cờ (không bị chiếu nhưng không có nước)
│
├── GetAllValidMoves(IBoard, Color) → List<Move>
│   Danh sách tất cả nước hợp lệ cho một màu
│
└── MoveLeavesKingInCheck(IBoard, Move, Color) → bool
    Nước này có để vua bị chiếu không
```

**Cài đặt cụ thể:**
- `ChessMoveValidator`: Kiểm tra quy tắc cờ vua
- `XiangqiMoveValidator`: Kiểm tra quy tắc cờ tướng

---

### 5️⃣ **Position Class** - Vị Trí Trên Bàn
```csharp
Tính chất:
├── Row (int): Hàng (0-7 cờ vua, 0-9 cờ tướng)
├── Col (int): Cột (0-7 cờ vua, 0-8 cờ tướng)

Phương thức:
└── Equals(Position) → bool: So sánh hai vị trí
```

---

### 6️⃣ **GameState Class** - Trạng Thái Ván
```csharp
Tính chất:
├── Fen (string): Ký hiệu FEN (vị trí bàn)
├── CurrentTurn (Color): Ai đang đi
├── WhiteTimeSeconds / BlackTimeSeconds (int): Thời gian còn lại
├── IsGameOver (bool): Ván đã kết thúc
├── GameOverReason (string): Lý do kết thúc
└── MoveNumber (int): Số nước đi
```

---

## 📊 Thành Phần Quản Lý

### 7️⃣ **GameClock Class** - Quản Lý Thời Gian

```csharp
Tính chất:
├── Settings (ClockSettings): Cài đặt thời gian
├── TimeWhiteSeconds / TimeBlackSeconds (int): Thời gian còn lại
└── Events:
    ├── OnTimeOut: Hết giờ
    ├── OnTimeUpdated: Cập nhật thời gian
    └── OnTurnStarted: Bắt đầu lượt

Phương thức chính:
├── StartTurn(Color): Bắt đầu lượt đi
├── StopTurn(): Kết thúc lượt đi (trừ thời gian, cộng increment)
└── GetRemainingTime(Color) → int: Lấy thời gian còn lại
```

**Chế độ hỗ trợ:**
- Blitz: 5 phút (300 giây)
- Rapid: 10 phút + 3 giây mỗi nước
- Classical: 30 phút

---

### 8️⃣ **HistoryManager Class** - Quản Lý Lịch Sử Nước Đi

```csharp
Dùng Command Pattern để quản lý Undo/Redo

Tính chất:
├── CanUndo (bool): Có thể undo không
└── CanRedo (bool): Có thể redo không

Phương thức chính:
├── ExecuteCommand(ICommand): Thực hiện nước đi
├── Undo() → bool: Lùi lại nước trước
├── Redo(out Move, out string) → bool: Làm lại nước
└── Clear(): Xóa lịch sử
```

**Cấu trúc nội tại:**
- `_undoStack`: Ngăn xếp nước đi đã thực hiện
- `_redoStack`: Ngăn xếp nước đi đã undo

---

### 9️⃣ **BranchTracker Class** - Quản Lý Nhánh Phân Tích

```csharp
Dùng Tree Structure để theo dõi các biến thể

Tính chất:
├── _currentNode (Node): Nút hiện tại
├── _mainLine (List<Move>): Nước chính
└── Node:
    ├── Move: Nước đi
    ├── Parent: Nút cha
    └── Children: Các nước thay thế

Phương thức chính:
├── AddMove(Move): Thêm nước đi
├── GoBack() → bool: Lùi về nước trước
├── GetCurrentLine() → List<Move>: Lấy nước chính
├── GetAvailableBranches() → List<Move>: Nước thay thế
└── GoToMoveIndex(int): Nhảy tới nước thứ N
```

**Ví dụ:**
```
Nước chính:    1.e4  c5  2.Nf3  d6  3.d4
                ↓    ↓    ↓
Nhánh 1:       1.e4 e5  2.Nf3
Nhánh 2:       1.e4 c5  2.c3
```

---

### 🔟 **GameSaver Class** - Lưu/Tải Game

```csharp
Giao diện cấp cao cho lưu/tải game

Phương thức chính:
├── SaveGame(string filePath, MatchRecord)
│   Lưu ván cờ sang file JSON
│
├── LoadGame(string filePath) → MatchRecord
│   Tải ván cờ từ file JSON
│
└── ReplayGame(MatchRecord, IBoard, IMoveValidator, 
               HistoryManager, BranchTracker) → bool
    Tái hiện lại ván cờ trên bàn
```

**Cấu trúc nội tại:**
- `NotationExporter`: Chuyển MatchRecord → JSON
- `NotationImporter`: Chuyển JSON → MatchRecord

---

### 1️⃣1️⃣ **MatchRecord Class** - Biên Bản Ván Cờ

```csharp
Chứa toàn bộ thông tin ván cờ

Tính chất:
├── GameType (string): "Chess" hoặc "Xiangqi"
├── Moves (List<Move>): Tất cả nước đi
├── FinalState (GameState): Vị trí cuối cùng
├── Metadata:
│   ├── Date (DateTime): Ngày tháng
│   ├── Event (string): Sự kiện
│   ├── WhitePlayer / BlackPlayer (string): Tên người chơi
│   └── TimeControl (string): Cài đặt thời gian

Phương thức chính:
├── GetMoveAtIndex(int) → Move: Lấy nước thứ N
├── GetTotalMoves() → int: Tổng số nước
└── ValidateAllMoves(IBoard, IMoveValidator, Color) → bool
    Xác thực tất cả nước có hợp lệ không
```

---

### 1️⃣2️⃣ **SanFormatter Class** - Chuyển Đổi SAN Notation

```csharp
Chuyển đổi giữa Move object và ký hiệu SAN

Ký hiệu SAN (Standard Algebraic Notation):
├── e4         → Tốt tới e4
├── Nf3        → Mã tới f3
├── Bxe5       → Tượng ăn e5
├── O-O        → Nhập thành cánh vua
├── O-O-O      → Nhập thành cánh hậu
├── e8=Q       → Tốt phong cấp thành Hậu
└── e4+        → Tới e4 và chiếu

Phương thức:
├── FormatSan(Move, IBoard) → string
    Chuyển Move → SAN
│
└── ParseSan(string, IBoard, Color) → Move
    Chuyển SAN → Move
```

---

## 🎮 Thành Phần UI

### 1️⃣3️⃣ **AppController Class** - Bộ Điều Khiển Chính

```csharp
Tính chất (Quản lý trạng thái game):
├── _board (IBoard): Bàn cờ hiện tại
├── _validator (IMoveValidator): Kiểm tra nước đi
├── _clock (GameClock): Đồng hồ
├── _historyManager (HistoryManager): Undo/Redo
├── _branchTracker (BranchTracker): Nhánh phân tích
├── _currentTurn (Color): Lượt đi
├── _gameOver (bool): Ván kết thúc
└── _isReplayMode (bool): Đang xem lại?

Phương thức chính:
├── Run(): Vòng lặp chơi game
├── ReplayGameFromFile(string): Tải và xem lại ván
├── PerformUndo() / PerformRedo(): Undo/Redo
├── ShowBranches(): Hiện nước thay thế
└── PromptToSaveGame(): Hỏi lưu ván
```

**Trách nhiệm:**
1. Quản lý luồng game
2. Xử lý input người chơi
3. Gọi validator, clock, history, branch tracker
4. Cập nhật giao diện
5. Kiểm tra kết thúc ván

---

### 1️⃣4️⃣ **BoardRenderer Class** - Hiển Thị Bàn Cờ

```csharp
Phương thức chính:
├── Render(IBoard, Color, int, int, List<string>)
│   Hiển thị toàn bộ trạng thái game
│   ├── Bàn cờ
│   ├── Lượt đi hiện tại
│   ├── Thời gian
│   └── Lịch sử nước đi
│
├── RenderChessBoard(IBoard)
│   Vẽ bàn cờ 8×8 với biểu tượng quân
│
└── RenderXiangqiBoard(IBoard)
    Vẽ bàn cờ 10×9 với cung điện & sông
```

**Biểu tượng quân:**
```
Cờ vua:  ♔ ♕ ♖ ♗ ♘ ♙ (trắng)
         ♚ ♛ ♜ ♝ ♞ ♟ (đen)

Cờ tướng: 帥 士 車 象 馬 砲 兵 (đỏ)
          將 士 車 象 馬 砲 兵 (đen)
```

---

### 1️⃣5️⃣ **InputHandler Class** - Xử Lý Input

```csharp
Phương thức chính:
├── GetCommandOrMove() → string
│   Nhận input từ người chơi
│
├── TryParseMove(string, IBoard, Color, out Move) → bool
│   Phân tích input thành Move object
│   Hỗ trợ định dạng:
│   ├── e2e4 (tọa độ)
│   ├── e4   (SAN - tốt)
│   └── Nf3  (SAN - quân khác)
│
└── DisplayInputHint(GameType)
    Hiển thị gợi ý nhập nước đi
```

**Lệnh đặc biệt:**
- `undo` / `Ctrl+Z`: Lùi lại nước
- `redo` / `Ctrl+Y`: Làm lại nước
- `moves`: Xem lịch sử nước đi
- `branch`: Xem nước thay thế
- `quit`: Thoát game
- `Backspace` trên input rỗng: Quay lại menu

---

### 1️⃣6️⃣ **GameReplayUI Class** - Giao Diện Xem Lại

```csharp
Phương thức chính:
├── DisplayGameInfo(MatchRecord)
│   Hiển thị thông tin ván cờ
│
├── DisplayReplayControls()
│   Hiển thị hướng dẫn lệnh replay
│
├── GetReplayCommand() → ReplayCommand
│   Nhận lệnh điều hướng
│   ├── ↑/←: Nước trước
│   ├── ↓/→: Nước sau
│   ├── Home: Về đầu
│   ├── End: Về cuối
│   ├── G: Nhảy tới nước thứ N
│   ├── E: Xuất vị trí
│   └── Q: Thoát
│
└── DisplayMoveCounter(int, int)
    Hiển thị "Move: 15/42"
```

---

## 🔗 Sơ Đồ Tương Tác

### Luồng Chơi Game
```
AppController.Run()
    ├─→ _renderer.Render()              [Hiển thị bàn]
    ├─→ _inputHandler.GetCommandOrMove() [Nhận input]
    ├─→ _inputHandler.TryParseMove()     [Phân tích]
    ├─→ _validator.IsValidMove()         [Kiểm tra]
    ├─→ _board.MakeMove()                [Thực hiện]
    ├─→ _historyManager.ExecuteCommand() [Lưu lịch sử]
    ├─→ _branchTracker.AddMove()         [Lưu nhánh]
    ├─→ _clock.StopTurn() → StartTurn()  [Chuyển lượt]
    ├─→ _validator.IsCheckmate()         [Kiểm tra kết thúc]
    └─→ Vòng lặp tiếp tục...
```

### Luồng Lưu/Tải Game
```
MatchRecord
    ↓
NotationExporter ← → JSON File
    ↓
NotationImporter
    ↓
GameSaver.ReplayGame()
    ├─→ Xác thực tất cả nước
    ├─→ Thực hiện lại từng nước
    ├─→ Cập nhật HistoryManager & BranchTracker
    └─→ Bàn cờ sẵn sàng xem lại
```

---

**Tiếp theo:** Đọc [03-TriểnKhaiLogic.md](03-TriểnKhaiLogic.md) để hiểu chi tiết từng thành phần.
