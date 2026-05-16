// /Program.cs
using System;
using System.Text;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Chess;
using ChessXiangqiSolution.Core.Models.Xiangqi;
using ChessXiangqiSolution.UI;

namespace ChessXiangqiSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var gameType = ShowMenu();
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

            var app = new AppController(board, validator);
            app.Run();
        }

        private static GameSelection ShowMenu()
        {
            string[] options = { "Cờ vua (Chess)", "Cờ tướng (Xiangqi)" };
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