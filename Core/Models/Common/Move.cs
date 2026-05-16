//Core/Model/Common/Move.cs
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;

namespace ChessXiangqiSolution.Core.Models.Common
{
    public class Move
    {
        public Position From { get; }
        public Position To { get; }
        public IPiece CapturedPiece { get; set; }   // Quân bị ăn (nếu có)
        public bool IsEnPassant { get; set; }       // Có phải bắt tốt qua đường không?
        public bool IsCastling { get; set; }        // Có phải nhập thành không?
        public PieceType? PromotionPiece { get; set; } // Loại quân sẽ phong (chỉ dùng cho tốt)
        public string San { get; set; }

        public bool IsCapture => CapturedPiece != null || IsEnPassant;

        public Move(Position from, Position to)
        {
            From = from;
            To = to;
            IsEnPassant = false;
            IsCastling = false;
            PromotionPiece = null;
            San = null;
        }
    }
}