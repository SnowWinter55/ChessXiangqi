using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Models.Xiangqi.Pieces
{
    public class Cannon : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.Cannon;

        public Cannon(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            int[][] directions = new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

            foreach (var dir in directions)
            {
                int row = current.Row + dir[0];
                int col = current.Col + dir[1];
                bool screenFound = false;

                while (board.IsValidPos(new Position(row, col)))
                {
                    var pos = new Position(row, col);
                    var occupant = board.GetPieceAt(pos);

                    if (!screenFound)
                    {
                        if (occupant == null)
                        {
                            moves.Add(pos);
                        }
                        else
                        {
                            screenFound = true;
                        }
                    }
                    else
                    {
                        if (occupant != null)
                        {
                            if (occupant.Color != Color)
                                moves.Add(pos);
                            break;
                        }
                    }

                    row += dir[0];
                    col += dir[1];
                }
            }

            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            var attacks = new List<Position>();
            int[][] directions = new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

            foreach (var dir in directions)
            {
                int row = current.Row + dir[0];
                int col = current.Col + dir[1];
                bool screenFound = false;

                while (board.IsValidPos(new Position(row, col)))
                {
                    var pos = new Position(row, col);
                    var occupant = board.GetPieceAt(pos);

                    if (!screenFound)
                    {
                        if (occupant != null)
                            screenFound = true;
                    }
                    else
                    {
                        if (occupant != null)
                        {
                            if (occupant.Color != Color)
                                attacks.Add(pos);
                            break;
                        }
                    }

                    row += dir[0];
                    col += dir[1];
                }
            }

            return attacks;
        }
    }
}
