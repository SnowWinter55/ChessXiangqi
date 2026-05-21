// /Program.cs
using System;
using System.Text;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Chess;
using ChessXiangqiSolution.Core.Models.Xiangqi;
using ChessXiangqiSolution.Modules.Clock;
using ChessXiangqiSolution.UI;
using ChessXiangqiSolution.UI.ConsoleUI;

namespace ChessXiangqiSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Vòng lặp chính - cho phép quay lại menu sau mỗi ván
            while (true)
            {
                var gameType = ShowGameSelectionMenu();
                if (gameType == null)
                {
                    // Người dùng chọn thoát
                    break;
                }

                // Tạo board và validator tùy theo loại game đã chọn
                IBoard board;
                IMoveValidator validator;

                switch (gameType)
                {
                    case GameSelection.Chess:
                        board = new ChessBoard();
                        validator = new ChessMoveValidator();
                        break;
                    case GameSelection.Xiangqi:
                        board = new XiangqiBoard();
                        validator = new XiangqiMoveValidator();
                        break;
                    default:
                        board = new ChessBoard();
                        validator = new ChessMoveValidator();
                        break;
                }

                // Chọn cài đặt đồng hồ
                var clockSettings = ClockSelectionUI.SelectClockSettings();

                // Chạy trò chơi với clock settings đã chọn
                var app = new AppController(board, validator, clockSettings);
                app.Run();

                // Sau khi trò chơi kết thúc, quay lại menu chính
                Console.Clear();
                Console.WriteLine("\n✓ Ván đấu đã kết thúc. Quay lại menu chính...\n");
                System.Threading.Thread.Sleep(2000);
            }

            Console.Clear();
            Console.WriteLine("Cảm ơn bạn đã chơi ChoiCo!");
            Console.WriteLine("Hẹn gặp lại!");
        }

        /// <summary>Hiển thị menu chọn loại cờ</summary>
        private static GameSelection? ShowGameSelectionMenu()
        {
            string[] options = { "♟ Cờ vua", "帥 Cờ tướng", "❌ Thoát" };
            int selectedIndex = 0;

            Console.Clear();
            Console.WriteLine("=== CHỌN LOẠI CỜ ===");
            Console.WriteLine("Dùng phím ↑/↓ để chọn, Enter để xác nhận.\n");

            while (true)
            {
                // In menu
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Length;
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        if (selectedIndex == 2)
                        {
                            // Thoát
                            return null;
                        }
                        return selectedIndex == 0 ? GameSelection.Chess : GameSelection.Xiangqi;
                }

                // Đưa con trỏ về đầu menu (dòng thứ 3) để ghi đè
                Console.SetCursorPosition(0, 3);
            }
        }

        private enum GameSelection
        {
            Chess,
            Xiangqi
        }
    }
}