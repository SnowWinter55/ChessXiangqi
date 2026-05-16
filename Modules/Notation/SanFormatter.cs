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
    }
}
