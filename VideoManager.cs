using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class VideoMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GameId { get; set; } = "";
        public string Title { get; set; } = "";
        public string FilePath { get; set; } = ""; // Relative to base dir
        public string CaptureDate { get; set; } = "";
        public string Duration { get; set; } = "";
    }

    public class VideoDatabase
    {
        public string FFmpegPath { get; set; } = "";
        public List<VideoMetadata> Clips { get; set; } = new();
    }

    public class VideoManager
    {
        private static readonly string VideosJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos.json");
        private static readonly string VideosBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos");
        private static readonly object FileLock = new object();

        private VideoDatabase _db = new();
        private static VideoManager? _instance;

        private Process? _ffmpegProcess;
        private string? _activeGameId;
        private string? _activeOutputPath;
        private DateTime _recordStartTime;

        public static VideoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VideoManager();
                }
                return _instance;
            }
        }

        public bool IsRecording => _ffmpegProcess != null && !_ffmpegProcess.HasExited;
        public string? ActiveGameId => _activeGameId;
        public int GetRecordingDurationSeconds() => IsRecording ? (int)(DateTime.Now - _recordStartTime).TotalSeconds : 0;

        public VideoManager()
        {
            LoadDatabase();
            if (string.IsNullOrEmpty(_db.FFmpegPath))
            {
                // Default path
                _db.FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "ffmpeg", "ffmpeg.exe");
            }
        }

        public string FFmpegPath
        {
            get => _db.FFmpegPath;
            set
            {
                _db.FFmpegPath = value;
                SaveDatabase();
            }
        }

        public bool IsFFmpegAvailable()
        {
            if (string.IsNullOrEmpty(_db.FFmpegPath)) return false;
            return File.Exists(_db.FFmpegPath);
        }

        public bool ShowFFmpegSetupWarning()
        {
            var result = MessageBox.Show(
                "FFmpeg executable was not found in 'tools/ffmpeg/ffmpeg.exe' or system PATH.\n\n" +
                "RetroLauncher requires FFmpeg to record gameplay clips.\n\n" +
                "Would you like to manually locate ffmpeg.exe now?",
                "FFmpeg Setup Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "FFmpeg Executable (ffmpeg.exe)|ffmpeg.exe";
                    ofd.Title = "Locate ffmpeg.exe";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        FFmpegPath = ofd.FileName;
                        return true;
                    }
                }
            }
            return false;
        }

        public void LoadDatabase()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(VideosJsonPath))
                    {
                        string json = File.ReadAllText(VideosJsonPath);
                        var db = JsonSerializer.Deserialize<VideoDatabase>(json);
                        if (db != null)
                        {
                            _db = db;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading videos.json: {ex.Message}");
                }
            }
        }

        public void SaveDatabase()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_db, options);
                    File.WriteAllText(VideosJsonPath, json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving videos.json: {ex.Message}");
                }
            }
        }

        public bool StartRecording(string gameId)
        {
            if (IsRecording) return false;

            if (!IsFFmpegAvailable())
            {
                if (!ShowFFmpegSetupWarning()) return false;
            }

            try
            {
                string gameFolder = Path.Combine(VideosBaseDir, gameId);
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                string filename = $"video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                string relativePath = Path.Combine("videos", gameId, filename);
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                // Run FFmpeg in background capturing Windows desktop using GDI+
                // Using ultrafast preset to minimize CPU impact on running emulators
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = _db.FFmpegPath,
                    Arguments = $"-y -f gdigrab -framerate 30 -i desktop -c:v libx264 -pix_fmt yuv420p -crf 25 -preset ultrafast -an \"{absolutePath}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process? p = Process.Start(psi);
                if (p != null)
                {
                    _ffmpegProcess = p;
                    _activeGameId = gameId;
                    _activeOutputPath = relativePath;
                    _recordStartTime = DateTime.Now;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start video recording: {ex.Message}", "Recording Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        public VideoMetadata? StopRecording()
        {
            if (!IsRecording || _ffmpegProcess == null || _activeGameId == null || _activeOutputPath == null)
            {
                return null;
            }

            try
            {
                // Send 'q' command on StandardInput to stop FFmpeg cleanly
                _ffmpegProcess.StandardInput.WriteLine("q");
                _ffmpegProcess.StandardInput.Flush();

                if (!_ffmpegProcess.WaitForExit(5000))
                {
                    _ffmpegProcess.Kill();
                }

                int durationSeconds = (int)(DateTime.Now - _recordStartTime).TotalSeconds;
                int mins = durationSeconds / 60;
                int secs = durationSeconds % 60;
                string durationStr = $"{mins:D2}:{secs:D2}";

                var clip = new VideoMetadata
                {
                    Id = Guid.NewGuid().ToString(),
                    GameId = _activeGameId,
                    Title = $"Gameplay Clip {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    FilePath = _activeOutputPath,
                    CaptureDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Duration = durationStr
                };

                lock (FileLock)
                {
                    _db.Clips.Add(clip);
                }
                SaveDatabase();

                _ffmpegProcess.Dispose();
                _ffmpegProcess = null;
                _activeGameId = null;
                _activeOutputPath = null;

                // Play system sound notification
                System.Media.SystemSounds.Beep.Play();

                return clip;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping video recording: {ex.Message}");
                _ffmpegProcess = null;
                return null;
            }
        }

        public List<VideoMetadata> GetVideos(string gameId)
        {
            lock (FileLock)
            {
                return _db.Clips.Where(v => v.GameId == gameId)
                                .OrderByDescending(v => v.CaptureDate)
                                .ToList();
            }
        }

        public bool DeleteVideo(string clipId)
        {
            try
            {
                lock (FileLock)
                {
                    var clip = _db.Clips.FirstOrDefault(v => v.Id == clipId);
                    if (clip != null)
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clip.FilePath);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }

                        _db.Clips.Remove(clip);
                        SaveDatabase();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting video: {ex.Message}");
                return false;
            }
        }

        public bool RenameVideo(string clipId, string newTitle)
        {
            lock (FileLock)
            {
                var clip = _db.Clips.FirstOrDefault(v => v.Id == clipId);
                if (clip != null)
                {
                    clip.Title = newTitle;
                    SaveDatabase();
                    return true;
                }
            }
            return false;
        }

        public bool ExportVideo(string clipId, string targetPath)
        {
            try
            {
                lock (FileLock)
                {
                    var clip = _db.Clips.FirstOrDefault(v => v.Id == clipId);
                    if (clip != null)
                    {
                        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clip.FilePath);
                        if (!File.Exists(sourcePath)) return false;

                        File.Copy(sourcePath, targetPath, true);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting video: {ex.Message}");
                return false;
            }
        }

        public bool OpenVideosFolder(string gameId)
        {
            try
            {
                string gameFolder = Path.Combine(VideosBaseDir, gameId);
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                System.Diagnostics.Process.Start("explorer.exe", $"\"{gameFolder}\"");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening video folder: {ex.Message}");
                return false;
            }
        }
    }
}
