# 📋 TodoList: Import/Export Biên Bản Ván Cờ

## 🎯 Mục Tiêu Chính
Implement hệ thống đọc và phát lại (playback) file biên bản ván cờ, cho phép:
- ✅ Đọc file biên bản ván cờ (định dạng PGN hoặc custom)
- ✅ Tạo lại ván cờ từ nước đi đầu tiên đến cuối cùng
- ✅ Điều hướng ván đã lưu bằng undo/redo qua BranchTracker
- ✅ Hỗ trợ xiangqi và chess

---

## 📁 Cấu Trúc Files Cần Sửa (Sử dụng Assets Có Sẵn)

```
Modules/
├── Notation/
│   ├── MatchRecord.cs            [✅ Có sẵn] Data model cho biên bản
│   ├── NotationImporter.cs       [Sửa] Parse PGN/XIANGQI file → MatchRecord
│   ├── NotationExporter.cs       [Sửa] Export MatchRecord → PGN/XIANGQI file
│   ├── GameSaver.cs              [Sửa] High-level API: Save/Load/Replay
│   ├── BranchTracker.cs          [Sửa] Enhance navigation cho replay
│   └── SanFormatter.cs           [✅ Có sẵn] Format moves thành SAN
│
└── Movement/
    ├── HistoryManager.cs         [Sửa] Thêm replay capability
    └── MoveParser.cs             [✅ Có sẵn] Parse move string
```

---

## 📋 Phases Implementation

### Phase 1: MatchRecord & Format Definition
**Task 1.1: Kiểm tra & Enhance MatchRecord (tại Modules/Notation/MatchRecord.cs)**
- [x] Đã có: GameType, Moves, FinalState, Result, Date, Players
- [ ] Thêm: TimeControl (để biết thời gian ván), Event (tên giải)
- [ ] Thêm: method `ValidateAllMoves()` - check moves hợp lệ
- [ ] Thêm: method `GetMoveAtIndex(index)` - lấy move cụ thể

**Task 1.2: Định nghĩa format PGN/XIANGQI**
- Chess: Standard PGN format (8x8, SAN notation)
- Xiangqi: Custom XIANGQI format (tương tự PGN nhưng with Chinese piece names)
- Cả hai: Metadata tags + Move section

```csharp
// Enhanced MatchRecord properties
public class MatchRecord
{
    public string GameType { get; set; }  // "Chess" or "Xiangqi"
    public string Event { get; set; }     // NEW: Tên giải/trận đấu
    public string TimeControl { get; set; } // NEW: "5+3", "10+0"
    public List<Move> Moves { get; set; }
    public GameState FinalState { get; set; }
    public string Result { get; set; }    // "1-0", "0-1", "1/2-1/2"
    public DateTime Date { get; set; }    // Changed from string
    public string WhitePlayer { get; set; }
    public string BlackPlayer { get; set; }
    
    // NEW Methods
    public Move GetMoveAtIndex(int index) => Moves[index];
    public int GetTotalMoves() => Moves.Count;
}
```

---

### Phase 2: NotationImporter & NotationExporter
**Task 2.1: Expand NotationImporter.cs (Modules/Notation/NotationImporter.cs)**
- [ ] Implement `ImportFromFile(string filePath)` method
  - Auto-detect format: `.pgn` → PGN, `.xqg` → XIANGQI, `.json` → JSON
- [ ] Implement `ParsePgnFile(string content)` for Chess
  - Parse metadata tags: [Event "..."], [Date "..."], etc.
  - Parse moves section: "1. e4 e5 2. Nf3 Nc6 ..."
  - Return MatchRecord
- [ ] Implement `ParseXiangqiFile(string content)` for Xiangqi
  - Same metadata structure
  - Parse xiangqi moves: "1. C4=5 H8+2 ..."
- [ ] Implement `ParseJsonFile(string content)` 
  - Use JsonConvert.DeserializeObject<MatchRecord>
- [ ] Error handling: File not found, invalid format, corrupt data

```csharp
public class NotationImporter
{
    public MatchRecord ImportFromFile(string filePath)
    {
        string content = File.ReadAllText(filePath);
        string extension = Path.GetExtension(filePath).ToLower();
        
        return extension switch
        {
            ".pgn" => ParsePgnFile(content),
            ".xqg" => ParseXiangqiFile(content),
            ".json" => JsonConvert.DeserializeObject<MatchRecord>(content),
            _ => throw new InvalidOperationException("Unsupported format")
        };
    }
    
    private MatchRecord ParsePgnFile(string content) { /* ... */ }
    private MatchRecord ParseXiangqiFile(string content) { /* ... */ }
}
```

**Task 2.2: Expand NotationExporter.cs (Modules/Notation/NotationExporter.cs)**
- [ ] Implement `ExportToFile(string filePath, MatchRecord record)` method
  - Auto-detect output format từ file extension
