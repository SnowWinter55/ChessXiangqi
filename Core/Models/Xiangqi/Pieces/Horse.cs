using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Xiangqi.Pieces
{
    public class Horse : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Horse;

        public Horse(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] offsets = new[]
            {
                new[] { -2, -1 }, new[] { -2, 1 },
                new[] { -1, -2 }, new[] { -1, 2 },
                new[] { 1, -2 }, new[] { 1, 2 },
                new[] { 2, -1 }, new[] { 2, 1 }
            };

            foreach (var offset in offsets)
            {
                var leg = new Position(current.Row + (offset[0] == 0 ? 0 : offset[0] / 2), current.Col + (offset[1] == 0 ? 0 : offset[1] / 2));
                if (!board.IsValidPos(leg) || !board.IsEmpty(leg))
                    continue;

                var target = new Position(current.Row + offset[0], current.Col + offset[1]);
                if (!board.IsValidPos(target))
                    continue;

                var occupant = board.GetPieceAt(target);
                if (occupant == null || occupant.Color != Color)
                    moves.Add(target);
            }
            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            return GetValidMoves(board, current);
        }
    }
}
