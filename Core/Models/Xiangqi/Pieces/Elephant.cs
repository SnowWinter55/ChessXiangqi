using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Xiangqi.Pieces
{
    public class Elephant : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Elephant;

        public Elephant(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] directions = new[] { new[] { -2, -2 }, new[] { -2, 2 }, new[] { 2, -2 }, new[] { 2, 2 } };

            foreach (var dir in directions)
            {
                var target = new Position(current.Row + dir[0], current.Col + dir[1]);
                if (!board.IsValidPos(target) || !IsOnOwnSide(target))
                    continue;

                var between = new Position(current.Row + dir[0] / 2, current.Col + dir[1] / 2);
                if (!board.IsValidPos(between) || !board.IsEmpty(between))
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

        private bool IsOnOwnSide(Position pos)
        {
            return (Color == Color.White && pos.Row >= 5) || (Color == Color.Black && pos.Row <= 4);
        }
    }
}
