using System;
using System.IO;
using ChessXiangqiSolution.Modules.Clock;

namespace ChessXiangqiSolution.Modules.Notation
{
    /// <summary>
    /// Manages game records storage location and default settings
    /// </summary>
    public static class GameRecordsManager
    {
        /// <summary>
        /// Get the project root directory by navigating up from BaseDirectory
        /// BaseDirectory: bin/Debug/net10.0 -> up 3 levels -> project root
        /// </summary>
        private static string GetProjectRootPath()
        {
            var baseDir = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            // Navigate up: net10.0 -> Debug -> bin -> project root
            var projectRoot = baseDir.Parent?.Parent?.Parent;
            return projectRoot?.FullName ?? AppDomain.CurrentDomain.BaseDirectory;
        }

        private static readonly string GameRecordsPath = Path.Combine(
            GetProjectRootPath(), 
            "GameRecords"
        );

        /// <summary>
        /// Get the game records directory path, creating it if it doesn't exist
        /// </summary>
        public static string GetGameRecordsPath()
        {
            if (!Directory.Exists(GameRecordsPath))
            {
                Directory.CreateDirectory(GameRecordsPath);
            }
            return GameRecordsPath;
        }

        /// <summary>
        /// Get default clock settings for replay mode (no time limit, just for initialization)
        /// </summary>
        public static ClockSettings GetDefaultClockSettings()
        {
            // Return a default clock setting with large time limit for replay mode
            // (replay mode won't actually use the clock, this is just to initialize AppController)
            return new ClockSettings(
                initialTimeSeconds: 367000, // super big number (not actually used)
                incrementSeconds: 0,
                mode: ClockMode.Standard
            );
        }

        /// <summary>
        /// Get full path for a game file in GameRecords directory
        /// </summary>
        public static string GetGameFilePath(string filename)
        {
            return Path.Combine(GetGameRecordsPath(), filename);
        }

        /// <summary>
        /// List all game record files in GameRecords directory
        /// </summary>
        public static string[] GetGameRecordFiles()
        {
            try
            {
                return Directory.GetFiles(GetGameRecordsPath(), "*");
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Auto-parse and resolve file path. If input is just a filename, prepend GameRecords path.
        /// If input is absolute path, use as-is. If relative path detected, combine with GameRecords path.
        /// </summary>
        public static string ResolveGameFilePath(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            string trimmedInput = userInput.Trim();

            // If it's already an absolute path, return it as-is
            if (Path.IsPathRooted(trimmedInput))
            {
                return trimmedInput;
            }

            // If it contains path separators, it's a relative path - combine with GameRecords
            if (trimmedInput.Contains("\\") || trimmedInput.Contains("/"))
            {
                return Path.Combine(GetGameRecordsPath(), trimmedInput);
            }

            // Otherwise, treat as just a filename and combine with GameRecords
            return Path.Combine(GetGameRecordsPath(), trimmedInput);
        }

        /// <summary>
        /// Display available game record files in a nice formatted list
        /// </summary>
        public static void DisplayAvailableGameRecords()
        {
            var files = GetGameRecordFiles();
            
            if (files.Length == 0)
            {
                Console.WriteLine("No game records found in GameRecords folder.\n");
                return;
            }

            Console.WriteLine("Available game records:");
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileName(files[i]);
                var fileInfo = new FileInfo(files[i]);
                string size = fileInfo.Length > 1024 
                    ? $"{fileInfo.Length / 1024} KB" 
                    : $"{fileInfo.Length} B";
                Console.WriteLine($"  {i + 1}. {fileName} ({size})");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Validate if a resolved file path exists and is accessible
        /// </summary>
        public static bool ValidateGameFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                return File.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save a game record as PGN format (for both Chess and Xiangqi)
        /// </summary>
        public static void SaveGameAsPgn(string filePath, MatchRecord record)
        {
            try
            {
                var notationExporter = new NotationExporter();
                notationExporter.ExportToFile(filePath, record);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save game as PGN: {ex.Message}", ex);
            }
        }
    }
}
