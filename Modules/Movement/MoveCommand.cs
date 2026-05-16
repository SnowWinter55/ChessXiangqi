using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Models.Chess.Pieces;
using ChessXiangqiSolution.Modules.Notation;

public class MoveCommand : ICommand
{
    private readonly IBoard _board;
    private readonly Move _move;
    public string San { get; private set; }  // Lưu SAN để dùng khi redo
    private IPiece _capturedPiece;
    private Position _enPassantCapturePos;
    private IPiece _movingPiece;
    private IPiece _promotedFrom;
    private IPiece _promotedTo;
    private bool _isCastling;
    private Position _rookFrom;
    private Position _rookTo;

    public Move Move => _move;   // ← property để lấy nước đi

    public MoveCommand(IBoard board, Move move)
    {
        _board = board;
        _move = move;
    }

    public void Execute()
    {
        _movingPiece = _board.GetPieceAt(_move.From);
        _capturedPiece = _board.GetPieceAt(_move.To);
        _isCastling = _move.IsCastling;
        _promotedFrom = null;
        _promotedTo = null;

        if (_move.IsEnPassant)
        {
            _enPassantCapturePos = new Position(_move.From.Row, _move.To.Col);
            _capturedPiece = _board.GetPieceAt(_enPassantCapturePos);
        }

        _move.CapturedPiece = _capturedPiece;
        if (string.IsNullOrEmpty(_move.San))
            _move.San = SanFormatter.FormatSan(_move, _board);
        San = _move.San;

        if (_isCastling)
        {
            int backRow = (_movingPiece.Color == Color.White) ? 7 : 0;
            int rookFromCol = (_move.To.Col > _move.From.Col) ? 7 : 0;
            int rookToCol = (_move.To.Col > _move.From.Col) ? 5 : 3;
            _rookFrom = new Position(backRow, rookFromCol);
            _rookTo = new Position(backRow, rookToCol);
        }

        if (_move.PromotionPiece.HasValue && _movingPiece.Type == PieceType.Pawn)
        {
            _promotedFrom = _movingPiece;
            _promotedTo = CreatePiece(_move.PromotionPiece.Value, _movingPiece.Color);
        }

        _board.MakeMove(_move);
    }

    public void Undo()
    {
        // Phục hồi trạng thái bàn cờ
        if (_isCastling)
        {
            // Di chuyển xe về vị trí cũ
            IPiece rook = _board.GetPieceAt(_rookTo);
            _board.SetPieceAt(_rookFrom, rook);
            _board.SetPieceAt(_rookTo, null);
        }

        if (_move.IsEnPassant)
        {
            // Khôi phục quân bị ăn en passant tại vị trí en-passant
            _board.SetPieceAt(_enPassantCapturePos, _capturedPiece);

            // Phục hồi quân di chuyển về ô nguồn
            if (_promotedFrom != null)
                _board.SetPieceAt(_move.From, _promotedFrom);
            else
                _board.SetPieceAt(_move.From, _movingPiece);

            // Trong en-passant, ô đích (To) sẽ rỗng sau khi undo
            _board.SetPieceAt(_move.To, null);
        }
        else
        {
            // Khôi phục quân bị ăn bình thường vào ô đích
            _board.SetPieceAt(_move.To, _capturedPiece);

            // Phục hồi quân di chuyển về ô nguồn
            if (_promotedFrom != null)
                _board.SetPieceAt(_move.From, _promotedFrom);
            else
                _board.SetPieceAt(_move.From, _movingPiece);

            // Không xóa ô To ở trường hợp capture bình thường — ô To phải giữ quân bị ăn
        }
    }

    public void Redo()
    {
        Execute();  // Redo bằng cách thực hiện lại
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
}