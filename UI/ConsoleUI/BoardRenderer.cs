using System;
using System.Collections.Generic;
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

            RenderBoard(board);
            RenderMoveHistory(moveHistorySAN);
        }

        private void RenderBoard(IBoard board)
        {
            int rows = board.Rows;
            int cols = board.Cols;
            bool isChess = board.GameType == GameType.Chess;

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
            if (piece == null) return " ";   // Ẩn dấu chấm, chỉ để trống
            return piece.Type switch
            {
                PieceType.Pawn => "♙",
                PieceType.Rook => "♖",
                PieceType.Knight => "♘",
                PieceType.Bishop => "♗",
                PieceType.Queen => "♕",
                PieceType.King => "♔",
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