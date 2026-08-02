using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class GameDetailForm : Form
    {
        public event EventHandler? PlayClicked;
        public Game Game { get; private set; }
        private readonly GameLibraryManager _libraryManager;
        private readonly AchievementManager _achievementManager = new();
        private System.Windows.Forms.Timer? _liveSessionTimer;

        public GameDetailForm(Game game, GameLibraryManager libraryManager)
        {
            InitializeComponent();
            Game = game;
            _libraryManager = libraryManager;

            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += GameDetailForm_Load;
            btnPlay.Click += (s, e) => PlayClicked?.Invoke(this, EventArgs.Empty);
            btnFavorite.Click += btnFavorite_Click;
            btnWatchTrailer.Click += btnWatchTrailer_Click;
            btnEditPaths.Click += btnEditPaths_Click;
            btnEditMedia.Click += btnEditMedia_Click;
            btnEditMetadata.Click += btnEditMetadata_Click;
            btnClose.Click += (s, e) => this.Close();
            lnkViewAll.Click += lnkViewAll_Click;

            // Wire process exit notification
            MainForm.GameProcessExited += MainForm_GameProcessExited;
            this.FormClosed += (s, e) => { MainForm.GameProcessExited -= MainForm_GameProcessExited; };

            // Setup live session timer
            _liveSessionTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _liveSessionTimer.Tick += LiveSessionTimer_Tick;
            this.Load += (s, e) => _liveSessionTimer.Start();
            this.FormClosed += (s, e) => { _liveSessionTimer.Stop(); _liveSessionTimer.Dispose(); };

            // Hover transitions
            SetupHover(btnPlay, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnFavorite, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnWatchTrailer, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnEditPaths, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnEditMedia, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnEditMetadata, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void GameDetailForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
            PopulateGameDetails();
        }

        private void PopulateGameDetails()
        {
            lblTitle.Text = Game.Title;
            lblPlatform.Text = Game.Platform;

            var rec = PlaytimeManager.Instance.GetOrCreateRecord(Game.Id);
            string lastSessionStr = rec.LastSessionMinutes > 0 ? $"{rec.LastSessionMinutes} mins" : "None";
            string lastPlayedStr = string.IsNullOrEmpty(rec.LastPlayed) ? "Never" : rec.LastPlayed;
            lblPlaytime.Text = $"Total Playtime: {rec.TotalPlaytimeMinutes} mins\nLast Played: {lastPlayedStr}\nToday: {PlaytimeManager.Instance.GetTodayPlaytime(Game.Id)}m • This Week: {PlaytimeManager.Instance.GetWeeklyPlaytime(Game.Id)}m\nLast Session: {lastSessionStr}";
            
            // Adjust metadata container sizes and padding programmatically to prevent text wrapping issues
            lblMetadata.Height = 68;
            rtbDescription.Top = lblMetadata.Bottom + 5;
            rtbDescription.Height = 80;

            string tagsStr = (Game.Tags != null && Game.Tags.Count > 0) ? string.Join(", ", Game.Tags) : "None";
            lblMetadata.Text = $"Developer: {Game.Developer}  |  Publisher: {Game.Publisher}\n" +
                               $"Genre: {Game.Genre}  |  Released: {Game.ReleaseYear} ({Game.ReleaseDate})\n" +
                               $"Players: {Game.PlayerCount}  |  Region: {Game.Region}  |  Format: {Game.FileFormat}  |  ID: {Game.GameId}\n" +
                               $"Tags: {tagsStr}";
            
            rtbDescription.Text = Game.Description.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

            // Safely dispose of existing image allocations to prevent file locks
            if (pbCover.Image != null)
            {
                var oldImg = pbCover.Image;
                pbCover.Image = null;
                oldImg.Dispose();
            }
            if (pbHero.Image != null)
            {
                var oldImg = pbHero.Image;
                pbHero.Image = null;
                oldImg.Dispose();
            }
            if (pbLogo.Image != null)
            {
                var oldImg = pbLogo.Image;
                pbLogo.Image = null;
                oldImg.Dispose();
            }

            // Load media utilizing MediaManager (handles fallback placeholders gracefully)
            pbCover.Image = MediaManager.GetImageOrPlaceholder(Game.CoverImagePath, "cover");
            pbHero.Image = MediaManager.GetImageOrPlaceholder(Game.HeroImagePath, "hero");
            pbLogo.Image = MediaManager.GetImageOrPlaceholder(Game.LogoImagePath, "logo");

            // Toggle favorite button text/color based on state
            UpdateFavoriteButtonUI();

            // Trailer button visibility
            btnWatchTrailer.Visible = !string.IsNullOrEmpty(Game.TrailerVideoPath);

            // Load screenshots gallery
            LoadScreenshots();

            // Load achievements
            LoadAchievements();

            // Load gameplay video clips
            LoadVideos();
        }

        private void LoadScreenshots()
        {
            // Safe disposal of existing screenshot allocations to prevent resource leaks
            foreach (Control ctrl in flpScreenshots.Controls)
            {
                if (ctrl is PictureBox pb && pb.Image != null)
                {
                    var img = pb.Image;
                    pb.Image = null;
                    img.Dispose();
                }
            }
            flpScreenshots.Controls.Clear();

            // Collect all screenshot paths
            List<string> paths = new List<string>();
            if (Game.ScreenshotPaths != null)
            {
                paths.AddRange(Game.ScreenshotPaths);
            }

            var userScreenshots = ScreenshotManager.Instance.GetScreenshots(Game.Id);
            foreach (var sc in userScreenshots)
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sc.FilePath);
                if (File.Exists(fullPath))
                {
                    paths.Add(fullPath);
                }
            }

            if (paths.Count == 0)
            {
                Label lblNoScreenshots = new Label
                {
                    Text = "No screenshots available.",
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font(this.Font.Name, 8.5F, FontStyle.Italic),
                    AutoSize = true,
                    Margin = new Padding(10, 30, 0, 0)
                };
                flpScreenshots.Controls.Add(lblNoScreenshots);
                return;
            }

            foreach (var screenshotPath in paths)
            {
                Image? img = MediaManager.LoadImage(screenshotPath);
                if (img == null) continue;

                PictureBox pb = new PictureBox
                {
                    Size = new Size(110, 70),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = img,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 10, 0)
                };

                // Click to enlarge screenshot
                pb.Click += (s, e) => EnlargeScreenshot(screenshotPath);
                flpScreenshots.Controls.Add(pb);
            }
        }

        private void EnlargeScreenshot(string imagePath)
        {
            Image? img = MediaManager.LoadImage(imagePath);
            if (img == null) return;

            Form viewer = new Form
            {
                Text = $"Screenshot Viewer - {Game.Title}",
                Size = new Size(960, 600),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.Black,
                MaximizeBox = true,
                MinimizeBox = false,
                ShowIcon = false
            };

            PictureBox pbFull = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            viewer.Controls.Add(pbFull);

            // Close on escape key
            viewer.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) viewer.Close();
            };
            viewer.KeyPreview = true;

            viewer.ShowDialog(this);
            img.Dispose();
        }

        private void LoadVideos()
        {
            flpVideos.Controls.Clear();
            var clips = VideoManager.Instance.GetVideos(Game.Id);

            if (clips.Count == 0)
            {
                Label lblNoVideos = new Label
                {
                    Text = "No recorded gameplay clips.",
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font(this.Font.Name, 8.5F, FontStyle.Italic),
                    AutoSize = true,
                    Margin = new Padding(10, 15, 0, 0)
                };
                flpVideos.Controls.Add(lblNoVideos);
                return;
            }

            foreach (var clip in clips)
            {
                Button btnClip = new Button
                {
                    Text = $"🎥  {clip.Duration}",
                    Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                    Size = new Size(80, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(55, 65, 81),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 8, 0)
                };
                btnClip.FlatAppearance.BorderSize = 0;
                
                btnClip.MouseEnter += (s, e) => btnClip.BackColor = Color.FromArgb(99, 102, 241);
                btnClip.MouseLeave += (s, e) => btnClip.BackColor = Color.FromArgb(55, 65, 81);

                ToolTip tt = new ToolTip();
                tt.SetToolTip(btnClip, $"{clip.Title}\nCaptured: {clip.CaptureDate}");

                btnClip.Click += (s, e) =>
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, clip.FilePath);
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = fullPath,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Unable to play video clip: {ex.Message}", "Play Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Video file not found.", "Play Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                flpVideos.Controls.Add(btnClip);
            }
        }

        private void btnFavorite_Click(object? sender, EventArgs e)
        {
            Game.IsFavorite = !Game.IsFavorite;
            _libraryManager.UpdateGame(Game);
            UpdateFavoriteButtonUI();
        }

        private void UpdateFavoriteButtonUI()
        {
            if (Game.IsFavorite)
            {
                btnFavorite.Text = "★  Favorited";
                btnFavorite.BackColor = Color.FromArgb(245, 158, 11); // Gold/Amber
            }
            else
            {
                btnFavorite.Text = "☆  Favorite";
                btnFavorite.BackColor = Color.FromArgb(55, 65, 81); // Slate gray
            }
        }

        private void btnWatchTrailer_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Game.TrailerVideoPath)) return;

            try
            {
                // In local simulated environment, it can open a local video file or web trailer link
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Game.TrailerVideoPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to play trailer: {ex.Message}", "Trailer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEditPaths_Click(object? sender, EventArgs e)
        {
            using (var editForm = new AddGameForm(Game))
            {
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _libraryManager.UpdateGame(Game);
                    PopulateGameDetails(); // Reload page with new values
                }
            }
        }

        private void btnEditMedia_Click(object? sender, EventArgs e)
        {
            using (var editForm = new AddGameForm(Game))
            {
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _libraryManager.UpdateGame(Game);
                    PopulateGameDetails(); // Reload page with new media assets/values
                }
            }
        }

        private void btnEditMetadata_Click(object? sender, EventArgs e)
        {
            using (var editForm = new EditMetadataForm(Game))
            {
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _libraryManager.UpdateGame(Game);
                    PopulateGameDetails(); // Reload page with new metadata values
                }
            }
        }

        private void lnkViewAll_Click(object? sender, EventArgs e)
        {
            using (var fullAchievementsForm = new GameAchievementsForm(Game, _achievementManager))
            {
                fullAchievementsForm.ShowDialog(this);
                // Reload achievements sidebar in case locks were toggled
                LoadAchievements();
            }
        }

        private void SetupHover(Button btn, Color normalColor, Color hoverColor)
        {
            btn.BackColor = normalColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        private void LoadAchievements()
        {
            // Safe disposal of existing achievement icons to prevent resource leak and file locks
            foreach (Control ctrl in flpAchievementsList.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control subCtrl in pnl.Controls)
                    {
                        if (subCtrl is PictureBox pb && pb.Image != null)
                        {
                            var img = pb.Image;
                            pb.Image = null;
                            img.Dispose();
                        }
                    }
                }
            }
            flpAchievementsList.Controls.Clear();

            var gameAchievements = _achievementManager.GetAchievementsByGame(Game.Id);
            if (gameAchievements.Count == 0)
            {
                Label lblNoAchievements = new Label
                {
                    Text = "No achievements for this game.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font(this.Font.Name, 8.5F, FontStyle.Italic),
                    Size = new Size(240, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 20, 0, 0)
                };
                flpAchievementsList.Controls.Add(lblNoAchievements);
                
                lblProgressCount.Text = "0 / 0 Unlocked (0%)";
                pbProgress.Value = 0;
                return;
            }

            int totalCount = gameAchievements.Count;
            int unlockedCount = gameAchievements.Count(a => a.IsUnlocked);
            double percentage = totalCount == 0 ? 0.0 : Math.Round(((double)unlockedCount / totalCount) * 100.0, 1);

            lblProgressCount.Text = $"{unlockedCount} / {totalCount} Unlocked ({percentage}%)";
            pbProgress.Maximum = 100;
            pbProgress.Value = (int)percentage;

            foreach (var achievement in gameAchievements)
            {
                Panel pnl = new Panel
                {
                    Size = new Size(234, 64),
                    BackColor = Color.FromArgb(30, 30, 36),
                    Margin = new Padding(0, 0, 0, 6),
                    Padding = new Padding(4)
                };

                // Gold border for Rare achievements (rarity contains "rare")
                pnl.Paint += (senderPanel, paintEventArgs) =>
                {
                    string rarity = achievement.Rarity.ToLower();
                    if (rarity.Contains("rare"))
                    {
                        using (Pen pen = new Pen(Color.FromArgb(245, 158, 11), 2))
                        {
                            Rectangle rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                            paintEventArgs.Graphics.DrawRectangle(pen, rect);
                        }
                    }
                };

                PictureBox pbIcon = new PictureBox
                {
                    Size = new Size(48, 48),
                    Location = new Point(8, 8),
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                Image originalIcon = MediaManager.GetImageOrPlaceholder(achievement.IconPath, "icon");
                if (!achievement.IsUnlocked)
                {
                    Image grayscale = GetGrayscaleImage(originalIcon);
                    originalIcon.Dispose();
                    pbIcon.Image = grayscale;
                }
                else
                {
                    pbIcon.Image = originalIcon;
                }

                Label lblTitle = new Label
                {
                    Text = achievement.Title,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    Location = new Point(62, 5),
                    Size = new Size(165, 16),
                    AutoEllipsis = true
                };

                string subText = achievement.IsUnlocked 
                    ? $"{achievement.Rarity} • Unlocked" 
                    : $"{achievement.Rarity} • Locked";

                Label lblSub = new Label
                {
                    Text = subText,
                    ForeColor = Color.FromArgb(99, 102, 241),
                    Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Italic),
                    Location = new Point(62, 21),
                    Size = new Size(165, 15),
                    AutoEllipsis = true
                };

                Label lblDesc = new Label
                {
                    Text = achievement.Description,
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 7.5F),
                    Location = new Point(62, 36),
                    Size = new Size(165, 24),
                    AutoEllipsis = true
                };

                pnl.Controls.Add(pbIcon);
                pnl.Controls.Add(lblTitle);
                pnl.Controls.Add(lblSub);
                pnl.Controls.Add(lblDesc);

                // Right click test context menu
                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem itemToggle = new ToolStripMenuItem(achievement.IsUnlocked ? "Lock Achievement" : "Unlock Achievement");
                itemToggle.Click += (s, e) =>
                {
                    if (achievement.IsUnlocked)
                    {
                        _achievementManager.LockAchievement(Game.Id, achievement.Id);
                    }
                    else
                    {
                        _achievementManager.UnlockAchievement(Game.Id, achievement.Id);
                    }
                    LoadAchievements();
                };
                menu.Items.Add(itemToggle);
                pnl.ContextMenuStrip = menu;
                pbIcon.ContextMenuStrip = menu;
                lblTitle.ContextMenuStrip = menu;
                lblSub.ContextMenuStrip = menu;
                lblDesc.ContextMenuStrip = menu;

                flpAchievementsList.Controls.Add(pnl);
            }
        }

        private Image GetGrayscaleImage(Image original)
        {
            Bitmap bmp = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(
                    new float[][]
                    {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    });

                using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return bmp;
        }

        private void MainForm_GameProcessExited(object? sender, string gameId)
        {
            if (gameId == Game.Id)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(PopulateGameDetails));
                }
                else
                {
                    PopulateGameDetails();
                }
            }
        }

        private void LiveSessionTimer_Tick(object? sender, EventArgs e)
        {
            if (PlaytimeManager.Instance.IsSessionActive(Game.Id))
            {
                var sessionStart = PlaytimeManager.Instance.GetSessionStart(Game.Id);
                if (sessionStart.HasValue)
                {
                    var elapsed = DateTime.Now - sessionStart.Value;
                    btnPlay.Text = $"⏳ RUNNING ({elapsed.Minutes:D2}:{elapsed.Seconds:D2})";
                    btnPlay.Enabled = false;

                    string elapsedStr = $"{elapsed.Minutes}m {elapsed.Seconds}s";
                    var rec = PlaytimeManager.Instance.GetOrCreateRecord(Game.Id);
                    string lastPlayedStr = string.IsNullOrEmpty(rec.LastPlayed) ? "Never" : rec.LastPlayed;
                    lblPlaytime.Text = $"Total Playtime: {rec.TotalPlaytimeMinutes} mins\nLast Played: {lastPlayedStr}\nToday: {PlaytimeManager.Instance.GetTodayPlaytime(Game.Id)}m • This Week: {PlaytimeManager.Instance.GetWeeklyPlaytime(Game.Id)}m\nActive Session: {elapsedStr}";
                }
            }
            else
            {
                btnPlay.Text = "▶  PLAY GAME";
                btnPlay.Enabled = true;

                // Sync UI metrics from PlaytimeManager
                var record = PlaytimeManager.Instance.GetOrCreateRecord(Game.Id);
                string lastSessionStr = record.LastSessionMinutes > 0 ? $"{record.LastSessionMinutes} mins" : "None";
                string lastPlayedStr = string.IsNullOrEmpty(record.LastPlayed) ? "Never" : record.LastPlayed;
                lblPlaytime.Text = $"Total Playtime: {record.TotalPlaytimeMinutes} mins\nLast Played: {lastPlayedStr}\nToday: {PlaytimeManager.Instance.GetTodayPlaytime(Game.Id)}m • This Week: {PlaytimeManager.Instance.GetWeeklyPlaytime(Game.Id)}m\nLast Session: {lastSessionStr}";
            }
        }
    }
}
