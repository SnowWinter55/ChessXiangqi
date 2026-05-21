using System;
using System.Linq;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.UI.ConsoleUI
{
    public class BoardRenderer
    {
        // Màu nền cho cờ vua (giữ nguyên)
        private readonly ConsoleColor _lightSquareBg = ConsoleColor.Gray;
        private readonly ConsoleColor _darkSquareBg = ConsoleColor.DarkBlue;

        // Màu nền cho cờ tướng
        private readonly ConsoleColor _xiangqiBg1 = ConsoleColor.DarkGray;   // nền ô thường
        private readonly ConsoleColor _xiangqiBg2 = ConsoleColor.Gray;      // nền ô thường xen kẽ
        private readonly ConsoleColor _palaceBg = ConsoleColor.Red;        // nền cung thành
        private readonly ConsoleColor _riverBg = ConsoleColor.DarkCyan;           // nền dòng sông
        private static int GetDisplayWidth(string s)
        {
            int width = 0;
            foreach (char c in s)
            {
                // CJK Unified Ideographs + các block full-width phổ biến
                if ((c >= 0x1100 && c <= 0x115F)   // Hangul
                || (c >= 0x2E80 && c <= 0x303E)   // CJK Radicals
                || (c >= 0x3040 && c <= 0xA4CF)   // CJK, Hiragana, Katakana...
                || (c >= 0xAC00 && c <= 0xD7AF)   // Hangul Syllables
                || (c >= 0xF900 && c <= 0xFAFF)   // CJK Compatibility
                || (c >= 0xFE10 && c <= 0xFE19)
                || (c >= 0xFE30 && c <= 0xFE6F)
                || (c >= 0xFF00 && c <= 0xFF60)   // Fullwidth Forms
                || (c >= 0xFFE0 && c <= 0xFFE6))
                    width += 2;
                else
                    width += 1;
            }
            return width;
        }
        private void WriteCell(string symbol, int cellWidth)
        {
            Console.Write(symbol);
            int padding = cellWidth - GetDisplayWidth(symbol);
            if (padding > 0)
                Console.Write(new string(' ', padding));
        }
        public void Render(IBoard board, Color currentTurn, int whiteTime, int blackTime, List<string> moveHistorySAN)
        {
            Console.Clear();
            Console.WriteLine($"Lượt: {(currentTurn == Color.White ? "Trắng" : "Đen")}   🕒 Trắng: {FormatTime(whiteTime)}  Đen: {FormatTime(blackTime)}\n");

            if (board.GameType == GameType.Xiangqi)
                RenderXiangqiBoard(board);
            else
                RenderChessBoard(board);

            RenderMoveHistory(moveHistorySAN);
        }

        // --- Bàn cờ tướng (Xiangqi) ---
        private void RenderXiangqiBoard(IBoard board)
        {
            int rows = board.Rows;   // = 10
            int cols = board.Cols;   // = 9

            for (int r = 0; r < rows; r++)
            {
                // Nhãn hàng (từ 10 đến 1)
                string rankLabel = (rows - r).ToString();
                Console.Write($"{rankLabel,-3}");

                for (int c = 0; c < cols; c++)
                {
                    var piece = board.GetPieceAt(new Position(r, c));
                    string symbol = GetPieceSymbol(piece);

                    bool isRiverRow = r == 4 || r == 5;
                    bool isInPalace = (r <= 2 && c >= 3 && c <= 5) || (r >= 7 && c >= 3 && c <= 5);

                    if (isRiverRow)
                        Console.BackgroundColor = _riverBg;
                    else if (isInPalace)
                        Console.BackgroundColor = _palaceBg;
                    else
                        Console.BackgroundColor = ((r + c) % 2 == 0) ? _xiangqiBg1 : _xiangqiBg2;

                    if (piece != null)
                        Console.ForegroundColor = piece.Color == Color.White ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;

                    // Sau: (space đầu + symbol + padding tự động)
                    Console.Write(" ");
                    WriteCell(symbol, 2);  // cellWidth=2: symbol chiếm đúng 2 cột, cộng space đầu = 3 tổng
                    Console.ResetColor();
                }

                // Nhãn hàng bên phải
                Console.WriteLine($" {rankLabel}");
            }

            // Nhãn cột (1..9)
            Console.Write("   ");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($" {c + 1} ");
            }
            Console.WriteLine("\n");
        }

        // --- Bàn cờ vua (cũ, giữ nguyên) ---
        private void RenderChessBoard(IBoard board)
        {
            int rows = board.Rows;
            int cols = board.Cols;
            bool isChess = true;

            for (int r = 0; r < rows; r++)
            {
                string rankLabel = isChess ? (8 - r).ToString() : (rows - r).ToString();
                Console.Write($"{rankLabel,-3}");

                for (int c = 0; c < cols; c++)
                {
                    var piece = board.GetPieceAt(new Position(r, c));
                    string symbol = GetPieceSymbol(piece);
                    bool isLightSquare = (r + c) % 2 == 0;
                    Console.BackgroundColor = isLightSquare ? _lightSquareBg : _darkSquareBg;
                    Console.ForegroundColor = piece != null ? (piece.Color == Color.White ? ConsoleColor.White : ConsoleColor.Black) : ConsoleColor.Gray;
                    Console.Write($" {symbol}  ");
                    Console.ResetColor();
                }

                Console.WriteLine($" {rankLabel}");
            }

            Console.Write("   ");
            for (int c = 0; c < cols; c++)
            {
                string fileLabel = isChess ? ((char)('a' + c)).ToString() : (c + 1).ToString();
                Console.Write($" {fileLabel}  ");
            }
            Console.WriteLine("\n");
        }

        private void RenderMoveHistory(List<string> moveHistorySAN)
        {
            Console.WriteLine("--- Lịch sử nước đi ---");
            if (moveHistorySAN == null || moveHistorySAN.Count == 0)
            {
                Console.WriteLine("(chưa có nước nào)");
            }
            else
            {
                for (int i = 0; i < moveHistorySAN.Count; i++)
                {
                    int moveNum = i / 2 + 1;
                    if (i % 2 == 0)
                        Console.Write($"{moveNum}. {moveHistorySAN[i]} ");
                    else
                        Console.WriteLine($"{moveHistorySAN[i]}");
                }
                if (moveHistorySAN.Count % 2 == 1)
                    Console.WriteLine();
            }
            Console.WriteLine();
        }

        private string GetPieceSymbol(IPiece piece)
        {
            if (piece == null) return " ";
            return piece.Type switch
            {
                // Cờ vua
                PieceType.Pawn => "♙",
                PieceType.Rook => "♖",
                PieceType.Knight => "♘",
                PieceType.Bishop => "♗",
                PieceType.Queen => "♕",
                PieceType.King => "♔",
                // Cờ tướng
                PieceType.Soldier => "兵",
                PieceType.Cannon => "炮",
                PieceType.Chariot => "車",
                PieceType.Horse => "馬",
                PieceType.Elephant => "象",
                PieceType.Advisor => "士",
                PieceType.General => "帥",
                _ => "?"
            };
        }

        private string FormatTime(int seconds)
        {
            int min = seconds / 60;
            int sec = seconds % 60;
            return $"{min:D2}:{sec:D2}";
        }
    }
}