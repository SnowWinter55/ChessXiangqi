# 04 - Luồng Chọn Game Và Chế Độ Chơi

## 🎮 Sơ Đồ Menu Chính

```
Program.Main()
│
└─→ Vòng lặp chính: While(true)
    │
    ├─→ 📍 BƯỚC 1: Hiển thị Menu Chọn Loại Game
    │   │
    │   ├─→ Console.Clear()
    │   ├─→ Hiển thị:
    │   │   ┌─────────────────────────────────┐
    │   │   │   ChoiCo - Chọn Loại Game       │
    │   │   ├─────────────────────────────────┤
    │   │   │ 1. Cờ Vua (Chess)               │
    │   │   │ 2. Cờ Tướng (Xiangqi)           │
    │   │   │ 0. Thoát                        │
    │   │   └─────────────────────────────────┘
    │   │
    │   ├─→ Nhập lựa chọn: 1/2/0
    │   │
    │   └─→ Switch(lựa chọn):
    │       ├─→ Case 0: Thoát ứng dụng
    │       ├─→ Case 1: gameType = Chess; Tiếp bước 2
    │       └─→ Case 2: gameType = Xiangqi; Tiếp bước 2
    │
    ├─→ 📍 BƯỚC 2: Tạo Board & Validator
    │   │
    │   ├─→ If gameType == Chess:
    │   │   ├─→ board = new ChessBoard()
    │   │   └─→ validator = new ChessMoveValidator()
    │   │
    │   └─→ If gameType == Xiangqi:
    │       ├─→ board = new XiangqiBoard()
    │       └─→ validator = new XiangqiMoveValidator()
    │
    ├─→ 📍 BƯỚC 3: Hiển thị Menu Chọn Chế Độ Chơi
    │   │
    │   ├─→ Console.Clear()
    │   ├─→ Hiển thị:
    │   │   ┌─────────────────────────────────┐
    │   │   │   Chọn Chế Độ Chơi              │
    │   │   ├─────────────────────────────────┤
    │   │   │ 1. Chơi Ván Mới                 │
    │   │   │ 2. Tải Ván Cũ (Replay)          │
    │   │   │ 0. Quay Lại                     │
    │   │   └─────────────────────────────────┘
    │   │
    │   ├─→ Nhập lựa chọn: 1/2/0
    │   │
    │   └─→ Switch(lựa chọn):
    │       ├─→ Case 0: Quay về bước 1
    │       ├─→ Case 1: Tiếp bước 4 (Chơi Mới)
    │       └─→ Case 2: Tiếp bước 5 (Tải Cũ)
    │
    ├─────────────────────────────────────────────────
    │
    │ 🔀 NHÁNH A: CHƠI VĂN MỚI
    │
    ├─→ 📍 BƯỚC 4: Chọn Cài Đặt Đồng Hồ
    │   │
    │   ├─→ Console.Clear()
    │   ├─→ Hiển thị:
    │   │   ┌─────────────────────────────────┐
    │   │   │   Cài Đặt Thời Gian Chơi        │
    │   │   ├─────────────────────────────────┤
    │   │   │ 1. Blitz (5 phút + 0s)          │
    │   │   │ 2. Rapid (10 phút + 3s)         │
    │   │   │ 3. Classical (30 phút + 0s)     │
    │   │   │ 0. Quay Lại                     │
    │   │   └─────────────────────────────────┘
    │   │
    │   ├─→ Nhập lựa chọn: 1/2/3/0
    │   │
    │   └─→ Switch(lựa chọn):
    │       ├─→ Case 1: ClockSettings(300, 0, Blitz)
    │       ├─→ Case 2: ClockSettings(600, 3, Rapid)
    │       ├─→ Case 3: ClockSettings(1800, 0, Classical)
    │       └─→ Case 0: Quay về bước 3
    │
    ├─→ 📍 BƯỚC 6A: Khởi Tạo & Chơi Game Mới
    │   │
    │   ├─→ Tạo AppController:
    │   │   ├─→ app = new AppController(board, validator, clockSettings)
    │   │   ├─→ Khởi tạo:
    │   │   │   ├── _currentTurn = Color.White/Red
    │   │   │   ├── _gameOver = false
    │   │   │   ├── _historyManager = new HistoryManager()
    │   │   │   ├── _branchTracker = new BranchTracker()
    │   │   │   ├── _clock = new GameClock(clockSettings)
    │   │   │   ├── _renderer = new BoardRenderer()
    │   │   │   ├── _inputHandler = new InputHandler()
    │   │   │   ├── _gameSaver = new GameSaver()
    │   │   │   └── Đăng ký event handlers
    │   │   │
    │   │   └─→ Console.WriteLine(\"Ván cờ mới bắt đầu!\")
    │   │
    │   ├─→ Chạy game:
    │   │   └─→ app.Run()  [Xem 05-LuồngChơiCờVua.md]\n    │   │       [Xem 06-LuồngChơiCờTướng.md]\n    │   │\n    │   └─→ Sau khi ván kết thúc:\n    │       └─→ Tiếp bước 9 (Hậu kỳ)\n    │\n    ├─────────────────────────────────────────────────\n    │\n    │ 🔀 NHÁNH B: TẢI & XEM LẠI VĂN CŨ\n    │\n    ├─→ 📍 BƯỚC 5: Chọn File Ván Cờ\n    │   │\n    │   ├─→ Console.Clear()\n    │   ├─→ GameRecordsManager.DisplayAvailableGameRecords()\n    │   │   ├─→ Lấy đường dẫn thư mục: GameRecords/\n    │   │   ├─→ Liệt kê tất cả .txt files\n    │   │   ├─→ Hiển thị:\n    │   │   │   ┌─────────────────────────────────┐\n    │   │   │   │  Các Ván Cờ Đã Lưu             │\n    │   │   │   ├─────────────────────────────────┤\n    │   │   │   │ 1. game1_20260523_143022.txt   │\n    │   │   │   │ 2. game2_20260520_100110.txt   │\n    │   │   │   │ 3. analysis_20260515_090000.txt│\n    │   │   │   │ 0. Quay Lại                    │\n    │   │   │   └─────────────────────────────────┘\n    │   │   │\n    │   │   └─→ Nhập số hoặc tên file\n    │   │\n    │   ├─→ Kiểm tra file tồn tại?\n    │   │   ├─→ YES: Tiếp bước 7\n    │   │   └─→ NO:  Hiển thị lỗi, quay về bước 5\n    │   │\n    │   └─→ Lấy đường dẫn đầy đủ: GameRecords/{filename}\n    │\n    ├─→ 📍 BƯỚC 7: Tải & Xác Thực Ván\n    │   │\n    │   ├─→ _gameSaver.LoadGame(filePath)\n    │   │   ├─→ Đọc file JSON\n    │   │   ├─→ Deserialize thành MatchRecord\n    │   │   └─→ Return MatchRecord hoặc null\n    │   │\n    │   ├─→ Nếu NULL:\n    │   │   ├─→ Console.WriteLine(\"Lỗi: Không tải được file\")\n    │   │   ├─→ Thread.Sleep(2000)\n    │   │   └─→ Quay về bước 5\n    │   │\n    │   ├─→ Validate MatchRecord:\n    │   │   ├─→ matchRecord.ValidateAllMoves(board, validator, startColor)\n    │   │   ├─→ Kiểm tra tất cả nước hợp lệ không\n    │   │   └─→ Nếu có nước không hợp lệ:\n    │   │       ├─→ Hiển thị lỗi\n    │   │       └─→ Quay về bước 5\n    │   │\n    │   └─→ OK: Tiếp bước 8\n    │\n    ├─→ 📍 BƯỚC 8: Khởi Tạo & Vào Replay Mode\n    │   │\n    │   ├─→ Tạo AppController (giống bước 6A)\n    │   │\n    │   ├─→ Đổ lại tất cả nước từ MatchRecord:\n    │   │   ├─→ _gameSaver.ReplayGame(matchRecord, board, validator,\n    │   │   │                         historyManager, branchTracker)\n    │   │   ├─→ Thực hiện lại mỗi nước:\n    │   │   │   ├── board.MakeMove(move)\n    │   │   │   ├── historyManager.ExecuteCommand()\n    │   │   │   └── branchTracker.AddMove()\n    │   │   │\n    │   │   ├─→ Bàn cờ giờ ở vị trí cuối cùng\n    │   │   └─→ Đặt _replayMoveIndex = -1 (về đầu)\n    │   │\n    │   ├─→ Vào Replay Mode:\n    │   │   └─→ app.EnterReplayMode(matchRecord)  [Xem 08-LuồngReplay.md]\n    │   │\n    │   └─→ Sau khi thoát Replay:\n    │       └─→ Tiếp bước 9 (Hậu kỳ)\n    │\n    ├─────────────────────────────────────────────────\n    │\n    ├─→ 📍 BƯỚC 9: Hậu Kỳ & Menu Kết Thúc\n    │   │\n    │   ├─→ Sau khi ván kết thúc hoặc thoát Replay:\n    │   │\n    │   ├─→ Hiển thị kết quả (nếu là game mới):\n    │   │   ├─→ Bàn cờ cuối cùng\n    │   │   ├─→ Thông báo kết quả\n    │   │   └─→ Danh sách nước đi\n    │   │\n    │   ├─→ PromptToSaveGame():  [Xem 09-LuồngHậuKỳ.md]\n    │   │   ├─→ \"Lưu ván cờ? (y/n): \"\n    │   │   └─→ Nếu YES: Tiếp bước 10\n    │   │\n    │   └─→ Xóa sạch resources:\n    │       ├─→ _clock?.Dispose()\n    │       ├─→ _gameSaver = null\n    │       └─→ Quay về bước 1\n    │\n    └─→ ✅ Chu kỳ lặp lại\n```\n\n---\n\n## 🎯 Chi Tiết Từng Bước\n\n### BƯỚC 1: Menu Chọn Loại Game\n\n```csharp\nProgram.ShowGameSelectionMenu() → GameSelection\n\nCode:\n├─→ Console.Clear()\n├─→ Console.WriteLine(\" ╔═════════════════════════════════╗\")\n├─→ Console.WriteLine(\" ║  ChoiCo - Chọn Loại Game        ║\")\n├─→ Console.WriteLine(\" ╠═════════════════════════════════╣\")\n├─→ Console.WriteLine(\" ║ 1. Cờ Vua (Chess)               ║\")\n├─→ Console.WriteLine(\" ║ 2. Cờ Tướng (Xiangqi)           ║\")\n├─→ Console.WriteLine(\" ║ 0. Thoát                         ║\")\n├─→ Console.WriteLine(\" ╚═════════════════════════════════╝\")\n├─→ Console.Write(\" Chọn: \")\n├─→ string input = Console.ReadLine()\n└─→ switch(input):\n    ├─→ \"1\" → return GameSelection.Chess\n    ├─→ \"2\" → return GameSelection.Xiangqi\n    ├─→ \"0\" → return null (exit)\n    └─→ Else → Hiển thị lỗi, lặp lại\n\nGiá trị return:\n├─→ GameSelection.Chess: Đi tới bước 2 (cờ vua)\n├─→ GameSelection.Xiangqi: Đi tới bước 2 (cờ tướng)\n└─→ null: Thoát ứng dụng\n```\n\n---\n\n### BƯỚC 2: Tạo Board & Validator\n\n```csharp\nif (gameSelection == GameSelection.Chess) {\n    board = new ChessBoard();\n    board.InitializeStandardPosition();\n    validator = new ChessMoveValidator();\n}\nelse if (gameSelection == GameSelection.Xiangqi) {\n    board = new XiangqiBoard();\n    board.InitializeStandardPosition();\n    validator = new XiangqiMoveValidator();\n}\n\nKết quả:\n├─→ board: Bàn cờ chuẩn (8×8 hoặc 10×9)\n├─→ validator: Trình kiểm tra nước phù hợp\n└─→ Tiếp bước 3\n```\n\n---\n\n### BƯỚC 3: Menu Chọn Chế Độ Chơi\n\n```csharp\nProgram.ShowGameModeMenu() → GameMode\n\nCode:\n├─→ Console.Clear()\n├─→ Console.WriteLine(\" ╔═════════════════════════════════╗\")\n├─→ Console.WriteLine(\" ║  Chọn Chế Độ Chơi               ║\")\n├─→ Console.WriteLine(\" ╠═════════════════════════════════╣\")\n├─→ Console.WriteLine(\" ║ 1. Chơi Ván Mới                 ║\")\n├─→ Console.WriteLine(\" ║ 2. Tải Ván Cũ (Replay)          ║\")\n├─→ Console.WriteLine(\" ║ 0. Quay Lại                     ║\")\n├─→ Console.WriteLine(\" ╚═════════════════════════════════╝\")\n├─→ Console.Write(\" Chọn: \")\n├─→ string input = Console.ReadLine()\n└─→ switch(input):\n    ├─→ \"1\" → return GameMode.PlayNew\n    ├─→ \"2\" → return GameMode.LoadGame\n    ├─→ \"0\" → return null (back)\n    └─→ Else → Hiển thị lỗi, lặp lại\n\nGiá trị return:\n├─→ GameMode.PlayNew: Đi tới bước 4 (chọn đồng hồ)\n├─→ GameMode.LoadGame: Đi tới bước 5 (chọn file)\n└─→ null: Quay về bước 1\n```\n\n---\n\n### BƯỚC 4: Menu Chọn Cài Đặt Đồng Hồ\n\n```csharp\nClockSelectionUI.SelectClockSettings() → ClockSettings\n\nCode:\n├─→ Console.Clear()\n├─→ Console.WriteLine(\" ╔════════════════════════════════════╗\")\n├─→ Console.WriteLine(\" ║  Cài Đặt Thời Gian Chơi            ║\")\n├─→ Console.WriteLine(\" ╠════════════════════════════════════╣\")\n├─→ Console.WriteLine(\" ║ 1. Blitz    (5 phút + 0s/nước)     ║\")\n├─→ Console.WriteLine(\" ║ 2. Rapid    (10 phút + 3s/nước)    ║\")\n├─→ Console.WriteLine(\" ║ 3. Classical (30 phút + 0s/nước)  ║\")\n├─→ Console.WriteLine(\" ║ 0. Quay Lại                        ║\")\n├─→ Console.WriteLine(\" ╚════════════════════════════════════╝\")\n├─→ Console.Write(\" Chọn: \")\n├─→ string input = Console.ReadLine()\n└─→ switch(input):\n    ├─→ \"1\" → return new ClockSettings(300, 0, BlitzMode)\n    ├─→ \"2\" → return new ClockSettings(600, 3, RapidMode)\n    ├─→ \"3\" → return new ClockSettings(1800, 0, ClassicalMode)\n    ├─→ \"0\" → return null (back)\n    └─→ Else → Hiển thị lỗi, lặp lại\n\nClockSettings:\n├─→ InitialTimeSeconds: Thời gian ban đầu (tính bằng giây)\n├─→ IncrementSeconds: Thêm thời gian mỗi nước\n├─→ Mode: Blitz/Rapid/Classical\n│\n└─→ Ví dụ:\n    ├─→ Blitz: 300 + 0 = 5 phút, không thêm\n    ├─→ Rapid: 600 + 3 = 10 phút, thêm 3s/nước\n    └─→ Classical: 1800 + 0 = 30 phút, không thêm\n```\n\n---\n\n### BƯỚC 5: Chọn File Ván Cờ\n\n```csharp\nProgram.PromptForGameFile() → string (filePath)\n\nQuá trình:\n├─→ GameRecordsManager.GetGameRecordsPath()\n│   └─→ return \"GameRecords/\" (tạo nếu không tồn tại)\n│\n├─→ Directory.GetFiles(GameRecordsPath, \"*.txt\")\n│   └─→ return [game1_20260523_143022.txt, game2_...txt, ...]\n│\n├─→ Hiển thị danh sách:\n│   ┌──────────────────────────────────┐\n│   │ Các Ván Cờ Đã Lưu:              │\n│   ├──────────────────────────────────┤\n│   │ 1. game1_20260523_143022.txt    │\n│   │ 2. game2_20260520_100110.txt    │\n│   │ 3. analysis_20260515_090000.txt │\n│   │ 0. Quay Lại                     │\n│   └──────────────────────────────────┘\n│\n├─→ Console.Write(\" Chọn file (số hoặc tên): \")\n├─→ string input = Console.ReadLine()\n│\n├─→ Kiểm tra input:\n│   ├─→ Nếu số (1-3): Lấy file tương ứng\n│   ├─→ Nếu tên file: Kiểm tra tồn tại\n│   ├─→ Nếu 0: Quay về bước 5\n│   └─→ Nếu không hợp lệ: Lỗi, lặp lại\n│\n└─→ return GameRecords/{filename}\n```\n\n---\n\n## 📊 Sơ Đồ Quyết Định\n\n```\nBắt đầu\n  ↓\n┌─────────────┐\n│ Chọn Loại   │ (Chess/Xiangqi)\n│ Game        │\n└────┬────────┘\n     ↓\n┌─────────────┐\n│ Tạo Board   │\n│ & Validator │\n└────┬────────┘\n     ↓\n┌─────────────┐\n│ Chọn Chế Độ │ (PlayNew/LoadGame)\n└────┬────────┘\n   │\n   ├─→ PlayNew ─→ ┌─────────────┐\n   │              │ Chọn Đồng    │\n   │              │ Hồ (Blitz/   │\n   │              │ Rapid/       │\n   │              │ Classical)   │\n   │              └────┬────────┘\n   │                   ↓\n   │              Chơi Game Mới\n   │\n   └─→ LoadGame ─→ ┌─────────────┐\n                    │ Chọn File   │\n                    │ Ván Cũ      │\n                    └────┬────────┘\n                         ↓\n                    Tải & Xem Lại\n                         ↓\n                    ┌─────────────┐\n                    │ Hậu Kỳ      │\n                    │ (Lưu/Thoát) │\n                    └────┬────────┘\n                         ↓\n                   Quay Lại Bước 1\n```\n\n---\n\n**Tiếp theo:**\n- [05-LuồngChơiCờVua.md](05-LuồngChơiCờVua.md) - Luồng chơi cờ vua\n- [06-LuồngChơiCờTướng.md](06-LuồngChơiCờTướng.md) - Luồng chơi cờ tướng\n