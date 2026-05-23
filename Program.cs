// /Program.cs
using System;
using System.IO;
using System.Text;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Chess;
using ChessXiangqiSolution.Core.Models.Xiangqi;
using ChessXiangqiSolution.Modules.Clock;
using ChessXiangqiSolution.Modules.Notation;
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

                // Show game mode menu (Play new or Load game)
                var gameMode = ShowGameModeMenu();
                if (gameMode == null)
                {
                    // User selected back, return to root menu
                    continue;
                }

                if (gameMode == GameMode.PlayNew)
                {
                    // Select clock settings for new game
                    var clockSettings = ClockSelectionUI.SelectClockSettings();
                    if (clockSettings == null)
                    {
                        // User cancelled clock selection, back to game mode menu
                        continue;
                    }
                    var app = new AppController(board, validator, clockSettings);
                    app.Run();
                }
                else if (gameMode == GameMode.LoadGame)
                {
                    // Load and replay game from file
                    string filePath = PromptForGameFile();
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        var app = new AppController(board, validator, GameRecordsManager.GetDefaultClockSettings());
                        app.ReplayGameFromFile(filePath);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("File not found or operation cancelled.");
                        System.Threading.Thread.Sleep(2000);
                    }
                }

                // After game ends, return to root menu
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
                // Display menu
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
                            // Exit
                            return null;
                        }
                        return selectedIndex == 0 ? GameSelection.Chess : GameSelection.Xiangqi;
                }

                // Đưa con trỏ về đầu menu (dòng thứ 3) để ghi đè
                Console.SetCursorPosition(0, 3);
            }
        }

        /// <summary>Prompt user for game file path with auto-parse functionality</summary>
        private static string PromptForGameFile()
        {
            Console.Clear();
            Console.WriteLine("=== LOAD GAME ===");
            Console.WriteLine($"Game records location: {GameRecordsManager.GetGameRecordsPath()}");
            Console.WriteLine("Supported formats: .pgn (Chess), .xqg (Xiangqi), .json\n");
            
            // Display available game records with file sizes
            GameRecordsManager.DisplayAvailableGameRecords();

            Console.Write("Enter filename or path (or leave empty to cancel): ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return null;

            // Auto-parse and resolve file path
            string filePath = GameRecordsManager.ResolveGameFilePath(input);

            // Validate the file exists
            if (!GameRecordsManager.ValidateGameFilePath(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                System.Threading.Thread.Sleep(2000);
                return null;
            }

            return filePath;
        }

        /// <summary>Display game mode menu (Play New or Load Game)</summary>
        private static GameMode? ShowGameModeMenu()
        {
            string[] options = { "▶ Play New Game", "📂 Load Game from File", "⬅ Back to Game Selection" };
            int selectedIndex = 0;

            Console.Clear();
            Console.WriteLine("=== SELECT GAME MODE ===");
            Console.WriteLine("Use ↑/↓ arrows to select, Enter to confirm.\n");

            while (true)
            {
                // Display menu
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
                            // Back to game selection
                            return null;
                        }
                        return selectedIndex == 0 ? GameMode.PlayNew : GameMode.LoadGame;
                }

                // Move cursor back to menu start to overwrite
                Console.SetCursorPosition(0, 3);
            }
        }

        private enum GameSelection
        {
            Chess,
            Xiangqi
        }

        private enum GameMode
        {
            PlayNew,
            LoadGame
        }
    }
}
        
    
