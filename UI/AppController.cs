// /UI/AppController.cs
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Extensions;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Chess;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Modules.Clock;
using ChessXiangqiSolution.Modules.Movement;
using ChessXiangqiSolution.Modules.Notation;
using ChessXiangqiSolution.UI.ConsoleUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChessXiangqiSolution.UI
{
    public class AppController
    {
        private IBoard _board;
        private IMoveValidator _validator;
        private GameClock _clock;
        private Color _currentTurn;
        private bool _gameOver;
        private BoardRenderer _renderer;
        private InputHandler _inputHandler;
        private HistoryManager _historyManager;
        private BranchTracker _branchTracker;
        private List<string> _moveHistorySAN;

        // Replay mode properties
        private bool _isReplayMode = false;
        private int _replayMoveIndex = -1;
        private MatchRecord _replayingRecord = null;
        private GameSaver _gameSaver = new GameSaver();

        // Match state
        private MatchState _matchState = MatchState.Game;

        public AppController(IBoard board, IMoveValidator validator, ClockSettings clockSettings)
        {
            _board = board;
            _validator = validator;
            _clock = new GameClock(clockSettings);
            _currentTurn = Color.White;
            _gameOver = false;
            _renderer = new BoardRenderer();
            _inputHandler = new InputHandler();
            _historyManager = new HistoryManager(_board);
            _branchTracker = new BranchTracker();
            _moveHistorySAN = new List<string>();

            _clock.OnTimeOut += OnTimeOut;
            _clock.OnTurnStarted += OnTurnStarted;
            _clock.OnTimeUpdated += OnTimeUpdated;
        }

        public void Run()
        {
            System.Console.Clear();
            if (_board.GameType == GameType.Xiangqi)
            {
                System.Console.WriteLine("=== XIANGQI GAME (Console) ===");
                System.Console.WriteLine("Nhập nước đi định dạng tọa độ (cột 1-9, hàng 1-10)");
            }
            else
            {
                System.Console.WriteLine("=== CHESS GAME (Console) ===");
                System.Console.WriteLine("Nhập nước đi định dạng SAN hoặc tọa độ");
            }
            System.Console.WriteLine("Gõ 'quit' để thoát, 'undo' (Ctrl+Z), 'redo' (Ctrl+Y), 'moves' để xem lịch sử");
            System.Console.WriteLine("Gõ 'branch' để xem các nhánh rẽ, Backspace (khi input rỗng) để quay về menu.\n");

            if (_matchState == MatchState.Game)
                _clock.StartTurn(_currentTurn);

            while (!_gameOver)
            {
                _renderer.Render(_board, _currentTurn,
                    _clock.GetRemainingTime(Color.White),
                    _clock.GetRemainingTime(Color.Black),
                    _moveHistorySAN);

                string input = _inputHandler.GetCommandOrMove();
                if (input == "quit") break;
                if (input == "menu")
                {
                    System.Console.WriteLine("\nQuay lại menu chính...");
                    Thread.Sleep(1000);
                    break;
                }
                if (input == "undo") { PerformUndo(); continue; }
                if (input == "redo") { PerformRedo(); continue; }
                if (input == "moves") { ShowMoveHistory(); continue; }
                if (input == "branch") { ShowBranches(); continue; }

                if (!_inputHandler.TryParseMove(input, _board, _currentTurn, out Move move))
                {
                    System.Console.WriteLine("Lỗi: Không thể hiểu nước đi.");
                    _inputHandler.DisplayInputHint(_board.GameType);
                    Thread.Sleep(4000);
                    continue;
                }

                if (NeedPromotion(move))
                {
                    System.Console.Write("Phong cấp thành (Q/R/B/N): ");
                    string promoInput = System.Console.ReadLine()?.ToUpper() ?? "Q";
                    move.PromotionPiece = promoInput switch
                    {
                        "Q" => PieceType.Queen,
                        "R" => PieceType.Rook,
                        "B" => PieceType.Bishop,
                        "N" => PieceType.Knight,
                        _ => PieceType.Queen
                    };
                }

                if (_validator.IsValidMove(_board, move, _currentTurn))
                {
                    AnnotateMove(move);
                    string san = ConvertMoveToSan(move);
                    _moveHistorySAN.Add(san);

                    var command = new MoveCommand(_board, move);
                    _historyManager.ExecuteCommand(command);
                    _branchTracker.AddMove(move);

                    if (_matchState == MatchState.Game)
                    {
                        _clock.StopTurn();
                        _currentTurn = _currentTurn.Opposite();
                        _clock.StartTurn(_currentTurn);
                    }
                    else
                    {
                        _currentTurn = _currentTurn.Opposite();
                    }

                    if (_validator.IsCheckmate(_board, _currentTurn))
                    {
                        _renderer.Render(_board, _currentTurn,
                            _clock.GetRemainingTime(Color.White),
                            _clock.GetRemainingTime(Color.Black),
                            _moveHistorySAN);
                        string winner = (_currentTurn == Color.White) ? "Đen" : "Trắng";
                        System.Console.WriteLine($"Chiếu hết! {winner} thắng.");
                        _gameOver = true;
                    }
                    else if (_validator.IsStalemate(_board, _currentTurn))
                    {
                        _renderer.Render(_board, _currentTurn,
                            _clock.GetRemainingTime(Color.White),
                            _clock.GetRemainingTime(Color.Black),
                            _moveHistorySAN);
                        System.Console.WriteLine("Hòa cờ.");
                        _gameOver = true;
                    }
                }
                else
                {
                    System.Console.WriteLine("Nước đi không hợp lệ!");
                    Thread.Sleep(1500);
                }
            }

            System.Console.WriteLine("Cảm ơn bạn đã chơi!");
            var mainLine = _branchTracker.GetCurrentLine();
            if (mainLine.Any())
            {
                System.Console.WriteLine("Biên bản ván đấu (SAN):");
                for (int i = 0; i < mainLine.Count; i++)
                {
                    if (i % 2 == 0) System.Console.Write($"{i / 2 + 1}. {ConvertMoveToSan(mainLine[i])} ");
                    else System.Console.WriteLine($"{ConvertMoveToSan(mainLine[i])}");
                }
                System.Console.WriteLine();
            }

            PromptToSaveGame();
        }

        private void PromptToSaveGame()
        {
            System.Console.WriteLine("\n--- SAVE GAME ---");
            System.Console.Write("Bạn có muốn lưu biên bản ván đấu? (y/n): ");

            string response = System.Console.ReadLine()?.Trim().ToLower();
            if (response != "y" && response != "yes")
            {
                System.Console.WriteLine("Không lưu.");
                Thread.Sleep(1000);
                return;
            }

            System.Console.Write("Nhập tên tệp (không có phần mở rộng): ");
            string filename = System.Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(filename))
            {
                System.Console.WriteLine("Tên không hợp lệ. Biên bản không được lưu.");
                Thread.Sleep(1000);
                return;
            }

            try
            {
                var record = CreateMatchRecordFromGame();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string finalFilename = $"{filename}_{timestamp}.pgn";
                string filePath = GameRecordsManager.ResolveGameFilePath(finalFilename);
                GameRecordsManager.SaveGameAsPgn(filePath, record);
                System.Console.WriteLine($"✓ Game saved successfully to: {finalFilename}");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error saving game: {ex.Message}");
                Thread.Sleep(2000);
            }
        }

        private MatchRecord CreateMatchRecordFromGame()
        {
            return new MatchRecord
            {
                GameType = _board.GameType.ToString(),
                Moves = _branchTracker.GetCurrentLine().ToList(),
                WhitePlayer = "Player 1",
                BlackPlayer = "Player 2",
                Event = $"{_board.GameType.ToString()} Game",
                Date = System.DateTime.Now,
                TimeControl = $"{_clock.Settings.InitialTimeSeconds}+{_clock.Settings.IncrementSeconds}",
                FinalState = new GameState
                {
                    IsGameOver = true,
                    GameOverReason = "Game completed",
                    CurrentTurn = _currentTurn,
                    WhiteTimeSeconds = _clock.GetRemainingTime(Color.White),
                    BlackTimeSeconds = _clock.GetRemainingTime(Color.Black),
                    Fen = ""
                }
            };
        }

        private void PerformUndo()
        {
            if (!_historyManager.CanUndo)
            {
                System.Console.WriteLine("Không thể undo thêm nữa.");
                Thread.Sleep(800);
                return;
            }

            _historyManager.Undo();
            _branchTracker.GoBack();
            if (_moveHistorySAN.Count > 0)
                _moveHistorySAN.RemoveAt(_moveHistorySAN.Count - 1);
            _currentTurn = _currentTurn.Opposite();
            System.Console.WriteLine("Đã undo nước đi cuối.");
            Thread.Sleep(800);
        }

        private void PerformRedo()
        {
            if (!_historyManager.CanRedo)
            {
                Console.WriteLine("Không thể redo thêm nữa.");
                Thread.Sleep(800);
                return;
            }

            if (_historyManager.Redo(out Move redoneMove, out string san))
            {
                if (string.IsNullOrEmpty(san))
                    san = ConvertMoveToSan(redoneMove);
                _moveHistorySAN.Add(san);
                _branchTracker.AddMove(redoneMove);
                _currentTurn = _currentTurn.Opposite();
                Console.WriteLine("Đã redo nước đi.");
                Thread.Sleep(800);
            }
            else
            {
                Console.WriteLine("Lỗi khi redo.");
                Thread.Sleep(800);
            }
        }

        private void ShowMoveHistory()
        {
            var currentLine = _branchTracker.GetCurrentLine();
            System.Console.WriteLine("\n--- Lịch sử nước đi (từ đầu đến hiện tại) ---");
            if (currentLine.Count == 0)
                System.Console.WriteLine("Chưa có nước nào.");
            else
            {
                for (int i = 0; i < currentLine.Count; i++)
                {
                    int moveNum = i / 2 + 1;
                    if (i % 2 == 0)
                        System.Console.Write($"{moveNum}. {ConvertMoveToSan(currentLine[i])} ");
                    else
                        System.Console.WriteLine($"{ConvertMoveToSan(currentLine[i])}");
                }
                if (currentLine.Count % 2 == 1)
                    System.Console.WriteLine();
            }
            System.Console.WriteLine("-----------------------------------------\n");
            Thread.Sleep(2000);
        }

        private void ShowBranches()
        {
            var branches = _branchTracker.GetAvailableBranches();
            if (branches.Count == 0)
            {
                System.Console.WriteLine("Tại thế cờ này không có nhánh rẽ nào.");
            }
            else
            {
                System.Console.WriteLine("Các nhánh rẽ khả dụng:");
                for (int i = 0; i < branches.Count; i++)
                    System.Console.WriteLine($"  {i + 1}. {ConvertMoveToSan(branches[i])}");
                System.Console.Write("Bạn có thể nhập một nước đi trong số đó để rẽ nhánh.");
            }
            System.Console.WriteLine();
            Thread.Sleep(3000);
        }

        private bool NeedPromotion(Move move)
        {
            var piece = _board.GetPieceAt(move.From);
            if (piece?.Type != PieceType.Pawn) return false;
            int targetRow = move.To.Row;
            return (piece.Color == Color.White && targetRow == 0) ||
                   (piece.Color == Color.Black && targetRow == 7);
        }

        private string ConvertMoveToSan(Move move)
        {
            if (!string.IsNullOrEmpty(move.San))
                return move.San;
            string san = SanFormatter.FormatSan(move, _board);
            move.San = san;
            return san;
        }

        private void AnnotateMove(Move move)
        {
            if (move == null) return;

            if (move.IsEnPassant)
            {
                var capturePos = new Position(move.From.Row, move.To.Col);
                move.CapturedPiece = _board.GetPieceAt(capturePos);
            }
            else
            {
                move.CapturedPiece = _board.GetPieceAt(move.To);
            }
        }

        private void OnTimeUpdated(Color color, int remainingTime) => RefreshClockDisplay();

        private void OnTimeOut(Color color)
        {
            _gameOver = true;
            RefreshClockDisplay();
            System.Console.WriteLine($"Hết giờ! {(color == Color.White ? "Trắng" : "Đen")} thua.");
        }

        private void OnTurnStarted(Color color, int timeForCurrent, int timeForOpponent) => RefreshClockDisplay();

        private void RefreshClockDisplay()
        {
            try
            {
                int originalLeft = Console.CursorLeft;
                int originalTop = Console.CursorTop;
                string status = $"Lượt: {(_currentTurn == Color.White ? "Trắng" : "Đen")}   🕒 Trắng: {FormatTime(_clock.GetRemainingTime(Color.White))}  Đen: {FormatTime(_clock.GetRemainingTime(Color.Black))}";
                int width = Math.Max(Console.WindowWidth, status.Length);
                Console.SetCursorPosition(0, 0);
                Console.Write(status.PadRight(width));
                Console.SetCursorPosition(originalLeft, originalTop);
            }
            catch { }
        }

        private string FormatTime(int seconds)
        {
            int min = seconds / 60;
            int sec = seconds % 60;
            return $"{min:D2}:{sec:D2}";
        }

        // =====================================================================
        // REPLAY MODE
        // =====================================================================

        public void ReplayGameFromFile(string filePath)
        {
            try
            {
                var record = _gameSaver.LoadGame(filePath);
                if (record == null)
                {
                    GameReplayUI.DisplayError("Failed to load game file");
                    return;
                }

                EnterReplayMode(record);
            }
            catch (Exception ex)
            {
                GameReplayUI.DisplayError($"Exception: {ex.Message}");
            }
        }

        private void EnterReplayMode(MatchRecord record)
        {
            _matchState = MatchState.Analyse;
            _isReplayMode = true;
            _replayingRecord = record;
            _replayMoveIndex = -1;

            // Reset về đầu ván (board sạch, history rỗng)
            _historyManager.Clear();
            _moveHistorySAN.Clear();
            _branchTracker.Reset();
            _currentTurn = Color.White;

            GameReplayUI.DisplayGameInfo(record);
            System.Console.WriteLine("\nTự động phát lại ván cờ...\n");

            // Auto-play đến cuối, hiển thị từng nước
            AutoPlayAllMoves();

            // Hiển thị kết quả ở vị trí cuối
            DisplayGameResult(record);

            GameReplayUI.DisplayReplayControls();
            System.Console.ReadKey();

            RunReplayMode();
        }

        /// <summary>
        /// Phát lại toàn bộ ván từ đầu đến cuối qua HistoryManager.
        /// Sau khi chạy xong, board ở trạng thái nước cuối cùng.
        /// </summary>
        private void AutoPlayAllMoves()
        {
            if (_replayingRecord?.Moves == null) return;

            foreach (var move in _replayingRecord.Moves)
            {
                // Parse SAN để lấy coordinates thực (moves từ file chỉ có SAN, From/To=(0,0))
                if (!MoveParser.TryParse(move.San, _board, _currentTurn, out Move resolvedMove))
                    continue;
                
                if (_validator.IsValidMove(_board, resolvedMove, _currentTurn))
                {
                    var cmd = new MoveCommand(_board, resolvedMove);
                    _historyManager.ExecuteCommand(cmd);
                    _branchTracker.AddMove(resolvedMove);
                    _moveHistorySAN.Add(resolvedMove.San ?? ConvertMoveToSan(resolvedMove));
                    _replayMoveIndex++;
                    _currentTurn = _currentTurn.Opposite();
                }
            }
        }

        private void DisplayGameResult(MatchRecord record)
        {
            System.Console.Clear();
            _renderer.Render(_board, _currentTurn, -1, -1, _moveHistorySAN);

            if (!string.IsNullOrEmpty(record.Result))
            {
                System.Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
                System.Console.WriteLine("║                    GAME RESULT                         ║");
                System.Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                System.Console.WriteLine($"Result: {record.Result}");

                if (record.Result.Contains("1-0"))
                    System.Console.WriteLine("Kết quả: Trắng thắng");
                else if (record.Result.Contains("0-1"))
                    System.Console.WriteLine("Kết quả: Đen thắng");
                else if (record.Result.Contains("1/2"))
                    System.Console.WriteLine("Kết quả: Hòa");

                System.Console.WriteLine();
            }
        }

        private void RunReplayMode()
        {
            while (_isReplayMode)
            {
                // Board đã ở đúng trạng thái — chỉ render, không rebuild ở đây
                RenderReplayBoard();

                var command = GameReplayUI.GetReplayCommand();

                switch (command)
                {
                    case ReplayCommand.Next:
                        NextReplayMove();
                        break;
                    case ReplayCommand.Previous:
                        PreviousReplayMove();
                        break;
                    case ReplayCommand.ToStart:
                        GoToStartReplay();
                        break;
                    case ReplayCommand.ToEnd:
                        GoToEndReplay();
                        break;
                    case ReplayCommand.GoToMove:
                        int moveIdx = GameReplayUI.GetMoveIndex(_replayingRecord.GetTotalMoves());
                        if (moveIdx >= 0)
                            GoToMoveIndexReplay(moveIdx);
                        break;
                    case ReplayCommand.Export:
                        ExportCurrentReplayPosition();
                        break;
                    case ReplayCommand.Quit:
                        ExitReplayMode();
                        break;
                    default:
                        continue;
                }
            }
        }

        /// <summary>
        /// Chỉ render board tại trạng thái hiện tại — KHÔNG rebuild board ở đây.
        /// Việc rebuild đã được thực hiện bởi Next/Prev/GoTo trước khi gọi hàm này.
        /// </summary>
        private void RenderReplayBoard()
        {
            System.Console.Clear();
            int totalMoves = _replayingRecord.GetTotalMoves();

            _renderer.Render(_board, _currentTurn, -1, -1, _moveHistorySAN);

            // Move counter
            System.Console.WriteLine($"Move: {_replayMoveIndex + 1}/{totalMoves}");

            // Thông tin nước đi hiện tại
            if (_replayMoveIndex >= 0 && _replayMoveIndex < totalMoves)
            {
                var move = _replayingRecord.GetMoveAtIndex(_replayMoveIndex);
                string sanMove = move.San ?? $"{move.From}-{move.To}";
                System.Console.WriteLine($"Nước đi hiện tại: {sanMove}");
            }
            else if (_replayMoveIndex == -1)
            {
                System.Console.WriteLine("Trạng thái: Đầu ván");
            }

            // Kết quả ở cuối ván
            if (_replayMoveIndex == totalMoves - 1 && !string.IsNullOrEmpty(_replayingRecord.Result))
            {
                System.Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
                System.Console.WriteLine("║                    GAME RESULT                         ║");
                System.Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                System.Console.WriteLine($"Result: {_replayingRecord.Result}");
                if (_replayingRecord.Result.Contains("1-0"))
                    System.Console.WriteLine("Kết quả: Trắng thắng");
                else if (_replayingRecord.Result.Contains("0-1"))
                    System.Console.WriteLine("Kết quả: Đen thắng");
                else if (_replayingRecord.Result.Contains("1/2"))
                    System.Console.WriteLine("Kết quả: Hòa");
                System.Console.WriteLine();
            }

            // Danh sách nước đi với highlight nước hiện tại
            System.Console.WriteLine("\n--- Danh sách nước đi ---");
            for (int i = 0; i < _replayingRecord.Moves.Count; i++)
            {
                string moveStr = _replayingRecord.Moves[i].San
                    ?? $"{_replayingRecord.Moves[i].From}-{_replayingRecord.Moves[i].To}";
                string prefix = (i == _replayMoveIndex) ? "> " : "  ";
                int moveNumber = i / 2 + 1;

                if (i % 2 == 0)
                    System.Console.Write($"{prefix}{moveNumber}. {moveStr} ");
                else
                    System.Console.WriteLine(moveStr);
            }
            if (_replayingRecord.Moves.Count % 2 == 1)
                System.Console.WriteLine();
            System.Console.WriteLine("-------------------------");

            System.Console.WriteLine("\n[↑/←: Prev] [↓/→: Next] [Home: Start] [End: Last] [G: Go to move] [Q: Quit]");
        }

        /// <summary>
        /// Tiến một nước: thực thi move tiếp theo qua HistoryManager.
        /// </summary>
        private void NextReplayMove()
        {
            int totalMoves = _replayingRecord.GetTotalMoves();
            if (_replayMoveIndex >= totalMoves - 1) return;

            _replayMoveIndex++;
            var move = _replayingRecord.GetMoveAtIndex(_replayMoveIndex);

            // Parse SAN để lấy coordinates thực (moves từ file chỉ có SAN, From/To=(0,0))
            if (!MoveParser.TryParse(move.San, _board, _currentTurn, out Move resolvedMove))
                return;

            var cmd = new MoveCommand(_board, resolvedMove);
            _historyManager.ExecuteCommand(cmd);
            _branchTracker.AddMove(resolvedMove);

            string san = resolvedMove.San ?? ConvertMoveToSan(resolvedMove);
            _moveHistorySAN.Add(san);

            _currentTurn = _currentTurn.Opposite();
        }

        /// <summary>
        /// Lùi một nước: undo qua HistoryManager.
        /// </summary>
        private void PreviousReplayMove()
        {
            if (_replayMoveIndex < 0) return;

            _historyManager.Undo();
            _branchTracker.GoBack();

            if (_moveHistorySAN.Count > 0)
                _moveHistorySAN.RemoveAt(_moveHistorySAN.Count - 1);

            _replayMoveIndex--;
            _currentTurn = _currentTurn.Opposite();
        }

        /// <summary>
        /// Về đầu ván: undo toàn bộ.
        /// </summary>
        private void GoToStartReplay()
        {
            // Undo từng bước cho đến khi hết
            while (_historyManager.CanUndo)
                _historyManager.Undo();

            _branchTracker.GoToStart();
            _moveHistorySAN.Clear();
            _replayMoveIndex = -1;
            _currentTurn = Color.White;
        }

        /// <summary>
        /// Đến cuối ván: rebuild từ đầu đến move cuối cùng.
        /// </summary>
        private void GoToEndReplay()
        {
            int lastIndex = _replayingRecord.GetTotalMoves() - 1;
            GoToMoveIndexReplay(lastIndex);
        }

        /// <summary>
        /// Nhảy đến nước bất kỳ: rebuild từ đầu đến moveIndex.
        /// </summary>
        private void GoToMoveIndexReplay(int moveIndex)
        {
            if (moveIndex < -1 || moveIndex >= _replayingRecord.GetTotalMoves()) return;

            // Bug fix: rebuild từ đầu (không dùng GoToMoveIndex cũ của BranchTracker)
            RebuildBoardToMoveIndex(moveIndex);
        }

        /// <summary>
        /// Rebuild board + history + BranchTracker từ đầu đến moveIndex.
        /// Đây là nguồn của sự thật duy nhất — Next/Prev không dùng hàm này (dùng undo/redo).
        /// GoToMoveIndex và GoToEnd mới dùng hàm này.
        /// </summary>
        private void RebuildBoardToMoveIndex(int moveIndex)
        {
            // Reset toàn bộ state
            _historyManager.Clear();
            _branchTracker.Reset();
            _moveHistorySAN.Clear();
            _currentTurn = Color.White;
            _replayMoveIndex = -1;

            if (moveIndex < 0) return;

            var sourceMoves = _replayingRecord.Moves;
            for (int i = 0; i <= moveIndex && i < sourceMoves.Count; i++)
            {
                var move = sourceMoves[i];

                // Parse SAN để lấy coordinates thực (moves từ file chỉ có SAN, From/To=(0,0))
                if (!MoveParser.TryParse(move.San, _board, _currentTurn, out Move resolvedMove))
                    continue;

                // FIX Bug 2: dùng MoveCommand thay vì _board.MakeMove() trực tiếp
                var cmd = new MoveCommand(_board, resolvedMove);
                _historyManager.ExecuteCommand(cmd);

                // FIX Bug 1: truyền sourceMoves vào BranchTracker thay vì dùng _mainLine nội bộ
                _branchTracker.AddMove(resolvedMove);

                string san = resolvedMove.San ?? ConvertMoveToSan(resolvedMove);
                _moveHistorySAN.Add(san);

                _currentTurn = _currentTurn.Opposite();
                _replayMoveIndex = i;
            }
        }

        private void ExportCurrentReplayPosition()
        {
            string filePath = GameReplayUI.GetExportFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var record = _replayingRecord.Clone();
                record.Moves = _replayMoveIndex >= 0
                    ? _replayingRecord.Moves.Take(_replayMoveIndex + 1).ToList()
                    : new List<Move>();

                _gameSaver.SaveGame(filePath, record);
                GameReplayUI.DisplaySuccess($"Exported to {filePath}");
                System.Console.ReadKey();
            }
            catch (Exception ex)
            {
                GameReplayUI.DisplayError($"Export failed: {ex.Message}");
                System.Console.ReadKey();
            }
        }

        private void ExitReplayMode()
        {
            _matchState = MatchState.Game;
            _isReplayMode = false;
            _replayingRecord = null;
            _replayMoveIndex = -1;
            System.Console.Clear();
            System.Console.WriteLine("Exited replay mode. Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}