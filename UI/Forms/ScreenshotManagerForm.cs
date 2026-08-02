using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class ScreenshotManagerForm : Form
    {
        private readonly GameLibraryManager _libraryManager = new();
        private Game? _selectedGame;
        private ScreenshotMetadata? _selectedScreenshot;
        private PictureBox? _selectedThumbnailPb;

        public ScreenshotManagerForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += ScreenshotManagerForm_Load;
            lbGames.SelectedIndexChanged += lbGames_SelectedIndexChanged;
            lbGames.DrawItem += lbGames_DrawItem;

            btnUpdateCaption.Click += btnUpdateCaption_Click;
            btnExport.Click += btnExport_Click;
            btnDelete.Click += btnDelete_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;
            btnClose.Click += (s, e) => this.Close();

            // Setup hover highlights
            SetupButtonHover(btnUpdateCaption, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnExport, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnDelete, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupButtonHover(btnOpenFolder, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void ScreenshotManagerForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
            PopulateGameList();
        }

        private void PopulateGameList()
        {
            lbGames.Items.Clear();
            foreach (var game in _libraryManager.Games)
            {
                lbGames.Items.Add(game);
            }

            if (lbGames.Items.Count > 0)
            {
                lbGames.SelectedIndex = 0;
            }
        }

        private void lbGames_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedGame = lbGames.SelectedItem as Game;
            ClearPreview();
            RefreshThumbnails();
        }

        private void RefreshThumbnails()
        {
            // Clear current thumbnail controls and dispose images to avoid locks
            foreach (Control ctrl in flpThumbnails.Controls)
            {
                if (ctrl is PictureBox pb && pb.Image != null)
                {
                    var img = pb.Image;
                    pb.Image = null;
                    img.Dispose();
                }
            }
            flpThumbnails.Controls.Clear();

            if (_selectedGame == null) return;

            var list = ScreenshotManager.Instance.GetScreenshots(_selectedGame.Id);
            if (list.Count == 0)
            {
                Label lblNoScreenshots = new Label
                {
                    Text = "No screenshots captured for this game.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font(this.Font.Name, 9F, FontStyle.Italic),
                    Size = new Size(380, 50),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 40, 0, 0)
                };
                flpThumbnails.Controls.Add(lblNoScreenshots);
                return;
            }

            foreach (var sc in list)
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sc.FilePath);
                if (!File.Exists(fullPath)) continue;

                // Load image safely to prevent locks
                Image? thumbImg = null;
                try
                {
                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        thumbImg = Image.FromStream(fs);
                    }
                }
                catch
                {
                    continue;
                }

                PictureBox pb = new PictureBox
                {
                    Size = new Size(115, 75),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = thumbImg,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(5),
                    Tag = sc
                };

                pb.Click += (s, e) => SelectThumbnail(pb, sc);

                // Add a hover effect border via Paint
                pb.Paint += (s, e) =>
                {
                    if (pb == _selectedThumbnailPb)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(99, 102, 241), 3))
                        {
                            e.Graphics.DrawRectangle(pen, 0, 0, pb.Width - 1, pb.Height - 1);
                        }
                    }
                };

                flpThumbnails.Controls.Add(pb);
            }

            // Automatically select first thumbnail if available
            if (flpThumbnails.Controls.Count > 0 && flpThumbnails.Controls[0] is PictureBox firstPb)
            {
                SelectThumbnail(firstPb, firstPb.Tag as ScreenshotMetadata);
            }
        }

        private void SelectThumbnail(PictureBox pb, ScreenshotMetadata? sc)
        {
            if (sc == null) return;

            // Clear old highlight
            var prevPb = _selectedThumbnailPb;
            _selectedThumbnailPb = pb;

            if (prevPb != null) prevPb.Invalidate();
            pb.Invalidate(); // Redraw new highlight

            _selectedScreenshot = sc;

            // Load large preview
            if (pbLargePreview.Image != null)
            {
                var oldImg = pbLargePreview.Image;
                pbLargePreview.Image = null;
                oldImg.Dispose();
            }

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sc.FilePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        pbLargePreview.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    pbLargePreview.Image = null;
                }
            }

            tbTitle.Text = sc.Title;
        }

        private void ClearPreview()
        {
            _selectedScreenshot = null;
            _selectedThumbnailPb = null;

            if (pbLargePreview.Image != null)
            {
                var oldImg = pbLargePreview.Image;
                pbLargePreview.Image = null;
                oldImg.Dispose();
            }

            tbTitle.Clear();
        }

        private void lbGames_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(99, 102, 241) : Color.FromArgb(31, 31, 35);
            Color fgColor = Color.White;

            using (Brush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            var game = lbGames.Items[e.Index] as Game;
            if (game != null)
            {
                Rectangle textBounds = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Width - 10, e.Bounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    game.Title,
                    e.Font ?? this.Font,
                    textBounds,
                    fgColor,
                    bgColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }

            e.DrawFocusRectangle();
        }

        private void btnUpdateCaption_Click(object? sender, EventArgs e)
        {
            if (_selectedScreenshot == null)
            {
                MessageBox.Show("Please select a screenshot to update.", "Caption Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newCaption = tbTitle.Text.Trim();
            if (string.IsNullOrEmpty(newCaption))
            {
                MessageBox.Show("Caption cannot be empty.", "Caption Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ScreenshotManager.Instance.RenameScreenshot(_selectedScreenshot.Id, newCaption))
            {
                MessageBox.Show("Caption updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_selectedScreenshot == null)
            {
                MessageBox.Show("Please select a screenshot to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image (*.png)|*.png";
                sfd.FileName = Path.GetFileName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedScreenshot.FilePath));
                sfd.Title = "Export Screenshot";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedScreenshot.FilePath);
                    try
                    {
                        File.Copy(sourcePath, sfd.FileName, true);
                        MessageBox.Show("Screenshot exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedScreenshot == null)
            {
                MessageBox.Show("Please select a screenshot to delete.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this screenshot?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            if (ScreenshotManager.Instance.DeleteScreenshot(_selectedScreenshot.Id))
            {
                ClearPreview();
                RefreshThumbnails();
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;
            ScreenshotManager.Instance.OpenScreenshotFolder(_selectedGame.Id);
        }
    }
}
