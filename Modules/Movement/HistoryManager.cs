// Modules/Movement/HistoryManager.cs
using System;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Modules.Movement;

namespace ChessXiangqiSolution.Modules.Movement
{
    public class HistoryManager
    {
        private readonly IBoard _board;
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;

        public HistoryManager(IBoard board)
        {
            _board = board;
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public bool Undo()
        {
            if (_undoStack.Count == 0) return false;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            return true;
        }

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
                move = moveCmd.Move;     // cần property Move (thêm vào)
                san = moveCmd.San;
            }
            return true;
        }


        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}