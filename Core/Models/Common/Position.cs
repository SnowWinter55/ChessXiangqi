//Position.cs
using Newtonsoft.Json;
using System.Collections.Generic;
using ChessXiangqiSolution.Core.Models.Common;
using System;
using ChessXiangqiSolution.Core.Enums;

namespace ChessXiangqiSolution.Core.Models.Common
{
    /// <summary>
    /// Biểu diễn một ô trên bàn cờ, dùng chung cho Chess và Xiangqi.
    /// Immutable (bất biến) – sau khi tạo không thể thay đổi Row/Col.
    /// </summary>
    public class Position : IEquatable<Position>
    {
        public int Row { get; }  // 0-based, hàng trên cùng là 0
        public int Col { get; }  // 0-based, cột trái cùng là 0

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }

        #region Kiểm tra biên (dùng chung)

        /// <summary>Kiểm tra tọa độ có nằm trong bàn cờ kích thước rows×cols hay không</summary>
        public bool IsValid(int boardRows, int boardCols)
        {
            return Row >= 0 && Row < boardRows && Col >= 0 && Col < boardCols;
        }

        /// <summary>Kiểm tra cho bàn cờ vua 8x8</summary>
        public bool IsValidChess() => IsValid(8, 8);

        /// <summary>Kiểm tra cho bàn cờ tướng 9x10</summary>
        public bool IsValidXiangqi() => IsValid(10, 9); // Chú ý: 10 hàng, 9 cột

        #endregion

        #region Tiện ích

        public int ManhattanDistance(Position other)
        {
            return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col);
        }

        #endregion

        #region Chuyển đổi ký hiệu (Algebraic Notation cho Chess)

        /// <summary>Chuyển từ ký hiệu cờ vua (vd "e2") sang Position (bàn 8x8)</summary>
        public static Position FromChessAlgebraic(string algebraic)
        {
            if (string.IsNullOrEmpty(algebraic) || algebraic.Length < 2)
                throw new ArgumentException("Algebraic notation must have at least 2 chars");

            char fileChar = algebraic[0];   // 'a'..'h'
            char rankChar = algebraic[1];   // '1'..'8'

            int col = fileChar - 'a';
            int row = 8 - (rankChar - '0'); // vì hàng 1 là dưới cùng

            if (!new Position(row, col).IsValidChess())
                throw new ArgumentException($"Position {algebraic} is out of chess board");

            return new Position(row, col);
        }

        /// <summary>Chuyển Position sang ký hiệu cờ vua (vd "e2")</summary>
        public string ToChessAlgebraic()
        {
            if (!IsValidChess())
                throw new InvalidOperationException($"Invalid position ({Row},{Col}) for chess algebraic conversion");
            char fileChar = (char)('a' + Col);
            char rankChar = (char)('1' + (7 - Row)); // 7 vì 8-1- Row
            return $"{fileChar}{rankChar}";
        }

        #endregion

        #region Chuyển đổi ký hiệu cho Cờ Tướng (kiểu "cột,hàng" với cột 1..9, hàng 1..10)

        /// <summary>Chuyển từ chuỗi "cột hàng" (vd "3,5" nghĩa là cột 3, hàng 5) sang Position (0-based). Cột, hàng bắt đầu từ 1.</summary>
        public static Position FromXiangqiCoord(string coord)
        {
            if (string.IsNullOrWhiteSpace(coord))
                throw new ArgumentException("Coordinate cannot be empty");

            var parts = coord.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException("Xiangqi coordinate must be in format 'col,row' (e.g. '3,5')");

            if (!int.TryParse(parts[0], out int col1Based) || !int.TryParse(parts[1], out int row1Based))
                throw new ArgumentException("Invalid number format");

            // Chuyển từ 1-based sang 0-based
            int col = col1Based - 1;
            int row = row1Based - 1;

            var pos = new Position(row, col);
            if (!pos.IsValidXiangqi())
                throw new ArgumentException($"Coordinate {coord} is out of Xiangqi board (9 columns, 10 rows)");
            return pos;
        }

        /// <summary>Chuyển Position sang chuỗi "cột,hàng" (1-based) dành cho cờ tướng</summary>
        public string ToXiangqiCoord()
        {
            if (!IsValidXiangqi())
                throw new InvalidOperationException($"Invalid position ({Row},{Col}) for Xiangqi conversion");
            return $"{Col + 1},{Row + 1}";
        }

        #endregion

        #region Override Equals & GetHashCode

        public bool Equals(Position other)
        {
            if (other is null) return false;
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public override string ToString()
        {
            return $"({Row},{Col})";
        }

        #endregion
    }
}