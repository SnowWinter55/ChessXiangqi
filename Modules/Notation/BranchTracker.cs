// Modules/Notation/BranchTracker.cs

using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Lưu trữ cấu trúc cây nước đi (branching) cho phép tạo các biến (variations)
    /// Hỗ trợ phân tích thế cờ, quay lui giữa các nhánh.
    /// </summary>
    public class BranchTracker
    {
        public class Node
        {
            public Move Move { get; set; }
            public Node Parent { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();
            public int VariationId { get; set; } // 0: main line, >0: sub-variation
        }

        private Node _root;
        private Node _currentNode;
        private List<Move> _mainLine;
        private readonly Stack<Move> _redoStack;

        public BranchTracker()
        {
            _root = new Node { Move = null, Parent = null };
            _currentNode = _root;
            _mainLine = new List<Move>();
            _redoStack = new Stack<Move>();
        }

        /// <summary>Thêm nước đi vào nhánh hiện tại (tạo node mới hoặc dùng node đã có)</summary>
        public void AddMove(Move move)
        {
            AddMoveInternal(move, clearRedo: true);
        }

        private void AddMoveInternal(Move move, bool clearRedo)
        {
            var existing = _currentNode.Children.FirstOrDefault(c => MoveEquals(c.Move, move));
            if (existing != null)
            {
                _currentNode = existing;
            }
            else
            {
                var newNode = new Node { Move = move, Parent = _currentNode };
                _currentNode.Children.Add(newNode);
                _currentNode = newNode;
            }

            _mainLine.Add(move);
            if (clearRedo)
                _redoStack.Clear();
        }

        /// <summary>Lùi về nước trước đó trong cùng nhánh (không sang nhánh phụ)</summary>
        public bool GoBack()
        {
            if (_currentNode.Parent != null)
            {
                _redoStack.Push(_mainLine.Last());
                _mainLine.RemoveAt(_mainLine.Count - 1);
                _currentNode = _currentNode.Parent;
                return true;
            }
            return false;
        }

        /// <summary>Chuyển đến một nhánh con theo chỉ số</summary>
        public bool GoToChild(int childIndex)
        {
            if (childIndex >= 0 && childIndex < _currentNode.Children.Count)
            {
                _currentNode = _currentNode.Children[childIndex];
                _mainLine.Add(_currentNode.Move);
                _redoStack.Clear();
                return true;
            }
            return false;
        }

        /// <summary>Lấy danh sách các nước đi có thể rẽ nhánh tại vị trí hiện tại</summary>
        public List<Move> GetAvailableBranches()
        {
            return _currentNode.Children.Select(c => c.Move).ToList();
        }

        /// <summary>Lấy toàn bộ dãy nước đi từ gốc đến node hiện tại</summary>
        public List<Move> GetCurrentLine()
        {
            return new List<Move>(_mainLine);
        }

        /// <summary>Đặt nhánh hiện tại làm main line (variation 0)</summary>
        public void SetCurrentAsMainLine()
        {
            _mainLine = GetCurrentLine();
            _redoStack.Clear();
            // Gán VariationId = 0 cho toàn bộ các node trong main line (có thể duyệt cây)
        }

        /// <summary>Reset toàn bộ (ván mới)</summary>
        public void Reset()
        {
            _root = new Node { Move = null, Parent = null };
            _currentNode = _root;
            _mainLine.Clear();
            _redoStack.Clear();
        }

        private bool MoveEquals(Move a, Move b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.From.Equals(b.From) && a.To.Equals(b.To);
        }

        public bool CanRedo() => _redoStack.Count > 0;

        public bool Redo()
        {
            if (_redoStack.Count == 0) return false;
            var move = _redoStack.Pop();
            AddMoveInternal(move, clearRedo: false);
            return true;
        }

        public bool GoToChildByMove(Move move)
        {
            var child = _currentNode.Children.FirstOrDefault(c => MoveEquals(c.Move, move));
            if (child != null)
            {
                _currentNode = child;
                _mainLine.Add(child.Move);
                _redoStack.Clear();
                return true;
            }
            return false;
        }
    }
}