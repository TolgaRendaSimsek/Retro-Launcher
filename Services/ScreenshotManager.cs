using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Services
{
    public class ScreenshotMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GameId { get; set; } = "";
        public string Title { get; set; } = ""; // User caption
        public string FilePath { get; set; } = ""; // Path relative to execution dir
        public string CaptureDate { get; set; } = ""; // "yyyy-MM-dd HH:mm:ss"
    }

    public class ScreenshotManager
    {
        private static readonly string ScreenshotsJsonPath = Path.Combine(ApplicationPaths.ConfigDir, "screenshots.json");
        private static readonly string ScreenshotsBaseDir = Path.Combine(ApplicationPaths.BaseDataDir, "screenshots");
        private static readonly object FileLock = new object();

        private Dictionary<string, ScreenshotMetadata> _screenshots = new();
        private static ScreenshotManager? _instance;

        public static ScreenshotManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScreenshotManager();
                }
                return _instance;
            }
        }

        public ScreenshotManager()
        {
            LoadMetadata();
        }

        public void LoadMetadata()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ScreenshotsJsonPath))
                    {
                        string json = File.ReadAllText(ScreenshotsJsonPath);
                        var list = JsonSerializer.Deserialize<List<ScreenshotMetadata>>(json);
                        _screenshots = new Dictionary<string, ScreenshotMetadata>();
                        if (list != null)
                        {
                            foreach (var sc in list)
                            {
                                if (!string.IsNullOrEmpty(sc.Id))
                                {
                                    _screenshots[sc.Id] = sc;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading screenshots.json: {ex.Message}");
                }
            }
        }

        public void SaveMetadata()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(new List<ScreenshotMetadata>(_screenshots.Values), options);
                    File.WriteAllText(ScreenshotsJsonPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving screenshots.json: {ex.Message}");
                }
            }
        }

        public ScreenshotMetadata? CaptureScreenshot(string gameId)
        {
            try
            {
                // Ensure output directory exists
                string gameFolder = Path.Combine(ScreenshotsBaseDir, gameId);
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                // Copy screen to bitmap
                var primaryScreen = Screen.PrimaryScreen;
                if (primaryScreen == null) return null;
                Rectangle bounds = primaryScreen.Bounds;
                string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string relativePath = Path.Combine("screenshots", gameId, filename);
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    bitmap.Save(absolutePath, System.Drawing.Imaging.ImageFormat.Png);
                }

                // Create metadata
                string displayTitle = $"Screenshot {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                var sc = new ScreenshotMetadata
                {
                    Id = Guid.NewGuid().ToString(),
                    GameId = gameId,
                    Title = displayTitle,
                    FilePath = relativePath,
                    CaptureDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                lock (FileLock)
                {
                    _screenshots[sc.Id] = sc;
                }
                SaveMetadata();

                // Play standard camera sound indicator
                System.Media.SystemSounds.Asterisk.Play();

                return sc;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing screenshot: {ex.Message}");
                return null;
            }
        }

        public List<ScreenshotMetadata> GetScreenshots(string gameId)
        {
            lock (FileLock)
            {
                return _screenshots.Values
                                   .Where(s => s.GameId == gameId)
                                   .OrderByDescending(s => s.CaptureDate)
                                   .ToList();
            }
        }

        public bool DeleteScreenshot(string screenshotId)
        {
            try
            {
                lock (FileLock)
                {
                    if (_screenshots.TryGetValue(screenshotId, out var sc))
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sc.FilePath);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }

                        _screenshots.Remove(screenshotId);
                        SaveMetadata();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting screenshot: {ex.Message}");
                return false;
            }
        }

        public bool RenameScreenshot(string screenshotId, string newName)
        {
            lock (FileLock)
            {
                if (_screenshots.TryGetValue(screenshotId, out var sc))
                {
                    sc.Title = newName;
                    SaveMetadata();
                    return true;
                }
            }
            return false;
        }

        public bool ExportScreenshot(string screenshotId, string targetFolder)
        {
            try
            {
                lock (FileLock)
                {
                    if (_screenshots.TryGetValue(screenshotId, out var sc))
                    {
                        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sc.FilePath);
                        if (!File.Exists(sourcePath)) return false;

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string destPath = Path.Combine(targetFolder, Path.GetFileName(sourcePath));
                        File.Copy(sourcePath, destPath, true);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting screenshot: {ex.Message}");
                return false;
            }
        }

        public bool OpenScreenshotFolder(string gameId)
        {
            try
            {
                string gameFolder = Path.Combine(ScreenshotsBaseDir, gameId);
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                System.Diagnostics.Process.Start("explorer.exe", $"\"{gameFolder}\"");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening screenshot folder: {ex.Message}");
                return false;
            }
        }
    }
}
