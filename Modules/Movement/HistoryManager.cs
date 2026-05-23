// Modules/Movement/HistoryManager.cs
using System;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Modules.Movement;

namespace ChessXiangqiSolution.Modules.Movement
{
    /// <summary>
    /// Quản lý lịch sử nước đi, hỗ trợ undo/redo và replay từ danh sách moves
    /// </summary>
    public class HistoryManager
    {
        private readonly IBoard _board;
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private readonly List<ICommand> _executedCommands; // Track all executed commands

        public HistoryManager(IBoard board)
        {
            _board = board;
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
            _executedCommands = new List<ICommand>();
        }

        /// <summary>Thực hiện một command và lưu vào history</summary>
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _executedCommands.Add(command);
            _redoStack.Clear();
        }

        /// <summary>Quay lui một nước đi</summary>
        public bool Undo()
        {
            if (_undoStack.Count == 0) return false;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            return true;
        }

        /// <summary>Quay trước một nước đi</summary>
        public bool Redo(out Move move, out string san)
        {
            move = null;
            san = null;
            if (_redoStack.Count == 0) return false;
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            if (command is MoveCommand moveCmd)
            {
                move = moveCmd.Move;     // cần property Move (đã có)
                san = moveCmd.San;
            }
            return true;
        }

        /// <summary>Clear lịch sử hoàn toàn</summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _executedCommands.Clear();
        }

        /// <summary>Replay một danh sách moves từ đầu</summary>
        public bool ReplayFromMoveList(List<Move>? moves, IBoard? board = null)
        {
            try
            {
                // Clear history hiện tại
                Clear();

                if (moves == null || moves.Count == 0)
                    return true;

                // Replay từng move
                var targetBoard = board ?? _board;
                foreach (var move in moves)
                {
                    var command = new MoveCommand(targetBoard, move);
                    ExecuteCommand(command);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Lấy tất cả moves từ executed commands</summary>
        public List<Move> GetAllMoves()
        {
            var moves = new List<Move>();
            foreach (var cmd in _executedCommands)
            {
                if (cmd is MoveCommand moveCmd)
                {
                    moves.Add(moveCmd.Move);
                }
            }
            return moves;
        }

        /// <summary>Lấy tổng số nước đi đã thực hiện</summary>
        public int GetTotalMoves() => _executedCommands.Count;

        /// <summary>Kiểm tra xem có thể quay lui không</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Kiểm tra xem có thể quay trước không</summary>
        public bool CanRedo => _redoStack.Count > 0;
    }
}