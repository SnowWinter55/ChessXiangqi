# CHƯƠNG 3: ÁP DỤNG DESIGN PATTERN & CLEAN CODE

## 3.1. Design Pattern 1: Command Pattern (Mẫu Lệnh)

### 3.1.1. Vấn đề gặp phải

**Tình huống**: Cần quản lý lịch sử nước đi (undo/redo) trong một ván cờ. Mỗi nước đi là một **action** có thể được:
- Thực hiện (execute)
- Hoàn tác (undo)
- Phục hồi (redo)

**Vấn đề nếu không dùng Command Pattern**:
```csharp
// ❌ Cách cũ - logic lộn xộn, khó mở rộng
public class GameController
{
    private List<Move> _moveHistory;
    private Stack<Move> _undoStack;
    private Stack<Move> _redoStack;
    
    public void ExecuteMove(Move move)
    {
        // Thực hiện nước
        _board.MakeMove(move);
        
        // Lưu history
        _moveHistory.Add(move);
        _undoStack.Push(move);
        _redoStack.Clear();
    }
    
    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        
        Move move = _undoStack.Pop();
        // Phải viết logic đảo ngược nước
        _board.RemoveMove(move);
        _redoStack.Push(move);
    }
    
    // Nếu có thêm action khác (lưu game, tải game...), phải thêm logic khác
    // => Vi phạm Single Responsibility Principle
}
```

**Vấn đề**:
1. Logic thực hiện và hoàn tác lộn xộn trong một class
2. Khó mở rộng (thêm action mới → sửa code hiện tại)
3. Vi phạm **Single Responsibility Principle** (SRP)

---

### 3.1.2. Cách giải quyết bằng Command Pattern

**Giải pháp**: Gói mỗi **action** vào một object (command) có hai phương thức:
- `Execute()`: Thực hiện action
- `Undo()`: Hoàn tác action

#### **Sơ đồ UML của Command Pattern**

```
┌────────────────────┐
│   ICommand         │  << Interface >>
├────────────────────┤
│ + Execute()        │
│ + Undo()           │
└────────┬───────────┘
         ▲
         │ Implements
         │
    ┌────┴─────┬──────────┬─────────────┐
    │           │          │             │
┌───┴──┐  ┌────┴───┐  ┌───┴────┐  ┌──┴─────┐
│Move  │  │SaveGame│  │LoadGame│  │Replay  │
│Cmd   │  │Cmd     │  │Cmd     │  │Cmd     │
└──────┘  └────────┘  └────────┘  └────────┘

┌──────────────────────┐
│ HistoryManager       │  Uses
├──────────────────────┤
│ - _undoStack: Stack  │
│ - _redoStack: Stack  │
├──────────────────────┤
│ + ExecuteCommand()   │
│ + Undo()             │
│ + Redo()             │
└──────────────────────┘
```

#### **Cài đặt cụ thể**

```csharp
// 1. Interface Command
public interface ICommand
{
    void Execute();
    void Undo();
    Move GetMove();  // Lấy move đã thực hiện
    string GetSan(); // Lấy ký hiệu SAN
}

// 2. Cài đặt cụ thể - MoveCommand
public class MoveCommand : ICommand
{
    private readonly IBoard _board;
    private readonly Move _move;
    private IPiece _capturedPiece;
    
    public MoveCommand(IBoard board, Move move)
    {
        _board = board;
        _move = move;
    }
    
    public void Execute()
    {
        // Lưu quân bị ăn (nếu có)
        _capturedPiece = _board.GetPieceAt(_move.To);
        
        // Thực hiện nước đi
        _board.MakeMove(_move);
    }
    
    public void Undo()
    {
        // Hoàn tác: quay về trạng thái trước khi Execute()
        _board.SetPieceAt(_move.From, _board.GetPieceAt(_move.To));
        _board.SetPieceAt(_move.To, _capturedPiece);
    }
    
    public Move GetMove() => _move;
    public string GetSan() => _move.San;
}

// 3. HistoryManager - sử dụng Command Pattern
public class HistoryManager
{
    private readonly IBoard _board;
    private readonly Stack<ICommand> _undoStack;
    private readonly Stack<ICommand> _redoStack;
    
    public HistoryManager(IBoard board)
    {
        _board = board;
        _undoStack = new Stack<ICommand>();
        _redoStack = new Stack<ICommand>();
    }
    
    // Thực hiện một command
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();  // Xóa redo khi có nước mới
    }
    
    // Hoàn tác một command
    public bool Undo()
    {
        if (_undoStack.Count == 0) return false;
        
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        return true;
    }
    
    // Phục hồi một command
    public bool Redo()
    {
        if (_redoStack.Count == 0) return false;
        
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        return true;
    }
}

// 4. Cách sử dụng trong AppController
public class AppController
{
    private HistoryManager _historyManager;
    
    public void MakeMove(Move move)
    {
        var command = new MoveCommand(_board, move);
        _historyManager.ExecuteCommand(command);
        // Xong! Không cần viết logic undo riêng
    }
    
    public void PerformUndo()
    {
        _historyManager.Undo();
    }
}
```

