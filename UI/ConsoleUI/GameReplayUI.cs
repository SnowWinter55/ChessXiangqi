using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Modules.Notation;

namespace ChessXiangqiSolution.UI.ConsoleUI
{
    /// <summary>
    /// UI component cho replay mode - hiển thị thông tin ván và điều khiển replay
    /// </summary>
    public class GameReplayUI
    {
        /// <summary>
        /// Hiển thị thông tin cơ bản của ván cờ
        /// </summary>
        public static void DisplayGameInfo(MatchRecord record)
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              REPLAY GAME INFORMATION                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Event:        {record.Event ?? "Casual Game"}");
            Console.WriteLine($"Date:         {record.Date:yyyy.MM.dd}");
            Console.WriteLine($"White:        {record.WhitePlayer ?? "Unknown"}");
            Console.WriteLine($"Black:        {record.BlackPlayer ?? "Unknown"}");
            Console.WriteLine($"Result:       {record.Result ?? "*"}");
            if (!string.IsNullOrEmpty(record.TimeControl))
                Console.WriteLine($"Time Control: {record.TimeControl}");
            Console.WriteLine($"Total Moves:  {record.GetTotalMoves()}");
            Console.WriteLine();
        }

        /// <summary>
        /// Hiển thị vị trí hiện tại trong ván (move counter)
        /// </summary>
        public static void DisplayMoveCounter(int current, int total)
        {
            string counter = $"Move: {current + 1}/{total}";
            Console.SetCursorPosition(0, 0);
            Console.Write(counter.PadRight(30));
        }

        /// <summary>
        /// Hiển thị các lệnh điều khiển replay
        /// </summary>
        public static void DisplayReplayControls()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              REPLAY CONTROLS                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  ↑ / ← : Previous move                                  ║");
            Console.WriteLine("║  ↓ / → : Next move                                      ║");
            Console.WriteLine("║  Home   : Go to start (move 0)                          ║");
            Console.WriteLine("║  End    : Go to end (last move)                         ║");
            Console.WriteLine("║  Q      : Quit replay                                   ║");
            Console.WriteLine("║  G      : Go to specific move (enter move number)       ║");
            Console.WriteLine("║  E      : Export current position                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Đọc lệnh replay từ người dùng
        /// </summary>
        public static ReplayCommand GetReplayCommand()
        {
            try
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                return keyInfo.Key switch
                {
                    ConsoleKey.UpArrow => ReplayCommand.Previous,
                    ConsoleKey.LeftArrow => ReplayCommand.Previous,
                    ConsoleKey.DownArrow => ReplayCommand.Next,
                    ConsoleKey.RightArrow => ReplayCommand.Next,
                    ConsoleKey.Home => ReplayCommand.ToStart,
                    ConsoleKey.End => ReplayCommand.ToEnd,
                    ConsoleKey.Q => ReplayCommand.Quit,
                    ConsoleKey.G => ReplayCommand.GoToMove,
                    ConsoleKey.E => ReplayCommand.Export,
                    _ => ReplayCommand.None
                };
            }
            catch
            {
                return ReplayCommand.None;
            }
        }

        /// <summary>
        /// Yêu cầu người dùng nhập chỉ số move để jump tới
        /// </summary>
        public static int GetMoveIndex(int maxMoves)
        {
            Console.Write($"Enter move number (1-{maxMoves}): ");
            if (int.TryParse(Console.ReadLine(), out int moveNum) && moveNum >= 1 && moveNum <= maxMoves)
            {
                return moveNum - 1; // Convert to 0-based index
            }
            return -1;
        }

        /// <summary>
        /// Hiển thị danh sách moves hiện tại
        /// </summary>
        public static void DisplayMoveList(List<Move> moves, int currentIndex = -1)
        {
            if (moves == null || moves.Count == 0)
            {
                Console.WriteLine("No moves to display.");
                return;
            }

            Console.WriteLine("\n--- Move List ---");
            for (int i = 0; i < moves.Count; i++)
            {
                string moveStr = moves[i].San ?? $"{moves[i].From}-{moves[i].To}";
                string prefix = (i == currentIndex) ? "> " : "  ";
                int moveNumber = i + 1;
                
                if (i % 2 == 0)
                    Console.Write($"{prefix}{moveNumber}. {moveStr} ");
                else
                {
                    Console.WriteLine($"{moveStr}");
                }
            }
            if (moves.Count % 2 == 1)
                Console.WriteLine();
            Console.WriteLine("----------------\n");
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        public static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public static void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Hiển thị thông báo thông tin
        /// </summary>
        public static void DisplayInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Yêu cầu đường dẫn file để export
        /// </summary>
        public static string GetExportFilePath()
        {
            Console.Write("Enter export file path (with extension .pgn/.xqg/.json): ");
            return Console.ReadLine() ?? "";
        }
    }

    /// <summary>
    /// Các lệnh điều khiển replay
    /// </summary>
    public enum ReplayCommand
    {
        None,        // Lệnh không được nhận dạng
        Previous,    // Lùi một nước
        Next,        // Tiến một nước
        ToStart,     // Đi tới bắt đầu ván
        ToEnd,       // Đi tới cuối ván
        GoToMove,    // Đi tới move cụ thể
        Export,      // Export position
        Quit         // Thoát replay
    }
}
