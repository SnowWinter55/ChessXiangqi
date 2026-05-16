using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Xiangqi.Pieces
{
    public class General : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.General;

        public General(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] directions = new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };
            foreach (var dir in directions)
            {
                var target = new Position(current.Row + dir[0], current.Col + dir[1]);
                if (!board.IsValidPos(target) || !IsInsidePalace(target))
                    continue;
                var occupant = board.GetPieceAt(target);
                if (occupant == null || occupant.Color != Color)
                    moves.Add(target);
            }
            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            var attacks = GetValidMoves(board, current);

            // Cờ tướng: hai tướng đối diện nhau trên cùng một cột không có quân chắn giữa
            for (int row = current.Row + (Color == Color.White ? -1 : 1); row >= 0 && row < board.Rows; row += (Color == Color.White ? -1 : 1))
            {
                var pos = new Position(row, current.Col);
                var piece = board.GetPieceAt(pos);
                if (piece != null)
                {
                    if (piece.Type == PieceType.General && piece.Color != Color)
                        attacks.Add(pos);
                    break;
                }
            }

            return attacks;
        }

        private bool IsInsidePalace(Position pos)
        {
            if (pos.Col < 3 || pos.Col > 5) return false;
            return (Color == Color.White && pos.Row >= 7 && pos.Row <= 9) ||
                   (Color == Color.Black && pos.Row >= 0 && pos.Row <= 2);
        }
    }
}