**Lợi ích**:
- ✅ Khử logic lộn xộn
- ✅ Dễ mở rộng (thêm SaveGameCommand, LoadGameCommand...)
- ✅ Tuân thủ **Single Responsibility Principle**
- ✅ Tuân thủ **Open/Closed Principle**

---

## 3.2. Design Pattern 2: Strategy Pattern (Mẫu Chiến Lược)

### 3.2.1. Vấn đề gặp phải

**Tình huống**: Cần xác thực nước đi cho **hai loại cờ khác nhau** (cờ vua và cờ tướng). Mỗi loại có:
- Quy tắc di chuyển quân khác nhau
- Điều kiện chiếu/chiếu hết khác nhau
- Nước đi đặc biệt riêng

**Vấn đề nếu không dùng Strategy Pattern**:
```csharp
// ❌ Cách cũ - if-else lộn xộn
public class MoveValidator
{
    public bool IsValidMove(IBoard board, Move move, Color turn, GameType gameType)
    {
        if (gameType == GameType.Chess)
        {
            // Logic cờ vua
            if (!CheckChessRules(move)) return false;
            if (!CheckEnPassant(move)) return false;
            if (!CheckCastling(move)) return false;
            if (!CheckPromotion(move)) return false;
        }
        else if (gameType == GameType.Xiangqi)
        {
            // Logic cờ tướng
            if (!CheckXiangqiRules(move)) return false;
            if (!CheckPalaceRestriction(move)) return false;
            if (!CheckRiverRestriction(move)) return false;
            if (!CheckCannonCapture(move)) return false;
        }
        // ...
    }
    // Nếu thêm loại cờ mới → phải sửa lại hàm này
    // => Vi phạm Open/Closed Principle
}
```

**Vấn đề**:
1. Quá nhiều if-else
2. Khó bảo trì (thay đổi logic cờ vua → sửa hàm chung)
3. Vi phạm **Open/Closed Principle** (đóng với sửa đổi, mở với mở rộng)

---

### 3.2.2. Cách giải quyết bằng Strategy Pattern

**Giải pháp**: Tách logic xác thực thành các **strategy** riêng biệt, mỗi strategy là một cách xác thực khác nhau.

#### **Sơ đồ UML của Strategy Pattern**

```
┌───────────────────────┐
│ IMoveValidator        │  << Interface >>
├───────────────────────┤
│ + IsValidMove()       │
│ + IsCheck()           │
│ + IsCheckmate()       │
│ + GetAllValidMoves()  │
└───────┬───────────────┘
        ▲
        │ Implements
        │
    ┌───┴──────────┬──────────────┐
    │              │              │
┌───┴──┐      ┌────┴────┐    ┌───┴─────┐
│Chess │      │Xiangqi  │    │[Future] │
│Move  │      │Move     │    │[New]    │
│Valid │      │Valid    │    │Type     │
│ator  │      │ator     │    │         │
└──────┘      └─────────┘    └─────────┘

┌──────────────────────┐
│ AppController        │  Uses
├──────────────────────┤
│ - _validator:        │
│   IMoveValidator     │
├──────────────────────┤
│ + HandleMove()       │
└──────────────────────┘
```

