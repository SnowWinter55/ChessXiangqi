using Newtonsoft.Json;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    public class MatchRecord
    {
        public string GameType { get; set; } // "Chess" hoặc "Xiangqi"
        public List<Move> Moves { get; set; }
        public GameState FinalState { get; set; }
        public string Result { get; set; } // "1-0", "0-1", "1/2-1/2"
        public string Date { get; set; }
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static MatchRecord FromJson(string json) => JsonConvert.DeserializeObject<MatchRecord>(json);
    }
}