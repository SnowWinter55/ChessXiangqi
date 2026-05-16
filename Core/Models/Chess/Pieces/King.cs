// /Core/Models/Chess/Pieces/King.cs
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Extensions;

namespace ChessXiangqiSolution.Core.Models.Chess.Pieces
{
    public class King : IPiece
    {
        public Color Color { get; }
        public PieceType Type => PieceType.King;

        public King(Color color) { Color = color; }

        public List<Position> GetValidMoves(IBoard board, Position current)
        {
            var moves = new List<Position>();
            // Các ô xung quanh
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    var pos = new Position(current.Row + dr, current.Col + dc);
                    if (!board.IsValidPos(pos)) continue;
                    var piece = board.GetPieceAt(pos);
                    if (piece == null || piece.Color != Color)
                        moves.Add(pos);
                }
            }

            // Castle (nhập thành) – cần board hỗ trợ IChessBoard
            if (board is IChessBoard chessBoard && !chessBoard.HasPieceMoved(current))
            {
                int backRow = (Color == Color.White) ? 7 : 0;
                if (current.Row == backRow && current.Col == 4)
                {
                    System.Console.WriteLine($"[DEBUG] King castling check - checking castling from {current}");
                    // Nhập thành cánh vua (phải)
                    var rookPos = new Position(backRow, 7);
                    if (chessBoard.GetPieceAt(rookPos) is Rook rook && rook.Color == Color && !chessBoard.HasPieceMoved(rookPos))
                    {
                        // Kiểm tra các ô giữa trống
                        bool emptyBetween = true;
                        for (int col = 5; col <= 6; col++)
                        {
                            if (!board.IsEmpty(new Position(backRow, col))) { emptyBetween = false; break; }
                        }
                        if (emptyBetween)
                        {
                            // Kiểm tra ô vua đi qua và ô đích không bị chiếu (gọi từ chessBoard)
                            if (!chessBoard.IsSquareAttacked(new Position(backRow, 4), Color.Opposite()) &&
                                !chessBoard.IsSquareAttacked(new Position(backRow, 5), Color.Opposite()) &&
                                !chessBoard.IsSquareAttacked(new Position(backRow, 6), Color.Opposite()))
                            {
                                System.Console.WriteLine($"[DEBUG] Adding kingside castling destination: {backRow},6");
                                moves.Add(new Position(backRow, 6));
                            }
                            else
                                System.Console.WriteLine($"[DEBUG] Kingside castling blocked: King or path under attack");
                        }
                        else
                            System.Console.WriteLine($"[DEBUG] Kingside castling blocked: Pieces between king and rook");
                    }
                    else
                        System.Console.WriteLine($"[DEBUG] Kingside castling blocked: No rook or rook moved");
                    // Nhập thành cánh hậu (trái)
                    rookPos = new Position(backRow, 0);
                    if (chessBoard.GetPieceAt(rookPos) is Rook rook2 && rook2.Color == Color && !chessBoard.HasPieceMoved(rookPos))
                    {
                        bool emptyBetween = true;
                        for (int col = 1; col <= 3; col++)
                        {
                            if (!board.IsEmpty(new Position(backRow, col))) { emptyBetween = false; break; }
                        }
                        if (emptyBetween)
                        {
                            if (!chessBoard.IsSquareAttacked(new Position(backRow, 4), Color.Opposite()) &&
                                !chessBoard.IsSquareAttacked(new Position(backRow, 3), Color.Opposite()) &&
                                !chessBoard.IsSquareAttacked(new Position(backRow, 2), Color.Opposite()))
                            {
                                System.Console.WriteLine($"[DEBUG] Adding queenside castling destination: {backRow},2");
                                moves.Add(new Position(backRow, 2));
                            }
                            else
                                System.Console.WriteLine($"[DEBUG] Queenside castling blocked: King or path under attack");
                        }
                        else
                            System.Console.WriteLine($"[DEBUG] Queenside castling blocked: Pieces between king and rook");
                    }
                    else
                        System.Console.WriteLine($"[DEBUG] Queenside castling blocked: No rook or rook moved");
                }
            }
            else
            {
                if (!(board is IChessBoard))
                    System.Console.WriteLine($"[DEBUG] Board is not IChessBoard!");
            }

            return moves;
        }

        public List<Position> GetAttackMoves(IBoard board, Position current)
        {
            var attacks = new List<Position>();
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    var pos = new Position(current.Row + dr, current.Col + dc);
                    if (!board.IsValidPos(pos)) continue;
                    attacks.Add(pos);
                }
            }
            return attacks;
        }
    }
}