- [ ] Implement `ToPgn(MatchRecord record)` for Chess
  - Format metadata tags
  - Format moves into PGN standard
  - Handle move numbers: "1. e4 e5 2. Nf3 ..."
- [ ] Implement `ToXiangqi(MatchRecord record)` for Xiangqi
  - Same metadata structure
  - Format xiangqi moves properly
- [ ] Implement `ToJson(MatchRecord record)`
  - Use JsonConvert.SerializeObject with Formatting.Indented

```csharp
public class NotationExporter
{
    public void ExportToFile(string filePath, MatchRecord record)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        string content = extension switch
        {
            ".pgn" => ToPgn(record),
            ".xqg" => ToXiangqi(record),
            ".json" => ToJson(record),
            _ => throw new InvalidOperationException("Unsupported format")
        };
        
        File.WriteAllText(filePath, content);
    }
    
    private string ToPgn(MatchRecord record) { /* ... */ }
    private string ToXiangqi(MatchRecord record) { /* ... */ }
    private string ToJson(MatchRecord record) { /* ... */ }
}
```

---

### Phase 3: BranchTracker Enhancement for Replay Navigation
**Task 3.1: Enhance BranchTracker.cs (Modules/Notation/BranchTracker.cs)**

Current structure: Tree node with Move, Parent, Children

New capabilities:
- [ ] Implement `RebuildFromMoveList(List<Move> moves)` method
  - Clear existing tree
  - Build linear path from root with all moves
  - Useful for replayed games (no variations initially)

- [ ] Implement `GoToMoveIndex(int index)` method
  - Jump directly to move at index N
  - Return to root and traverse N steps
  - Better performance than multiple GoBack()

- [ ] Implement `GetMoveAtIndex(int index)` method
  - Return the move at specific index in main line
  - Used for UI display

- [ ] Implement `GetMainLine()` improvement
  - Already exists, but ensure it returns full line from root
  
- [ ] Add indexing support
  - Track move index in current node

```csharp
public class BranchTracker
{
    // NEW
    public void RebuildFromMoveList(List<Move> moves)
    {
        _root = new Node { Move = null, Parent = null };
        _currentNode = _root;
        foreach (var move in moves)
            AddMove(move);
    }
    
    public void GoToMoveIndex(int index)
    {
        Reset();  // Go to root
        for (int i = 0; i < index && i < GetMainLine().Count; i++)
            GoForward();
    }
    
    public Move GetMoveAtIndex(int index)
    {
        var mainLine = GetMainLine();
        return index >= 0 && index < mainLine.Count ? mainLine[index] : null;
    }
    
    public int GetTotalMoves() => GetMainLine().Count;
}
```

---

### Phase 4: HistoryManager & GameSaver Enhancement
**Task 4.1: Enhance HistoryManager.cs (Modules/Movement/HistoryManager.cs)**
- [ ] Implement `ReplayFromMoveList(List<Move> moves, IBoard board)` method
  - Clear current history
  - Execute từng move từ list
  - Ensure undo/redo state correct after
  
- [ ] Implement `GetAllMoves()` method
  - Return full move history từ root
  
```csharp
public class HistoryManager
{
    public void ReplayFromMoveList(List<Move> moves, IBoard board)
    {
        _undoStack.Clear();
        _redoStack.Clear();
        
        foreach (var move in moves)
        {
            var command = new MoveCommand(board, move);
            ExecuteCommand(command);
        }
    }
    
    public List<Move> GetAllMoves() 
    {
        // Return tất cả moves từ undo stack
        return _executedCommands.Select(c => c.Move).ToList();
    }
}
```

**Task 4.2: Expand GameSaver.cs (Modules/Notation/GameSaver.cs)**
- [ ] Implement `SaveGame(string filePath, MatchRecord record)` method
  - Use NotationExporter to save
  
- [ ] Implement `LoadGame(string filePath)` method
  - Use NotationImporter to load
  - Return MatchRecord
  
- [ ] Implement `ReplayGame(MatchRecord record, IBoard board, IMoveValidator validator)` method
  - Create new board if needed
  - Use HistoryManager.ReplayFromMoveList()
  - Use BranchTracker.RebuildFromMoveList()
  - Validate all moves are legal
  - Return success/failure
  
