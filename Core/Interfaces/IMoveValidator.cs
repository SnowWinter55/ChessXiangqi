// /Core/Interfaces/IMoveValidator.cs
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Core.Interfaces
{
    /// <summary>
    /// Xác thực tính hợp lệ của nước đi dựa trên luật của từng loại cờ.
    /// Kiểm tra bao gồm: di chuyển đúng kiểu quân, chướng ngại vật, chiếu, chiếu bí, các luật đặc biệt.
    /// </summary>
    public interface IMoveValidator
    {
        /// <summary>
        /// Kiểm tra một nước đi cụ thể có hợp lệ trong trạng thái bàn cờ hiện tại hay không.
        /// </summary>
        /// <param name="board">Bàn cờ hiện tại (đã có đầy đủ quân)</param>
        /// <param name="move">Nước đi cần kiểm tra (From, To)</param>
        /// <param name="currentTurn">Lượt đi hiện tại (màu nào đang đi)</param>
        /// <returns>true nếu nước đi hợp lệ, false nếu không</returns>
        bool IsValidMove(IBoard board, Move move, Color currentTurn);

        /// <summary>
        /// Kiểm tra xem một màu có đang bị chiếu không.
        /// </summary>
        bool IsCheck(IBoard board, Color kingColor);

        /// <summary>
        /// Kiểm tra xem một màu có bị chiếu hết không (không còn nước đi hợp lệ nào).
        /// </summary>
        bool IsCheckmate(IBoard board, Color kingColor);

        /// <summary>
        /// Kiểm tra xem có phải hòa cờ không (stalemate - không còn nước đi nhưng không bị chiếu).
        /// </summary>
        bool IsStalemate(IBoard board, Color turnColor);

        /// <summary>
        /// Trả về danh sách tất cả các nước đi hợp lệ của một màu.
        /// (Dùng cho AI hoặc kiểm tra chiếu hết)
        /// </summary>
        List<Move> GetAllValidMoves(IBoard board, Color color);

        /// <summary>
        /// Kiểm tra nước đi có vi phạm luật chiếu không (tức là sau khi đi, vua của bên đi có bị chiếu không).
        /// </summary>
        bool MoveLeavesKingInCheck(IBoard board, Move move, Color movingColor);
    }
}