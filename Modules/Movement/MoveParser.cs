// Module/Movement/MoveParser.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChessXiangqiSolution.Core.Enums;
using ChessXiangqiSolution.Core.Interfaces;
using ChessXiangqiSolution.Core.Models.Common;

namespace ChessXiangqiSolution.Modules.Movement
{
    /// <summary>
    /// Parse nước đi từ chuỗi nhập.
    ///
    /// Cờ vua: tọa độ (e2e4) hoặc SAN (Nf3, O-O, exd5…)
    ///
    /// Cờ tướng: ký hiệu tiếng Việt dạng
    ///   [Quân][Disambig?][ColNgườiDùng][Dir][ColĐích]
    ///
    ///   Quân (hoa hoặc thường):
    ///     Tg/tg = Tướng (General)
    ///     S/s   = Sĩ    (Advisor)
    ///     T/t   = Tịnh  (Elephant)
    ///     X/x   = Xe    (Chariot)
    ///     P/p   = Pháo  (Cannon)
    ///     M/m   = Mã    (Horse)
    ///     B/b   = Binh  (Soldier)
    ///
    ///   Disambig (tùy chọn, xuất hiện khi nhiều quân cùng loại cùng cột):
    ///     t / s          = trước / sau  (dùng khi có đúng 2 quân)
    ///     (hN)           = quân đứng ở hàng N tuyệt đối (1-10 theo góc nhìn người chơi)
    ///
    ///   Dir: + hoặc t = tiến | - hoặc l = lui | = hoặc b = bằng (ngang)
    ///
    ///   ColĐích: số 1-9 theo góc nhìn người chơi
    ///     - Xe/Pháo/Tốt đi ngang: cột đích
    ///     - Xe/Pháo tiến/lui:     số bước
    ///     - Mã/Tượng:             cột đích (hàng suy ra từ hướng + luật)
    ///     - Tướng/Sĩ:             cột đích
    ///     - Tốt tiến:             số bước (luôn = 1, có thể bỏ qua)
    /// </summary>
    public static class MoveParser
    {
        // ─────────────────────────────────────────────────────────────
        // Public entry point
        // ─────────────────────────────────────────────────────────────

        public static bool TryParse(string input, IBoard board, Color currentTurn, out Move move)
        {
            move = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();

            if (TryParseCoordinate(input, board, currentTurn, out move))
                return true;

            if (board.GameType == GameType.Xiangqi)
                return TryParseXiangqiSan(input, board, currentTurn, out move);

            return TryParseChessSan(input, board, currentTurn, out move);
        }

        // ─────────────────────────────────────────────────────────────
        // Tọa độ thuần (dùng chung Chess + Xiangqi)
        // ─────────────────────────────────────────────────────────────

        private static bool TryParseCoordinate(string input, IBoard board, Color currentTurn, out Move move)
        {
            move = null;

            if (board.GameType == GameType.Chess)
            {
                if (input.Length != 4) return false;
                try
                {
                    var from = Position.FromChessAlgebraic(input.Substring(0, 2));
                    var to   = Position.FromChessAlgebraic(input.Substring(2, 2));
                    move = new Move(from, to);
                    return true;
                }
                catch { return false; }
            }

            // Xiangqi tọa độ dạng "cột,hàng - cột,hàng" (theo góc nhìn người chơi)
            var m = Regex.Match(input, @"^(\d{1,2}),(\d{1,2})\s*[-\s]\s*(\d{1,2}),(\d{1,2})$");
            if (!m.Success) return false;
            try
            {
                int userFromCol = int.Parse(m.Groups[1].Value);
                int userFromRow = int.Parse(m.Groups[2].Value);
                int userToCol   = int.Parse(m.Groups[3].Value);
                int userToRow   = int.Parse(m.Groups[4].Value);

                // Áp dụng mirroring cho White (quân đỏ)
                int internalFromCol = UserColToInternal(userFromCol, currentTurn);
                int internalFromRow = UserRowToInternal(userFromRow, currentTurn, board.Rows);
                int internalToCol   = UserColToInternal(userToCol, currentTurn);
                int internalToRow   = UserRowToInternal(userToRow, currentTurn, board.Rows);

                var from = new Position(internalFromRow, internalFromCol);
                var to   = new Position(internalToRow, internalToCol);

                if (!board.IsValidPos(from) || !board.IsValidPos(to))
                    return false;

                move = new Move(from, to);
                return true;
            }
            catch { return false; }
        }

