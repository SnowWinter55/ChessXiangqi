// /Core/Interfaces/IBoard.cs
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Models.Common;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace ChessXiangqiSolution.Core.Interfaces
{
    /// <summary>
    /// Đại diện cho bàn cờ (trừu tượng hóa cả Chess lẫn Xiangqi).
    /// Cung cấp các phương thức truy xuất và thao tác cơ bản.
    /// </summary>
    public interface IBoard
    {
        /// <summary>Số hàng của bàn cờ (Chess: 8, Xiangqi: 10)</summary>
        int Rows { get; }

        /// <summary>Số cột của bàn cờ (Chess: 8, Xiangqi: 9)</summary>
        int Cols { get; }

        /// <summary>Loại bàn cờ: Chess hay Xiangqi</summary>
        GameType GameType { get; }

        /// <summary>Lấy quân cờ tại một ô. Trả về null nếu ô trống.</summary>
        IPiece GetPieceAt(Position pos);

        /// <summary>Đặt quân cờ vào một ô (có thể đặt null để xóa quân).</summary>
        void SetPieceAt(Position pos, IPiece piece);

        /// <summary>Kiểm tra một ô có trống không.</summary>
        bool IsEmpty(Position pos);

        bool IsValidPos(Position pos);

        /// <summary>
        /// Thực hiện một nước đi (không kiểm tra hợp lệ, chỉ cập nhật bàn cờ).
        /// Nên được gọi sau khi đã xác thực bởi IMoveValidator.
        /// </summary>
        /// <param name="move">Nước đi cần thực hiện</param>
        void MakeMove(Move move);

        /// <summary>Tạo bản sao (deep copy) của bàn cờ hiện tại.</summary>
        IBoard Clone();

        IBoard Reset();

        /// <summary>Lấy tất cả các quân cờ trên bàn (dùng cho AI hoặc kiểm tra).</summary>
        IEnumerable<(Position Pos, IPiece Piece)> GetAllPieces();

        /// <summary>Lấy tất cả quân cờ của một màu.</summary>
        IEnumerable<(Position Pos, IPiece Piece)> GetPiecesByColor(Color color);
    }
}