using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    public static class SanFormatter
    {
        public static string FormatSan(Move move, IBoard board)
        {
            if (!string.IsNullOrEmpty(move.San))
                return move.San;

            if (move.IsCastling)
                return move.To.Col == 6 ? "O-O" : "O-O-O";

            var piece = board.GetPieceAt(move.From);
            if (piece == null)
                return "???";

            if (board.GameType == GameType.Xiangqi)
                return FormatXiangqiSan(move, piece, board);

            string pieceChar = piece.Type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                _ => string.Empty
            };

            bool isCapture = move.CapturedPiece != null || move.IsEnPassant;
            string capture = isCapture ? "x" : string.Empty;
            string toSquare = move.To.ToChessAlgebraic();
            string promotion = move.PromotionPiece.HasValue ? "=" + move.PromotionPiece.Value.ToString()[0] : string.Empty;

            if (piece.Type == PieceType.Pawn)
            {
                if (isCapture)
                    return $"{move.From.ToChessAlgebraic()[0]}{capture}{toSquare}{promotion}";
                return $"{toSquare}{promotion}";
            }

            string disambiguation = GetDisambiguation(board, piece, move);
            return $"{pieceChar}{disambiguation}{capture}{toSquare}{promotion}";
        }

        private static string GetDisambiguation(IBoard board, IPiece movingPiece, Move move)
        {
            var fromSquare = move.From.ToChessAlgebraic();
            var sameTypePieces = board.GetPiecesByColor(movingPiece.Color)
                .Where(p => p.Piece.Type == movingPiece.Type && !p.Pos.Equals(move.From))
                .Where(p => p.Piece.GetValidMoves(board, p.Pos).Any(dest => dest.Equals(move.To)))
                .ToList();

            if (!sameTypePieces.Any())
                return string.Empty;

            bool sameFileExists = sameTypePieces.Any(p => p.Pos.ToChessAlgebraic()[0] == fromSquare[0]);
            bool sameRankExists = sameTypePieces.Any(p => p.Pos.ToChessAlgebraic()[1] == fromSquare[1]);

            if (!sameFileExists)
                return fromSquare[0].ToString();
            if (!sameRankExists)
                return fromSquare[1].ToString();

            return fromSquare;
        }

        /// <summary>
        /// Format nước đi cờ tướng sang ký hiệu SAN tiếng Việt
        /// Ví dụ: X5+3 (Xe từ cột 5 tiến 3 bước), M4=7 (Mã từ cột 4 đi ngang cột 7)
        /// </summary>
        private static string FormatXiangqiSan(Move move, IPiece piece, IBoard board)
        {
            // 1. Ký hiệu quân
            string pieceChar = GetXiangqiPieceChar(piece.Type);
            
            // 2. Chuyển cột nội bộ (0-based) sang cột người dùng (1-9)
            int userFromCol = InternalColToUser(move.From.Col, piece.Color);
            int userToCol = InternalColToUser(move.To.Col, piece.Color);
            
            // 3. Xác định hướng di chuyển
            char dirChar = GetXiangqiDirection(move.From, move.To, piece.Color);
            
            // 4. Tính toNum (cột đích hoặc số bước)
            string toNum = CalculateXiangqiToNum(piece.Type, move, piece.Color, userFromCol, userToCol);
            
            // 5. Kiểm tra disambiguation (khi có nhiều quân cùng loại cùng cột)
            string disambig = GetXiangqiDisambiguation(board, piece, move, userFromCol);
            
            // 6. Ký hiệu bắt quân
            string capture = move.CapturedPiece != null ? "x" : "-";
            
            return $"{pieceChar}{disambig}{userFromCol}{dirChar}{toNum}{capture}";
        }

        /// <summary>
        /// Chuyển PieceType sang ký hiệu tiếng Việt cho cờ tướng
        /// </summary>
        private static string GetXiangqiPieceChar(PieceType type)
        {
            return type switch
            {
                PieceType.General => "Tg",
                PieceType.Advisor => "S",
                PieceType.Elephant => "T",
                PieceType.Chariot => "X",
                PieceType.Cannon => "P",
                PieceType.Horse => "M",
                PieceType.Soldier => "B",
                _ => "?"
            };
        }

        /// <summary>
        /// Xác định ký tự hướng di chuyển (+, -, =) cho cờ tướng
        /// </summary>
        private static char GetXiangqiDirection(Position from, Position to, Color color)
        {
            int rowDiff = to.Row - from.Row;
            
            // Với White: row nhỏ = phía trước (tiến), row lớn = phía sau (lui)
            // Với Black: row lớn = phía trước (tiến), row nhỏ = phía sau (lui)
            bool isForward = (color == Color.White && rowDiff < 0) || (color == Color.Black && rowDiff > 0);
            bool isBackward = (color == Color.White && rowDiff > 0) || (color == Color.Black && rowDiff < 0);
            bool isSideways = rowDiff == 0;
            
            if (isForward) return '+';
            if (isBackward) return '-';
            if (isSideways) return '=';
            
            return '?';
        }

        /// <summary>
        /// Tính toNum cho SAN cờ tướng (cột đích hoặc số bước)
        /// </summary>
        private static string CalculateXiangqiToNum(PieceType type, Move move, Color color, int userFromCol, int userToCol)
        {
            // Xe/Pháo/Binh đi ngang: cột đích
            if (move.From.Row == move.To.Row)
                return userToCol.ToString();
            
            // Xe/Pháo tiến/lui: số bước
            if (type == PieceType.Chariot || type == PieceType.Cannon)
                return Math.Abs(move.To.Row - move.From.Row).ToString();
            
            // Binh tiến: số bước (thường là 1)
            if (type == PieceType.Soldier)
                return Math.Abs(move.To.Row - move.From.Row).ToString();
            
            // Mã/Tượng/Sĩ/Tướng: cột đích
            return userToCol.ToString();
        }

        /// <summary>
        /// Lấy disambiguation nếu có nhiều quân cùng loại cùng cột
        /// Trả về (hN) nếu cần chỉ định hàng, hoặc rỗng nếu không cần
        /// </summary>
        private static string GetXiangqiDisambiguation(IBoard board, IPiece movingPiece, Move move, int userFromCol)
        {
            // Lọc tất cả quân cùng loại cùng cột
            int internalCol = UserColToInternal(userFromCol, movingPiece.Color);
            var sameTypeAndCol = board.GetPiecesByColor(movingPiece.Color)
                .Where(x => x.Piece.Type == movingPiece.Type && x.Pos.Col == internalCol)
                .ToList();
            
            // Nếu chỉ có 1 quân, không cần disambiguation
            if (sameTypeAndCol.Count <= 1)
                return string.Empty;
            
            // Chỉ định hàng theo góc nhìn người chơi
            int userRow = InternalRowToUser(move.From.Row, movingPiece.Color, board.Rows);
            return $"(h{userRow})";
        }

        /// <summary>
        /// Chuyển cột người dùng (1-9) sang cột nội bộ (0-8)
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// user col 1-9 → internal col 0-8.
        /// </summary>
        private static int UserColToInternal(int userCol, Color color)
        {
            return userCol - 1;
        }

        /// <summary>
        /// Chuyển cột nội bộ (0-8) sang cột người dùng (1-9)
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// internal col 0-8 → user col 1-9.
        /// </summary>
        private static int InternalColToUser(int internalCol, Color color)
        {
            return internalCol + 1;
        }

        /// <summary>
        /// Chuyển hàng nội bộ (0-based) sang hàng người dùng (1-10 theo góc nhìn người chơi)
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// internal row 0-9 → user row 10-1 (row 0 = hàng 10, row 9 = hàng 1).
        /// </summary>
        private static int InternalRowToUser(int internalRow, Color color, int totalRows)
        {
            return totalRows - internalRow;
        }
    }
}
