using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Export biên bản ván cờ sang file (PGN cho Chess, XIANGQI cho Xiangqi, JSON cho cả hai)
    /// </summary>
    public class NotationExporter
    {
        /// <summary>
        /// Export sang file - tự động detect format từ extension
        /// </summary>
        public void ExportToFile(string filePath, MatchRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            string extension = Path.GetExtension(filePath).ToLower();
            string content = extension switch
            {
                ".pgn" => ToPgn(record),
                ".xqg" => ToXiangqi(record),
                ".json" => ToJson(record),
                _ => throw new InvalidOperationException($"Unsupported format: {extension}")
            };

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// Export MatchRecord sang PGN format (cho Chess)
        /// </summary>
        private string ToPgn(MatchRecord record)
        {
            var sb = new StringBuilder();

            // Metadata tags
            sb.AppendLine($"[Event \"{record.Event ?? "Casual Game"}\"]");
            sb.AppendLine($"[Date \"{record.Date:yyyy.MM.dd}\"]");
            sb.AppendLine($"[White \"{record.WhitePlayer ?? "Unknown"}\"]");
            sb.AppendLine($"[Black \"{record.BlackPlayer ?? "Unknown"}\"]");
            sb.AppendLine($"[Result \"{record.Result ?? "*"}\"]");
            
            if (!string.IsNullOrEmpty(record.TimeControl))
                sb.AppendLine($"[TimeControl \"{record.TimeControl}\"]");

            sb.AppendLine();

            // Moves
            var moveLines = FormatMovesForPgn(record.Moves);
            sb.Append(moveLines);

            // Result
            if (!string.IsNullOrEmpty(record.Result) && record.Result != "*")
                sb.AppendLine(record.Result);

            return sb.ToString();
        }

        /// <summary>
        /// Format moves cho PGN (dạng: 1. e4 e5 2. Nf3 Nc6 ...)
        /// </summary>
        private string FormatMovesForPgn(List<Move> moves)
        {
            if (moves == null || moves.Count == 0)
                return "";

            var sb = new StringBuilder();
            int moveNumber = 1;

            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                string moveStr = move.San ?? $"{move.From}-{move.To}"; // Fallback to coordinate notation

                if (i % 2 == 0)
                {
                    // White move
                    if (i > 0) sb.Append(" ");
                    sb.Append($"{moveNumber}. {moveStr}");
                }
                else
                {
                    // Black move
                    sb.Append($" {moveStr}");
                    moveNumber++;
                }
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Export MatchRecord sang XIANGQI format (custom format giống PGN)
        /// </summary>
        private string ToXiangqi(MatchRecord record)
        {
            var sb = new StringBuilder();

            // Metadata tags (dùng "Red" thay vì "White" cho Xiangqi)
            sb.AppendLine($"[Event \"{record.Event ?? "Casual Game"}\"]");
            sb.AppendLine($"[Date \"{record.Date:yyyy.MM.dd}\"]");
            sb.AppendLine($"[Red \"{record.WhitePlayer ?? "Unknown"}\"]"); // Red thay White
            sb.AppendLine($"[Black \"{record.BlackPlayer ?? "Unknown"}\"]");
            sb.AppendLine($"[Result \"{record.Result ?? "*"}\"]");
            
            if (!string.IsNullOrEmpty(record.TimeControl))
                sb.AppendLine($"[TimeControl \"{record.TimeControl}\"]");

            sb.AppendLine();

            // Moves (format tương tự PGN)
            var moveLines = FormatMovesForXiangqi(record.Moves);
            sb.Append(moveLines);

            // Result
            if (!string.IsNullOrEmpty(record.Result) && record.Result != "*")
                sb.AppendLine(record.Result);

            return sb.ToString();
        }

        /// <summary>
        /// Format moves cho Xiangqi notation
        /// </summary>
        private string FormatMovesForXiangqi(List<Move> moves)
        {
            if (moves == null || moves.Count == 0)
                return "";

            var sb = new StringBuilder();
            int moveNumber = 1;

            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                // Trong Xiangqi, có thể dùng coordinate notation như "3-4" hoặc piece notation
                string moveStr = move.San ?? FormatXiangqiCoordinate(move);

                if (i % 2 == 0)
                {
                    // Red move
                    if (i > 0) sb.Append(" ");
                    sb.Append($"{moveNumber}. {moveStr}");
                }
                else
                {
                    // Black move
                    sb.Append($" {moveStr}");
                    moveNumber++;
                }
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Format Xiangqi move thành coordinate notation
        /// </summary>
        private string FormatXiangqiCoordinate(Move move)
        {
            // Simple coordinate format: "e2-e4" hoặc "a2-a3"
            return $"{move.From}-{move.To}";
        }

        /// <summary>
        /// Export MatchRecord sang JSON format
        /// </summary>
        private string ToJson(MatchRecord record)
        {
            return JsonConvert.SerializeObject(record, Formatting.Indented);
        }
    }
}