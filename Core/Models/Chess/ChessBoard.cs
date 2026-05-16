using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Models.Chess.Pieces;

namespace ChessXiangqiSolution.Core.Models.Chess
{
    public class ChessBoard : IChessBoard
    {
        private IPiece[,] _board;
        private Dictionary<Position, bool> _pieceMoved; // true nếu đã từng di chuyển (cho castle)
        private Move _lastMove;

        public int Rows => 8;
        public int Cols => 8;
        public GameType GameType => GameType.Chess;
        public Move LastMove => _lastMove;

        public ChessBoard()
        {
            _board = new IPiece[Rows, Cols];
            _pieceMoved = new Dictionary<Position, bool>();
            InitializeStandardPosition();
        }

        private void InitializeStandardPosition()
        {
            // Xếp quân trắng (hàng 7) và đen (hàng 0)
            // Hàng 0: Đen, hàng 7: Trắng
            for (int col = 0; col < 8; col++)
            {
                _board[1, col] = new Pawn(Color.Black);
                _board[6, col] = new Pawn(Color.White);
            }

            // Xe
            _board[0, 0] = new Rook(Color.Black); _board[0, 7] = new Rook(Color.Black);
            _board[7, 0] = new Rook(Color.White); _board[7, 7] = new Rook(Color.White);
            // Mã
            _board[0, 1] = new Knight(Color.Black); _board[0, 6] = new Knight(Color.Black);
            _board[7, 1] = new Knight(Color.White); _board[7, 6] = new Knight(Color.White);
            // Tượng
            _board[0, 2] = new Bishop(Color.Black); _board[0, 5] = new Bishop(Color.Black);
            _board[7, 2] = new Bishop(Color.White); _board[7, 5] = new Bishop(Color.White);
            // Hậu
            _board[0, 3] = new Queen(Color.Black);
            _board[7, 3] = new Queen(Color.White);
            // Vua
            _board[0, 4] = new King(Color.Black);
            _board[7, 4] = new King(Color.White);
        }

        public IPiece GetPieceAt(Position pos) => IsValidPos(pos) ? _board[pos.Row, pos.Col] : null;
        public void SetPieceAt(Position pos, IPiece piece) { if (IsValidPos(pos)) _board[pos.Row, pos.Col] = piece; }
        public bool IsEmpty(Position pos) => GetPieceAt(pos) == null;
        public bool IsValidPos(Position pos) => pos.Row >= 0 && pos.Row < Rows && pos.Col >= 0 && pos.Col < Cols;

        public void MakeMove(Move move)
        {
            IPiece movingPiece = GetPieceAt(move.From);
            if (movingPiece == null) throw new InvalidOperationException("No piece at source");

            IPiece captured = GetPieceAt(move.To);
            IPiece promotedFrom = null, promotedTo = null;

            // Xử lý đặc biệt
            if (move.IsEnPassant)
            {
                // Quân bị ăn nằm ở hàng current.Row, cột move.To.Col
                Position capturedPos = new Position(move.From.Row, move.To.Col);
                captured = GetPieceAt(capturedPos);
                SetPieceAt(capturedPos, null);
            }
            else if (move.IsCastling)
            {
                // Di chuyển vua và xe
                int backRow = (movingPiece.Color == Color.White) ? 7 : 0;
                int fromCol = move.From.Col;
                int toCol = move.To.Col;
                int rookFromCol = (toCol > fromCol) ? 7 : 0;
                int rookToCol = (toCol > fromCol) ? 5 : 3;
                Position rookFrom = new Position(backRow, rookFromCol);
                Position rookTo = new Position(backRow, rookToCol);
                IPiece rook = GetPieceAt(rookFrom);
                SetPieceAt(rookTo, rook);
                SetPieceAt(rookFrom, null);
                // Đánh dấu đã di chuyển cho cả vị trí gốc và vị trí mới của xe
                _pieceMoved[rookFrom] = true;
                _pieceMoved[rookTo] = true;
            }

            // Phong cấp
            if (move.PromotionPiece.HasValue && movingPiece.Type == PieceType.Pawn)
            {
                promotedFrom = movingPiece;
                promotedTo = CreatePiece(move.PromotionPiece.Value, movingPiece.Color);
                movingPiece = promotedTo;
            }

            // Thực hiện di chuyển chính
            SetPieceAt(move.To, movingPiece);
            SetPieceAt(move.From, null);
            move.CapturedPiece = captured;

            _lastMove = move;

            // Đánh dấu vị trí nguồn và đích là đã có quân di chuyển
            _pieceMoved[move.From] = true;
            _pieceMoved[move.To] = true;
        }

        private IPiece CreatePiece(PieceType type, Color color)
        {
            return type switch
            {
                PieceType.Queen => new Queen(color),
                PieceType.Rook => new Rook(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Knight => new Knight(color),
                _ => throw new ArgumentException("Invalid promotion piece")
            };
        }

        public IBoard Clone()
        {
            var clone = new ChessBoard();
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    clone._board[r, c] = _board[r, c] != null ? ClonePiece(_board[r, c]) : null;
            clone._lastMove = _lastMove;
            foreach (var kv in _pieceMoved)
                clone._pieceMoved[kv.Key] = kv.Value;
            return clone;
        }

        private IPiece ClonePiece(IPiece piece)
        {
            // Mỗi class quân cờ có constructor nhận color
            return piece.Type switch
            {
                PieceType.Pawn => new Pawn(piece.Color),
                PieceType.Rook => new Rook(piece.Color),
                PieceType.Knight => new Knight(piece.Color),
                PieceType.Bishop => new Bishop(piece.Color),
                PieceType.Queen => new Queen(piece.Color),
                PieceType.King => new King(piece.Color),
                _ => null
            };
        }

        public IEnumerable<(Position Pos, IPiece Piece)> GetAllPieces()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_board[r, c] != null)
                        yield return (new Position(r, c), _board[r, c]);
        }

        public IEnumerable<(Position Pos, IPiece Piece)> GetPiecesByColor(Color color)
        {
            foreach (var (pos, piece) in GetAllPieces())
                if (piece.Color == color)
                    yield return (pos, piece);
        }

        public bool HasPieceMoved(Position pos)
        {
            return _pieceMoved.ContainsKey(pos) && _pieceMoved[pos];
        }

        public bool IsSquareAttacked(Position pos, Color attackerColor)
        {
            // Duyệt tất cả quân của attackerColor, xem có nước đi hợp lệ đến pos không
            foreach (var (p, piece) in GetPiecesByColor(attackerColor))
            {
                var moves = piece.GetAttackMoves(this, p);
                if (moves.Any(m => m.Equals(pos)))
                    return true;
            }
            return false;
        }
    }
}