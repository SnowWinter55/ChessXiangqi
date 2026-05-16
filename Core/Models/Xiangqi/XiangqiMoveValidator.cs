using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Extensions;

namespace ChessXiangqiSolution.Core.Models.Xiangqi
{
    public class XiangqiMoveValidator : IMoveValidator
    {
        public bool IsValidMove(IBoard board, Move move, Color currentTurn)
        {
            if (!board.IsValidPos(move.From) || !board.IsValidPos(move.To))
                return false;

            var piece = board.GetPieceAt(move.From);
            if (piece == null || piece.Color != currentTurn)
                return false;

            var basicMoves = piece.GetValidMoves(board, move.From);
            if (!basicMoves.Any(m => m.Equals(move.To)))
                return false;

            if (MoveLeavesKingInCheck(board, move, currentTurn))
                return false;

            return true;
        }

        public bool IsCheck(IBoard board, Color kingColor)
        {
            var generalPos = FindGeneral(board, kingColor);
            if (generalPos == null)
                return false;

            foreach (var (pos, piece) in board.GetPiecesByColor(kingColor.Opposite()))
            {
                var attackMoves = piece.GetAttackMoves(board, pos);
                if (attackMoves.Any(m => m.Equals(generalPos)))
                    return true;
            }

            return false;
        }

        public bool IsCheckmate(IBoard board, Color kingColor)
        {
            if (!IsCheck(board, kingColor))
                return false;
            return GetAllValidMoves(board, kingColor).Count == 0;
        }

        public bool IsStalemate(IBoard board, Color turnColor)
        {
            if (IsCheck(board, turnColor))
                return false;
            return GetAllValidMoves(board, turnColor).Count == 0;
        }

        public List<Move> GetAllValidMoves(IBoard board, Color color)
        {
            var moves = new List<Move>();
            foreach (var (pos, piece) in board.GetPiecesByColor(color))
            {
                var targets = piece.GetValidMoves(board, pos);
                foreach (var target in targets)
                {
                    var move = new Move(pos, target);
                    if (IsValidMove(board, move, color))
                        moves.Add(move);
                }
            }
            return moves;
        }

        public bool MoveLeavesKingInCheck(IBoard board, Move move, Color movingColor)
        {
            IBoard copy = board.Clone();
            copy.MakeMove(move);
            return IsCheck(copy, movingColor);
        }

        private Position FindGeneral(IBoard board, Color color)
        {
            foreach (var (pos, piece) in board.GetPiecesByColor(color))
            {
                if (piece.Type == PieceType.General)
                    return pos;
            }
            return null;
        }
    }
}
