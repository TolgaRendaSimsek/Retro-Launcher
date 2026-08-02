using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RetroLauncher.Services
{
    public class GamePlaytimeRecord
    {
        public string GameId { get; set; } = "";
        public int TotalPlaytimeMinutes { get; set; } = 0;
        public string LastPlayed { get; set; } = ""; // "yyyy-MM-dd HH:mm"
        public int LastSessionMinutes { get; set; } = 0;
        
        // Key: "yyyy-MM-dd" -> minutes
        public Dictionary<string, int> DailyPlaytime { get; set; } = new();
        
        // Key: "yyyy-Www" -> minutes
        public Dictionary<string, int> WeeklyPlaytime { get; set; } = new();
    }

    public class PlaytimeManager
    {
        private static readonly string PlaytimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playtime.json");
        private static readonly object FileLock = new object();
        
        private Dictionary<string, GamePlaytimeRecord> _records = new();
        private Dictionary<string, (DateTime StartTime, int ProcessId)> _activeSessions = new();

        private static PlaytimeManager? _instance;
        public static PlaytimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlaytimeManager();
                }
                return _instance;
            }
        }

        public bool IsSessionActive(string gameId) => _activeSessions.ContainsKey(gameId);
        public DateTime? GetSessionStart(string gameId) => _activeSessions.TryGetValue(gameId, out var s) ? s.StartTime : null;
        public string? ActiveGameId => _activeSessions.Keys.FirstOrDefault();

        public PlaytimeManager()
        {
            LoadPlaytime();
        }

        public void LoadPlaytime()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(PlaytimePath))
                    {
                        string json = File.ReadAllText(PlaytimePath);
                        var list = JsonSerializer.Deserialize<List<GamePlaytimeRecord>>(json);
                        _records = new Dictionary<string, GamePlaytimeRecord>();
                        if (list != null)
                        {
                            foreach (var record in list)
                            {
                                if (!string.IsNullOrEmpty(record.GameId))
                                {
                                    _records[record.GameId] = record;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading playtime: {ex.Message}");
                }
            }
        }

        public void SavePlaytime()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(new List<GamePlaytimeRecord>(_records.Values), options);
                    File.WriteAllText(PlaytimePath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving playtime: {ex.Message}");
                }
            }
        }

        public GamePlaytimeRecord GetOrCreateRecord(string gameId)
        {
            if (!_records.TryGetValue(gameId, out var record))
            {
                record = new GamePlaytimeRecord { GameId = gameId };
                _records[gameId] = record;
            }
            return record;
        }

        public void StartSession(string gameId, int processId)
        {
            _activeSessions[gameId] = (DateTime.Now, processId);
        }

        public int EndSession(string gameId)
        {
            if (!_activeSessions.TryGetValue(gameId, out var session))
            {
                return 0;
            }

            _activeSessions.Remove(gameId);

            int elapsedMinutes = (int)(DateTime.Now - session.StartTime).TotalMinutes;
            if (elapsedMinutes < 1)
            {
                // Round up to 1 minute for testing/minimal play sessions
                elapsedMinutes = 1;
            }

            var record = GetOrCreateRecord(gameId);
            record.TotalPlaytimeMinutes += elapsedMinutes;
            record.LastSessionMinutes = elapsedMinutes;
            
            UpdateLastPlayed(gameId);

            // Record Daily
            string dailyKey = DateTime.Today.ToString("yyyy-MM-dd");
            if (record.DailyPlaytime.ContainsKey(dailyKey))
            {
                record.DailyPlaytime[dailyKey] += elapsedMinutes;
            }
            else
            {
                record.DailyPlaytime[dailyKey] = elapsedMinutes;
            }

            // Record Weekly
            string weeklyKey = GetWeekOfYearKey(DateTime.Today);
            if (record.WeeklyPlaytime.ContainsKey(weeklyKey))
            {
                record.WeeklyPlaytime[weeklyKey] += elapsedMinutes;
            }
            else
            {
                record.WeeklyPlaytime[weeklyKey] = elapsedMinutes;
            }

            SavePlaytime();
            return elapsedMinutes;
        }

        public int GetTotalPlaytime(string gameId)
        {
            return _records.TryGetValue(gameId, out var record) ? record.TotalPlaytimeMinutes : 0;
        }

        public int GetTodayPlaytime(string gameId)
        {
            if (_records.TryGetValue(gameId, out var record))
            {
                string key = DateTime.Today.ToString("yyyy-MM-dd");
                return record.DailyPlaytime.TryGetValue(key, out int mins) ? mins : 0;
            }
            return 0;
        }

        public int GetWeeklyPlaytime(string gameId)
        {
            if (_records.TryGetValue(gameId, out var record))
            {
                string key = GetWeekOfYearKey(DateTime.Today);
                return record.WeeklyPlaytime.TryGetValue(key, out int mins) ? mins : 0;
            }
            return 0;
        }

        public void UpdateLastPlayed(string gameId)
        {
            var record = GetOrCreateRecord(gameId);
            record.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            SavePlaytime();
        }

        public List<GamePlaytimeRecord> GetAllRecords()
        {
            lock (FileLock)
            {
                return new List<GamePlaytimeRecord>(_records.Values);
            }
        }

        public static string GetWeekOfYearKey(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            int weekNum = calendar.GetWeekOfYear(date, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            return $"{date.Year}-W{weekNum:D2}";
        }
    }
}
