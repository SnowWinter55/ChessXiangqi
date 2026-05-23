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

        // Constructor nhận board, validator và clockSettings từ bên ngoài
        public AppController(IBoard board, IMoveValidator validator, ClockSettings clockSettings)
        {
            _board = board;
            _validator = validator;
            _clock = new GameClock(clockSettings);
            _currentTurn = Color.White; // Trong chess là White, Xiangqi có thể là Red nhưng enum dùng chung
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

                // Xử lý phong cấp nếu cần
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
                    // Thêm nước đi dạng SAN vào lịch sử (trước khi thay đổi bàn cờ)
                    string san = ConvertMoveToSan(move);
                    _moveHistorySAN.Add(san);

                    // Tạo Command và thực thi qua HistoryManager
                    var command = new MoveCommand(_board, move);
                    _historyManager.ExecuteCommand(command);

                    // Cập nhật BranchTracker (thêm nước đi vào cây)
                    _branchTracker.AddMove(move);

                    // Kết thúc lượt hiện tại và chuyển sang lượt đối thủ
                    _clock.StopTurn();
                    _currentTurn = _currentTurn.Opposite();
                    _clock.StartTurn(_currentTurn);

                    // Kiểm tra kết thúc ván
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
            // Hiển thị biên bản đầy đủ (main line)
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

            // Hỏi người chơi có muốn lưu ván game
            PromptToSaveGame();
        }

        /// <summary>Prompt user to save the current game</summary>
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
                // Create MatchRecord from current game state
                var record = CreateMatchRecordFromGame();
                
                // Generate timestamp for default filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string finalFilename = $"{filename}_{timestamp}.txt";
                
                // Resolve path using GameRecordsManager
                string filePath = GameRecordsManager.ResolveGameFilePath(finalFilename);
                
                // Export to file (use GameSaver for proper formatting)
                _gameSaver.SaveGame(filePath, record);
                
                System.Console.WriteLine($"✓ Game saved successfully to: {finalFilename}");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error saving game: {ex.Message}");
                Thread.Sleep(2000);
            }
        }

        /// <summary>Create MatchRecord from current game state</summary>
        private MatchRecord CreateMatchRecordFromGame()
        {
            var record = new MatchRecord
            {
                GameType = _board.GameType.ToString(), // Convert enum to string
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

            return record;
        }

        private void PerformUndo()
        {
            if (!_historyManager.CanUndo)
            {
                System.Console.WriteLine("Không thể undo thêm nữa.");
                Thread.Sleep(800);
                return;
            }

            // Undo command (thay đổi bàn cờ)
            _historyManager.Undo();
            // Cập nhật BranchTracker: lùi về node cha
            _branchTracker.GoBack();
            // Xóa nước đi cuối khỏi lịch sử hiển thị
            if (_moveHistorySAN.Count > 0)
                _moveHistorySAN.RemoveAt(_moveHistorySAN.Count - 1);
            // Đảo lượt đi (vì undo đã lùi lại nước đi, nên lượt hiện tại là của người vừa bị undo)
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
                // Thêm nước đi redo vào lịch sử hiển thị dưới dạng SAN
                if (string.IsNullOrEmpty(san))
                    san = ConvertMoveToSan(redoneMove);
                _moveHistorySAN.Add(san);
                // Cập nhật BranchTracker: thêm nước đi (sẽ tạo hoặc chuyển đến node con)
                _branchTracker.AddMove(redoneMove);
                // Đảo lượt đi (vì redo đã thực hiện nước đi, chuyển sang đối thủ)
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
                System.Console.WriteLine("Tại thế cờ này không có nhánh rẽ nào (chỉ có một nước đi tiếp theo).");
            }
            else
            {
                System.Console.WriteLine("Các nhánh rẽ khả dụng (các nước đi khác từ thế cờ này):");
                for (int i = 0; i < branches.Count; i++)
                {
                    System.Console.WriteLine($"  {i + 1}. {ConvertMoveToSan(branches[i])}");
                }
                System.Console.Write("Bạn có thể nhập một nước đi trong số đó để rẽ nhánh, hoặc gõ 'branch' lại để xóa.");
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
            if (move == null)
                return;

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

        private void OnTimeUpdated(Color color, int remainingTime)
        {
            RefreshClockDisplay();
        }

        private void OnTimeOut(Color color)
        {
            _gameOver = true;
            RefreshClockDisplay();
            System.Console.WriteLine($"Hết giờ! {(color == Color.White ? "Trắng" : "Đen")} thua.");
        }

        private void OnTurnStarted(Color color, int timeForCurrent, int timeForOpponent)
        {
            RefreshClockDisplay();
        }

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
            catch
            {
                // Ignore if console is not available or window size changed during update.
            }
        }

        private string FormatTime(int seconds)
        {
            int min = seconds / 60;
            int sec = seconds % 60;
            return $"{min:D2}:{sec:D2}";
        }

        /// <summary>
        /// Vào chế độ replay ván cờ từ file
        /// </summary>
        public void ReplayGameFromFile(string filePath)
        {
            try
            {
                // Load game từ file
                var record = _gameSaver.LoadGame(filePath);
                if (record == null)
                {
                    GameReplayUI.DisplayError("Failed to load game file");
                    return;
                }

                // Replay game
                bool success = _gameSaver.ReplayGame(record, _board, _validator, _historyManager, _branchTracker);
                if (!success)
                {
                    GameReplayUI.DisplayError("Failed to replay game - invalid moves detected");
                    return;
                }

                // Enter replay mode
                EnterReplayMode(record);
            }
            catch (Exception ex)
            {
                GameReplayUI.DisplayError($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Vào chế độ replay
        /// </summary>
        private void EnterReplayMode(MatchRecord record)
        {
            _isReplayMode = true;
            _replayingRecord = record;
            _replayMoveIndex = -1; // Start at beginning

            GameReplayUI.DisplayGameInfo(record);
            GameReplayUI.DisplayReplayControls();
            System.Console.ReadKey();

            RunReplayMode();
        }

        /// <summary>
        /// Chạy vòng lặp chính cho chế độ replay
        /// </summary>
        private void RunReplayMode()
        {
            while (_isReplayMode)
            {
                System.Console.Clear();
                
                // Render board tại move hiện tại
                RenderReplayBoard();

                // Nhập lệnh
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
        /// Render board cho chế độ replay
        /// </summary>
        private void RenderReplayBoard()
        {
            int totalMoves = _replayingRecord.GetTotalMoves();
            
            // Render board at current position
            _renderer.Render(_board, Color.White,
                -1, -1, // No clock display
                new List<string>()); // No move history display

            // Show move counter
            GameReplayUI.DisplayMoveCounter(_replayMoveIndex, totalMoves);
            
            // Show current move info
            if (_replayMoveIndex >= 0 && _replayMoveIndex < totalMoves)
            {
                var move = _replayingRecord.GetMoveAtIndex(_replayMoveIndex);
                string sanMove = move.San ?? $"{move.From}-{move.To}";
                Console.WriteLine($"\nCurrent move: {sanMove}");
            }

            GameReplayUI.DisplayMoveList(_replayingRecord.Moves, _replayMoveIndex);
        }

        /// <summary>
        /// Đi tới nước tiếp theo
        /// </summary>
        private void NextReplayMove()
        {
            if (_replayMoveIndex < _replayingRecord.GetTotalMoves() - 1)
            {
                _replayMoveIndex++;
                _branchTracker.GoToMoveIndex(_replayMoveIndex);
                
                // Rebuild board from start to this position
                RebuildBoardToMoveIndex(_replayMoveIndex);
            }
        }

        /// <summary>
        /// Đi tới nước trước đó
        /// </summary>
        private void PreviousReplayMove()
        {
            if (_replayMoveIndex > -1)
            {
                _replayMoveIndex--;
                _branchTracker.GoToMoveIndex(_replayMoveIndex);
                
                // Rebuild board from start to this position
                RebuildBoardToMoveIndex(_replayMoveIndex);
            }
        }

        /// <summary>
        /// Đi tới đầu ván
        /// </summary>
        private void GoToStartReplay()
        {
            _replayMoveIndex = -1;
            _branchTracker.GoToStart();
            ResetBoard();
        }

        /// <summary>
        /// Đi tới cuối ván
        /// </summary>
        private void GoToEndReplay()
        {
            _replayMoveIndex = _replayingRecord.GetTotalMoves() - 1;
            _branchTracker.GoToEnd();
            RebuildBoardToMoveIndex(_replayMoveIndex);
        }

        /// <summary>
        /// Đi tới move cụ thể
        /// </summary>
        private void GoToMoveIndexReplay(int moveIndex)
        {
            if (moveIndex >= 0 && moveIndex < _replayingRecord.GetTotalMoves())
            {
                _replayMoveIndex = moveIndex;
                _branchTracker.GoToMoveIndex(moveIndex);
                RebuildBoardToMoveIndex(moveIndex);
            }
        }

        /// <summary>
        /// Rebuild board từ đầu đến move cụ thể
        /// </summary>
        private void RebuildBoardToMoveIndex(int moveIndex)
        {
            ResetBoard();
            
            if (moveIndex < 0)
                return;

            for (int i = 0; i <= moveIndex && i < _replayingRecord.GetTotalMoves(); i++)
            {
                var move = _replayingRecord.GetMoveAtIndex(i);
                _board.MakeMove(move);
            }
        }

        /// <summary>
        /// Reset board về trạng thái ban đầu
        /// </summary>
        private void ResetBoard()
        {
            // Create new board instance with same type
            // This is simplified - in reality you'd need to reinitialize properly
            _historyManager.Clear();
        }

        /// <summary>
        /// Export vị trí hiện tại
        /// </summary>
        private void ExportCurrentReplayPosition()
        {
            string filePath = GameReplayUI.GetExportFilePath();
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var record = _replayingRecord.Clone();
                // Trim moves to current position
                if (_replayMoveIndex >= 0)
                {
                    record.Moves = _replayingRecord.Moves.Take(_replayMoveIndex + 1).ToList();
                }
                else
                {
                    record.Moves.Clear();
                }

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

        /// <summary>
        /// Thoát chế độ replay
        /// </summary>
        private void ExitReplayMode()
        {
            _isReplayMode = false;
            _replayingRecord = null;
            _replayMoveIndex = -1;
            System.Console.Clear();
            System.Console.WriteLine("Exited replay mode. Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}
