// /UI/ConsoleUI/InputHandler.cs
using System;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Modules.Movement;
using ChessXiangqiSolution.Core.Enums;

namespace ChessXiangqiSolution.UI.ConsoleUI
{
    public class InputHandler
    {
        private string _currentInput = "";
        private bool _awaitingInput = true;

        /// <summary>
        /// Đọc lệnh hoặc nước đi từ bàn phím, hỗ trợ phím tắt Ctrl+Z, Ctrl+Y, Enter.
        /// Trả về chuỗi lệnh (nếu là lệnh đặc biệt) hoặc chuỗi nước đi (nếu nhập xong và nhấn Enter).
        /// </summary>
        public string GetCommandOrMove()
        {
            _currentInput = "";
            _awaitingInput = true;
            Console.Write("Nước đi: ");

            while (_awaitingInput)
            {
                var key = Console.ReadKey(intercept: true); // intercept true để không hiển thị phím điều khiển
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(); // xuống dòng
                    _awaitingInput = false;
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (_currentInput.Length > 0)
                    {
                        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                        Console.Write("\b \b"); // xóa ký tự trên console
                    }
                }
                else if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    // Xử lý Ctrl+Z (Undo)
                    if (key.Key == ConsoleKey.Z)
                    {
                        Console.WriteLine(); // xuống dòng để tránh lỗi hiển thị
                        return "undo";
                    }
                    // Xử lý Ctrl+Y (Redo)
                    else if (key.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine();
                        return "redo";
                    }
                    // Có thể thêm Ctrl+C để thoát
                    else if (key.Key == ConsoleKey.C)
                    {
                        Console.WriteLine();
                        return "quit";
                    }
                    else
                    {
                        // Các tổ hợp phím khác thì bỏ qua, không hiển thị
                        continue;
                    }
                }
                else if (IsValidInputChar(key.KeyChar))
                {
                    _currentInput += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
                // Bỏ qua các phím chức năng khác (F1, Home, End...)
            }

            return _currentInput.Trim().ToLower();
        }

        // Giữ phương thức cũ để tương thích nếu cần, nhưng khuyến khích dùng GetCommandOrMove
        public string GetInput() => GetCommandOrMove();

        public bool TryParseMove(string input, IBoard board, Color currentTurn, out Move move)
        {
            return MoveParser.TryParse(input, board, currentTurn, out move);
        }

        /// <summary>
        /// Kiểm tra ký tự có hợp lệ để nhập nước đi không.
        /// Hỗ trợ: chữ cái, số, dấu +, -, =, (, ), x, h
        /// </summary>
        private bool IsValidInputChar(char c)
        {
            // Chữ cái, số
            if (char.IsLetterOrDigit(c))
                return true;

            // Ký hiệu đặc biệt cho cờ tướng: +, -, =, (, ), x, h
            return c switch
            {
                '+' => true,  // Tiến
                '-' => true,  // Lui
                '=' => true,  // Bằng (ngang)
                '(' => true,  // Bắt đầu disambiguator
                ')' => true,  // Kết thúc disambiguator
                'x' => true,  // Bắt quân
                'X' => true,  // Bắt quân (hoa)
                'h' => true,  // Hàng (trong disambiguator)
                'H' => true,  // Hàng (hoa)
                _ => false
            };
        }

        /// <summary>
        /// Kiểm tra xem input có phải là nước đi cờ tướng hợp lệ không.
        /// Format: [Quân][Disambig?][Cột][Hướng][ToNum]
        /// </summary>
        public bool IsValidXiangqiInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Regex cơ bản: quân + cột + hướng + số
            var pattern = @"^([Tt]g|[SsTtXxPpMmBb])(\([hH]\d{1,2}\)|[ts])?[1-9][+\-=][1-9][x\-]?$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// Kiểm tra xem input có phải là tọa độ tuyệt đối hợp lệ không.
        /// Format: hàng,cột - hàng,cột hoặc e2e4 (cờ vua)
        /// </summary>
        public bool IsValidCoordinateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Tọa độ cờ tướng: \d,\d - \d,\d hoặc tương tự
            var xiangqiPattern = @"^\d{1,2},\d{1,2}\s*[-\s]\s*\d{1,2},\d{1,2}$";
            if (System.Text.RegularExpressions.Regex.IsMatch(input, xiangqiPattern))
                return true;

            // Tọa độ cờ vua: e2e4
            var chessPattern = @"^[a-h][1-8][a-h][1-8]$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, chessPattern);
        }

        /// <summary>
        /// Hỗ trợ nhập với gợi ý format tùy theo loại trò chơi
        /// </summary>
        public void DisplayInputHint(GameType gameType)
        {
            if (gameType == GameType.Xiangqi)
            {
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine(  "║ Cờ Tướng - Format nước đi:                                 ║");
                Console.WriteLine(  "║ • SAN: X5+3 (Xe từ cột 5 tiến 3 bước)                      ║");
                Console.WriteLine(  "║ • SAN: M4=7 (Mã từ cột 4 đi ngang cột 7)                   ║");
                Console.WriteLine(  "║ • Tọa độ: 5,5 - 5,6 (từ hàng 5 cột 5 đến hàng 5 cột 6)     ║");
                Console.WriteLine(  "║ Quân: Tg/S/T/X/P/M/B | Hướng: +/- /= | Phím: Ctrl+Z/Y/C    ║");
                Console.WriteLine(  "╚════════════════════════════════════════════════════════════╝");
            }
            else
            {
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine(  "║ Cờ Vua - Format nước đi:                                   ║");
                Console.WriteLine(  "║ • SAN: Nf3 (Mã đến f3), exd5 (Tốt ăn quân ở d5)            ║");
                Console.WriteLine(  "║ • Tọa độ: e2e4 (từ e2 đến e4)                              ║");
                Console.WriteLine(  "║ • Nhập: O-O (Nhập thành), O-O-O (Nhập thành rộng)          ║");
                Console.WriteLine(  "║ Phím: Ctrl+Z/Y/C (Undo/Redo/Quit)                          ║");
                Console.WriteLine(  "╚════════════════════════════════════════════════════════════╝");
            }
        }
    }
}