```csharp
public class GameSaver
{
    private NotationExporter _exporter = new NotationExporter();
    private NotationImporter _importer = new NotationImporter();
    
    public void SaveGame(string filePath, MatchRecord record)
        => _exporter.ExportToFile(filePath, record);
    
    public MatchRecord LoadGame(string filePath)
        => _importer.ImportFromFile(filePath);
    
    public bool ReplayGame(
        MatchRecord record, 
        IBoard board, 
        IMoveValidator validator,
        HistoryManager historyMgr,
        BranchTracker branchTracker)
    {
        try
        {
            // Validate all moves
            foreach (var move in record.Moves)
                if (!validator.IsValidMove(board, move, GetCurrentTurn(board)))
                    return false;
            
            // Replay
            historyMgr.ReplayFromMoveList(record.Moves, board);
            branchTracker.RebuildFromMoveList(record.Moves);
            return true;
        }
        catch { return false; }
    }
}
```

---

### Phase 6: RecordExporter Enhancement
**Task 6.1: Cải thiện export functionality**
- [ ] Implement `ExportToFile(string filePath)` method
- [ ] Hỗ trợ export PGN cho Chess
- [ ] Hỗ trợ export XIANGQI cho Xiangqi
- [ ] Include metadata (Date, Players, TimeControl, Result)
- [ ] Include all moves

```csharp
public class RecordExporter
{
    public void ExportToFile(
        string filePath, 
        IBoard board, 
        BranchTracker branchTracker,
        string playerWhite, 
        string playerBlack,
        DateTime date);
}
```

---

### Phase 5: UI Integration for Replay
**Task 5.1: Thêm Load Game feature vào menu (Program.cs)**
- [ ] Thêm option "Load Game" trong main menu
- [ ] Cho user chọn file .pgn / .xqg / .json
- [ ] Call GameSaver.LoadGame() + Replay

**Task 5.2: Tạo GameReplayUI.cs (UI/ConsoleUI/GameReplayUI.cs)**
- [ ] Method `DisplayGameInfo(MatchRecord record)` - Show metadata
- [ ] Method `DisplayCurrentPosition(int moveIndex, int totalMoves)` - Show "Move 15/60"
- [ ] Method `DisplayReplayControls()` - Show help text
- [ ] Method `GetReplayCommand()` - Intercept arrow keys + special keys

```csharp
public class GameReplayUI
{
    public void DisplayGameInfo(MatchRecord record)
    {
        Console.WriteLine($"Event: {record.Event}");
        Console.WriteLine($"Date: {record.Date:yyyy.MM.dd}");
        Console.WriteLine($"White: {record.WhitePlayer} vs Black: {record.BlackPlayer}");
        Console.WriteLine($"Result: {record.Result}");
    }
    
    public void DisplayMoveCounter(int current, int total)
        => Console.WriteLine($"Move: {current + 1}/{total}");
    
    public ReplayCommand GetReplayCommand() { /* ... */ }
}

public enum ReplayCommand { Next, Previous, ToStart, ToEnd, Quit }
```

**Task 5.3: Sửa AppController.cs để support Replay Mode**
- [ ] Thêm `bool _isReplayMode` property
- [ ] Thêm `int _replayMoveIndex` property
- [ ] Nếu replay, render từ BranchTracker position thay vì current turn
- [ ] Dùng GameReplayUI thay vì InputHandler
- [ ] Arrow Up/Down: Navigate moves
- [ ] 'S': Skip to end, 'B': Back to start

### Phase 6: File Format Specifications

#### 6.1 Chess PGN Format (Standard)
```
[Event "Casual Game"]
[Date "2026.05.21"]
[White "Player 1"]
[Black "Player 2"]
[Result "1-0"]

1. e4 e5 2. Nf3 Nc6 3. Bc4 Bc5 4. d3 Nf6 5. O-O O-O
6. Bg5 a6 7. a4 d6 8. c3 h6 9. Bh4 g6 10. Nbd2 1-0
```

#### 6.2 Xiangqi XIANGQI Format (Custom)
```
[Event "Xiangqi Game"]
[Date "2026.05.21"]
[Red "Player 1"]
[Black "Player 2"]
[Result "1-0"]
[TimeControl "5+3"]

1. C4=5 H8+2 2. H2+1 H9-1 3. C5=1 E7+5
...
```

#### 6.3 Alternative: Coordinate Format
```
[GameType "Xiangqi"]
[Date "2026.05.21"]

1. 3,1-4,1 9,1-8,1
2. 2,1-3,1 9,2-8,2
```

---

## 🔧 Implementation Details

### BranchTracker Internal Structure (Current)
```
Node
├── Move (nước đi)
├── Parent (node cha)
└── Children (list nước đi có thể tiếp theo)
    ├── Child 1
    │   └── Children...
    └── Child 2
        └── Children...
```

### BranchTracker Enhancement (New)
- Thêm `Index` field để track vị trí move trong main line
- Thêm `GetPathToRoot()` - lấy danh sách moves từ current position tới root
- Implement direct navigation bằng index

---

## 📊 Method Flow

