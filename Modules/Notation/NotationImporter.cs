using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Modules.Movement;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Nhập biên bản ván cờ từ file (PGN cho Chess, XIANGQI cho Xiangqi, JSON cho cả hai)
    /// </summary>
    public class NotationImporter
    {
        /// <summary>
        /// Import từ file - tự động detect format dựa trên extension
        /// </summary>
        public MatchRecord ImportFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            string content = File.ReadAllText(filePath);
            string extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".pgn" => ParsePgnFile(content),
                ".xqg" => ParseXiangqiFile(content),
                ".json" => ParseJsonFile(content),
                _ => throw new InvalidOperationException($"Unsupported format: {extension}")
            };
        }

        /// <summary>
        /// Parse PGN file cho Chess
        /// Format: [Tag "Value"] ... 1. e4 e5 2. Nf3 ...
        /// </summary>
        private MatchRecord ParsePgnFile(string content)
        {
            var record = new MatchRecord { GameType = "Chess" };
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int i = 0;
            // Parse metadata tags
            while (i < lines.Length)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                // Pattern: [Tag "Value"]
                if (line.StartsWith("[") && line.Contains("]"))
                {
                    var match = Regex.Match(line, @"\[(\w+)\s+""([^""]*)""\]");
                    if (match.Success)
                    {
                        string tag = match.Groups[1].Value;
                        string value = match.Groups[2].Value;

                        switch (tag)
                        {
                            case "Date":
                                if (DateTime.TryParse(value.Replace(".", "-"), out var date))
                                    record.Date = date;
                                break;
                            case "White":
                                record.WhitePlayer = value;
                                break;
                            case "Black":
                                record.BlackPlayer = value;
                                break;
                            case "Event":
                                record.Event = value;
                                break;
                            case "TimeControl":
                                record.TimeControl = value;
                                break;
                            case "Result":
                                record.Result = value;
                                break;
                        }
                    }
                    i++;
                }
                else
                {
                    // Bắt đầu phần moves
                    break;
                }
            }

            // Parse moves
            var movesText = string.Join(" ", lines.Skip(i));
            var moves = ParseMovesFromPgn(movesText);
            record.Moves = moves;

            return record;
        }

        /// <summary>
        /// Parse moves từ PGN text: "1. e4 e5 2. Nf3 Nc6 ..."
        /// Returns list of Move objects with SAN notation
        /// </summary>
        private List<Move> ParseMovesFromPgn(string movesText)
        {
            var moves = new List<Move>();
            // Remove comments và variations
            movesText = Regex.Replace(movesText, @"\{[^}]*\}", "");
            movesText = Regex.Replace(movesText, @"\([^)]*\)", "");

            // Split by whitespace
            var tokens = movesText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                // Skip move numbers (1., 2., etc.) and result (1-0, 0-1, 1/2-1/2, *)
                if (Regex.IsMatch(token, @"^\d+\.?$") || Regex.IsMatch(token, @"^(1-0|0-1|1/2-1/2|\*)$"))
                    continue;

                // Parse move from SAN notation (create a placeholder Move with SAN set)
                var move = ParseSANMove(token);
                if (move != null)
                    moves.Add(move);
            }

            return moves;
        }

        /// <summary>
        /// Tạo Move object từ SAN string (placeholder positions, SAN được set)
        /// Positions sẽ được update khi replay game
        /// </summary>
        private Move ParseSANMove(string san)
        {
            if (string.IsNullOrWhiteSpace(san))
                return null;

            try
            {
                // Create a placeholder move with SAN notation
                // The actual from/to positions will be determined during replay when board is available
                var move = new Move(new Position(0, 0), new Position(0, 0))
                {
                    San = san
                };
                return move;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse XIANGQI file cho Xiangqi
        /// Format tương tự PGN nhưng dùng tên quân Xiangqi
        /// </summary>
        private MatchRecord ParseXiangqiFile(string content)
        {
            var record = new MatchRecord { GameType = "Xiangqi" };
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int i = 0;
            // Parse metadata tags (giống PGN)
            while (i < lines.Length)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                if (line.StartsWith("[") && line.Contains("]"))
                {
                    var match = Regex.Match(line, @"\[(\w+)\s+""([^""]*)""\]");
                    if (match.Success)
                    {
                        string tag = match.Groups[1].Value;
                        string value = match.Groups[2].Value;

                        switch (tag)
                        {
                            case "Date":
                                if (DateTime.TryParse(value.Replace(".", "-"), out var date))
                                    record.Date = date;
                                break;
                            case "Red":
                                record.WhitePlayer = value; // Map Red → White
                                break;
                            case "Black":
                                record.BlackPlayer = value;
                                break;
                            case "Event":
                                record.Event = value;
                                break;
                            case "TimeControl":
                                record.TimeControl = value;
                                break;
                            case "Result":
                                record.Result = value;
                                break;
                        }
                    }
                    i++;
                }
                else
                {
                    break;
                }
            }

            // Parse xiangqi moves
            var movesText = string.Join(" ", lines.Skip(i));
            var moves = ParseMovesFromXiangqi(movesText);
            record.Moves = moves;

            return record;
        }

        /// <summary>
        /// Parse Xiangqi moves từ text
        /// Format: "1. C4=5 H8+2 2. H2+1 ..."
        /// </summary>
        private List<Move> ParseMovesFromXiangqi(string movesText)
        {
            var moves = new List<Move>();
            // Remove comments và variations
            movesText = Regex.Replace(movesText, @"\{[^}]*\}", "");
            movesText = Regex.Replace(movesText, @"\([^)]*\)", "");

            var tokens = movesText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                // Skip move numbers và result
                if (Regex.IsMatch(token, @"^\d+\.?$") || Regex.IsMatch(token, @"^(1-0|0-1|1/2-1/2|\*)$"))
                    continue;

                // Parse xiangqi move (store as SAN)
                var move = ParseSANMove(token);
                if (move != null)
                    moves.Add(move);
            }

            return moves;
        }

        /// <summary>
        /// Parse JSON format
        /// </summary>
        private MatchRecord ParseJsonFile(string content)
        {
            try
            {
                var record = JsonConvert.DeserializeObject<MatchRecord>(content);
                return record ?? throw new InvalidOperationException("Invalid JSON format");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse JSON: {ex.Message}", ex);
            }
        }
    }
}