        // ─────────────────────────────────────────────────────────────
        // Chess SAN (giữ nguyên logic cũ)
        // ─────────────────────────────────────────────────────────────

        private static bool TryParseChessSan(string san, IBoard board, Color currentTurn, out Move move)
        {
            move = null;
            string upper = san.ToUpperInvariant();
            string clean = Regex.Replace(upper, @"[+#!?]", "");

            if (clean == "O-O"   || clean == "0-0")
                return TryParseCastling(board, currentTurn, false, out move);
            if (clean == "O-O-O" || clean == "0-0-0")
                return TryParseCastling(board, currentTurn, true,  out move);

            var match = Regex.Match(clean, @"^([KQRNB])?([A-H])?([1-8])?X?([A-H][1-8])$");
            if (!match.Success) return false;

            string pieceLetter = match.Groups[1].Value;
            string fromFile    = match.Groups[2].Value;
            string fromRank    = match.Groups[3].Value;
            string toSquare    = match.Groups[4].Value;

            string toSquareLower = toSquare[0].ToString().ToLowerInvariant() + toSquare[1];
            Position to;
            try { to = Position.FromChessAlgebraic(toSquareLower); }
            catch { return false; }

            PieceType pieceType = pieceLetter switch
            {
                "K" => PieceType.King,
                "Q" => PieceType.Queen,
                "R" => PieceType.Rook,
                "N" => PieceType.Knight,
                "B" => PieceType.Bishop,
                _   => PieceType.Pawn
            };

            var candidates = new List<(Position pos, IPiece piece)>();
            foreach (var (pos, piece) in board.GetPiecesByColor(currentTurn))
            {
                if (piece.Type != pieceType) continue;
                if (!string.IsNullOrEmpty(fromFile))
                {
                    char ef = char.ToLowerInvariant(fromFile[0]);
                    if (pos.ToChessAlgebraic()[0] != ef) continue;
                }
                if (!string.IsNullOrEmpty(fromRank))
                {
                    if (pos.ToChessAlgebraic()[1] != fromRank[0]) continue;
                }
                if (piece.GetValidMoves(board, pos).Any(mv => mv.Equals(to)))
                    candidates.Add((pos, piece));
            }

            if (candidates.Count != 1) return false;

            move = new Move(candidates[0].pos, to);
            if (pieceType == PieceType.Pawn
                && board.IsEmpty(to)
                && Math.Abs(move.From.Col - move.To.Col) == 1)
            {
                move.IsEnPassant    = true;
                move.CapturedPiece  = board.GetPieceAt(new Position(move.From.Row, move.To.Col));
            }
            else
            {
                move.CapturedPiece = board.GetPieceAt(to);
            }
            return true;
        }

