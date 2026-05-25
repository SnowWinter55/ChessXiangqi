# CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG

## 2.1. Sơ đồ Lớp (Class Diagram)

### 2.1.1. Sơ đồ Tổng Thể

```
┌───────────────────────────────────────────────────────────────┐
│                         GIAO DIỆN (UI)                        │
├───────────────────────────────────────────────────────────────┤
│ AppController         │  BoardRenderer      │  InputHandler    │
│ - Run()               │  - Render()         │  - GetCommand()  │
│ - HandleCommand()     │  - RenderChess()    │  - TryParseMove()│
│ - PerformUndo()       │  - RenderXiangqi()  │  - DisplayHint() │
└────────────┬──────────────────────┬──────────────────┬─────────┘
             │                      │                  │
         Sử dụng                 Sử dụng            Sử dụng
             │                      │                  │
┌────────────▼──────────────────────▼──────────────────▼─────────┐
│                    QUẢN LÝ (Management)                        │
├───────────────────────────────────────────────────────────────┤
│ GameClock             │ HistoryManager    │ BranchTracker     │
│ - StartTurn()         │ - ExecuteCmd()    │ - AddMove()       │
│ - StopTurn()          │ - Undo()          │ - GoBack()        │
│ - CheckTimeOut()      │ - Redo()          │ - GetBranches()   │
│                       │                   │                   │
│ GameSaver            │ MatchRecord        │ SanFormatter      │
│ - Save()             │ - AddMove()        │ - FormatSan()    │
│ - Load()             │ - GetFen()         │ - ParseSan()      │
└─┬──────┬──────┬──────┬──────┬────────┬───────┬──────┬───────┬─┘
  │      │      │      │      │        │       │      │       │
┌─▼──────▼──────▼──────▼──────▼────────▼───────▼──────▼───────▼─┐
│               LÕI LOGIC (Core/Logic Layer)                    │
├───────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─ IBoard ────────────────┐                                 │
│  │  + GetPieceAt()         │                                 │
│  │  + SetPieceAt()         │                                 │
│  │  + MakeMove()           │                                 │
│  │  + Clone()              │                                 │
│  └─────────────────────────┘                                 │
│        ▲                 ▲                                    │
│        │                 │ Implements                        │
│     Cấp 1            Cấp 2                                   │
│        │                 │                                   │
│  ┌─────┴──────┐   ┌──────┴────────┐                         │
│  │ ChessBoard │   │ XiangqiBoard  │                         │
│  └────────────┘   └───────────────┘                         │
│                                                               │
│  ┌─ IPiece ───────────────┐                                 │
│  │ + GetValidMoves()       │                                 │
│  │ + GetAttackMoves()      │                                 │
│  └─────────────────────────┘                                 │
│        ▲                 ▲                                    │
│        │                 │ Implements                        │
│        │ Chess Pieces    Xiangqi Pieces                     │
│   ┌────┴─────┐  ┌────────┴────────┐                        │
│   │Pawn,King │  │Soldier,General,│                        │
│   │Queen,... │  │Cannon,Horse,...│                        │
│   └──────────┘  └─────────────────┘                        │
│                                                               │
│  ┌─ IMoveValidator ────────────────┐                       │
│  │ + IsValidMove()                  │                       │
│  │ + IsCheck()                      │                       │
│  │ + IsCheckmate()                  │                       │
│  │ + GetAllValidMoves()             │                       │
│  └──────────────────────────────────┘                       │
│        ▲              ▲                                      │
│        │              │ Implements                          │
│        │              │                                      │
│  ┌─────┴──────┐   ┌───┴──────────┐                        │
│  │ChessMoveVal│   │XiangqiMoveVal│                        │
│  └────────────┘   └───────────────┘                        │
│                                                               │
│  Models: Move, Position, GameState, GameType, Color,        │
│          PieceType                                           │
└───────────────────────────────────────────────────────────────┘
```

### 2.1.2. Chi Tiết các Thành Phần Chính

#### **Layer 1: Giao Diện (UI)**

