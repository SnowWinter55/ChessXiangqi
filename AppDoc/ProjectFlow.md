# ChoiCo Project Flow

## Table of Contents
1. [Core Objects & Architecture](#core-objects--architecture)
2. [Flow 1: Playing Chess](#flow-1-playing-chess)
3. [Flow 2: Playing Xiangqi](#flow-2-playing-xiangqi)
4. [Flow 3: Replaying and Branching Saved Games](#flow-3-replaying-and-branching-saved-games)

---

## Core Objects & Architecture

### Fundamental Core Objects

#### **IBoard Interface** (`Core/Interfaces/IBoard.cs`)
The abstract representation of a game board for both Chess and Xiangqi.

**Key Properties:**
- `Rows` (int): Board height (Chess: 8, Xiangqi: 10)
- `Cols` (int): Board width (Chess: 8, Xiangqi: 9)
- `GameType` (enum): Chess or Xiangqi
- `LastMove` (Move): Most recent move made

**Key Methods:**
- `GetPieceAt(Position)` → `IPiece`: Retrieve piece at location
- `SetPieceAt(Position, IPiece)`: Place/remove piece
- `MakeMove(Move)`: Execute move on board without validation
- `Clone()` → `IBoard`: Deep copy for undo/replay
- `GetAllPieces()` → Collection of (Position, IPiece): Get all pieces on board

**Concrete Implementations:**
- `ChessBoard` (8x8): Standard chess with standard piece initialization
- `XiangqiBoard` (10x9): Chinese chess with palace (3x3 center area) and river

---

#### **IPiece Interface** (`Core/Interfaces/IPiece.cs`)
Represents a chess/xiangqi piece with movement rules.

**Key Properties:**
- `Type` (enum): PieceType (Pawn, King, Queen, etc.)
- `Color` (enum): Color.White or Color.Black
- `IsPromoted` (bool): Only for pawns

**Key Methods:**
- `GetValidMoves(IBoard, Position)` → List<Position>: Legal moves this piece can make from a position

**Concrete Implementations (Chess):**
- `King`, `Queen`, `Rook`, `Bishop`, `Knight`, `Pawn`

**Concrete Implementations (Xiangqi):**
- `General`, `Advisor`, `Elephant`, `Horse`, `Chariot`, `Cannon`, `Soldier`

---

#### **Move Class** (`Core/Models/Common/Move.cs`)
Represents a single move in the game.

**Key Properties:**
- `From` (Position): Source square
- `To` (Position): Destination square
- `CapturedPiece` (IPiece): Piece removed, if any
- `IsEnPassant` (bool): Special chess rule
- `IsCastling` (bool): Special chess rule
- `PromotionPiece` (PieceType?): Pawn promotion target
- `San` (string): Standard Algebraic Notation representation

---

#### **IMoveValidator Interface** (`Core/Interfaces/IMoveValidator.cs`)
Validates moves and game state according to chess/xiangqi rules.

**Key Methods:**
- `IsValidMove(IBoard, Move, Color)` → bool: Check if move is legal
- `IsCheck(IBoard, Color)` → bool: King/General under attack
- `IsCheckmate(IBoard, Color)` → bool: King/General mated (no legal moves & in check)
- `IsStalemate(IBoard, Color)` → bool: No legal moves but not in check
- `GetAllValidMoves(IBoard, Color)` → List<Move>: All legal moves for a color
- `MoveLeavesKingInCheck(IBoard, Move, Color)` → bool: Would move expose own king?

**Concrete Implementations:**
- `ChessMoveValidator`: Implements chess rules with special moves (en passant, castling)
- `XiangqiMoveValidator`: Implements xiangqi rules (palace restrictions, elephant blocking)

---

### Core Management Objects

#### **GameClock** (`Modules/Clock/GameClock.cs`)
Manages game time for both players.

**Key Properties:**
- `Settings` (ClockSettings): Initial time and increment
- `TimeWhiteSeconds` / `TimeBlackSeconds` (int): Remaining time
- Events: `OnTimeOut`, `OnTimeUpdated`, `OnTurnStarted`

**Key Methods:**
- `StartTurn(Color)`: Begin time for a color
- `StopTurn()`: End turn, deduct time, add increment
- `GetRemainingTime(Color)` → int: Current remaining seconds

**Supported Modes:**
- Standard: Fixed increment per move
- Blitz/Bullet: Rapid time controls

---

#### **HistoryManager** (`Modules/Movement/HistoryManager.cs`)
Manages undo/redo stack and move history using Command Pattern.

**Key Properties:**
- `CanUndo` (bool): Whether undo is available
- `CanRedo` (bool): Whether redo is available

**Key Methods:**
- `ExecuteCommand(ICommand)`: Execute and record move
- `Undo()` → bool: Revert last move
- `Redo(out Move, out string)` → bool: Restore undone move
- `Clear()`: Reset history
- `ReplayFromMoveList(List<Move>, IBoard)` → bool: Rebuild board from move list

**Design Pattern:** Uses Command Pattern (`ICommand` interface) with `MoveCommand` implementing board modifications.

---

#### **BranchTracker** (`Modules/Notation/BranchTracker.cs`)
Maintains a tree structure of moves supporting game variations/branching.

**Key Data Structure:**
- `Node`: Tree node containing Move, Parent, Children (variations)
- `_mainLine`: Current line of play
- Root node at initialization

**Key Methods:**
- `AddMove(Move)`: Add move to current line, creating/traversing nodes
- `GoBack()` → bool: Revert to parent node (undo)
- `GetCurrentLine()` → List<Move>: All moves to current position
- `GetAvailableBranches()` → List<Move>: Alternative moves at current position
- `GoToMoveIndex(int)`: Navigate to specific move number
- `GoToStart()`: Jump to beginning
- `GoToEnd()`: Jump to final position

**Purpose:** Enables analysis variations without losing main game line.

---

#### **GameSaver** (`Modules/Notation/GameSaver.cs`)
High-level API for saving/loading/replaying games.

**Key Methods:**
- `SaveGame(string filePath, MatchRecord)`: Serialize game to file
- `LoadGame(string filePath)` → MatchRecord: Deserialize game from file
- `ReplayGame(MatchRecord, IBoard, IMoveValidator, HistoryManager, BranchTracker)` → bool: Load and validate moves on board

**Supported Formats:**
- JSON serialization via Newtonsoft.Json (NotationExporter/Importer)

---

#### **MatchRecord** (`Modules/Notation/MatchRecord.cs`)
Complete record of a finished game.

**Key Properties:**
- `GameType` (string): "Chess" or "Xiangqi"
- `Moves` (List<Move>): All moves in main line
- `FinalState` (GameState): End position and result
- `Date`, `Event`, `WhitePlayer`, `BlackPlayer`, `TimeControl`: Metadata

**Key Methods:**
- `GetMoveAtIndex(int)` → Move: Access move by position
- `GetTotalMoves()` → int: Count of moves
- `ValidateAllMoves(IBoard, IMoveValidator, Color)` → bool: Verify all moves are legal

---

#### **GameState** (`Core/Models/Common/GameState.cs`)
Snapshot of game state at any point.

**Key Properties:**
- `Fen` (string): Position notation
- `CurrentTurn` (Color): Whose turn it is
- `WhiteTimeSeconds`, `BlackTimeSeconds` (int): Remaining time
- `IsGameOver` (bool): Game finished?
- `GameOverReason` (string): Why game ended
- `MoveNumber` (int): Total moves so far

---

### UI & Rendering Objects

#### **AppController** (`UI/AppController.cs`)
Main application controller orchestrating all game components.

**Key Properties:**
- `_board` (IBoard): Current game board
- `_validator` (IMoveValidator): Move validation
- `_clock` (GameClock): Game timing
- `_currentTurn` (Color): Active player
- `_historyManager` (HistoryManager): Move undo/redo
- `_branchTracker` (BranchTracker): Move tree
- `_renderer` (BoardRenderer): Display
- `_isReplayMode` (bool): In replay mode?

**Key Methods:**
- `Run()`: Main game loop for new game
- `ReplayGameFromFile(string)`: Load and enter replay mode
- `PromptToSaveGame()`: Ask player to save game
- `PerformUndo()` / `PerformRedo()`: History navigation
- `ShowBranches()`: Display variation moves

**Responsibilities:**
1. Integrate board, validator, clock, and UI components
2. Process user input and execute moves
3. Manage game-over conditions
4. Handle undo/redo and branching
5. Persist games to disk

---

#### **BoardRenderer** (`UI/ConsoleUI/BoardRenderer.cs`)
Renders game state to console.

**Key Methods:**
- `Render(IBoard, Color, int, int, List<string>)`: Display board, turn, time, move history
- `RenderChessBoard(IBoard)`: 8x8 board with piece symbols
- `RenderXiangqiBoard(IBoard)`: 10x9 board with palace and river
- `RenderMoveHistory(List<string>)`: Show all moves to current position

**Display Features:**
- Color-coded board squares
- UTF-8 piece symbols (♟♞♗♖♕♔)
- Real-time clock display
- Move list in Standard Algebraic Notation

---

#### **InputHandler** (`UI/ConsoleUI/InputHandler.cs`)
Reads and parses player input.

**Key Methods:**
- `GetCommandOrMove()` → string: Read move or special command
- `TryParseMove(string, IBoard, Color, out Move)` → bool: Parse input into Move
- `DisplayInputHint(GameType)`: Show input format help

**Supported Commands:**
- Move in SAN (e.g., "e4", "Nf3") or coordinate form (e.g., "e2e4")
- "undo" / Ctrl+Z: Revert last move
- "redo" / Ctrl+Y: Restore undone move
- "moves": Show move history
- "branch": Display variation options
- "quit": End game
- Backspace on empty input: Return to menu

---

#### **GameReplayUI** (`UI/ConsoleUI/GameReplayUI.cs`)
Controls replay mode display and navigation.

**Key Methods:**
- `DisplayGameInfo(MatchRecord)`: Show metadata
- `DisplayReplayControls()`: Show replay command help
- `GetReplayCommand()` → ReplayCommand: Read player input
- `DisplayMoveCounter(int, int)`: Show current move / total
- `DisplayMoveList(List<Move>, int)`: Highlight current move

**Replay Commands:**
- Arrow keys: Previous/Next move
- Home/End: Jump to start/end
- G: Go to specific move number
- E: Export current position
- Q: Quit replay

---

### Support Objects

#### **SanFormatter** (`Modules/Notation/SanFormatter.cs`)
Converts Move objects to/from Standard Algebraic Notation (SAN).

**Notation Examples:**
- `e4`: Pawn to e4
- `Nf3`: Knight to f3
- `Bxe5`: Bishop captures on e5
- `O-O`: Kingside castling
- `e8=Q`: Pawn promotes to queen

---

#### **ClockSettings** (`Modules/Clock/ClockSettings.cs`)
Configuration for game timer.

**Key Properties:**
- `InitialTimeSeconds` (int): Starting time per player
- `IncrementSeconds` (int): Added per move
- `Mode` (ClockMode): Standard, Blitz, Bullet, etc.

---

#### **Position** (`Core/Models/Common/Position.cs`)
Represents a square on the board.

**Key Properties:**
- `Row` (int): Rank (0-7 for chess, 0-9 for xiangqi)
- `Col` (int): File (0-7 for chess, 0-8 for xiangqi)

**Key Methods:**
- `Equals(Position)` → bool: Same square?

---

---

# FLOW 1: Playing Chess

## Top-Level Chess Game Flow

```
Program.Main()
    ↓
    └─→ While(true) {
        ├─→ ShowGameSelectionMenu() → Select Chess
        │   └─→ Program.ShowGameSelectionMenu() [displays menu, returns GameSelection.Chess]
        │
        ├─→ Create Board & Validator
        │   ├─→ new ChessBoard()
        │   │   └─→ ChessBoard.InitializeStandardPosition()
        │   │       └─→ SetPieceAt() × 32 [place all pieces]
        │   └─→ new ChessMoveValidator()
        │
        ├─→ ShowGameModeMenu() → Select "Play New"
        │   └─→ Program.ShowGameModeMenu() [displays menu, returns GameMode.PlayNew]
        │
        ├─→ ClockSelectionUI.SelectClockSettings() → Choose time control
        │   └─→ Creates ClockSettings object
        │
        ├─→ new AppController(board, validator, clockSettings)
        │   └─→ AppController constructor:
        │       ├─→ _board = board
        │       ├─→ _validator = validator
        │       ├─→ _clock = new GameClock(clockSettings)
        │       ├─→ _renderer = new BoardRenderer()
        │       ├─→ _inputHandler = new InputHandler()
        │       ├─→ _historyManager = new HistoryManager(board)
        │       ├─→ _branchTracker = new BranchTracker()
        │       └─→ Register clock event handlers
        │
        └─→ app.Run() [Main game loop]
    }
```

---

## Sub-Flow 1a: Game Selection Menu

```
Program.ShowGameSelectionMenu()
    ↓
    Display options:
    ├─→ "1. Chess"
    ├─→ "2. Xiangqi"
    └─→ "0. Exit"
    ↓
    Read input
    ↓
    Switch input:
    ├─→ Case 1: return GameSelection.Chess
    ├─→ Case 2: return GameSelection.Xiangqi
    └─→ Case 0: return null (exit)
    ↓
    Returns to Program.Main() to create appropriate board
```

---

## Sub-Flow 1b: Clock Selection Menu

```
ClockSelectionUI.SelectClockSettings()
    ↓
    Display clock options:
    ├─→ "1. Blitz (5 min)"
    ├─→ "2. Rapid (10 min)"
    ├─→ "3. Classical (30 min)"
    └─→ "0. Cancel"
    ↓
    Read input
    ↓
    Switch input:
    ├─→ Case 1: return new ClockSettings(300, 0, BlitzMode)
    ├─→ Case 2: return new ClockSettings(600, 3, RapidMode)
    ├─→ Case 3: return new ClockSettings(1800, 0, ClassicalMode)
    └─→ Case 0: return null (back to mode menu)
    ↓
    Returns ClockSettings to Program.Main()
```

---

## Sub-Flow 1c: Game Mode Menu

```
Program.ShowGameModeMenu()
    ↓
    Display options:
    ├─→ "1. Play New Game"
    ├─→ "2. Load Saved Game"
    └─→ "0. Back"
    ↓
    Read input
    ↓
    Switch input:
    ├─→ Case 1: return GameMode.PlayNew → Proceed to clock selection
    ├─→ Case 2: return GameMode.LoadGame → Proceed to load game flow (Flow 3)
    └─→ Case 0: return null (back to game selection)
    ↓
    Returns to Program.Main()
```

---

## Sub-Flow 1d: Chess Game Initialization

```
new AppController(ChessBoard, ChessMoveValidator, ClockSettings)
    ↓
    Constructor initializes:
    ├─→ _currentTurn = Color.White
    ├─→ _gameOver = false
    ├─→ Create empty stacks: _undoStack, _redoStack
    ├─→ Create empty node tree: _branchTracker._root
    ├─→ _clock.OnTimeOut += OnTimeOut handler
    ├─→ _clock.OnTurnStarted += OnTurnStarted handler
    └─→ _clock.OnTimeUpdated += OnTimeUpdated handler
    ↓
    Returns AppController instance to Program.Main()
```

---

## Sub-Flow 1e: Main Chess Game Loop

```
AppController.Run()
    ↓
    Console.Clear()
    Display game start message
    _clock.StartTurn(Color.White)
    │
    While(!_gameOver) {
    │
    ├─→ _renderer.Render(_board, _currentTurn, whiteTime, blackTime, _moveHistorySAN)
    │   ├─→ BoardRenderer.Render():
    │   │   ├─→ Console.Clear()
    │   │   ├─→ Display turn and time: "Lượt: Trắng   🕒 Trắng: MM:SS  Đen: MM:SS"
    │   │   ├─→ RenderChessBoard(_board)
    │   │   │   ├─→ Draw 8×8 grid
    │   │   │   ├─→ Loop rows 0-7, cols 0-7:
    │   │   │   │   └─→ GetPieceAt(Position) → Render piece symbol or empty
    │   │   │   └─→ Color alternating squares (light/dark)
    │   │   └─→ RenderMoveHistory(_moveHistorySAN)
    │   │       └─→ Display all moves: "1. e4 c5 2. Nf3 ..."
    │   └─→ Display controls hint
    │
    ├─→ _inputHandler.GetCommandOrMove()
    │   ├─→ Console.Write("Nước đi: ")
    │   ├─→ While(true) {
    │   │   ├─→ Read key (intercept=true, no echo)
    │   │   ├─→ If Key == Enter: return input string
    │   │   ├─→ If Key == Backspace:
    │   │   │   ├─→ If empty: return "menu"
    │   │   │   └─→ Else: delete char from _currentInput
    │   │   ├─→ If Ctrl+Z: return "undo"
    │   │   ├─→ If Ctrl+Y: return "redo"
    │   │   └─→ Else: append char to _currentInput
    │   │   }
    │   └─→ Return input string
    │
    ├─→ Parse input:
    │   ├─→ If input == "quit": break loop
    │   ├─→ If input == "menu": break loop (return to main menu)
    │   ├─→ If input == "undo": PerformUndo(); continue
    │   ├─→ If input == "redo": PerformRedo(); continue
    │   ├─→ If input == "moves": ShowMoveHistory(); continue
    │   ├─→ If input == "branch": ShowBranches(); continue
    │   └─→ Else: Parse as move
    │
    ├─→ _inputHandler.TryParseMove(input, _board, _currentTurn, out move)
    │   ├─→ Try parse "e2e4" or "e4" or "Nf3" format
    │   ├─→ MoveParser.ParseMove(input, _board, _currentTurn) → Move object
    │   │   ├─→ For SAN format (Nf3):
    │   │   │   ├─→ Extract piece type letter (N) or empty for pawn
    │   │   │   ├─→ Extract destination file/rank (f3)
    │   │   │   ├─→ GetAllLegalMovesForPiece() of type
    │   │   │   ├─→ Filter by destination square
    │   │   │   └─→ If multiple pieces can move there, use disambiguation
    │   │   └─→ For coordinate format (e2e4):
    │   │       └─→ Parse source (e2) → Position(6, 4)
    │   │           Parse dest (e4) → Position(4, 4)
    │   └─→ Return parsed Move object or null if parse fails
    │
    ├─→ Check if promotion needed:
    │   ├─→ NeedPromotion(move):
    │   │   ├─→ GetPieceAt(move.From)
    │   │   ├─→ If Pawn AND (White reaching rank 0 OR Black reaching rank 7)
    │   │   └─→ Return true
    │   ├─→ If true:
    │   │   ├─→ Prompt: "Phong cấp thành (Q/R/B/N): "
    │   │   ├─→ Read input: Q/R/B/N
    │   │   └─→ Set move.PromotionPiece to chosen type
    │   └─→ Else: continue
    │
    ├─→ _validator.IsValidMove(_board, move, _currentTurn)
    │   ├─→ ChessMoveValidator.IsValidMove():
    │   │   ├─→ Check move.From and move.To within bounds
    │   │   │   └─→ board.IsValidPos(move.From/To)
    │   │   ├─→ Check piece exists at move.From
    │   │   │   └─→ board.GetPieceAt(move.From)
    │   │   ├─→ Check piece belongs to currentTurn
    │   │   │   └─→ piece.Color == currentTurn
    │   │   ├─→ Get all basic moves for piece
    │   │   │   └─→ piece.GetValidMoves(_board, move.From)
    │   │   ├─→ Check move.To in basic moves
    │   │   ├─→ Handle special moves:
    │   │   │   ├─→ If Pawn && IsEnPassantMove(): set move.IsEnPassant = true
    │   │   │   ├─→ If King && IsCastlingMove(): set move.IsCastling = true
    │   │   │   └─→ If Pawn && IsPromotionMove(): verify move.PromotionPiece set
    │   │   ├─→ Test move on clone board:
    │   │   │   ├─→ Clone board
    │   │   │   ├─→ Execute move on clone
    │   │   │   ├─→ Check if own king now in check
    │   │   │   └─→ MoveLeavesKingInCheck() → bool
    │   │   └─→ Return true if all checks pass
    │   └─→ If valid: proceed to execute
    │
    ├─→ If VALID MOVE:
    │   │
    │   ├─→ AnnotateMove(move):
    │   │   ├─→ If IsEnPassant: Set move.CapturedPiece
    │   │   └─→ Else: Set move.CapturedPiece = GetPieceAt(move.To)
    │   │
    │   ├─→ _moveHistorySAN.Add(ConvertMoveToSan(move))
    │   │   └─→ SanFormatter.FormatSan(move, _board) → "e4", "Nf3", "Bxe5", etc.
    │   │
    │   ├─→ Create command and execute:
    │   │   ├─→ var command = new MoveCommand(_board, move)
    │   │   ├─→ _historyManager.ExecuteCommand(command)
    │   │   │   ├─→ command.Execute()
    │   │   │   │   └─→ _board.MakeMove(move)
    │   │   │   │       ├─→ movingPiece = GetPieceAt(move.From)
    │   │   │   │       ├─→ Handle special moves:
    │   │   │   │       │   ├─→ If IsEnPassant: Remove pawn at (move.From.Row, move.To.Col)
    │   │   │   │       │   ├─→ If IsCastling: Also move rook appropriately
    │   │   │   │       │   └─→ If promotion: Replace pawn with promoted piece
    │   │   │   │       ├─→ SetPieceAt(move.From, null)
    │   │   │   │       ├─→ SetPieceAt(move.To, movingPiece)
    │   │   │   │       ├─→ Record this as LastMove
    │   │   │   │       └─→ Update _pieceMoved dictionary (for castling)
    │   │   │   ├─→ Push to _undoStack
    │   │   │   └─→ Clear _redoStack
    │   │
    │   ├─→ _branchTracker.AddMove(move)
    │   │   ├─→ Check if move already exists as child of _currentNode
    │   │   ├─→ If exists: _currentNode = existing node
    │   │   ├─→ If not: Create new Node, add to _currentNode.Children
    │   │   ├─→ Move _currentNode = newNode
    │   │   ├─→ Add move to _mainLine
    │   │   └─→ Clear _redoStack
    │   │
    │   ├─→ _clock.StopTurn()
    │   │   ├─→ Calculate elapsed time
    │   │   ├─→ Deduct from _currentTurn's remaining time
    │   │   ├─→ Add increment based on ClockMode
    │   │   └─→ Timer stops
    │   │
    │   ├─→ _currentTurn = _currentTurn.Opposite() [White ↔ Black]
    │   │
    │   ├─→ _clock.StartTurn(_currentTurn)
    │   │   ├─→ Set _currentTurn
    │   │   ├─→ Record _turnStartTime = DateTime.UtcNow
    │   │   ├─→ Timer.Start() [1-second interval]
    │   │   └─→ Trigger OnTurnStarted event
    │   │
    │   ├─→ Check if _validator.IsCheckmate(_board, _currentTurn)
    │   │   ├─→ Check if king in check
    │   │   │   └─→ Any enemy piece attacking king position
    │   │   ├─→ Check if no legal moves available
    │   │   │   └─→ GetAllValidMoves(_board, _currentTurn).Count == 0
    │   │   ├─→ If both true: Checkmate detected
    │   │   │   ├─→ _renderer.Render(...) [show final board]
    │   │   │   ├─→ Display: "Chiếu hết! [Trắng/Đen] thắng."
    │   │   │   ├─→ _gameOver = true
    │   │   │   └─→ Break loop
    │   │   └─→ Else: Continue game
    │   │
    │   ├─→ Else check if _validator.IsStalemate(_board, _currentTurn)
    │   │   ├─→ Check if king NOT in check
    │   │   ├─→ Check if no legal moves available
    │   │   ├─→ If both true: Stalemate
    │   │   │   ├─→ _renderer.Render(...) [show final board]
    │   │   │   ├─→ Display: "Hòa cờ."
    │   │   │   ├─→ _gameOver = true
    │   │   │   └─→ Break loop
    │   │   └─→ Else: Continue game
    │   │
    │   └─→ Loop back to render board for next move
    │
    └─→ If INVALID MOVE:
        ├─→ Display: "Lỗi: Không thể hiểu nước đi."
        ├─→ _inputHandler.DisplayInputHint(_board.GameType)
        ├─→ Thread.Sleep(4000)
        └─→ Loop back to request move again
    }
```

---

## Sub-Flow 1f: Undo Move in Chess Game

```
PerformUndo():
    ├─→ If !_historyManager.CanUndo:
    │   ├─→ Display: "Không thể undo thêm nữa."
    │   ├─→ Thread.Sleep(800)
    │   └─→ Return
    │
    ├─→ _historyManager.Undo()
    │   ├─→ Pop command from _undoStack
    │   ├─→ command.Undo()
    │   │   └─→ Reverse MakeMove: restore captured piece, move piece back, etc.
    │   ├─→ Push command to _redoStack
    │   └─→ Return true
    │
    ├─→ _branchTracker.GoBack()
    │   ├─→ Check if _currentNode.Parent exists
    │   ├─→ If yes:
    │   │   ├─→ Remove last move from _mainLine
    │   │   ├─→ _currentNode = _currentNode.Parent
    │   │   └─→ Return true
    │   └─→ Else: Return false
    │
    ├─→ If _moveHistorySAN.Count > 0:
    │   └─→ _moveHistorySAN.RemoveAt(Count - 1)
    │
    ├─→ _currentTurn = _currentTurn.Opposite()
    │
    ├─→ Display: "Đã undo nước đi cuối."
    ├─→ Thread.Sleep(800)
    │
    └─→ Loop back to render board (move undone, waiting for next input)
```

---

## Sub-Flow 1g: Redo Move in Chess Game

```
PerformRedo():
    ├─→ If !_historyManager.CanRedo:
    │   ├─→ Display: "Không thể redo thêm nữa."
    │   ├─→ Thread.Sleep(800)
    │   └─→ Return
    │
    ├─→ _historyManager.Redo(out Move redoneMove, out string san)
    │   ├─→ Check if _redoStack.Count > 0
    │   ├─→ If yes:
    │   │   ├─→ Pop command from _redoStack
    │   │   ├─→ command.Execute() [re-apply move]
    │   │   ├─→ Push to _undoStack
    │   │   ├─→ If command is MoveCommand:
    │   │   │   ├─→ redoneMove = moveCmd.Move
    │   │   │   └─→ san = moveCmd.San
    │   │   └─→ Return true
    │   └─→ Else: Return false
    │
    ├─→ If successful:
    │   ├─→ If san is empty: san = ConvertMoveToSan(redoneMove)
    │   ├─→ _moveHistorySAN.Add(san)
    │   ├─→ _branchTracker.AddMove(redoneMove)
    │   ├─→ _currentTurn = _currentTurn.Opposite()
    │   ├─→ Display: "Đã redo nước đi."
    │   ├─→ Thread.Sleep(800)
    │   └─→ Loop back to render board
    │
    └─→ Else:
        ├─→ Display: "Lỗi khi redo."
        ├─→ Thread.Sleep(800)
        └─→ Return
```

---

## Sub-Flow 1h: End Game & Save Chess Game

```
After Game Loop Exits (_gameOver == true or user quits):
    │
    ├─→ Display: "Cảm ơn bạn đã chơi!"
    │
    ├─→ Get main line from BranchTracker:
    │   ├─→ var mainLine = _branchTracker.GetCurrentLine()
    │   ├─→ If mainLine has moves:
    │   │   ├─→ Display: "Biên bản ván đấu (SAN):"
    │   │   ├─→ For each move i in mainLine:
    │   │   │   ├─→ If i is even (white move): Write move number
    │   │   │   └─→ Display SAN notation
    │   │   └─→ Example: "1. e4 c5 2. Nf3 d6"
    │   └─→ Else: "Chưa có nước nào."
    │
    └─→ PromptToSaveGame():
        ├─→ Display: "Bạn có muốn lưu biên bản ván đấu? (y/n): "
        ├─→ Read response
        ├─→ If response == "y" or "yes":
        │   │
        │   ├─→ Display: "Nhập tên tệp (không có phần mở rộng): "
        │   ├─→ Read filename
        │   │
        │   ├─→ CreateMatchRecordFromGame():
        │   │   └─→ Create MatchRecord object:
        │   │       ├─→ GameType = "Chess"
        │   │       ├─→ Moves = _branchTracker.GetCurrentLine().ToList()
        │   │       ├─→ WhitePlayer = "Player 1"
        │   │       ├─→ BlackPlayer = "Player 2"
        │   │       ├─→ Event = "Chess Game"
        │   │       ├─→ Date = DateTime.Now
        │   │       ├─→ TimeControl = "300+0" (example)
        │   │       └─→ FinalState = new GameState {...}
        │   │
        │   ├─→ Generate timestamp: "20260523_143022"
        │   ├─→ finalFilename = "{filename}_20260523_143022.txt"
        │   │
        │   ├─→ _gameSaver.SaveGame(filePath, record):
        │   │   ├─→ _exporter.ExportToFile(filePath, record)
        │   │   │   ├─→ Serialize MatchRecord to JSON
        │   │   │   ├─→ Write to file (GameRecords/filename_timestamp.txt)
        │   │   │   └─→ File contains all moves and metadata
        │   │   └─→ Return success
        │   │
        │   ├─→ Display: "✓ Game saved successfully to: filename_timestamp.txt"
        │   └─→ Thread.Sleep(2000)
        │
        └─→ Else (if response != "y"):
            ├─→ Display: "Không lưu."
            ├─→ Thread.Sleep(1000)
            └─→ Return to main menu (Program.Main while loop continues)
```

---

# FLOW 2: Playing Xiangqi

The Xiangqi flow is nearly identical to Chess flow, with these key differences:

## Key Differences from Chess

1. **Board Size**: XiangqiBoard (10 rows × 9 cols) vs ChessBoard (8×8)
2. **Piece Types**: Different pieces with different movement rules
   - General (King equivalent, restricted to 3×3 palace)
   - Advisor (Queen-like, palace-restricted)
   - Elephant (restricted, cannot cross river)
   - Horse (knight-like, but blocks adjacent)
   - Chariot (rook equivalent)
   - Cannon (special capture rules)
   - Soldier (pawn variant)
3. **Board Zones**: Palace (center 3×3) and River (middle row)
4. **Starting Color**: Color.Red vs Color.White in chess
5. **Movement Validation**: XiangqiMoveValidator implements xiangqi rules

---

## Sub-Flow 2a: Xiangqi Board Initialization

```
new XiangqiBoard()
    └─→ Constructor:
        ├─→ _board = new IPiece[10, 9]
        ├─→ InitializeStandardPosition()
        │   ├─→ Black pieces (top):
        │   │   ├─→ Row 0: Chariot(0), Horse(1), Elephant(2), Advisor(3), General(4), Advisor(5), Elephant(6), Horse(7), Chariot(8)
        │   │   ├─→ Row 2: Cannon(1), Cannon(7)
        │   │   └─→ Row 3: Soldiers at cols 0,2,4,6,8
        │   │
        │   └─→ White pieces (bottom):
        │       ├─→ Row 9: Chariot(0), Horse(1), Elephant(2), Advisor(3), General(4), Advisor(5), Elephant(6), Horse(7), Chariot(8)
        │       ├─→ Row 7: Cannon(1), Cannon(7)
        │       └─→ Row 6: Soldiers at cols 0,2,4,6,8
        │
        └─→ Returns XiangqiBoard with all pieces initialized
```

---

## Sub-Flow 2b: Xiangqi Move Validation

```
XiangqiMoveValidator.IsValidMove(IBoard board, Move move, Color currentTurn)
    ├─→ Basic validation (same as chess):
    │   ├─→ Check bounds
    │   ├─→ Check piece exists and belongs to currentTurn
    │   └─→ Check move in piece's basic moves
    │
    ├─→ Xiangqi-specific rules:
    │   ├─→ If piece is General:
    │   │   └─→ Validate move stays within palace (rows 7-9, cols 3-5 for white)
    │   │
    │   ├─→ If piece is Advisor:
    │   │   └─→ Validate move within palace and diagonal only
    │   │
    │   ├─→ If piece is Elephant:
    │   │   ├─→ Validate diagonal move 2 squares
    │   │   ├─→ Check blocking piece at midpoint
    │   │   └─→ Validate doesn't cross river (stay on own side)
    │   │
    │   ├─→ If piece is Horse:
    │   │   ├─→ Validate move like knight
    │   │   └─→ Check blocking piece adjacent to starting square
    │   │
    │   └─→ If piece is Cannon:
    │       └─→ Validate special capture rule (must jump piece to capture)
    │
    └─→ Check if own general remains un-checked after move
```

---

## Sub-Flow 2c: Xiangqi Game Flow

```
AppController.Run() for Xiangqi
    │
    ├─→ Display: "=== XIANGQI GAME (Console) ==="
    ├─→ Display: "Nhập nước đi định dạng tọa độ (cột 1-9, hàng 1-10)"
    │
    └─→ [IDENTICAL to Chess game loop]
        └─→ Main differences in move validation only
            └─→ XiangqiMoveValidator checks instead of ChessMoveValidator
```

The remainder of the Xiangqi flow (undo/redo, move history, saving, etc.) is identical to the Chess flow.

---

# FLOW 3: Replaying and Branching Saved Games

## Top-Level Replay Flow

```
Program.Main()
    └─→ While(true) {
        ├─→ ShowGameSelectionMenu() → Select game type (Chess/Xiangqi)
        │
        ├─→ Create appropriate board & validator
        │   └─→ Same as Flow 1/2
        │
        ├─→ ShowGameModeMenu() → Select "Load Game"
        │
        ├─→ PromptForGameFile()
        │   └─→ Display available game files in GameRecords/ directory
        │   └─→ User selects file or enters filename
        │
        └─→ [If file valid] {
            ├─→ new AppController(board, validator, defaultClockSettings)
            │
            └─→ app.ReplayGameFromFile(filePath) [Start Replay Mode]
        }
    }
```

---

## Sub-Flow 3a: Game File Selection & Loading

```
Program.PromptForGameFile()
    │
    ├─→ GameRecordsManager.DisplayAvailableGameRecords()
    │   ├─→ GameRecordsManager.GetGameRecordsPath()
    │   │   └─→ Return "c:\...ChoiCo\GameRecords"
    │   │       └─→ Create directory if not exists
    │   │
    │   ├─→ Directory.GetFiles(GameRecordsPath)
    │   │   └─→ Return all .txt files: ["game1.txt", "game2.txt", ...]
    │   │
    │   └─→ Display list:
    │       "Available game records:"
    │       "1. game1_20260523_143022.txt"
    │       "2. game2_20260520_100110.txt"
    │       ...
    │
    └─→ User enters filename or filepath
        └─→ Return selected file path
```

---

## Sub-Flow 3b: Load Game from File

```
AppController.ReplayGameFromFile(string filePath)
    │
    ├─→ _gameSaver.LoadGame(filePath)
    │   ├─→ _importer.ImportFromFile(filePath)
    │   │   ├─→ Read file content
    │   │   ├─→ JsonConvert.DeserializeObject<MatchRecord>(json)
    │   │   │   └─→ Parse JSON back to MatchRecord object
    │   │   │       ├─→ GameType: "Chess" or "Xiangqi"
    │   │   │       ├─→ Moves: List<Move>
    │   │   │       ├─→ Metadata: Date, Players, Event, TimeControl
    │   │   │       └─→ FinalState: Game end reason and final clock times
    │   │   │
    │   │   └─→ Return MatchRecord object
    │   │
    │   └─→ Return MatchRecord (null if load failed)
    │
    ├─→ If record == null:
    │   ├─→ GameReplayUI.DisplayError("Failed to load game file")
    │   └─→ Return (back to menu)
    │
    ├─→ Validate all moves:
    │   ├─→ _gameSaver.ReplayGame(record, _board, _validator, _historyManager, _branchTracker)
    │   │   ├─→ DetermineStartingColor(record.GameType)
    │   │   │   └─→ Return Color.White for Chess, Color.Red for Xiangqi
    │   │   │
    │   │   ├─→ record.ValidateAllMoves(_board, _validator, startingColor)
    │   │   │   ├─→ For each Move in record.Moves:
    │   │   │   │   ├─→ _validator.IsValidMove(_board, move, currentColor)
    │   │   │   │   ├─→ If false: return false (invalid game)
    │   │   │   │   ├─→ _board.MakeMove(move) [execute on board]
    │   │   │   │   └─→ currentColor = currentColor.Opposite()
    │   │   │   └─→ Return true (all moves valid)
    │   │   │
    │   │   ├─→ If validation passed:
    │   │   │   ├─→ _historyManager.Clear()
    │   │   │   ├─→ _branchTracker.Reset()
    │   │   │   │
    │   │   │   ├─→ Replay each move:
    │   │   │   │   ├─→ For each Move in record.Moves:
    │   │   │   │   │   ├─→ Create MoveCommand
    │   │   │   │   │   ├─→ _historyManager.ExecuteCommand(command)
    │   │   │   │   │   ├─→ _branchTracker.AddMove(move)
    │   │   │   │   │   └─→ currentColor = currentColor.Opposite()
    │   │   │   │   │
    │   │   │   └─→ Return true (replay successful)
    │   │   │
    │   │   └─→ Else: Return false (validation or replay failed)
    │   │
    │   ├─→ If success == false:
    │   │   ├─→ GameReplayUI.DisplayError("Failed to replay game - invalid moves detected")
    │   │   └─→ Return
    │   │
    │   └─→ If success == true:
    │       └─→ Continue to EnterReplayMode(record)
    │
    └─→ [Proceed to Replay Mode]
```

---

## Sub-Flow 3c: Enter Replay Mode & Display Game Info

```
AppController.EnterReplayMode(MatchRecord record)
    │
    ├─→ _isReplayMode = true
    ├─→ _replayingRecord = record
    ├─→ _replayMoveIndex = -1 [Start at beginning]
    │
    ├─→ GameReplayUI.DisplayGameInfo(record)
    │   ├─→ Console.Clear()
    │   ├─→ Display header: "╔════════════════════════════════════════════════════════╗"
    │   ├─→ Display "║              REPLAY GAME INFORMATION                   ║"
    │   ├─→ Display game details:
    │   │   ├─→ Event: "Chess Game"
    │   │   ├─→ Date: "2026.05.23"
    │   │   ├─→ White: "Player 1"
    │   │   ├─→ Black: "Player 2"
    │   │   ├─→ Result: "1-0" (or "0-1" or "1/2-1/2")
    │   │   ├─→ Time Control: "5+0"
    │   │   └─→ Total Moves: "42"
    │   │
    │   └─→ Display footer: "╚════════════════════════════════════════════════════════╝"
    │
    ├─→ GameReplayUI.DisplayReplayControls()
    │   ├─→ Display controls help:
    │   │   ├─→ "↑ / ← : Previous move"
    │   │   ├─→ "↓ / → : Next move"
    │   │   ├─→ "Home   : Go to start (move 0)"
    │   │   ├─→ "End    : Go to end (last move)"
    │   │   ├─→ "Q      : Quit replay"
    │   │   ├─→ "G      : Go to specific move number"
    │   │   └─→ "E      : Export current position"
    │   │
    │   └─→ Display footer
    │
    ├─→ System.Console.ReadKey() [Wait for user acknowledgement]
    │
    └─→ RunReplayMode()
```

---

## Sub-Flow 3d: Replay Mode Main Loop & Navigation

```
AppController.RunReplayMode()
    │
    While(_isReplayMode) {
    │
    ├─→ Console.Clear()
    │
    ├─→ RenderReplayBoard()
    │   ├─→ ResetBoard() [if needed - rebuild from move list]
    │   ├─→ _renderer.Render(_board, Color.White, -1, -1, [])
    │   │   └─→ Display current board position
    │   │
    │   ├─→ GameReplayUI.DisplayMoveCounter(_replayMoveIndex, totalMoves)
    │   │   └─→ Display: "Move: 15/42" (current/total)
    │   │
    │   ├─→ If _replayMoveIndex >= 0:
    │   │   ├─→ Get move = _replayingRecord.GetMoveAtIndex(_replayMoveIndex)
    │   │   ├─→ sanMove = move.San ?? "{From}-{To}"
    │   │   └─→ Console.WriteLine("Current move: " + sanMove)
    │   │
    │   └─→ GameReplayUI.DisplayMoveList(_replayingRecord.Moves, _replayMoveIndex)
    │       └─→ Show all moves with current move highlighted
    │
    ├─→ GameReplayUI.GetReplayCommand()
    │   └─→ Console.ReadKey(true) [Wait for command]
    │       ├─→ UpArrow / LeftArrow → ReplayCommand.Previous
    │       ├─→ DownArrow / RightArrow → ReplayCommand.Next
    │       ├─→ Home → ReplayCommand.ToStart
    │       ├─→ End → ReplayCommand.ToEnd
    │       ├─→ Q → ReplayCommand.Quit
    │       ├─→ G → ReplayCommand.GoToMove
    │       ├─→ E → ReplayCommand.Export
    │       └─→ Else: ReplayCommand.Unknown (continue)
    │
    ├─→ Switch(command) {
    │
    ├─→ Case ReplayCommand.Next:
    │   └─→ NextReplayMove():
    │       ├─→ If _replayMoveIndex < totalMoves - 1:
    │       │   ├─→ _replayMoveIndex++
    │       │   ├─→ _branchTracker.GoToMoveIndex(_replayMoveIndex)
    │       │   ├─→ RebuildBoardToMoveIndex(_replayMoveIndex)
    │       │   │   ├─→ ResetBoard() [clear board]
    │       │   │   ├─→ For i = 0 to _replayMoveIndex:
    │       │   │   │   └─→ _board.MakeMove(_replayingRecord.Moves[i])
    │       │   │   └─→ Board now at this position
    │       │   │
    │       │   └─→ Loop back to render next move
    │       └─→ Else: [Already at end, do nothing]
    │
    ├─→ Case ReplayCommand.Previous:
    │   └─→ PreviousReplayMove():
    │       ├─→ If _replayMoveIndex > -1:
    │       │   ├─→ _replayMoveIndex--
    │       │   ├─→ _branchTracker.GoToMoveIndex(_replayMoveIndex)
    │       │   ├─→ RebuildBoardToMoveIndex(_replayMoveIndex)
    │       │   └─→ Loop back to render previous move
    │       └─→ Else: [Already at start, do nothing]
    │
    ├─→ Case ReplayCommand.ToStart:
    │   └─→ GoToStartReplay():
    │       ├─→ _replayMoveIndex = -1
    │       ├─→ _branchTracker.GoToStart()
    │       ├─→ ResetBoard()
    │       └─→ Loop back
    │
    ├─→ Case ReplayCommand.ToEnd:
    │   └─→ GoToEndReplay():
    │       ├─→ _replayMoveIndex = totalMoves - 1
    │       ├─→ _branchTracker.GoToEnd()
    │       ├─→ RebuildBoardToMoveIndex(_replayMoveIndex)
    │       └─→ Loop back
    │
    ├─→ Case ReplayCommand.GoToMove:
    │   └─→ moveIdx = GameReplayUI.GetMoveIndex(totalMoves)
    │       └─→ If moveIdx >= 0:
    │           ├─→ GoToMoveIndexReplay(moveIdx)
    │           │   ├─→ _replayMoveIndex = moveIdx
    │           │   ├─→ _branchTracker.GoToMoveIndex(moveIdx)
    │           │   ├─→ RebuildBoardToMoveIndex(moveIdx)
    │           │   └─→ Return
    │           └─→ Loop back
    │
    ├─→ Case ReplayCommand.Export:
    │   └─→ ExportCurrentReplayPosition():
    │       └─→ [Export FEN or board position to file]
    │
    └─→ Case ReplayCommand.Quit:
        └─→ ExitReplayMode():
            ├─→ _isReplayMode = false
            └─→ Break loop
    }
    
    } [End while]
    
    └─→ Return to Program.Main() (back to game selection menu)
```

---

## Sub-Flow 3e: Branching in Replay Mode

While in replay mode, a user can create branches (variations) if analysis feature is added:

```
BranchTracker._currentNode.Children contains alternative moves
    │
    └─→ User can switch between:
        ├─→ Main line: Original moves from loaded game
        ├─→ Variation 1: Alternative move at some point
        ├─→ Variation 2: Different alternative
        └─→ Etc.
    
    When user makes alternative move:
        ├─→ Check if move already exists in Children
        ├─→ If yes: Switch to that node's line
        ├─→ If no: Create new branch and continue
```

---

## Sub-Flow 3f: Game Files & Save Format

```
File Structure:
├─→ GameRecords/
│   ├─→ game1_20260523_143022.txt
    │   └─→ Contains JSON serialized MatchRecord:
    │       {
    │         "GameType": "Chess",
    │         "Moves": [
    │           {"From": {"Row": 6, "Col": 4}, "To": {"Row": 4, "Col": 4}, ...},
    │           {"From": {"Row": 1, "Col": 4}, "To": {"Row": 3, "Col": 4}, ...},
    │           ...
    │         ],
    │         "WhitePlayer": "Player 1",
    │         "BlackPlayer": "Player 2",
    │         "Event": "Chess Game",
    │         "Date": "2026-05-23T14:30:22",
    │         "TimeControl": "300+0",
    │         "Result": "1-0",
    │         "FinalState": {
    │           "IsGameOver": true,
    │           "GameOverReason": "Checkmate",
    │           "CurrentTurn": "Black",
    │           "WhiteTimeSeconds": 245,
    │           "BlackTimeSeconds": 0
    │         }
    │       }
    │
    │   └─→ Properties:
    │       └─→ Serialized by NotationExporter
    │       └─→ Deserialized by NotationImporter
    │       └─→ All moves validate against appropriate validator
    │
    └─→ File managed by:
        ├─→ GameRecordsManager: Path handling, file listing
        ├─→ GameSaver: High-level save/load/replay API
        ├─→ NotationExporter: Serialize to file
        └─→ NotationImporter: Deserialize from file
```

---

## Core Object Interactions During Replay

```
During replay loading and navigation:

AppController
    ├─→ Holds: board, validator, historyManager, branchTracker, gameSaver
    │
    ├─→ ReplayGameFromFile():
    │   ├─→ gameSaver.LoadGame()
    │   │   └─→ importer.ImportFromFile() → MatchRecord
    │   │
    │   ├─→ gameSaver.ReplayGame()
    │   │   ├─→ Validate all moves via validator
    │   │   ├─→ Execute moves via historyManager
    │   │   ├─→ Build tree via branchTracker
    │   │   └─→ Board reflects all moves
    │   │
    │   └─→ EnterReplayMode()
    │       └─→ Allow navigation via branchTracker.GoToMoveIndex()
    │
    └─→ During navigation:
        ├─→ _replayMoveIndex tracks current position
        ├─→ RebuildBoardToMoveIndex() reconstructs position
        ├─→ branchTracker.GoToMoveIndex() updates tree position
        └─→ renderer.Render() displays current board
```

---

## End-to-End Replay Example

```
1. User selects "Load Game" from mode menu
2. Program prompts for filename
3. User enters "mygame"
4. System finds GameRecords/mygame_20260523_143022.txt
5. GameSaver loads file → MatchRecord object with 42 moves
6. All 42 moves validated against ChessMoveValidator
7. Board reconstructed with all 42 moves played
8. ReplayMode enters:
   - Display board at move 0 (starting position)
   - User presses → (next)
   - Board updates to position after move 1
   - User presses → repeatedly, watches game unfold
   - User presses Home, jumps back to move 0
   - User presses End, jumps to final position after move 42
   - User presses G, enters "15"
   - Board jumps to position after move 15
   - User presses Q, exits replay mode
9. Return to main menu
```

---

# Summary

**ChoiCo** is an object-oriented chess/xiangqi game with:

- **Separation of Concerns**: Board logic, move validation, UI rendering, and game management separated into distinct components
- **Strategy Pattern**: IMoveValidator implementations (Chess vs Xiangqi)
- **Command Pattern**: Move execution/undo via MoveCommand
- **Tree Structure**: BranchTracker maintains game tree for variations
- **Observer Pattern**: GameClock events notify AppController of time changes
- **Serialization**: NotationExporter/Importer for game persistence

The three main flows (play chess, play xiangqi, replay games) share common infrastructure (Board, Validator, Clock, UI) but differ in validator implementation and initial setup. Branching support allows exploring alternative moves during replay without losing the main game line.
