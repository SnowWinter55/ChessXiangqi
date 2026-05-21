// /Modules/Clock/ClockSettings.cs
using System;
using System.Collections.Generic;

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

        /// <summary>Tạo cài đặt cho cờ Bullet (1 phút + 1s Fischer)</summary>
        public static ClockSettings Bullet()
        {
            return new ClockSettings(1 * 60, 1, ClockMode.Fischer);
        }

        /// <summary>Tạo cài đặt Rapid (10 phút + 0s)</summary>
        public static ClockSettings Rapid()
        {
            return new ClockSettings(10 * 60, 0, ClockMode.Standard);
        }

        /// <summary>Tạo cài đặt Klassic (5 phút + 3s Fischer)</summary>
        public static ClockSettings Klassic()
        {
            return new ClockSettings(5 * 60, 3, ClockMode.Fischer);
        }

        /// <summary>Lấy danh sách tất cả cài đặt sẵn có</summary>
        public static Dictionary<string, ClockSettings> GetPresetClocks()
        {
            return new Dictionary<string, ClockSettings>
            {
                { "Bullet (1p + 1s)", Bullet() },
                { "Blitz (3p + 2s)", Blitz() },
                { "Klassic (5p + 3s)", Klassic() },
                { "Rapid (10p)", Rapid() },
                { "Chuẩn (15p + 10s)", DefaultFischer() }
            };
        }
    }
}