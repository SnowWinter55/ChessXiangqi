// /Core/Models/Chess/Pieces/Knight.cs
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Chess.Pieces
{
    public class Knight : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Knight;

        public Knight(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] offsets = new int[][]
            {
                new[] { -2, -1 }, new[] { -2, 1 }, new[] { -1, -2 }, new[] { -1, 2 },
                new[] { 1, -2 }, new[] { 1, 2 }, new[] { 2, -1 }, new[] { 2, 1 }
            };
            foreach (var off in offsets)
            {
                var pos = new Position(current.Row + off[0], current.Col + off[1]);
                if (!board.IsValidPos(pos)) continue;
                var piece = board.GetPieceAt(pos);
                if (piece == null || piece.Color != Color)
                    moves.Add(pos);
            }
            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            return GetValidMoves(board, current);
        }
    }
}