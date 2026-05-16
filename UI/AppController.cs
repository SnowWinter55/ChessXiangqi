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

        // Constructor nhận board và validator từ bên ngoài
        public AppController(IBoard board, IMoveValidator validator)
        {
            _board = board;
            _validator = validator;
            var clockSettings = ClockSettings.DefaultFischer();
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
            System.Console.WriteLine("=== CHESS GAME (Console) ===");
            System.Console.WriteLine("Nhập nước đi định dạng SAN hoặc tọa độ");
            System.Console.WriteLine("Gõ 'quit' để thoát, 'undo' (Ctrl+Z), 'redo' (Ctrl+Y), 'moves' để xem lịch sử");
            System.Console.WriteLine("Gõ 'branch' để xem các nhánh rẽ có thể tại thế cờ hiện tại.\n");

            _clock.StartTurn(_currentTurn);

            while (!_gameOver)
            {
                _renderer.Render(_board, _currentTurn,
                    _clock.GetRemainingTime(Color.White),
                    _clock.GetRemainingTime(Color.Black),
                    _moveHistorySAN);

                string input = _inputHandler.GetCommandOrMove();
                if (input == "quit") break;
                if (input == "undo") { PerformUndo(); continue; }
                if (input == "redo") { PerformRedo(); continue; }
                if (input == "moves") { ShowMoveHistory(); continue; }
                if (input == "branch") { ShowBranches(); continue; }

                if (!_inputHandler.TryParseMove(input, _board, _currentTurn, out Move move))
                {
                    System.Console.WriteLine("Lỗi: Không thể hiểu nước đi.");
                    Thread.Sleep(1500);
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
    }
}
