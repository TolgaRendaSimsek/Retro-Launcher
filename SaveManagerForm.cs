using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class SaveManagerForm : Form
    {
        private readonly GameLibraryManager _libraryManager = new();
        private Game? _selectedGame;

        public SaveManagerForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += SaveManagerForm_Load;
            lbGames.SelectedIndexChanged += lbGames_SelectedIndexChanged;
            lbGames.DrawItem += lbGames_DrawItem;

            btnOpenFolder.Click += btnOpenFolder_Click;
            btnBackupNow.Click += btnBackupNow_Click;
            btnRestore.Click += btnRestore_Click;
            btnRename.Click += btnRename_Click;
            btnDelete.Click += btnDelete_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;
            btnClose.Click += (s, e) => this.Close();

            // Hover styles
            SetupButtonHover(btnOpenFolder, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnBackupNow, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnRestore, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnRename, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnDelete, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupButtonHover(btnImport, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnExport, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void SaveManagerForm_Load(object? sender, EventArgs e)
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
            RefreshSavesAndBackups();
        }

        private void RefreshSavesAndBackups()
        {
            lvActiveSaves.Items.Clear();
            lvBackups.Items.Clear();

            if (_selectedGame == null) return;

            // Load Active Saves
            var activeSaves = LocalSaveManager.GetActiveSaveFiles(_selectedGame);
            foreach (var file in activeSaves)
            {
                var item = new ListViewItem(file.FileName);
                item.SubItems.Add(file.SizeDisplay);
                item.SubItems.Add(file.LastModified.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = file;
                lvActiveSaves.Items.Add(item);
            }

            // Load Backups
            var backups = LocalSaveManager.GetLocalBackups(_selectedGame.Id);
            foreach (var backup in backups)
            {
                var item = new ListViewItem(backup.BackupName);
                item.SubItems.Add(backup.SizeDisplay);
                item.SubItems.Add(backup.CreatedDate.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = backup;
                lvBackups.Items.Add(item);
            }
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
                // Align text padding
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

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;

            string saveFolder = SaveManager.Instance.DetectSaveFolder(_selectedGame.EmulatorId);
            if (!Directory.Exists(saveFolder))
            {
                try
                {
                    Directory.CreateDirectory(saveFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not create saves folder: {ex.Message}", "Explorer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{saveFolder}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open save folder: {ex.Message}", "Explorer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBackupNow_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;

            string defaultName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            string backupName = InputPrompt.Show("Enter a name for this backup:", "Create Backup", defaultName);

            if (string.IsNullOrWhiteSpace(backupName)) return;

            // Remove invalid file path characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                backupName = backupName.Replace(c, '_');
            }

            if (LocalSaveManager.CreateBackup(_selectedGame, backupName))
            {
                MessageBox.Show("Backup created successfully!", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshSavesAndBackups();
            }
        }

        private void btnRestore_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null || lvBackups.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a backup to restore.", "Restore Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var backup = lvBackups.SelectedItems[0].Tag as LocalBackupInfo;
            if (backup == null) return;

            if (LocalSaveManager.RestoreBackup(_selectedGame, backup.BackupName))
            {
                MessageBox.Show("Backup restored successfully!", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshSavesAndBackups();
            }
        }

        private void btnRename_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null || lvBackups.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a backup to rename.", "Rename Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var backup = lvBackups.SelectedItems[0].Tag as LocalBackupInfo;
            if (backup == null) return;

            string newName = InputPrompt.Show("Enter new name for the backup:", "Rename Backup", backup.BackupName);
            if (string.IsNullOrWhiteSpace(newName) || newName == backup.BackupName) return;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                newName = newName.Replace(c, '_');
            }

            if (LocalSaveManager.RenameBackup(_selectedGame.Id, backup.BackupName, newName))
            {
                RefreshSavesAndBackups();
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null || lvBackups.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a backup to delete.", "Delete Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var backup = lvBackups.SelectedItems[0].Tag as LocalBackupInfo;
            if (backup == null) return;

            var confirm = MessageBox.Show($"Are you sure you want to delete backup '{backup.BackupName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            if (LocalSaveManager.DeleteBackup(_selectedGame.Id, backup.BackupName))
            {
                RefreshSavesAndBackups();
            }
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null || lvBackups.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a backup to export.", "Export Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var backup = lvBackups.SelectedItems[0].Tag as LocalBackupInfo;
            if (backup == null) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "ZIP Archive (*.zip)|*.zip";
                sfd.FileName = $"{_selectedGame.Title.Replace(" ", "_")}_{backup.BackupName}.zip";
                sfd.Title = "Export Save Backup as ZIP";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    if (LocalSaveManager.ExportBackup(_selectedGame.Id, backup.BackupName, sfd.FileName))
                    {
                        MessageBox.Show("Backup exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnImport_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "ZIP Archive (*.zip)|*.zip";
                ofd.Title = "Import Save Backup from ZIP";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string defaultName = $"Imported_{Path.GetFileNameWithoutExtension(ofd.FileName)}";
                    string backupName = InputPrompt.Show("Enter a name for the imported backup:", "Import Backup", defaultName);

                    if (string.IsNullOrWhiteSpace(backupName)) return;

                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        backupName = backupName.Replace(c, '_');
                    }

                    if (LocalSaveManager.ImportBackup(_selectedGame.Id, ofd.FileName, backupName))
                    {
                        MessageBox.Show("Backup imported successfully!", "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshSavesAndBackups();
                    }
                }
            }
        }
    }

    public static class InputPrompt
    {
        public static string Show(string prompt, string title, string defaultValue = "")
        {
            Form promptForm = new Form()
            {
                Width = 350,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(24, 24, 28),
                ForeColor = Color.White,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 15, Text = prompt, Width = 300 };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 300, Text = defaultValue, BackColor = Color.FromArgb(36, 36, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Button confirmation = new Button() { Text = "OK", Left = 220, Width = 100, Top = 75, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White };
            confirmation.FlatAppearance.BorderSize = 0;
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.Controls.Add(textLabel);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }
    }
}
