using System;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.UI.ConsoleUI
{
    public class BoardRenderer
    {
        // Màu nền ô sáng (thay cho ô trắng truyền thống)
        private readonly ConsoleColor _lightSquareBg = ConsoleColor.Gray;
        // Màu nền ô tối (thay cho ô đen truyền thống)
        private readonly ConsoleColor _darkSquareBg = ConsoleColor.DarkBlue;

        public void Render(IBoard board, Color currentTurn, int whiteTime, int blackTime, List<string> moveHistorySAN)
        {
            Console.Clear();
            Console.WriteLine($"Lượt: {(currentTurn == Color.White ? "Trắng" : "Đen")}   🕒 Trắng: {FormatTime(whiteTime)}  Đen: {FormatTime(blackTime)}\n");

            // Vẽ bàn cờ
            for (int r = 0; r < 8; r++)
            {
                Console.Write($"{8 - r}  ");
                for (int c = 0; c < 8; c++)
                {
                    var piece = board.GetPieceAt(new Position(r, c));
                    string symbol = GetPieceSymbol(piece);
                    bool isLightSquare = (r + c) % 2 == 0;
                    Console.BackgroundColor = isLightSquare ? _lightSquareBg : _darkSquareBg;
                    Console.ForegroundColor = piece != null ? (piece.Color == Color.White ? ConsoleColor.White : ConsoleColor.Black) : ConsoleColor.Gray;
                    Console.Write($" {symbol}  ");
                    Console.ResetColor();
                }
                Console.WriteLine($" {8 - r}");
            }
            Console.Write("   ");
            for (char c = 'a'; c <= 'h'; c++) Console.Write($" {c}  ");
            Console.WriteLine("\n");

            // In lịch sử nước đi
            Console.WriteLine("--- Lịch sử nước đi ---");
            if (moveHistorySAN.Count == 0)
                Console.WriteLine("(chưa có nước nào)");
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
            if (piece == null) return " ";   // Ẩn dấu chấm, chỉ để trống
            return piece.Type switch
            {
                PieceType.Pawn => "♙",
                PieceType.Rook => "♖",
                PieceType.Knight => "♘",
                PieceType.Bishop => "♗",
                PieceType.Queen => "♕",
                PieceType.King => "♔",
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