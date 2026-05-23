using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Biên bản ván cờ - lưu trữ thông tin về một ván đấu hoàn chỉnh
    /// Hỗ trợ Chess và Xiangqi
    /// </summary>
    public class MatchRecord
    {
        // Thông tin cơ bản
        public string GameType { get; set; } // "Chess" hoặc "Xiangqi"
        public List<Move> Moves { get; set; } = new List<Move>();
        public GameState FinalState { get; set; }
        public string Result { get; set; } // "1-0", "0-1", "1/2-1/2"
        
        // Thông tin metadata
        public DateTime Date { get; set; } // Ngày thi đấu
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public string Event { get; set; } // Tên giải/trận đấu
        public string TimeControl { get; set; } // VD: "5+3", "10+0"

        public MatchRecord()
        {
            Moves = new List<Move>();
            Date = DateTime.Now;
            Result = "*"; // * = ongoing
        }

        /// <summary>Lấy nước đi tại chỉ số cụ thể</summary>
        public Move GetMoveAtIndex(int index)
        {
            if (index < 0 || index >= Moves.Count)
                return null;
            return Moves[index];
        }

        /// <summary>Lấy tổng số nước đi</summary>
        public int GetTotalMoves() => Moves.Count;

        /// <summary>Kiểm tra toàn bộ nước đi có hợp lệ không (cần board và validator)</summary>
        public bool ValidateAllMoves(IBoard board, IMoveValidator validator, Color startingColor)
        {
            try
            {
                Color currentColor = startingColor;
                foreach (var move in Moves)
                {
                    if (!validator.IsValidMove(board, move, currentColor))
                        return false;
                    
                    // Thực hiện nước đi để tiếp tục validate nước tiếp theo
                    board.MakeMove(move);
                    currentColor = currentColor == Color.White ? Color.Black : Color.White;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Convert thành JSON string</summary>
        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        /// <summary>Tạo MatchRecord từ JSON string</summary>
        public static MatchRecord FromJson(string json) => 
            JsonConvert.DeserializeObject<MatchRecord>(json);

        /// <summary>Tạo bản sao của MatchRecord</summary>
        public MatchRecord Clone()
        {
            return new MatchRecord
            {
                GameType = this.GameType,
                Moves = new List<Move>(this.Moves),
                FinalState = this.FinalState,
                Result = this.Result,
                Date = this.Date,
                WhitePlayer = this.WhitePlayer,
                BlackPlayer = this.BlackPlayer,
                Event = this.Event,
                TimeControl = this.TimeControl
            };
        }
    }
}