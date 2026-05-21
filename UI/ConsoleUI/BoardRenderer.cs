using System;
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
        private readonly ConsoleColor _xiangqiNormalBg = ConsoleColor.DarkGray;   // nền ô thường
        private readonly ConsoleColor _palaceBg = ConsoleColor.DarkYellow;        // nền cung thành
        private readonly ConsoleColor _riverBg = ConsoleColor.DarkBlue;           // nền dòng sông

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

                    // Chọn màu nền: nếu ô nằm trong cung thành của một bên
                    bool isInPalace = (r <= 2 && c >= 3 && c <= 5) || (r >= 7 && c >= 3 && c <= 5);
                    Console.BackgroundColor = isInPalace ? _palaceBg : _xiangqiNormalBg;

                    // Màu chữ: quân trắng (đỏ) -> Đỏ, quân đen -> Xanh lơ
                    if (piece != null)
                        Console.ForegroundColor = piece.Color == Color.White ? ConsoleColor.Red : ConsoleColor.Cyan;
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write($" {symbol}  ");
                    Console.ResetColor();
                }

                // Nhãn hàng bên phải
                Console.WriteLine($" {rankLabel}");

                // Sau khi vẽ hàng thứ 4 (chỉ số 3) thì vẽ dòng sông
                if (r == 3)
                {
                    DrawRiver();
                }
            }

            // Nhãn cột (1..9)
            Console.Write("   ");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($" {(c + 1)}  ");
            }
            Console.WriteLine("\n");
        }

        private void DrawRiver()
        {
            // Canh lề: 3 ký tự cho rankLabel bên trái, sau đó 36 ký tự cho 9 ô (mỗi ô 4 ký tự)
            Console.Write("   ");  // thay cho rankLabel
            Console.BackgroundColor = _riverBg;
            Console.ForegroundColor = ConsoleColor.White;
            // Dòng chữ đại diện cho sông, dài khoảng 36 ký tự
            Console.Write("  ~~~~~~~~~~~~ SÔNG ~~~~~~~~~~~~  ");
            Console.ResetColor();
            Console.WriteLine("   "); // căn lề phải (rankLabel ảo)
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