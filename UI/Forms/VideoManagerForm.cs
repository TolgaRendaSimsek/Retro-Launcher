using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class VideoManagerForm : Form
    {
        private readonly GameLibraryManager _libraryManager = new();
        private Game? _selectedGame;
        private VideoMetadata? _selectedVideo;

        public VideoManagerForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += VideoManagerForm_Load;
            lbGames.SelectedIndexChanged += lbGames_SelectedIndexChanged;
            lbGames.DrawItem += lbGames_DrawItem;
            lvVideos.SelectedIndexChanged += lvVideos_SelectedIndexChanged;

            btnPlayClip.Click += btnPlayClip_Click;
            btnUpdateCaption.Click += btnUpdateCaption_Click;
            btnExport.Click += btnExport_Click;
            btnDelete.Click += btnDelete_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;
            btnClose.Click += (s, e) => this.Close();

            // Setup hover styles
            SetupButtonHover(btnPlayClip, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
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

        private void VideoManagerForm_Load(object? sender, EventArgs e)
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
            RefreshVideosList();
        }

        private void RefreshVideosList()
        {
            lvVideos.Items.Clear();
            if (_selectedGame == null) return;

            var list = VideoManager.Instance.GetVideos(_selectedGame.Id);
            foreach (var clip in list)
            {
                var item = new ListViewItem(clip.Title);
                item.SubItems.Add(clip.Duration);
                item.SubItems.Add(clip.CaptureDate);
                item.Tag = clip;
                lvVideos.Items.Add(item);
            }

            if (lvVideos.Items.Count > 0)
            {
                lvVideos.Items[0].Selected = true;
            }
            else
            {
                ClearPreview();
            }
        }

        private void lvVideos_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvVideos.SelectedItems.Count == 0)
            {
                ClearPreview();
                return;
            }

            _selectedVideo = lvVideos.SelectedItems[0].Tag as VideoMetadata;
            if (_selectedVideo != null)
            {
                tbTitle.Text = _selectedVideo.Title;
                lblPlaceholderText.Text = $"🎬\nSelected: {_selectedVideo.Title}\nDuration: {_selectedVideo.Duration}";
            }
        }

        private void ClearPreview()
        {
            _selectedVideo = null;
            tbTitle.Clear();
            lblPlaceholderText.Text = "🎥\nSelect a clip from the list";
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

        private void btnPlayClip_Click(object? sender, EventArgs e)
        {
            if (_selectedVideo == null)
            {
                MessageBox.Show("Please select a clip to play.", "Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedVideo.FilePath);
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
        }

        private void btnUpdateCaption_Click(object? sender, EventArgs e)
        {
            if (_selectedVideo == null)
            {
                MessageBox.Show("Please select a clip to update.", "Caption Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newCaption = tbTitle.Text.Trim();
            if (string.IsNullOrEmpty(newCaption))
            {
                MessageBox.Show("Caption cannot be empty.", "Caption Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (VideoManager.Instance.RenameVideo(_selectedVideo.Id, newCaption))
            {
                MessageBox.Show("Caption updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                int selectedIndex = lvVideos.SelectedIndices[0];
                RefreshVideosList();
                if (lvVideos.Items.Count > selectedIndex)
                {
                    lvVideos.Items[selectedIndex].Selected = true;
                }
            }
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_selectedVideo == null)
            {
                MessageBox.Show("Please select a clip to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "MP4 Video (*.mp4)|*.mp4";
                sfd.FileName = Path.GetFileName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedVideo.FilePath));
                sfd.Title = "Export Gameplay Clip";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    if (VideoManager.Instance.ExportVideo(_selectedVideo.Id, sfd.FileName))
                    {
                        MessageBox.Show("Clip exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to export clip.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedVideo == null)
            {
                MessageBox.Show("Please select a clip to delete.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this video clip?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            if (VideoManager.Instance.DeleteVideo(_selectedVideo.Id))
            {
                ClearPreview();
                RefreshVideosList();
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;
            VideoManager.Instance.OpenVideosFolder(_selectedGame.Id);
        }
    }
}
