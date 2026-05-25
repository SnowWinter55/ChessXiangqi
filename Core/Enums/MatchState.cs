// /Core/Enums/MatchState.cs
namespace ChessXiangqiSolution.Core.Enums
{
    /// <summary>
    /// Trạng thái của ván đấu
    /// </summary>
    public enum MatchState
    {
        /// <summary>Chế độ chơi thực - tính giờ, không thể undo/redo tùy tiện</summary>
        Game = 0,

        /// <summary>Chế độ phân tích - không tính giờ, có thể phân tích bàn cờ với undo/redo</summary>
        Analyse = 1
    }
}
