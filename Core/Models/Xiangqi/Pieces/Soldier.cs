using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Xiangqi.Pieces
{
    public class Soldier : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Soldier;

        public Soldier(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int forward = Color == Color.White ? -1 : 1;

            var forwardPos = new Position(current.Row + forward, current.Col);
            if (board.IsValidPos(forwardPos))
            {
                var occupant = board.GetPieceAt(forwardPos);
                if (occupant == null || occupant.Color != Color)
                    moves.Add(forwardPos);
            }

            if (HasCrossedRiver(current))
            {
                var left = new Position(current.Row, current.Col - 1);
                if (board.IsValidPos(left))
                {
                    var occupant = board.GetPieceAt(left);
                    if (occupant == null || occupant.Color != Color)
                        moves.Add(left);
                }

                var right = new Position(current.Row, current.Col + 1);
                if (board.IsValidPos(right))
                {
                    var occupant = board.GetPieceAt(right);
                    if (occupant == null || occupant.Color != Color)
                        moves.Add(right);
                }
            }

            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            return GetValidMoves(board, current);
        }

        private bool HasCrossedRiver(Position pos)
        {
            return (Color == Color.White && pos.Row <= 4) || (Color == Color.Black && pos.Row >= 5);
        }
    }
}
