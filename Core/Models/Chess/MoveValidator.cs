// Core/Models/Chess/MoveValidator.cs
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Chess.Pieces;
using ChessXiangqiSolution.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Extensions;

namespace ChessXiangqiSolution.Core.Models.Chess
{
    public class ChessMoveValidator : IMoveValidator
    {
        public bool IsValidMove(IBoard board, Move move, Color currentTurn)
        {
            if (!board.IsValidPos(move.From) || !board.IsValidPos(move.To))
                return false;

            IPiece piece = board.GetPieceAt(move.From);
            if (piece == null || piece.Color != currentTurn)
                return false;

            // Kiểm tra nước đi có nằm trong danh sách các nước đi cơ bản của quân không
            var basicMoves = piece.GetValidMoves(board, move.From);
            
            // DEBUG: Log castling check
            if (piece.Type == PieceType.King && (move.To.Col == 6 || move.To.Col == 2))
            {
                System.Console.WriteLine($"\n[DEBUG CASTLING] King move to col {move.To.Col}");
                System.Console.WriteLine($"  board is IChessBoard: {board is IChessBoard}");
                System.Console.WriteLine($"  BasicMoves count: {basicMoves.Count}");
                System.Console.WriteLine($"  Move found in basicMoves: {basicMoves.Any(m => m.Equals(move.To))}");
                foreach (var bm in basicMoves)
                    System.Console.WriteLine($"    - {bm.Row},{bm.Col}");
            }
            
            if (!basicMoves.Any(m => m.Equals(move.To)))
                return false;

            // Xử lý các nước đặc biệt (cần chuyển đổi move bình thường thành move đặc biệt)
            // En passant
            if (piece.Type == PieceType.Pawn && IsEnPassantMove(board, move, currentTurn))
                move.IsEnPassant = true;
            // Castling
            else if (piece.Type == PieceType.King && IsCastlingMove(board, move, currentTurn))
                move.IsCastling = true;
            // Promotion
            else if (piece.Type == PieceType.Pawn && IsPromotionMove(board, move, currentTurn))
            {
                // PromotionPiece cần được set từ bên ngoài (UI sẽ hỏi)
                if (!move.PromotionPiece.HasValue)
                    return false; // Chưa chọn quân phong
            }

            // Kiểm tra sau nước đi vua không bị chiếu
            if (MoveLeavesKingInCheck(board, move, currentTurn))
                return false;

            return true;
        }

        private bool IsEnPassantMove(IBoard board, Move move, Color currentTurn)
        {
            if (board is IChessBoard chessBoard && chessBoard.LastMove != null)
            {
                Pawn pawn = board.GetPieceAt(move.From) as Pawn;
                if (pawn == null) return false;
                int direction = (currentTurn == Color.White) ? -1 : 1;
                if (move.To.Row != move.From.Row + direction) return false;
                if (Math.Abs(move.To.Col - move.From.Col) != 1) return false;

                var last = chessBoard.LastMove;
                var lastPiece = board.GetPieceAt(last.To);
                if (lastPiece is Pawn && Math.Abs(last.From.Row - last.To.Row) == 2)
                {
                    if (last.To.Row == move.From.Row && last.To.Col == move.To.Col)
                        return true;
                }
            }
            return false;
        }

        private bool IsCastlingMove(IBoard board, Move move, Color currentTurn)
        {
            if (board is IChessBoard chessBoard)
            {
                King king = board.GetPieceAt(move.From) as King;
                if (king == null) return false;
                if (chessBoard.HasPieceMoved(move.From)) return false;

                int backRow = (currentTurn == Color.White) ? 7 : 0;
                if (move.From.Row != backRow || move.From.Col != 4) return false;

                // Vua phải di chuyển 2 ô về bên phải hoặc trái
                if (move.To.Row != backRow) return false;
                if (move.To.Col == 6) // cánh vua
                {
                    Position rookPos = new Position(backRow, 7);
                    Rook rook = board.GetPieceAt(rookPos) as Rook;
                    if (rook == null || rook.Color != currentTurn) return false;
                    if (chessBoard.HasPieceMoved(rookPos)) return false;
                    // Các ô giữa phải trống
                    for (int col = 5; col <= 6; col++)
                        if (!board.IsEmpty(new Position(backRow, col))) return false;
                    // Vua không bị chiếu tại ô đi qua và ô đích
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 4), currentTurn.Opposite())) return false;
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 5), currentTurn.Opposite())) return false;
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 6), currentTurn.Opposite())) return false;
                    return true;
                }
                else if (move.To.Col == 2) // cánh hậu
                {
                    Position rookPos = new Position(backRow, 0);
                    Rook rook = board.GetPieceAt(rookPos) as Rook;
                    if (rook == null || rook.Color != currentTurn) return false;
                    if (chessBoard.HasPieceMoved(rookPos)) return false;
                    for (int col = 1; col <= 3; col++)
                        if (!board.IsEmpty(new Position(backRow, col))) return false;
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 4), currentTurn.Opposite())) return false;
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 3), currentTurn.Opposite())) return false;
                    if (chessBoard.IsSquareAttacked(new Position(backRow, 2), currentTurn.Opposite())) return false;
                    return true;
                }
            }
            return false;
        }

        private bool IsPromotionMove(IBoard board, Move move, Color currentTurn)
        {
            IPiece piece = board.GetPieceAt(move.From);
            if (piece?.Type != PieceType.Pawn) return false;
            int targetRow = move.To.Row;
            if (currentTurn == Color.White && targetRow == 0) return true;
            if (currentTurn == Color.Black && targetRow == 7) return true;
            return false;
        }

        public bool IsCheck(IBoard board, Color kingColor)
        {
            // Tìm vua
            Position kingPos = FindKing(board, kingColor);
            if (kingPos == null) return false;
            return (board as IChessBoard)?.IsSquareAttacked(kingPos, kingColor.Opposite()) ?? false;
        }

        private Position FindKing(IBoard board, Color color)
        {
            foreach (var (pos, piece) in board.GetAllPieces())
                if (piece.Type == PieceType.King && piece.Color == color)
                    return pos;
            return null;
        }

        public bool IsCheckmate(IBoard board, Color kingColor)
        {
            if (!IsCheck(board, kingColor)) return false;
            return GetAllValidMoves(board, kingColor).Count == 0;
        }

        public bool IsStalemate(IBoard board, Color turnColor)
        {
            if (IsCheck(board, turnColor)) return false;
            return GetAllValidMoves(board, turnColor).Count == 0;
        }

        public List<Move> GetAllValidMoves(IBoard board, Color color)
        {
            var moves = new List<Move>();
            foreach (var (pos, piece) in board.GetPiecesByColor(color))
            {
                var basic = piece.GetValidMoves(board, pos);
                foreach (var to in basic)
                {
                    Move move = new Move(pos, to);
                    if (piece.Type == PieceType.Pawn && IsPromotionMove(board, move, color))
                    {
                        // Thử với từng loại quân phong
                        foreach (var promo in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        {
                            Move testMove = new Move(pos, to) { PromotionPiece = promo };
                            if (IsValidMove(board, testMove, color))
                                moves.Add(testMove);
                        }
                    }
                    else
                    {
                        if (IsValidMove(board, move, color))
                            moves.Add(move);
                    }
                }
            }
            return moves;
        }

        public bool MoveLeavesKingInCheck(IBoard board, Move move, Color movingColor)
        {
            IBoard copy = board.Clone();
            // Thực hiện nước đi trên bản copy (cần xử lý đặc biệt)
            copy.MakeMove(move);
            return IsCheck(copy, movingColor);
        }
    }
}