| Lớp | Trách Nhiệm | Phương Thức Chính |
|-----|-------------|------------------|
| `AppController` | Bộ điều khiển chính, điều phối luồng game | `Run()`, `HandleCommand()`, `PerformUndo()`, `PerformRedo()` |
| `BoardRenderer` | Hiển thị bàn cờ trên console | `Render()`, `RenderChessBoard()`, `RenderXiangqiBoard()` |
| `InputHandler` | Xử lý input từ người dùng | `GetCommand()`, `TryParseMove()`, `DisplayInputHint()` |
| `GameReplayUI` | Giao diện xem lại ván cờ | `ShowReplay()`, `NavigateMoves()` |

#### **Layer 2: Quản Lý (Management)**

| Lớp | Trách Nhiệm | Phương Thức Chính |
|-----|-------------|------------------|
| `GameClock` | Quản lý thời gian chơi | `StartTurn()`, `StopTurn()`, `CheckTimeOut()` |
| `HistoryManager` | Quản lý undo/redo | `ExecuteCommand()`, `Undo()`, `Redo()` |
| `BranchTracker` | Quản lý nhánh phân tích | `AddMove()`, `GoBack()`, `GetBranches()` |
| `GameSaver` | Lưu/tải ván cờ | `Save()`, `Load()` |
| `MatchRecord` | Ghi chép toàn bộ ván cờ | `AddMove()`, `GetFen()`, `ExportToJson()` |
| `SanFormatter` | Chuyển đổi SAN notation | `FormatSan()`, `ParseSan()` |

#### **Layer 3: Lõi Logic (Core)**

| Lớp/Interface | Trách Nhiệm | Phương Thức Chính |
|---------------|-------------|------------------|
| `IBoard` | Trừu tượng hóa bàn cờ | `GetPieceAt()`, `SetPieceAt()`, `MakeMove()`, `Clone()` |
| `ChessBoard` | Cài đặt bàn cờ vua (8×8) | `InitializeStandardPosition()` |
| `XiangqiBoard` | Cài đặt bàn cờ tướng (10×9) | `InitializeStandardPosition()` |
| `IPiece` | Trừu tượng hóa quân cờ | `GetValidMoves()`, `GetAttackMoves()` |
| Các lớp Piece | Cài đặt từng loại quân | `GetValidMoves()` (riêng từng quân) |
| `IMoveValidator` | Trừu tượng hóa xác thực nước đi | `IsValidMove()`, `IsCheck()`, `IsCheckmate()` |
| `ChessMoveValidator` | Xác thực nước cờ vua | `IsValidMove()`, `IsCheck()` |
| `XiangqiMoveValidator` | Xác thực nước cờ tướng | `IsValidMove()`, `IsCheck()` |

---

## 2.2. Vận dụng các đặc trưng cốt lõi của OOP

### 2.2.1. Tính Đóng Gói (Encapsulation)

**Vấn đề**: Nếu không sử dụng encapsulation, các attribute public của lớp sẽ bị thay đổi tùy tiện, dẫn đến trạng thái bàn cờ không nhất quán.

**Cách giải quyết**: Sử dụng **private/protected fields** kết hợp với **properties**:

```csharp
// Ví dụ: GameClock class
public class GameClock
{
    private int _whiteTimeSeconds;    // Private field
    private int _blackTimeSeconds;    // Private field
    private Timer _timer;             // Private field
    
    // Public properties - chỉ cho phép đọc
    public int WhiteTimeRemaining => _whiteTimeSeconds;
    public int BlackTimeRemaining => _blackTimeSeconds;
    
    // Phương thức public để kiểm soát đặt giá trị
    public void SetTime(Color color, int seconds)
    {
        if (seconds < 0)
            throw new ArgumentException("Thời gian không thể âm!");
        
        if (color == Color.White)
            _whiteTimeSeconds = seconds;
        else
            _blackTimeSeconds = seconds;
    }
}
```

**Lợi ích**:
- Dữ liệu được bảo vệ khỏi thay đổi không hợp lệ (validation)
- Dễ mở rộng (thêm logic trong setter mà không ảnh hưởng client code)
- Tránh state inconsistency

---

### 2.2.2. Tính Kế Thừa (Inheritance)

**Vấn đề**: Cả ChessBoard và XiangqiBoard đều có cấu trúc giống nhau (ma trận quân cờ, phương thức GetPieceAt, SetPieceAt...), dẫn đến code duplication.

**Cách giải quyết**: Sử dụng **Interface** và **Base Class** pattern:

