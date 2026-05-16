// /Modules/Clock/GameClock.cs
using System;
using System.Timers;
using ChessXiangqiSolution.Core.Enums;

namespace ChessXiangqiSolution.Modules.Clock
{
    /// <summary>
    /// Quản lý thời gian của hai bên, bắt đầu/dừng khi đến lượt.
    /// Phát ra sự kiện khi hết giờ, cập nhật từng giây, và khi chuyển lượt.
    /// </summary>
    public class GameClock
    {
        private System.Timers.Timer _timer;
        private DateTime _turnStartTime;
        private bool _isRunning;
        private Color _currentTurn;

        /// <summary>Thời gian còn lại của quân Đen (giây)</summary>
        public int TimeBlackSeconds { get; private set; }

        /// <summary>Thời gian còn lại của quân Đỏ/Trắng (giây)</summary>
        public int TimeWhiteSeconds { get; private set; }

        /// <summary>Cài đặt đồng hồ ban đầu</summary>
        public ClockSettings Settings { get; private set; }

        /// <summary>Sự kiện: hết giờ của một bên</summary>
        public event Action<Color> OnTimeOut;

        /// <summary>Sự kiện: thời gian còn lại thay đổi (mỗi giây)</summary>
        public event Action<Color, int> OnTimeUpdated;

        /// <summary>Sự kiện: khi bắt đầu lượt mới (gửi màu và thời gian còn lại)</summary>
        public event Action<Color, int, int> OnTurnStarted; // (màu, thời gian bên đó, thời gian đối thủ)

        public GameClock(ClockSettings settings)
        {
            Settings = settings;
            TimeWhiteSeconds = settings.InitialTimeSeconds;
            TimeBlackSeconds = settings.InitialTimeSeconds;
            _timer = new System.Timers.Timer(1000); // 1 giây
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
        }

        /// <summary>Bắt đầu lượt cho một màu (dừng lượt cũ nếu đang chạy)</summary>
        public void StartTurn(Color color)
        {
            if (_isRunning)
                StopTurn();

            _currentTurn = color;
            _turnStartTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();

            int currentTime = GetCurrentTime(color);
            int opponentTime = GetCurrentTime(GetOppositeColor(color));
            OnTurnStarted?.Invoke(color, currentTime, opponentTime);
        }

        /// <summary>Kết thúc lượt hiện tại, cập nhật thời gian đã tiêu tốn và cộng increment</summary>
        public void StopTurn()
        {
            if (!_isRunning) return;

            _timer.Stop();
            var elapsed = (DateTime.UtcNow - _turnStartTime).TotalSeconds;
            int elapsedInt = (int)Math.Ceiling(elapsed); // Làm tròn lên để tránh âm
            if (elapsedInt < 0) elapsedInt = 0;

            // Trừ thời gian đã dùng
            if (_currentTurn == Color.White)
                TimeWhiteSeconds = Math.Max(0, TimeWhiteSeconds - elapsedInt);
            else
                TimeBlackSeconds = Math.Max(0, TimeBlackSeconds - elapsedInt);

            // Cộng increment theo chế độ
            AddIncrement(_currentTurn, elapsedInt);

            _isRunning = false;
            OnTimeUpdated?.Invoke(_currentTurn, GetCurrentTime(_currentTurn));
        }

        private void AddIncrement(Color color, int elapsedSeconds)
        {
            switch (Settings.Mode)
            {
                case ClockMode.Fischer:
                    // Cộng thêm incrementSeconds ngay sau khi kết thúc nước đi
                    AddTimeTo(color, Settings.IncrementSeconds);
                    break;
                case ClockMode.Bronstein:
                    // Cộng tối đa incrementSeconds, nhưng không quá thời gian đã tiêu tốn
                    int bonus = Math.Min(Settings.IncrementSeconds, elapsedSeconds);
                    bonus = Math.Min(bonus, Settings.MaxBronsteinBonusSeconds);
                    AddTimeTo(color, bonus);
                    break;
                default:
                    break;
            }
        }

        private void AddTimeTo(Color color, int seconds)
        {
            if (seconds <= 0) return;
            if (color == Color.White)
                TimeWhiteSeconds += seconds;
            else
                TimeBlackSeconds += seconds;
            OnTimeUpdated?.Invoke(color, GetCurrentTime(color));
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Mỗi giây, tính lại thời gian còn lại dựa trên thời điểm bắt đầu
            if (!_isRunning) return;

            var elapsed = (DateTime.UtcNow - _turnStartTime).TotalSeconds;
            int remaining = GetCurrentTime(_currentTurn) - (int)Math.Ceiling(elapsed);
            remaining = Math.Max(0, remaining);

            // Gửi sự kiện cập nhật (UI có thể hiển thị)
            OnTimeUpdated?.Invoke(_currentTurn, remaining);

            if (remaining <= 0)
            {
                // Hết giờ
                StopTurn(); // dừng timer, không cộng increment
                OnTimeOut?.Invoke(_currentTurn);
            }
        }

        private int GetCurrentTime(Color color)
        {
            return color == Color.White ? TimeWhiteSeconds : TimeBlackSeconds;
        }

        private Color GetOppositeColor(Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }

        /// <summary>Đặt lại đồng hồ về thời gian khởi tạo</summary>
        public void Reset()
        {
            bool wasRunning = _isRunning;
            if (wasRunning) StopTurn();
            TimeWhiteSeconds = Settings.InitialTimeSeconds;
            TimeBlackSeconds = Settings.InitialTimeSeconds;
            _isRunning = false;
            if (wasRunning) StartTurn(_currentTurn); // Nếu có thể
        }

        /// <summary>Lấy thời gian còn lại của một bên (chuẩn bị hiển thị)</summary>
        public int GetRemainingTime(Color color) => GetCurrentTime(color);
    }
}