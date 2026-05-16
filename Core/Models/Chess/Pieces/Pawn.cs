// /Core/Models/Chess/Pieces/Pawn.cs
using System;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Chess.Pieces
{
    public class Pawn : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Pawn;

        public Pawn(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int direction = (Color == Color.White) ? -1 : 1; // White đi lên (giảm Row), Black đi xuống (tăng Row)
            int startRow = (Color == Color.White) ? 6 : 1;
            int promotionRow = (Color == Color.White) ? 0 : 7;

            // Tiến 1 ô
            Position oneStep = new Position(current.Row + direction, current.Col);
            if (board.IsValidPos(oneStep) && board.IsEmpty(oneStep))
            {
                moves.Add(oneStep);
                // Tiến 2 ô lần đầu
                if (current.Row == startRow)
                {
                    Position twoStep = new Position(current.Row + direction * 2, current.Col);
                    if (board.IsValidPos(twoStep) && board.IsEmpty(twoStep))
                        moves.Add(twoStep);
                }
            }

            // Ăn chéo trái
            Position leftDiag = new Position(current.Row + direction, current.Col - 1);
            if (board.IsValidPos(leftDiag) && !board.IsEmpty(leftDiag) && board.GetPieceAt(leftDiag).Color != Color)
                moves.Add(leftDiag);

            // Ăn chéo phải
            Position rightDiag = new Position(current.Row + direction, current.Col + 1);
            if (board.IsValidPos(rightDiag) && !board.IsEmpty(rightDiag) && board.GetPieceAt(rightDiag).Color != Color)
                moves.Add(rightDiag);

            // En passant (chỉ nếu board hỗ trợ IChessBoard)
            if (board is IChessBoard chessBoard && chessBoard.LastMove != null)
            {
                var last = chessBoard.LastMove;
                var lastPiece = chessBoard.GetPieceAt(last.To);
                if (lastPiece is Pawn && Math.Abs(last.From.Row - last.To.Row) == 2) // Đối phương vừa đi 2 ô
                {
                    // Vị trí tốt đối phương đứng sau khi đi 2 ô
                    if (last.To.Row == current.Row && Math.Abs(last.To.Col - current.Col) == 1)
                    {
                        // Ô bắt en passant
                        Position capturePos = new Position(current.Row + direction, last.To.Col);
                        if (board.IsValidPos(capturePos))
                            moves.Add(capturePos);
                    }
                }
            }

            // Ghi chú: Promotion sẽ được xử lý trong MoveValidator hoặc Board khi thực hiện nước đi
            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            var attacks = new List<Position>();
            int direction = (Color == Color.White) ? -1 : 1;

            Position leftDiag = new Position(current.Row + direction, current.Col - 1);
            if (board.IsValidPos(leftDiag))
                attacks.Add(leftDiag);

            Position rightDiag = new Position(current.Row + direction, current.Col + 1);
            if (board.IsValidPos(rightDiag))
                attacks.Add(rightDiag);

            return attacks;
        }
    }
}