#### **Cài đặt cụ thể**

```csharp
// 1. Interface Strategy
public interface IMoveValidator
{
    bool IsValidMove(IBoard board, Move move, Color currentTurn);
    bool IsCheck(IBoard board, Color kingColor);
    bool IsCheckmate(IBoard board, Color kingColor);
    List<Move> GetAllValidMoves(IBoard board, Color color);
}

// 2. Cài đặt Strategy cho Cờ Vua
public class ChessMoveValidator : IMoveValidator
{
    public bool IsValidMove(IBoard board, Move move, Color currentTurn)
    {
        // Logic xác thực riêng cho cờ vua
        if (!BasicValidation(board, move, currentTurn))
            return false;
        
        // Kiểm tra ăn tây tạp
        if (IsEnPassantMove(board, move))
        {
            // Xử lý en passant
            HandleEnPassant(board, move);
            return true;
        }
        
        // Kiểm tra nhập thành
        if (IsCastlingMove(board, move))
            return ValidateCastling(board, move);
        
        // Kiểm tra phong cấp
        if (NeedsPromotion(move))
            return ValidatePromotion(move);
        
        // Kiểm tra xem vua có bị chiếu
        return !MoveLeavesKingInCheck(board, move, currentTurn);
    }
    
    public bool IsCheck(IBoard board, Color kingColor)
    {
        var kingPos = FindKing(board, kingColor);
        return IsPositionAttacked(board, kingPos, kingColor.Opposite());
    }
    
    public bool IsCheckmate(IBoard board, Color kingColor)
    {
        if (!IsCheck(board, kingColor))
            return false;
        
        return GetAllValidMoves(board, kingColor).Count == 0;
    }
    
    // Các phương thức hỗ trợ riêng cho Chess
    private bool IsEnPassantMove(IBoard board, Move move) { ... }
    private bool IsCastlingMove(IBoard board, Move move) { ... }
    private bool NeedsPromotion(Move move) { ... }
    // ...
}

// 3. Cài đặt Strategy cho Cờ Tướng
public class XiangqiMoveValidator : IMoveValidator
{
    public bool IsValidMove(IBoard board, Move move, Color currentTurn)
    {
        // Logic xác thực riêng cho cờ tướng
        if (!BasicValidation(board, move, currentTurn))
            return false;
        
        var piece = board.GetPieceAt(move.From);
        
        // Kiểm tra General giới hạn cung điện
        if (piece.Type == PieceType.General)
            if (!IsWithinPalace(board, move.To, piece.Color))
                return false;
        
        // Kiểm tra Elephant không qua sông
        if (piece.Type == PieceType.Elephant)
            if (IsCrossingSide(move))
                return false;
        
        // Kiểm tra Cannon ăn phải có trung điểm
        if (piece.Type == PieceType.Cannon)
            return ValidateCannonCapture(board, move);
        
        return !MoveLeavesKingInCheck(board, move, currentTurn);
    }
    
    public bool IsCheck(IBoard board, Color kingColor)
    {
        var generalPos = FindGeneral(board, kingColor);
        return IsPositionAttacked(board, generalPos, kingColor.Opposite());
    }
    
    // Các phương thức hỗ trợ riêng cho Xiangqi
    private bool IsWithinPalace(IBoard board, Position pos, Color color) { ... }
    private bool IsCrossingSide(Move move) { ... }
    private bool ValidateCannonCapture(IBoard board, Move move) { ... }
    // ...
}

// 4. Cách sử dụng trong AppController
public class AppController
{
    private IMoveValidator _validator;
    private IBoard _board;
    
    public AppController(IBoard board, IMoveValidator validator)
    {
        _board = board;
        _validator = validator;  // Có thể là Chess hoặc Xiangqi validator
    }
    
    public bool HandleMove(Move move, Color currentTurn)
    {
        // Không cần if-else để kiểm tra loại cờ!
        // Polymorphism: gọi phương thức của validator thực tế
        if (_validator.IsValidMove(_board, move, currentTurn))
        {
            _board.MakeMove(move);
            return true;
        }
        return false;
    }
}

// 5. Cách tạo AppController (trong Program.cs)
IBoard board;
IMoveValidator validator;

switch (gameType)
{
    case GameType.Chess:
        board = new ChessBoard();
        validator = new ChessMoveValidator();  // Chọn strategy Chess
        break;
    case GameType.Xiangqi:
        board = new XiangqiBoard();
        validator = new XiangqiMoveValidator();  // Chọn strategy Xiangqi
        break;
}

var controller = new AppController(board, validator);
// Strategy được chọn tại runtime, dễ mở rộng
```

