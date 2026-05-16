// /Core/Models/Chess/Pieces/Queen.cs
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Chess.Pieces
{
    public class Queen : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Queen;

        public Queen(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] directions = new int[][]
            {
                new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 },
                new[] { -1, -1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { 1, 1 }
            };
            foreach (var dir in directions)
            {
                int row = current.Row + dir[0];
                int col = current.Col + dir[1];
                while (board.IsValidPos(new Position(row, col)))
                {
                    var pos = new Position(row, col);
                    var piece = board.GetPieceAt(pos);
                    if (piece == null)
                        moves.Add(pos);
                    else
                    {
                        if (piece.Color != Color)
                            moves.Add(pos);
                        break;
                    }
                    row += dir[0];
                    col += dir[1];
                }
            }
            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            return GetValidMoves(board, current);
        }
    }
}