        private static bool TryParseCastling(IBoard board, Color currentTurn, bool isQueenSide, out Move move)
        {
            move = null;
            int backRow  = currentTurn == Color.White ? 7 : 0;
            var kingFrom = new Position(backRow, 4);
            var kingTo   = isQueenSide ? new Position(backRow, 2) : new Position(backRow, 6);
            var king     = board.GetPieceAt(kingFrom);
            if (king?.Type != PieceType.King || king.Color != currentTurn) return false;
            move = new Move(kingFrom, kingTo) { IsCastling = true };
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Xiangqi SAN
        // ─────────────────────────────────────────────────────────────

        // Regex tổng quát cho một nước đi cờ tướng:
        //
        //   Group 1 (piece)  : Tg|tg|[SsTtXxPpMmBb]
        //   Group 2 (disambig-coord) : \(h(\d{1,2})\)   → số hàng tuyệt đối (góc nhìn người chơi)
        //   Group 3 (disambig-ts)    : [ts]              → trước / sau
        //   Group 4 (fromCol): [1-9]
        //   Group 5 (dir)    : [+\-=tTlLbB]
        //   Group 6 (toNum)  : [1-9]
        //   Hậu tố [+#!?]* bỏ qua
        //
        private static readonly Regex XiangqiSanRegex = new Regex(
            @"^(Tg|tg|[SsTtXxPpMmBb])" +  // quân
            @"(?:\(h(\d{1,2})\)|([ts]))?" + // disambig tọa độ hoặc trước/sau
            @"([1-9])"                      + // cột xuất phát (góc nhìn người chơi)
            @"([+\-=tTlLbB])"              + // hướng
            @"([1-9])"                      + // số đích
            @"[+#!?]*$",
            RegexOptions.None);

        private static bool TryParseXiangqiSan(string san, IBoard board, Color currentTurn, out Move move)
        {
            move = null;

            var m = XiangqiSanRegex.Match(san.Trim());
            if (!m.Success) return false;

            // ── 1. Parse các nhóm token ──────────────────────────────
            string pieceToken   = m.Groups[1].Value;
            string hRowStr      = m.Groups[2].Value;   // hàng tuyệt đối (góc người chơi), rỗng nếu không có
            string tsToken      = m.Groups[3].Value;   // "t" hoặc "s", rỗng nếu không có
            int    userFromCol  = int.Parse(m.Groups[4].Value); // 1-9 góc người chơi
            char   dirChar      = char.ToLowerInvariant(m.Groups[5].Value[0]);
            int    toNum        = int.Parse(m.Groups[6].Value); // 1-9 góc người chơi

            // ── 2. Map ký hiệu quân → PieceType ─────────────────────
            PieceType pieceType = MapPieceToken(pieceToken);
            if (pieceType != PieceType.General && pieceType != PieceType.Soldier && pieceType != PieceType.Cannon && pieceType != PieceType.Chariot && pieceType != PieceType.Horse && pieceType != PieceType.Elephant && pieceType != PieceType.Advisor) return false;

            // ── 3. Chuẩn hóa hướng → enum Direction ─────────────────
            MoveDirection dir = dirChar switch
            {
                '+' or 't' => MoveDirection.Forward,
                '-' or 'l' => MoveDirection.Backward,
                '=' or 'b' => MoveDirection.Sideways,
                _           => MoveDirection.Unknown
            };
            if (dir == MoveDirection.Unknown) return false;

            // ── 4. Chuyển cột người dùng → col nội bộ (0-based) ─────
            //   White: cột 1 người dùng = col 8 nội bộ (đếm từ phải sang trái)
            //   Black: cột 1 người dùng = col 0 nội bộ (đếm từ trái sang phải)
            int internalFromCol = UserColToInternal(userFromCol, currentTurn);

            // ── 5. Lọc candidates cùng loại + cùng cột ──────────────
            var allOfType = board.GetPiecesByColor(currentTurn)
                .Where(x => x.Piece.Type == pieceType && x.Pos.Col == internalFromCol)
                .ToList();

            if (allOfType.Count == 0) return false;

            // ── 6. Resolve disambiguator ──────────────────────────────
            Position fromPos;

            if (!string.IsNullOrEmpty(hRowStr))
            {
                // Dạng (hN): hàng N theo góc nhìn người chơi
                int userRow      = int.Parse(hRowStr);
                int internalRow  = UserRowToInternal(userRow, currentTurn, board.Rows);
                var found        = allOfType.Where(x => x.Pos.Row == internalRow).ToList();
                if (found.Count != 1) return false;
                fromPos = found[0].Pos;
            }
            else if (!string.IsNullOrEmpty(tsToken))
            {
                // Dạng t/s: sắp xếp theo hướng gần địch
                // "trước" = gần địch hơn = White: row nhỏ hơn, Black: row lớn hơn
                var sorted = currentTurn == Color.White
                    ? allOfType.OrderBy(x => x.Pos.Row).ToList()   // row nhỏ = gần địch với White
                    : allOfType.OrderByDescending(x => x.Pos.Row).ToList();
                if (sorted.Count < 2) return false; // có đúng 1 quân thì không cần t/s

                fromPos = tsToken == "t" ? sorted[0].Pos : sorted[^1].Pos;
            }
            else
            {
                // Không có disambiguator: phải tìm đúng 1 quân
                if (allOfType.Count != 1) return false;
                fromPos = allOfType[0].Pos;
            }

            // ── 7. Tính ô đích (toPos) theo loại quân + hướng ────────
            Position toPos = CalculateToPosition(pieceType, fromPos, dir, toNum, currentTurn, board);
            if (toPos == null) return false;
            if (!board.IsValidPos(toPos)) return false;

            // ── 8. Kiểm tra quân đích không phải quân mình ───────────
            var occupant = board.GetPieceAt(toPos);
            if (occupant != null && occupant.Color == currentTurn) return false;

            // ── 9. Xác nhận nước đi nằm trong tập hợp hợp lệ của quân
            var piece = board.GetPieceAt(fromPos);
            if (piece == null) return false;
            var validMoves = piece.GetValidMoves(board, fromPos);
            if (!validMoves.Any(v => v.Row == toPos.Row && v.Col == toPos.Col)) return false;

            move = new Move(fromPos, toPos)
            {
                CapturedPiece = occupant
            };
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers: mapping & coordinate conversion
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Map ký hiệu quân tiếng Việt (hoa/thường) → PieceType.
        /// Tg/tg → General | S/s → Advisor | T/t → Elephant
        /// X/x   → Chariot | P/p → Cannon  | M/m → Horse | B/b → Soldier
        /// </summary>
        private static PieceType MapPieceToken(string token)
        {
            return token.ToLowerInvariant() switch
            {
                "tg" => PieceType.General,
                "s"  => PieceType.Advisor,
                "t"  => PieceType.Elephant,
                "x"  => PieceType.Chariot,
                "p"  => PieceType.Cannon,
                "m"  => PieceType.Horse,
                "b"  => PieceType.Soldier,
            };
        }

        /// <summary>
        /// Chuyển cột theo góc nhìn người chơi (1-9) sang col nội bộ 0-based.
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// userCol 1-9 → internal col 0-8.
        /// </summary>
        private static int UserColToInternal(int userCol, Color color)
        {
            return userCol - 1;
        }

        /// <summary>
        /// Chuyển hàng theo góc nhìn người chơi (1-10 từ dưới cùng) sang row nội bộ 0-based.
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// userRow 1-10 → internal row 9-0 (hàng 1 = row 9, hàng 10 = row 0).
        /// </summary>
        private static int UserRowToInternal(int userRow, Color color, int totalRows)
        {
            return totalRows - userRow;
        }

        /// <summary>
        /// Chuyển col nội bộ 0-based sang cột theo góc nhìn người chơi (1-9).
        /// Cả hai màu sử dụng cùng hệ thống tọa độ (không mirror).
        /// internal col 0-8 → user col 1-9.
        /// </summary>
        private static int InternalColToUser(int internalCol, Color color)
        {
            return internalCol + 1;
        }

        // ─────────────────────────────────────────────────────────────
        // Tính ô đích từ ô xuất phát + hướng + toNum
        // ─────────────────────────────────────────────────────────────

        private static Position CalculateToPosition(
            PieceType type, Position from, MoveDirection dir, int toNum,
            Color color, IBoard board)
        {
            // Chiều tiến về mặt row nội bộ:
            //   White tiến → row giảm (forward = -1)
            //   Black tiến → row tăng (forward = +1)
            int forward = color == Color.White ? -1 : 1;

            switch (type)
            {
                // ── Xe, Pháo ───────────────────────────────────────
                //   Tiến/lui: toNum = số bước → row thay đổi
                //   Ngang   : toNum = cột đích (góc người chơi)
                case PieceType.Chariot:
                case PieceType.Cannon:
                    return dir switch
                    {
                        MoveDirection.Forward  => new Position(from.Row + forward * toNum, from.Col),
                        MoveDirection.Backward => new Position(from.Row - forward * toNum, from.Col),
                        MoveDirection.Sideways => new Position(from.Row, UserColToInternal(toNum, color)),
                        _ => null
                    };

                // ── Tốt / Binh ─────────────────────────────────────
                //   Tiến: 1 bước (toNum thường = 1, bắt buộc)
                //   Ngang: cột đích (góc người chơi); chỉ hợp lệ sau khi qua sông
                case PieceType.Soldier:
                    return dir switch
                    {
                        MoveDirection.Forward  => new Position(from.Row + forward, from.Col),
                        MoveDirection.Sideways => new Position(from.Row, UserColToInternal(toNum, color)),
                        MoveDirection.Backward => null, // Tốt không được lui
                        _ => null
                    };

                // ── Tướng ──────────────────────────────────────────
                //   Tiến/lui: 1 bước theo hàng
                //   Ngang: cột đích
                case PieceType.General:
                    return dir switch
                    {
                        MoveDirection.Forward  => new Position(from.Row + forward, from.Col),
                        MoveDirection.Backward => new Position(from.Row - forward, from.Col),
                        MoveDirection.Sideways => new Position(from.Row, UserColToInternal(toNum, color)),
                        _ => null
                    };

                // ── Sĩ ─────────────────────────────────────────────
                //   toNum = cột đích (góc người chơi)
                //   hàng: tiến → +forward, lui → -forward
                case PieceType.Advisor:
                {
                    int toCol = UserColToInternal(toNum, color);
                    int toRow = dir == MoveDirection.Forward
                        ? from.Row + forward
                        : from.Row - forward;
                    return new Position(toRow, toCol);
                }

                // ── Tịnh / Tượng ───────────────────────────────────
                //   toNum = cột đích (góc người chơi)
                //   Tượng đi chéo 2 ô → hàng thay đổi ±2
                //   Trong tất cả ô đích hợp lệ của Tượng, chọn ô có col == toCol
                //   và hướng (tiến/lui) khớp với dir
                case PieceType.Elephant:
                {
                    int toCol     = UserColToInternal(toNum, color);
                    int rowDelta  = dir == MoveDirection.Forward ? forward * 2 : -forward * 2;
                    return new Position(from.Row + rowDelta, toCol);
                }

                // ── Mã ─────────────────────────────────────────────
                //   toNum = cột đích (góc người chơi)
                //   Mã đi 2-1 hoặc 1-2.
                //   dir (tiến/lui) xác định nhóm ô đích:
                //     Tiến: hàng đích = from.Row + forward*2 (bước 2 hàng) hoặc
                //           hàng đích = from.Row + forward*1 (bước 1 hàng rồi 2 cột)
                //   Trong tất cả ô Mã hợp lệ, chọn ô có col == toCol và dir đúng
                case PieceType.Horse:
                {
                    int toCol = UserColToInternal(toNum, color);
                    // Tìm trong tập nước đi hợp lệ của Mã: ô nào có col == toCol
                    // và hướng row phù hợp với dir
                    var horsePiece = board.GetPieceAt(from);
                    if (horsePiece == null) return null;
                    var validMoves = horsePiece.GetValidMoves(board, from);
                    foreach (var v in validMoves)
                    {
                        if (v.Col != toCol) continue;
                        int rowDiff = v.Row - from.Row; // dương = xuống (Black forward), âm = lên (White forward)
                        bool isForward = (rowDiff * forward) > 0;
                        if (dir == MoveDirection.Forward  && isForward)  return v;
                        if (dir == MoveDirection.Backward && !isForward) return v;
                    }
                    return null;
                }

                default:
                    return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Direction enum nội bộ (tránh phụ thuộc enum ngoài)
        // ─────────────────────────────────────────────────────────────

        private enum MoveDirection { Forward, Backward, Sideways, Unknown }
    }
}