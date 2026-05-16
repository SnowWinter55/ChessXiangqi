// Module/Movement/MoveParser.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Movement
{
    /// <summary>
    /// Parse nước đi từ chuỗi nhập (SAN hoặc tọa độ như e2e4).
    /// Trả về Move object nếu thành công.
    /// </summary>
    public static class MoveParser
    {
        /// <summary>
        /// Parse input string thành Move.
        /// Hỗ trợ:
        /// - Tọa độ: e2e4, e7e5, g1f3
        /// - SAN: e4, Nf3, Bb5+, O-O, O-O-O, exd5, Nxd4, ...
        /// </summary>
        /// <param name="input">Chuỗi nhập từ người dùng</param>
        /// <param name="board">Bàn cờ hiện tại (để tìm quân)</param>
        /// <param name="currentTurn">Lượt đi hiện tại</param>
        /// <param name="move">Nước đi kết quả (nếu parse thành công)</param>
        /// <returns>True nếu parse thành công</returns>
        public static bool TryParse(string input, IBoard board, Color currentTurn, out Move move)
        {
            move = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();

            // Thử parse dạng tọa độ "e2e4"
            if (TryParseCoordinate(input, out move))
                return true;

            // Thử parse dạng SAN
            if (TryParseSan(input, board, currentTurn, out move))
                return true;

            return false;
        }

        private static bool TryParseCoordinate(string input, out Move move)
        {
            move = null;
            if (input.Length != 4) return false;
            try
            {
                string fromStr = input.Substring(0, 2);
                string toStr = input.Substring(2, 2);
                Position from = Position.FromChessAlgebraic(fromStr);
                Position to = Position.FromChessAlgebraic(toStr);
                move = new Move(from, to);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseSan(string san, IBoard board, Color currentTurn, out Move move)
        {
            move = null;
            // Chuyển toàn bộ chuỗi nhập thành chữ HOA để dễ xử lý
            string upperSan = san.ToUpperInvariant();
            string clean = Regex.Replace(upperSan, @"[+#!?]", "");

            // Nhập thành: O-O hoặc O-O-O (có thể dùng số 0 thay chữ O)
            if (clean == "O-O" || clean == "0-0" || clean == "o-o")
                return TryParseCastling(board, currentTurn, false, out move);
            if (clean == "O-O-O" || clean == "0-0-0" || clean == "o-o-o")
                return TryParseCastling(board, currentTurn, true, out move);

            // Regex khớp với chữ hoa cho quân (KQRNB) và file (A-H), số (1-8)
            var match = Regex.Match(clean, @"^([KQRNB])?([A-H])?([1-8])?X?([A-H][1-8])$");
            if (!match.Success) return false;

            string pieceLetter = match.Groups[1].Value;
            string fromFile = match.Groups[2].Value;   // Ví dụ "E"
            string fromRank = match.Groups[3].Value;   // Ví dụ "2"
            string toSquare = match.Groups[4].Value;   // Ví dụ "E5"

            // Chuyển ô đích sang dạng chữ thường cho Position.FromChessAlgebraic
            string toSquareLower = toSquare[0].ToString().ToLowerInvariant() + toSquare[1];
            Position to;
            try { to = Position.FromChessAlgebraic(toSquareLower); }
            catch { return false; }

            PieceType pieceType = pieceLetter switch
            {
                "K" => PieceType.King,
                "Q" => PieceType.Queen,
                "R" => PieceType.Rook,
                "N" => PieceType.Knight,
                "B" => PieceType.Bishop,
                _ => PieceType.Pawn
            };

            var candidates = new List<(Position pos, IPiece piece)>();
            foreach (var (pos, piece) in board.GetPiecesByColor(currentTurn))
            {
                if (piece.Type != pieceType) continue;

                // Kiểm tra từ file/rank (không phân biệt hoa/thường)
                if (!string.IsNullOrEmpty(fromFile))
                {
                    char expectedFile = char.ToLowerInvariant(fromFile[0]);
                    if (pos.ToChessAlgebraic()[0] != expectedFile) continue;
                }
                if (!string.IsNullOrEmpty(fromRank))
                {
                    char expectedRank = fromRank[0];
                    if (pos.ToChessAlgebraic()[1] != expectedRank) continue;
                }

                var moves = piece.GetValidMoves(board, pos);
                if (moves.Any(m => m.Equals(to)))
                    candidates.Add((pos, piece));
            }

            if (candidates.Count != 1) return false;

            move = new Move(candidates[0].pos, to);
            if (pieceType == PieceType.Pawn && board.IsEmpty(to) && Math.Abs(move.From.Col - move.To.Col) == 1)
            {
                move.IsEnPassant = true;
                move.CapturedPiece = board.GetPieceAt(new Position(move.From.Row, move.To.Col));
            }
            else
            {
                move.CapturedPiece = board.GetPieceAt(to);
            }

            return true;
        }

        private static bool TryParseCastling(IBoard board, Color currentTurn, bool isQueenSide, out Move move)
        {
            move = null;
            int backRow = (currentTurn == Color.White) ? 7 : 0;
            int kingCol = 4;
            Position kingFrom = new Position(backRow, kingCol);
            Position kingTo = isQueenSide ? new Position(backRow, 2) : new Position(backRow, 6);
            var king = board.GetPieceAt(kingFrom);
            if (king?.Type != PieceType.King || king.Color != currentTurn) return false;
            move = new Move(kingFrom, kingTo);
            move.IsCastling = true;
            return true;
        }
    }
}