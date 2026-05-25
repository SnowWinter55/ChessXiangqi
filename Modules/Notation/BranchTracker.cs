// Modules/Notation/BranchTracker.cs

using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Lưu trữ cấu trúc cây nước đi (branching) cho phép tạo các biến (variations).
    /// Hỗ trợ phân tích thế cờ, quay lui giữa các nhánh, và replay ván cờ.
    /// </summary>
    public class BranchTracker
    {
        public class Node
        {
            public Move Move { get; set; }
            public Node Parent { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();
            public int VariationId { get; set; }
            public int MoveIndex { get; set; }
        }

        private Node _root;
        private Node _currentNode;
        private List<Move> _mainLine;
        private readonly Stack<Move> _redoStack;

        public BranchTracker()
        {
            _root = new Node { Move = null, Parent = null, MoveIndex = -1 };
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
                var newNode = new Node
                {
                    Move = move,
                    Parent = _currentNode,
                    MoveIndex = _mainLine.Count
                };
                _currentNode.Children.Add(newNode);
                _currentNode = newNode;
            }

            _mainLine.Add(move);
            if (clearRedo)
                _redoStack.Clear();
        }

        /// <summary>Lùi về nước trước đó trong cùng nhánh</summary>
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

        /// <summary>Lấy toàn bộ dãy nước đi main line</summary>
        public List<Move> GetMainLine()
        {
            return new List<Move>(_mainLine);
        }

        /// <summary>Đặt nhánh hiện tại làm main line</summary>
        public void SetCurrentAsMainLine()
        {
            _mainLine = GetCurrentLine();
            _redoStack.Clear();
        }

        /// <summary>Reset toàn bộ (ván mới)</summary>
        public void Reset()
        {
            _root = new Node { Move = null, Parent = null, MoveIndex = -1 };
            _currentNode = _root;
            _mainLine.Clear();
            _redoStack.Clear();
        }

        /// <summary>Rebuild tree từ danh sách moves (cho replay)</summary>
        public void RebuildFromMoveList(List<Move> moves)
        {
            Reset();
            if (moves == null || moves.Count == 0) return;
            foreach (var move in moves)
                AddMove(move);
        }

        /// <summary>
        /// Navigate đến move tại index dựa trên sourceMoves được truyền vào từ ngoài.
        /// FIX Bug 1: không dùng _mainLine nội bộ vì Reset() đã clear nó trước.
        /// </summary>
        public void GoToMoveIndex(int index, List<Move> sourceMoves)
        {
            Reset();
            if (index < 0 || sourceMoves == null || sourceMoves.Count == 0) return;

            for (int i = 0; i <= index && i < sourceMoves.Count; i++)
                AddMove(sourceMoves[i]);
        }

        /// <summary>
        /// Overload không tham số sourceMoves — dùng _mainLine hiện tại.
        /// Chỉ dùng khi _mainLine còn đúng (tức là TRƯỚC khi Reset).
        /// </summary>
        public void GoToMoveIndex(int index)
        {
            // Snapshot _mainLine trước khi Reset xóa nó
            var snapshot = new List<Move>(_mainLine);
            GoToMoveIndex(index, snapshot);
        }

        /// <summary>Lấy nước đi tại chỉ số cụ thể (0-based)</summary>
        public Move GetMoveAtIndex(int index)
        {
            if (index < 0 || index >= _mainLine.Count) return null;
            return _mainLine[index];
        }

        /// <summary>Lấy tổng số nước đi trong main line</summary>
        public int GetTotalMoves() => _mainLine.Count;

        /// <summary>Lấy chỉ số move hiện tại (0-based)</summary>
        public int GetCurrentMoveIndex()
        {
            if (_currentNode == null || _currentNode.Parent == null) return -1;
            return _currentNode.MoveIndex;
        }

        /// <summary>Quay về root</summary>
        public void GoToStart()
        {
            _currentNode = _root;
            _mainLine.Clear();
            _redoStack.Clear();
        }

        /// <summary>Đi tới cuối ván</summary>
        public void GoToEnd()
        {
            var snapshot = new List<Move>(_mainLine);
            GoToMoveIndex(snapshot.Count - 1, snapshot);
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

        public bool CanGoBack() => _currentNode.Parent != null;
        public bool CanGoForward() => _redoStack.Count > 0;
    }
}