**Lợi ích**:
- ✅ Khử if-else lộn xộn
- ✅ Dễ thêm loại cờ mới (chỉ tạo class mới implement IMoveValidator)
- ✅ Tuân thủ **Open/Closed Principle**
- ✅ Tuân thủ **Single Responsibility Principle**

---

## 3.3. Áp dụng chuẩn mực viết mã (Clean Code)

### 3.3.1. Quy tắc đặt tên (Naming Convention)

**Dự án tuân thủ C# Naming Convention**:

| Loại | Quy tắc | Ví dụ |
|------|--------|-------|
| **Class/Interface** | PascalCase | `ChessBoard`, `IPiece`, `GameClock` |
| **Method** | PascalCase | `GetValidMoves()`, `IsValidMove()`, `ExecuteCommand()` |
| **Property** | PascalCase | `Rows`, `Cols`, `WhiteTimeRemaining` |
| **Private Field** | _camelCase (underscore + lowercase) | `_board`, `_undoStack`, `_currentTurn` |
| **Local Variable** | camelCase | `move`, `validMoves`, `kingPosition` |
| **Constant** | UPPERCASE_SNAKE_CASE | `MAX_TIME_PER_GAME` |

**Ví dụ từ code**:
```csharp
public class GameClock  // PascalCase
{
    private int _whiteTimeSeconds;  // _camelCase
    
    public int WhiteTimeRemaining => _whiteTimeSeconds;  // Property: PascalCase
    
    public void StartTurn(Color color)  // Method: PascalCase
    {
        int elapsedTime = 0;  // Local var: camelCase
    }
}
```

---

### 3.3.2. Áp dụng các nguyên lý SOLID

#### **S - Single Responsibility Principle (SRP)**

**Định nghĩa**: Một lớp chỉ nên có **một lý do duy nhất** để thay đổi.

**Áp dụng trong dự án**:
```csharp
// ✅ Đúng - Mỗi class có một trách nhiệm
public class ChessBoard : IBoard
{
    // Chỉ quản lý: quân cờ trên bàn, di chuyển cơ bản
    public IPiece GetPieceAt(Position pos) { ... }
    public void SetPieceAt(Position pos, IPiece piece) { ... }
    public void MakeMove(Move move) { ... }
}

public class ChessMoveValidator : IMoveValidator
{
    // Chỉ quản lý: xác thực nước đi theo quy tắc cờ vua
    public bool IsValidMove(IBoard board, Move move, Color turn) { ... }
}

public class BoardRenderer
{
    // Chỉ quản lý: hiển thị bàn cờ trên console
    public void Render(IBoard board) { ... }
}
```

---

#### **O - Open/Closed Principle (OCP)**

**Định nghĩa**: Lớp nên **mở rộng (open for extension)** nhưng **đóng kín (closed for modification)**.

**Áp dụng trong dự án**:
```csharp
// ✅ Đúng - Mở rộng bằng cách thêm class mới, không sửa code cũ
public interface IMoveValidator { }
public class ChessMoveValidator : IMoveValidator { }
public class XiangqiMoveValidator : IMoveValidator { }
// Muốn thêm loại cờ mới? Chỉ cần: public class NewGameMoveValidator : IMoveValidator { }
// Không cần sửa code AppController
```

---

#### **L - Liskov Substitution Principle (LSP)**

**Định nghĩa**: Các lớp con có thể thay thế lớp cha mà không làm hỏng chương trình.