### 1. Load Game Flow (Using NotationImporter + GameSaver)
```
User chọn "Load Game"
    ↓
FileDialog (chọn .pgn / .xqg / .json)
    ↓
GameSaver.LoadGame(filePath)
    ↓
NotationImporter.ImportFromFile(filePath)
    ↓
MatchRecord object
    ↓
GameSaver.ReplayGame(record, board, validator)
    ↓
HistoryManager.ReplayFromMoveList(record.Moves)
BranchTracker.RebuildFromMoveList(record.Moves)
    ↓
Display in Replay Mode (GameReplayUI)
```

### 2. Replay Navigation Flow
```
User nhấn Arrow Up (Previous Move)
    ↓
BranchTracker.GoToMoveIndex(currentIndex - 1)
    ↓
AppController renders board at that position
    ↓
Display move counter (N/Total)
```

### 3. Save Game Flow (Using NotationExporter + GameSaver)
```
Game End or User nhấn "Save Game"
    ↓
GameSaver.SaveGame(filePath, matchRecord)
    ↓
NotationExporter.ExportToFile(filePath, matchRecord)
    ↓
Format theo PGN/XIANGQI/JSON (auto-detect từ extension)
    ↓
Write to file
```

---

## 🎯 Priority & Difficulty

| Phase | Task                          | Priority   | Difficulty |
|-------|-------------------------------|------------|------------|
| 1     | MatchRecord Enhancement       | 🔴 High   | 🟢 Easy    |
| 2     | NotationImporter              | 🔴 High   | 🟡 Medium  |
| 2     | NotationExporter              | 🔴 High   | 🟡 Medium  |
| 3     | BranchTracker Enhancement     | 🔴 High   | 🟡 Medium  |
| 4     | HistoryManager + GameSaver    | 🔴 High   | 🟡 Medium  |
| 5     | UI Integration (GameReplayUI) | 🟡 Medium | 🟡 Medium  |


---

## ✅ Acceptance Criteria

- [ ] Có thể load file PGN (chess) từ disk
- [ ] Có thể load file XIANGQI (xiangqi) từ disk
- [ ] Tái tạo đúng board state sau replay
- [ ] Arrow keys navigate moves
- [ ] BranchTracker maintains proper state during replay
- [ ] Export game to file (PGN/XIANGQI)
- [ ] Undo/Redo hoạt động đúng trong replay mode
- [ ] Hiển thị move counter (current/total)
- [ ] Error handling for invalid files

---

## 🔗 Related Files & Structures

**Sử dụng (Expand/Enhance):**
- ✅ `Modules/Notation/MatchRecord.cs` - Data model (có code, thêm properties)
- 🔧 `Modules/Notation/NotationImporter.cs` - Parse file (skeleton)
- 🔧 `Modules/Notation/NotationExporter.cs` - Export file (skeleton)
- 🔧 `Modules/Notation/GameSaver.cs` - High-level API (skeleton)
- 🔧 `Modules/Notation/BranchTracker.cs` - Game tree navigation (có code, enhance)
- 🔧 `Modules/Movement/HistoryManager.cs` - Command history (có code, add replay)
- ✅ `Modules/Movement/MoveParser.cs` - Already capable
- ✅ `Modules/Notation/SanFormatter.cs` - Already capable

**Tạo Mới (UI):**
- 🆕 `UI/ConsoleUI/GameReplayUI.cs` - Replay UI component
- 🆕 Update `Program.cs` - Add "Load Game" menu option
- 🆕 Update `UI/AppController.cs` - Replay mode support

---

## 📝 Notes

1. **Reuse Existing Assets**: Expand NotationImporter/Exporter/GameSaver thay vì tạo files mới
2. **MatchRecord**: Đã có data model, chỉ cần thêm vài properties và validation method
3. **Move Validation**: Luôn validate move khi replay từ file (file có thể corrupt)
4. **Format Flexibility**: Hỗ trợ PGN (Chess), XIANGQI (Custom), và JSON
5. **Performance**: Cache parsed games nếu cần
6. **Error Recovery**: Nếu move invalid, dừng replay và báo lỗi chi tiết
7. **Backward Compatibility**: Không break existing game flow, chỉ add new features

---

## 🚀 Next Steps
1. **Phase 1**: Enhance MatchRecord.cs (add Event, TimeControl, validation)
2. **Phase 2**: Implement NotationImporter.cs (PGN/XIANGQI parsing)
3. **Phase 2**: Implement NotationExporter.cs (export to PGN/XIANGQI)
4. **Phase 3**: Enhance BranchTracker.cs (replay navigation)
5. **Phase 4**: Enhance HistoryManager + GameSaver (replay capability)
6. **Phase 5**: Create GameReplayUI.cs + UI integration
7. **Testing**: Test with sample PGN files, error cases
8. **Integration**: Add Load Game to main menu, full flow testing
