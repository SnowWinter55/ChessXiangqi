using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Modules.Movement;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// High-level API để Save/Load/Replay game records
    /// </summary>
    public class GameSaver
    {
        private readonly NotationExporter _exporter = new NotationExporter();
        private readonly NotationImporter _importer = new NotationImporter();

        /// <summary>
        /// Save MatchRecord sang file
        /// </summary>
        public void SaveGame(string filePath, MatchRecord record)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _exporter.ExportToFile(filePath, record);
        }

        /// <summary>
        /// Load MatchRecord từ file
        /// </summary>
        public MatchRecord LoadGame(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            return _importer.ImportFromFile(filePath);
        }

        /// <summary>
        /// Replay một ván cờ đã lưu lên board
        /// Validate tất cả moves và rebuild tree structure
        /// </summary>
        public bool ReplayGame(
            MatchRecord record,
            IBoard board,
            IMoveValidator validator,
            HistoryManager historyMgr,
            BranchTracker branchTracker)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            try
            {
                // Xác định màu bắt đầu dựa vào GameType
                Color startingColor = DetermineStartingColor(record.GameType);

                // Validate tất cả moves
                if (!record.ValidateAllMoves(board, validator, startingColor))
                {
                    return false;
                }

                // Clear history và branch tracker
                historyMgr.Clear();
                branchTracker.Reset();

                // Replay từng move
                Color currentColor = startingColor;
                foreach (var move in record.Moves)
                {
                    // Thực hiện move trên board
                    var command = new MoveCommand(board, move);
                    historyMgr.ExecuteCommand(command);

                    // Thêm vào branch tracker
                    branchTracker.AddMove(move);

                    // Chuyển lượt
                    currentColor = currentColor == Color.White ? Color.Black : Color.White;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tạo MatchRecord từ game hiện tại
        /// </summary>
        public MatchRecord CreateMatchRecord(
            string gameType,
            List<Move>? moves,
            GameState finalState,
            string result,
            string whitePlayer,
            string blackPlayer,
            string? eventName = null,
            string? timeControl = null)
        {
            return new MatchRecord
            {
                GameType = gameType,
                Moves = moves ?? new List<Move>(),
                FinalState = finalState,
                Result = result ?? "*",
                Date = DateTime.Now,
                WhitePlayer = whitePlayer ?? "Unknown",
                BlackPlayer = blackPlayer ?? "Unknown",
                Event = eventName ?? "Casual Game",
                TimeControl = timeControl
            };
        }

        /// <summary>
        /// Xác định màu bắt đầu dựa vào loại cờ
        /// </summary>
        private Color DetermineStartingColor(string gameType)
        {
            // Chess: White starts
            // Xiangqi: Red (map to White) starts
            return Color.White;
        }
    }
}