**Áp dụng trong dự án**:
```csharp
// ✅ Đúng - ChessBoard và XiangqiBoard đều có thể thay thế IBoard
public class AppController
{
    private IBoard _board;
    
    public AppController(IBoard board)
    {
        _board = board;  // Có thể là ChessBoard hoặc XiangqiBoard
    }
    
    public void Run()
    {
        _board.MakeMove(move);  // Hoạt động giống nhau
    }
}
```

---

#### **I - Interface Segregation Principle (ISP)**

**Định nghĩa**: Client không nên bắt buộc phụ thuộc vào các interface mà nó không sử dụng.

**Áp dụng trong dự án**:
```csharp
// ✅ Đúng - Tách interface nhỏ, dùng cần cái nào lấy cái nấy
public interface IBoard
{
    int Rows { get; }
    int Cols { get; }
    IPiece GetPieceAt(Position pos);
    void SetPieceAt(Position pos, IPiece piece);
}

public interface IChessBoard : IBoard
{
    bool CanCastle(Color color);
}

public interface IXiangqiBoard : IBoard
{
    bool IsWithinPalace(Position pos, Color color);
}
```

---

#### **D - Dependency Inversion Principle (DIP)**

**Định nghĩa**: Phụ thuộc vào **abstraction (interface)**, không phụ thuộc vào **concrete class**.

**Áp dụng trong dự án**:
```csharp
// ✅ Đúng - Phụ thuộc vào interface
public class AppController
{
    private readonly IBoard _board;  // Interface
    private readonly IMoveValidator _validator;  // Interface
    
    public AppController(IBoard board, IMoveValidator validator)
    {
        _board = board;
        _validator = validator;
    }
}

// ❌ Sai - Phụ thuộc vào concrete class
public class AppController
{
    private ChessBoard _board;  // Concrete class - khó mở rộng
}
```

---

### 3.3.3. Các quy tắc Clean Code khác

| Quy tắc | Mô Tả | Ví Dụ |
|--------|-------|-------|
| **Phương thức nhỏ** | Mỗi phương thức nên có ~10 dòng code | `IsValidMove()` không quá 15 dòng |
| **Tránh đặt tên không rõ** | Đặt tên phải dễ hiểu | ✅ `GetValidMoves()` ❌ `GetMoves()` |
| **Khử code trùng lặp (DRY)** | Don't Repeat Yourself | Interface `IBoard` giảm code dùp |
| **Comments khi cần** | Viết comment cho logic phức tạp | FEN parsing, palace checking |
| **Xử lý exception** | Try-catch cho trường hợp lỗi | Validate input, file I/O |
| **Formatting đẹp** | Indent 4 spaces, line breaks hợp lý | Theo .editorconfig |

---

## 3.4. Tóm tắt Design Patterns & Clean Code

```
┌─────────────────────────────────────────────────────┐
│  ChoiCo - Design Patterns & Clean Code             │
├─────────────────────────────────────────────────────┤
│                                                     │
│  🎯 Design Patterns:                                │
│  ├─ Command Pattern                                │
│  │  └─ Quản lý Undo/Redo via ICommand              │
│  │     HistoryManager, MoveCommand                 │
│  │                                                 │
│  └─ Strategy Pattern                               │
│     └─ Xác thực nước đi via IMoveValidator         │
│        ChessMoveValidator, XiangqiMoveValidator    │
│                                                     │
│  🏆 SOLID Principles:                               │
│  ├─ S: Mỗi class một trách nhiệm                   │
│  ├─ O: Mở rộng, không sửa code cũ                 │
│  ├─ L: Thay thế lớp con liền mạch                 │
│  ├─ I: Interface nhỏ, dùng cần cái nấy            │
│  └─ D: Phụ thuộc abstraction, không concrete      │
│                                                     │
│  📝 Clean Code:                                     │
│  ├─ Naming Convention: C# Standard                │
│  ├─ Phương thức nhỏ (<15 dòng)                    │
│  ├─ DRY: Khử code trùng                           │
│  ├─ Comments rõ ràng                              │
│  └─ Formatting đẹp                                │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

*Tiếp theo: [Chương 4 - Kết Quả Thực Hiện & Hướng Dẫn Sử Dụng](ReportContentReq.Section4.md)*
