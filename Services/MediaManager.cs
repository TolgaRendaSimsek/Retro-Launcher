using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace RetroLauncher.Services
{
    public static class MediaManager
    {
        public static string GetGameMediaFolder(string gameId)
        {
            if (string.IsNullOrEmpty(gameId)) return "";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string mediaFolder = Path.Combine(baseDir, "media", gameId);
            if (!Directory.Exists(mediaFolder))
            {
                Directory.CreateDirectory(mediaFolder);
            }
            return mediaFolder;
        }

        public static string AddMediaFile(string gameId, string sourcePath, string assetType)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                return "";

            try
            {
                string mediaFolder = GetGameMediaFolder(gameId);
                string extension = Path.GetExtension(sourcePath).ToLower();
                string destinationName = $"{assetType}{extension}";

                // screenshots use custom sequential names to allow multiple entries
                if (assetType.Equals("screenshot", StringComparison.OrdinalIgnoreCase))
                {
                    destinationName = $"screenshot_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
                }

                string destPath = Path.Combine(mediaFolder, destinationName);

                // Copy to local folder and overwrite if it already exists
                File.Copy(sourcePath, destPath, true);

                // Return relative path for standardized serialization: media/{gameId}/filename
                return Path.Combine("media", gameId, destinationName).Replace('\\', '/');
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to add media file: {ex.Message}");
                return "";
            }
        }

        public static string AddCoverImage(string gameId, string sourcePath)
        {
            return AddMediaFile(gameId, sourcePath, "cover");
        }

        public static string AddHeroImage(string gameId, string sourcePath)
        {
            return AddMediaFile(gameId, sourcePath, "hero");
        }

        public static string AddLogoImage(string gameId, string sourcePath)
        {
            return AddMediaFile(gameId, sourcePath, "logo");
        }

        public static string AddIconImage(string gameId, string sourcePath)
        {
            return AddMediaFile(gameId, sourcePath, "icon");
        }

        public static string AddScreenshot(string gameId, string sourcePath)
        {
            return AddMediaFile(gameId, sourcePath, "screenshot");
        }

        public static bool RemoveScreenshot(string gameId, string screenshotPath)
        {
            if (string.IsNullOrEmpty(screenshotPath)) return false;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.IsPathRooted(screenshotPath) ? screenshotPath : Path.Combine(baseDir, screenshotPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to remove screenshot: {ex.Message}");
                return false;
            }
        }

        public static Image? LoadImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(baseDir, relativePath);

                if (!File.Exists(fullPath))
                {
                    string currentDir = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
                    if (File.Exists(currentDir))
                    {
                        fullPath = currentDir;
                    }
                    else
                    {
                        return null;
                    }
                }

                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        public static Image GetImageOrPlaceholder(string relativePath, string placeholderType)
        {
            Image? img = LoadImage(relativePath);
            return img ?? GetPlaceholderImage(placeholderType);
        }

        public static Image GetPlaceholderImage(string type)
        {
            string cleanType = type?.ToLower() ?? "";

            if (cleanType == "cover")
            {
                Bitmap bmp = new Bitmap(180, 240);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, 180, 240), Color.FromArgb(40, 40, 50), Color.FromArgb(20, 20, 25), 45f))
                    {
                        g.FillRectangle(brush, 0, 0, 180, 240);
                    }
                    using (var pen = new Pen(Color.FromArgb(70, 75, 95), 2))
                    {
                        g.DrawRectangle(pen, 1, 1, 178, 238);
                    }
                    using (var accent = new SolidBrush(Color.FromArgb(99, 102, 241)))
                    {
                        g.FillRectangle(accent, 15, 30, 150, 6);
                    }
                    using (Font font = new Font("Segoe UI", 11, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.FromArgb(220, 225, 235)))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("NO COVER", font, brush, new Rectangle(10, 60, 160, 120), sf);
                    }
                    using (Font mini = new Font("Segoe UI", 8, FontStyle.Bold))
                    using (Brush badgeBg = new SolidBrush(Color.FromArgb(99, 102, 241)))
                    using (Brush text = new SolidBrush(Color.White))
                    {
                        g.FillRectangle(badgeBg, 50, 200, 80, 20);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("RETRO", mini, text, new Rectangle(50, 200, 80, 20), sf);
                    }
                }
                return bmp;
            }
            else if (cleanType == "hero" || cleanType == "banner")
            {
                Bitmap bmp = new Bitmap(800, 250);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, 800, 250), Color.FromArgb(30, 30, 40), Color.FromArgb(15, 15, 20), 0f))
                    {
                        g.FillRectangle(brush, 0, 0, 800, 250);
                    }
                    using (var pen = new Pen(Color.FromArgb(50, 50, 60), 2))
                    {
                        g.DrawRectangle(pen, 1, 1, 798, 248);
                    }
                    using (Font font = new Font("Segoe UI Semibold", 22, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.FromArgb(200, 200, 220)))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("NO BANNER", font, brush, new Rectangle(20, 20, 760, 210), sf);
                    }
                }
                return bmp;
            }
            else if (cleanType == "logo")
            {
                Bitmap bmp = new Bitmap(300, 80);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (Font font = new Font("Segoe UI Black", 16, FontStyle.Bold | FontStyle.Italic))
                    using (Brush brush = new SolidBrush(Color.FromArgb(99, 102, 241)))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                        g.DrawString("NO LOGO", font, brush, new Rectangle(10, 10, 280, 60), sf);
                    }
                }
                return bmp;
            }
            else // icon or fallback
            {
                Bitmap bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (var brush = new SolidBrush(Color.FromArgb(99, 102, 241)))
                    {
                        g.FillEllipse(brush, 4, 4, 24, 24);
                    }
                    using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
                    using (Brush text = new SolidBrush(Color.White))
                    {
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("🕹️", font, text, new Rectangle(0, 0, 32, 32), sf);
                    }
                }
                return bmp;
            }
        }
    }
}
