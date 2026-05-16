// /Modules/Clock/ClockSettings.cs
using System;

namespace ChessXiangqiSolution.Modules.Clock
{
    /// <summary>Loại chế độ đồng hồ</summary>
    public enum ClockMode
    {
        Standard,   // Đếm ngược đơn thuần, không cộng thêm giờ
        Fischer,    // Mỗi nước đi được cộng thêm increment (áp dụng ngay sau khi đi)
        Bronstein   // Mỗi nước đi được cộng thêm tối đa increment, nhưng không vượt quá thời gian đã tiêu tốn
    }

    /// <summary>Cài đặt cho đồng hồ của ván cờ</summary>
    public class ClockSettings
    {
        /// <summary>Thời gian ban đầu cho mỗi bên (tính bằng giây)</summary>
        public int InitialTimeSeconds { get; set; }

        /// <summary>Thời gian cộng thêm mỗi nước (tính bằng giây), áp dụng cho Fischer và Bronstein</summary>
        public int IncrementSeconds { get; set; }

        /// <summary>Chế độ đồng hồ</summary>
        public ClockMode Mode { get; set; }

        /// <summary>Giới hạn thời gian tối đa (Bronstein), mặc định bằng IncrementSeconds</summary>
        public int MaxBronsteinBonusSeconds { get; set; }

        public ClockSettings(int initialTimeSeconds, int incrementSeconds = 0,
                             ClockMode mode = ClockMode.Standard,
                             int maxBronsteinBonusSeconds = -1)
        {
            InitialTimeSeconds = initialTimeSeconds;
            IncrementSeconds = incrementSeconds;
            Mode = mode;
            MaxBronsteinBonusSeconds = maxBronsteinBonusSeconds > 0
                ? maxBronsteinBonusSeconds
                : incrementSeconds; // Mặc định bằng increment
        }

        /// <summary>Tạo cài đặt mặc định (15 phút mỗi bên, Fischer +10s)</summary>
        public static ClockSettings DefaultFischer()
        {
            return new ClockSettings(15 * 60, 10, ClockMode.Fischer);
        }

        /// <summary>Tạo cài đặt cho cờ chớp (3 phút + 2s Fischer)</summary>
        public static ClockSettings Blitz()
        {
            return new ClockSettings(3 * 60, 2, ClockMode.Fischer);
        }
    }
}