```csharp
// Interface trừu tượng
public interface IBoard
{
    int Rows { get; }
    int Cols { get; }
    IPiece GetPieceAt(Position pos);
    void SetPieceAt(Position pos, IPiece piece);
    void MakeMove(Move move);
    IBoard Clone();
}

// Cài đặt cụ thể cho Cờ Vua
public class ChessBoard : IBoard
{
    private IPiece[,] _board;  // Ma trận 8x8
    
    public int Rows => 8;
    public int Cols => 8;
    
    public ChessBoard()
    {
        _board = new IPiece[8, 8];
        InitializeStandardPosition();  // Xếp quân chuẩn
    }
    
    public IPiece GetPieceAt(Position pos) => _board[pos.Row, pos.Col];
    public void SetPieceAt(Position pos, IPiece piece) => _board[pos.Row, pos.Col] = piece;
    // ... các phương thức khác
}

// Cài đặt cụ thể cho Cờ Tướng
public class XiangqiBoard : IBoard
{
    private IPiece[,] _board;  // Ma trận 10x9
    
    public int Rows => 10;
    public int Cols => 9;
    
    public XiangqiBoard()
    {
        _board = new IPiece[10, 9];
        InitializeStandardPosition();  // Xếp quân chuẩn xiangqi
    }
    
    public IPiece GetPieceAt(Position pos) => _board[pos.Row, pos.Col];
    // ... các phương thức khác
}
```

**Lợi ích**:
- Giảm code duplication
- Dễ bảo trì (thay đổi chỉ một lần)
- Dễ thêm loại bàn cờ mới (chỉ implement IBoard)

---

### 2.2.3. Tính Đa Hình (Polymorphism)

**Vấn đề**: Mỗi loại quân cờ có cách di chuyển khác nhau. Nếu không dùng polymorphism, phải dùng nhiều `if-else` hoặc `switch-case`.

**Cách giải quyết**: Dùng **virtual methods** trong `IPiece` interface:

```csharp
// ✅ Cách mới - dùng polymorphism
public interface IPiece
{
    Color Color { get; }
    PieceType Type { get; }
    
    List<Position> GetValidMoves(IBoard board, Position currentPosition);
}

// Cài đặt cho Pawn
public class Pawn : IPiece
{
    public Color Color { get; }
    public PieceType Type => PieceType.Pawn;
    
    public List<Position> GetValidMoves(IBoard board, Position currentPosition)
    {
        var moves = new List<Position>();
        int direction = Color == Color.White ? -1 : 1;
        
        var nextPos = new Position(currentPosition.Row + direction, currentPosition.Col);
        if (board.IsValidPos(nextPos) && board.IsEmpty(nextPos))
            moves.Add(nextPos);
        
        return moves;
    }
}

// Cách sử dụng - KHÔNG cần if-else
public List<Position> GetValidMoves(IPiece piece, IBoard board, Position pos)
{
    return piece.GetValidMoves(board, pos);
}
```

**Lợi ích**:
- Khử hàng loạt `if-else`/`switch-case` → code sạch hơn
- Dễ thêm loại quân mới
- Thay đổi cách di chuyển của 1 quân không ảnh hưởng quân khác

---

## 2.3. Cấu trúc lưu trữ dữ liệu

### 2.3.1. Định dạng file: pgn

Ứng dụng lưu biên bản ván cờ dưới dạng PGN tại thư mục `GameRecords/`.

### 2.3.2. Cấu trúc pgn cho một ván cờ

```json
{
  "metadata": {
    "gameType": "Chess",
    "date": "2026-05-23T10:30:00Z",
    "whitePlayer": "Player 1",
    "blackPlayer": "Player 2",
    "clockSettings": {
      "timePerSide": 300,
      "increment": 0,
      "mode": "Blitz"
    },
    "result": "White Win by Checkmate",
    "moveCount": 42
  },
  "moves": [
    {
      "moveNumber": 1,
      "color": "White",
      "from": { "row": 6, "col": 4 },
      "to": { "row": 4, "col": 4 },
      "san": "e4",
      "capturedPiece": null
    }
  ]
}
```

---

*Tiếp theo: [Chương 3 - Áp dụng Design Pattern & Clean Code](ReportContentReq.Section3.md)*
