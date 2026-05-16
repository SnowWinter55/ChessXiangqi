// /Core/Interfaces/IChessBoard.cs
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Enums;

namespace ChessXiangqiSolution.Core.Interfaces
{
    public interface IChessBoard : IBoard
    {
        /// <summary>Nước đi cuối cùng (dùng cho en passant, castle)</summary>
        Move LastMove { get; }

        /// <summary>Kiểm tra vua hoặc xe đã từng di chuyển chưa</summary>
        bool HasPieceMoved(Position pos);

        /// <summary>Kiểm tra vua có đang bị chiếu không (cần cho castle)</summary>
        bool IsSquareAttacked(Position pos, Color attackerColor);
    }
}