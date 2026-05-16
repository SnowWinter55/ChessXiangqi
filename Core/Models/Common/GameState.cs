// /Core/Models/Common/GameState.cs
using Newtonsoft.Json;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Modules.Clock;

namespace ChessXiangqiSolution.Core.Models.Common
{
    /// <summary>
    /// Lưu trạng thái hiện tại của ván đấu, bao gồm bàn cờ (sẽ được tham chiếu riêng),
    /// FEN (chuỗi ký hiệu), lượt đi, thông tin đồng hồ.
    /// Class này dùng để lưu trạng thái tĩnh, không chứa logic đồng hồ chạy.
    /// </summary>
    public class GameState
    {
        /// <summary>FEN string đại diện cho bàn cờ (tùy theo loại cờ). Có thể null nếu chưa khởi tạo.</summary>
        public string Fen { get; set; }

        /// <summary>Lượt đi hiện tại của bên nào</summary>
        public Color CurrentTurn { get; set; }

        /// <summary>Đồng hồ đã được cấu hình (thời gian còn lại, chế độ) – bản sao tĩnh, không chạy</summary>
        public GameClock ClockSnapshot { get; set; } // Hoặc bạn có thể lưu thời gian dưới dạng số

        // Hoặc đơn giản hơn: chỉ lưu thời gian còn lại của mỗi bên
        public int WhiteTimeSeconds { get; set; }
        public int BlackTimeSeconds { get; set; }

        /// <summary>Cờ hiệu ván đấu đã kết thúc chưa</summary>
        public bool IsGameOver { get; set; }

        /// <summary>Lý do kết thúc (Checkmate, Stalemate, Timeout, Resign...)</summary>
        public string GameOverReason { get; set; }

        /// <summary>Lưu thêm nếu cần: số nước đi, history...</summary>
        public int MoveNumber { get; set; } // Số nước đi đã thực hiện

        public GameState()
        {
            CurrentTurn = Color.White; // hoặc Red trong cờ tướng
            IsGameOver = false;
            GameOverReason = "";
            MoveNumber = 0;
            WhiteTimeSeconds = 0;
            BlackTimeSeconds = 0;
        }

        /// <summary>Khởi tạo từ FEN và cài đặt thời gian</summary>
        public GameState(string fen, Color startingTurn, int whiteTimeSec, int blackTimeSec)
        {
            Fen = fen;
            CurrentTurn = startingTurn;
            WhiteTimeSeconds = whiteTimeSec;
            BlackTimeSeconds = blackTimeSec;
            IsGameOver = false;
            GameOverReason = "";
            MoveNumber = 0;
        }

        /// <summary>Cập nhật thời gian từ đồng hồ đang chạy</summary>
        public void UpdateTimeFromClock(GameClock clock)
        {
            WhiteTimeSeconds = clock.TimeWhiteSeconds;
            BlackTimeSeconds = clock.TimeBlackSeconds;
        }

        /// <summary>Tạo bản sao (deep copy) của GameState (cần cho Undo/Redo)</summary>
        public GameState Clone()
        {
            return new GameState
            {
                Fen = this.Fen,
                CurrentTurn = this.CurrentTurn,
                WhiteTimeSeconds = this.WhiteTimeSeconds,
                BlackTimeSeconds = this.BlackTimeSeconds,
                IsGameOver = this.IsGameOver,
                GameOverReason = this.GameOverReason,
                MoveNumber = this.MoveNumber
                // ClockSnapshot không clone sâu vì nó chỉ để tham khảo
            };
        }
    }
}