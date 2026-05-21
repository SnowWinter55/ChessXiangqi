using System;
using System.Collections.Generic;
using System.Linq;
using ChessXiangqiSolution.Modules.Clock;

namespace ChessXiangqiSolution.UI.ConsoleUI
{
    /// <summary>Giao diện chọn cài đặt đồng hồ (time control)</summary>
    public class ClockSelectionUI
    {
        /// <summary>Hiển thị menu chọn cài đặt đồng hồ (preset hoặc custom)</summary>
        public static ClockSettings SelectClockSettings()
        {
            var presets = ClockSettings.GetPresetClocks();
            var options = new List<string>(presets.Keys)
            {
                "Tùy chỉnh (Custom)"
            };

            int selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== CHỌN KIỂU THỜI GIAN (CLOCK) ===");
                Console.WriteLine("Dùng phím ↑/↓ để chọn, Enter để xác nhận.\n");

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % options.Count;
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        if (selectedIndex == options.Count - 1)
                        {
                            // Tùy chỉnh - tạo custom clock
                            return CreateCustomClockSettings();
                        }
                        else
                        {
                            // Trả về preset đã chọn
                            return presets[options[selectedIndex]];
                        }
                }
            }
        }

        /// <summary>Tạo cài đặt đồng hồ tùy chỉnh từ nhập vào của người dùng</summary>
        private static ClockSettings CreateCustomClockSettings()
        {
            Console.WriteLine("=== TẠO KIỂU THỜI GIAN TÙY CHỈNH ===\n");

            // Nhập thời gian ban đầu (tính bằng phút)
            int initMinutes = 0;
            while (true)
            {
                Console.Write("Nhập thời gian ban đầu (phút): ");
                if (int.TryParse(Console.ReadLine(), out initMinutes) && initMinutes > 0)
                {
                    break;
                }
                Console.WriteLine("Lỗi: Vui lòng nhập một số dương.");
            }

            // Nhập thời gian cộng thêm (tính bằng giây)
            int addSeconds = 0;
            Console.Write("\nNhập thời gian cộng thêm mỗi nước (giây, mặc định 0): ");
            if (!int.TryParse(Console.ReadLine(), out addSeconds) || addSeconds < 0)
            {
                addSeconds = 0;
            }

            // Chọn chế độ đồng hồ
            var mode = SelectClockMode();

            Console.WriteLine("\n✓ Cài đặt tùy chỉnh đã tạo thành công!");
            Console.WriteLine($"  Thời gian: {initMinutes} phút + {addSeconds} giây");
            Console.WriteLine($"  Chế độ: {mode}");
            System.Threading.Thread.Sleep(2000);

            return new ClockSettings(
                initialTimeSeconds: initMinutes * 60,
                incrementSeconds: addSeconds,
                mode: mode
            );
        }

        /// <summary>Chọn chế độ đồng hồ</summary>
        private static ClockMode SelectClockMode()
        {
            var modes = new[]
            {
                ("Standard (không cộng thêm)", ClockMode.Standard),
                ("Fischer (cộng ngay)", ClockMode.Fischer),
                ("Bronstein (cộng có giới hạn)", ClockMode.Bronstein)
            };

            int selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== CHỌN CHẾ ĐỘ ĐỒNG HỒ ===\n");

                for (int i = 0; i < modes.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {modes[i].Item1}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {modes[i].Item1}");
                    }
                }

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + modes.Length) % modes.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % modes.Length;
                        break;
                    case ConsoleKey.Enter:
                        return modes[selectedIndex].Item2;
                }
            }
        }
    }
}
