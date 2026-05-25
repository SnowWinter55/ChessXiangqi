using System;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Models.Xiangqi.Pieces;

namespace ChessXiangqiSolution.Core.Models.Xiangqi
{
    public class XiangqiBoard : IBoard
    {
        private readonly IPiece[,] _board;

        public int Rows => 10;
        public int Cols => 9;
        public GameType GameType => GameType.Xiangqi;

        public XiangqiBoard()
        {
            _board = new IPiece[Rows, Cols];
            InitializeStandardPosition();
        }

        private void InitializeStandardPosition()
        {
            // Black side (top)
            _board[0, 0] = new Chariot(Color.Black);
            _board[0, 1] = new Horse(Color.Black);
            _board[0, 2] = new Elephant(Color.Black);
            _board[0, 3] = new Advisor(Color.Black);
            _board[0, 4] = new General(Color.Black);
            _board[0, 5] = new Advisor(Color.Black);
            _board[0, 6] = new Elephant(Color.Black);
            _board[0, 7] = new Horse(Color.Black);
            _board[0, 8] = new Chariot(Color.Black);

            _board[2, 1] = new Cannon(Color.Black);
            _board[2, 7] = new Cannon(Color.Black);

            for (int col = 0; col < 9; col += 2)
                _board[3, col] = new Soldier(Color.Black);

            // White side (bottom)
            _board[9, 0] = new Chariot(Color.White);
            _board[9, 1] = new Horse(Color.White);
            _board[9, 2] = new Elephant(Color.White);
            _board[9, 3] = new Advisor(Color.White);
            _board[9, 4] = new General(Color.White);
            _board[9, 5] = new Advisor(Color.White);
            _board[9, 6] = new Elephant(Color.White);
            _board[9, 7] = new Horse(Color.White);
            _board[9, 8] = new Chariot(Color.White);

            _board[7, 1] = new Cannon(Color.White);
            _board[7, 7] = new Cannon(Color.White);

            for (int col = 0; col < 9; col += 2)
                _board[6, col] = new Soldier(Color.White);
        }

        public IPiece GetPieceAt(Position pos) => pos != null && IsValidPos(pos) ? _board[pos.Row, pos.Col] : null;
        public void SetPieceAt(Position pos, IPiece piece)
        {
            if (pos != null && IsValidPos(pos))
                _board[pos.Row, pos.Col] = piece;
        }

        public bool IsEmpty(Position pos) => GetPieceAt(pos) == null;

        public bool IsValidPos(Position pos) => pos != null && pos.IsValid(Rows, Cols);

        public void MakeMove(Move move)
        {
            if (move == null) throw new ArgumentNullException(nameof(move));
            IPiece movingPiece = GetPieceAt(move.From);
            if (movingPiece == null) throw new InvalidOperationException("No piece at source");

            move.CapturedPiece = GetPieceAt(move.To);
            SetPieceAt(move.To, movingPiece);
            SetPieceAt(move.From, null);
        }

        public IBoard Clone()
        {
            var clone = new XiangqiBoard();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    clone._board[row, col] = _board[row, col] != null ? ClonePiece(_board[row, col]) : null;
                }
            }
            return clone;
        }

        private IPiece ClonePiece(IPiece piece)
        {
            return piece.Type switch
            {
                PieceType.Soldier => new Soldier(piece.Color),
                PieceType.Cannon => new Cannon(piece.Color),
                PieceType.Chariot => new Chariot(piece.Color),
                PieceType.Horse => new Horse(piece.Color),
                PieceType.Elephant => new Elephant(piece.Color),
                PieceType.Advisor => new Advisor(piece.Color),
                PieceType.General => new General(piece.Color),
                _ => null
            };
        }

        public IEnumerable<(Position Pos, IPiece Piece)> GetAllPieces()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (_board[row, col] != null)
                        yield return (new Position(row, col), _board[row, col]);
                }
            }
        }
        public IBoard Reset()
        {
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Cols; col++)
                    _board[row, col] = null;
            InitializeStandardPosition();
            return this;
        }



        public IEnumerable<(Position Pos, IPiece Piece)> GetPiecesByColor(Color color)
        {
            foreach (var (pos, piece) in GetAllPieces())
            {
                if (piece.Color == color)
                    yield return (pos, piece);
            }
        }
    }
}
