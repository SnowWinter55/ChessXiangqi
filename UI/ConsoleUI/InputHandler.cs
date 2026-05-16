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
                else if (char.IsLetterOrDigit(key.KeyChar) || key.KeyChar == '-')
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
    }
}