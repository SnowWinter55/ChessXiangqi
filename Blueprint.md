# ChoiCo — Project Blueprint (Detailed)

This file lists the current project structure and shows all classes, interfaces, enums, modules and UI components present in the workspace (including work-in-progress files). It also highlights completed areas, known issues, and remaining TODOs.

Project root (relative paths)

ChoiCo/
│
├── `Program.cs`
│
├── Core/
│   ├── Interfaces/
│   │   ├── `IBoard.cs`
│   │   ├── `IPiece.cs`
│   │   ├── `IEngine.cs`
│   │   ├── `IMoveValidator.cs`
│   │   ├── `IChessBoard.cs`
│   │   └── `IXiangqiBoard.cs`
│   │
│   ├── Models/
│   │   ├── Common/
│   │   │   ├── `Position.cs`
│   │   │   ├── `Move.cs`
│   │   │   └── `GameState.cs`
│   │   │
│   │   ├── Chess/
│   │   │   ├── `ChessBoard.cs`
│   │   │   ├── `MoveValidator.cs`    — (WIP, rules, check detection)
│   │   │   └── Pieces/
│   │   │       ├── `Pawn.cs`
│   │   │       ├── `Rook.cs`
│   │   │       ├── `Knight.cs`
│   │   │       ├── `Bishop.cs`
│   │   │       ├── `Queen.cs`
│   │   │       └── `King.cs`
│   │   │
│   │   └── Xiangqi/
│   │       ├── `XiangqiBoard.cs`
│   │       └── Pieces/
│   │           ├── `Soldier.cs`
│   │           ├── `Cannon.cs`
│   │           ├── `Chariot.cs`
│   │           ├── `Horse.cs`
│   │           ├── `Elephant.cs`
│   │           ├── `Advisor.cs`
│   │           └── `General.cs`
│   │
│   ├── Enums/
│   │   ├── `Color.cs`
│   │   ├── `GameType.cs`
│   │   └── `PieceType.cs`
│   │
│   └── Extention/
│       └── `ColorExtention.cs`
│
├── Modules/
│   ├── Movement/
│   │   ├── `MoveParser.cs`
│   │   ├── `PremoveHandler.cs`
│   │   ├── `HistoryManager.cs`
│   │   ├── `ICommand.cs`
│   │   └── `MoveCommand.cs`
│   │
│   ├── Clock/
│   │   ├── `GameClock.cs`
│   │   └── `ClockSettings.cs`
│   │
│   ├── Notation/
│   │   ├── `NotationExporter.cs`
│   │   ├── `NotationImporter.cs`
│   │   ├── `BranchTracker.cs`
│   │   ├── `GameSaver.cs`
│   │   └── `MatchRecord.cs`
│   │
│   └── Engine/
│       └── `PlaceholderEngine.cs`   — (stub / WIP)
│
├── UI/
│   ├── `AppController.cs`            — integrates `BranchTracker` and `HistoryManager` for Chess
│   └── ConsoleUI/
│       ├── `BoardRenderer.cs`
│       ├── `InputHandler.cs`
│       └── `VisualEffects.cs`
│
└── `Blueprint.md` (this file)

Completed / Implemented
 - Project skeleton and core models (`Position`, `Move`, `GameState`)
 - Core interfaces (`IBoard`, `IPiece`, `IChessBoard`, `IXiangqiBoard`, `IEngine`, `IMoveValidator`)
 - Chess board and piece classes: `ChessBoard`, `Pawn`, `Rook`, `Knight`, `Bishop`, `Queen`, `King`
 - Xiangqi board and pieces: `XiangqiBoard`, `Soldier`, `Cannon`, `Chariot`, `Horse`, `Elephant`, `Advisor`, `General`
 - Movement helpers: `MoveParser`, `PremoveHandler`, `HistoryManager`, `MoveCommand` and `ICommand`
 - Notation modules and persistence: `NotationImporter`, `NotationExporter`, `BranchTracker`, `GameSaver`, `MatchRecord`
 - Console UI rendering and input: `BoardRenderer`, `InputHandler`, `VisualEffects`
 - AppController: now wires `BranchTracker` and `HistoryManager` into the Chess flow (branching and undo/redo)

Work-in-progress / Under development
 - `MoveValidator.cs` — needs completeness for special chess rules (castling, en passant), check/checkmate detection and consistent use with `IMoveValidator`.
 - `PlaceholderEngine.cs` — engine is a stub; needs search, evaluation, and time control integration.
 - `BranchTracker.cs` / `HistoryManager.cs` — basic implementations exist; edge cases, persistence and concurrency need tests.
 - Clock: `GameClock.cs` shows issues with counting behavior (see Known Issues).

Known Issues / Confusing behaviors
 - Clock Timer does not seem to count properly at the moment. Needs investigation in `GameClock.cs` and the wiring from `AppController`.
 - Some console color choices may render poorly on certain terminals (black on black). See `BoardRenderer.cs` for color logic.

Recommended Next Tasks
 - Stabilize `MoveValidator` and add unit tests to cover:
   - Legal move generation for each piece
   - Check/checkmate detection and draw conditions
   - Special moves: castling, en passant, promotion
 - Implement a basic minimax/negamax engine with depth-limited search in `PlaceholderEngine.cs` and wire into `IEngine`.
 - Add unit tests for `HistoryManager` and `BranchTracker` (branch creation, rollback, branch switching).
 - Fix clock issues and add integration tests covering time controls.
 - Expand notation importer/exporter tests and retry/resume scenarios.

Guidelines & Notes for Contributors
 - Add new public types in files named after the type (PascalCase).
 - Keep core model classes free of UI/presentation code.
 - Use dependency injection where possible (AppController should accept modules via constructor for easier testing).
 - Keep `IMoveValidator` implementations side-effect free; state mutations should be done by `IBoard` or commands.
 - When modifying branching/history flows, update tests for `HistoryManager` and `BranchTracker` together.
 - Target framework: .NET 10. Ensure new packages are compatible.

If you want, I can split the TODOs into GitHub issues or generate a checklist for the project board.