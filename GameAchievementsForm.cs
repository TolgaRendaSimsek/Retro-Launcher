using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class GameAchievementsForm : Form
    {
        public Game Game { get; private set; }
        private readonly AchievementManager _achievementManager;

        public GameAchievementsForm(Game game, AchievementManager achievementManager)
        {
            InitializeComponent();
            Game = game;
            _achievementManager = achievementManager;

            SetupEvents();
        }

        private void SetupEvents()
        {
            this.Load += GameAchievementsForm_Load;
            btnClose.Click += (s, e) => this.Close();

            // Hover transitions
            btnClose.BackColor = Color.FromArgb(55, 65, 81);
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(31, 41, 55);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(55, 65, 81);
        }

        private void GameAchievementsForm_Load(object? sender, EventArgs e)
        {
            lblHeaderTitle.Text = $"{Game.Title} - Achievements";
            PopulateAchievements();
        }

        private void PopulateAchievements()
        {
            // Safe disposal of existing icons to prevent memory leaks and file locks
            foreach (Control ctrl in flpAchievements.Controls)
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
            flpAchievements.Controls.Clear();

            var gameAchievements = _achievementManager.GetAchievementsByGame(Game.Id);
            if (gameAchievements.Count == 0)
            {
                Label lblNoAchievements = new Label
                {
                    Text = "No achievements for this game.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font(this.Font.Name, 9.5F, FontStyle.Italic),
                    Size = new Size(520, 60),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 50, 0, 0)
                };
                flpAchievements.Controls.Add(lblNoAchievements);
                
                lblHeaderProgress.Text = "0 / 0 Unlocked (0%)";
                pbHeaderProgress.Value = 0;
                return;
            }

            int totalCount = gameAchievements.Count;
            int unlockedCount = gameAchievements.Count(a => a.IsUnlocked);
            double percentage = totalCount == 0 ? 0.0 : Math.Round(((double)unlockedCount / totalCount) * 100.0, 1);

            lblHeaderProgress.Text = $"{unlockedCount} / {totalCount} Unlocked ({percentage}%)";
            pbHeaderProgress.Maximum = 100;
            pbHeaderProgress.Value = (int)percentage;

            foreach (var achievement in gameAchievements)
            {
                Panel pnl = new Panel
                {
                    Size = new Size(530, 72),
                    BackColor = Color.FromArgb(30, 30, 36),
                    Margin = new Padding(0, 0, 0, 8),
                    Padding = new Padding(6)
                };

                // Gold border for Rare achievements (rarity string contains "rare")
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
                    Location = new Point(12, 12),
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
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Location = new Point(72, 8),
                    Size = new Size(320, 20),
                    AutoEllipsis = true
                };

                string subText = achievement.IsUnlocked 
                    ? $"{achievement.Rarity} • Unlocked on {achievement.UnlockedAt}" 
                    : $"{achievement.Rarity} • Locked";

                Label lblSub = new Label
                {
                    Text = subText,
                    ForeColor = Color.FromArgb(99, 102, 241),
                    Font = new Font("Segoe UI Semibold", 8F, FontStyle.Italic),
                    Location = new Point(72, 28),
                    Size = new Size(320, 16),
                    AutoEllipsis = true
                };

                Label lblDesc = new Label
                {
                    Text = achievement.Description,
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8F),
                    Location = new Point(72, 44),
                    Size = new Size(340, 20),
                    AutoEllipsis = true
                };

                // Points Badge
                Label lblPoints = new Label
                {
                    Text = $"+{achievement.Points} XP",
                    ForeColor = Color.FromArgb(16, 185, 129),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Location = new Point(430, 25),
                    Size = new Size(80, 22),
                    TextAlign = ContentAlignment.MiddleRight
                };

                pnl.Controls.Add(pbIcon);
                pnl.Controls.Add(lblTitle);
                pnl.Controls.Add(lblSub);
                pnl.Controls.Add(lblDesc);
                pnl.Controls.Add(lblPoints);

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
                    PopulateAchievements();
                };
                menu.Items.Add(itemToggle);
                pnl.ContextMenuStrip = menu;
                pbIcon.ContextMenuStrip = menu;
                lblTitle.ContextMenuStrip = menu;
                lblSub.ContextMenuStrip = menu;
                lblDesc.ContextMenuStrip = menu;
                lblPoints.ContextMenuStrip = menu;

                flpAchievements.Controls.Add(pnl);
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
    }
}
