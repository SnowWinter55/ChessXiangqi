// /Core/Interfaces/IPiece.cs
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Interfaces
{
    /// <summary>
    /// Đại diện cho một quân cờ (dùng chung cho Chess và Xiangqi).
    /// Mỗi quân cờ tự biết cách di chuyển của nó (không quan tâm đến chiếu, chiếu hết).
    /// </summary>
    public interface IPiece
    {
        /// <summary>Màu quân cờ (Đen/Trắng hoặc Xanh/Đỏ)</summary>
        Color Color { get; }

        /// <summary>Loại quân (Tốt, Xe, Mã, ...)</summary>
        PieceType Type { get; }

        /// <summary>
        /// Trả về danh sách các vị trí có thể di chuyển đến từ vị trí hiện tại.
        /// Không kiểm tra chiếu, không kiểm tra chướng ngại vật (sẽ do Board hoặc MoveValidator đảm nhiệm).
        /// </summary>
        /// <param name="board">Bàn cờ hiện tại (để lấy kích thước, quân cản...)</param>
        /// <param name="currentPosition">Vị trí của quân cờ này trên bàn</param>
        /// <returns>Danh sách các ô có thể đi (chưa lọc chiếu, chưa lọc chướng ngại)</returns>
        List<Position> GetValidMoves(IBoard board, Position currentPosition);

        /// <summary>
        /// Trả về danh sách các vị trí mà quân cờ này tấn công.
        /// Sử dụng để kiểm tra xem một ô có bị chiếu hay không.
        /// Không bao gồm các điều kiện hậu như "vua không thể đi vào ô bị chiếu".
        /// </summary>
        /// <param name="board">Bàn cờ hiện tại</param>
        /// <param name="currentPosition">Vị trí của quân cờ</param>
        /// <returns>Danh sách các ô bị kiểm soát/tấn công bởi quân cờ này</returns>
        List<Position> GetAttackMoves(IBoard board, Position currentPosition);
    }
}