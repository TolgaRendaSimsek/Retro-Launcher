using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class EditMetadataForm : Form
    {
        public Game Game { get; private set; }

        public EditMetadataForm(Game game)
        {
            InitializeComponent();
            Game = game;

            // Load data
            tbTitle.Text = game.Title;
            tbPlatform.Text = game.Platform;
            tbReleaseDate.Text = game.ReleaseDate;
            tbReleaseYear.Text = game.ReleaseYear;
            tbGenre.Text = game.Genre;
            tbDeveloper.Text = game.Developer;
            tbPublisher.Text = game.Publisher;
            tbPlayerCount.Text = game.PlayerCount;
            tbRegion.Text = game.Region;
            tbFileFormat.Text = game.FileFormat;
            tbGameId.Text = game.GameId;
            tbTags.Text = game.Tags != null ? string.Join(", ", game.Tags) : "";
            tbDescription.Text = game.Description;

            // Event handlers
            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
            this.Load += EditMetadataForm_Load;

            // Setup styling
            SetupHover(btnSave, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnCancel, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void EditMetadataForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
        }

        private void SetupHover(Button btn, Color normalColor, Color hoverColor)
        {
            btn.BackColor = normalColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                MessageBox.Show("Game Title is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update Game properties
            Game.Title = tbTitle.Text.Trim();
            Game.Platform = tbPlatform.Text.Trim();
            Game.ReleaseDate = tbReleaseDate.Text.Trim();
            Game.ReleaseYear = tbReleaseYear.Text.Trim();
            Game.Genre = tbGenre.Text.Trim();
            Game.Developer = tbDeveloper.Text.Trim();
            Game.Publisher = tbPublisher.Text.Trim();
            Game.PlayerCount = tbPlayerCount.Text.Trim();
            Game.Region = tbRegion.Text.Trim();
            Game.FileFormat = tbFileFormat.Text.Trim();
            Game.GameId = tbGameId.Text.Trim();
            
            Game.Tags = tbTags.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim())
                                   .Where(t => !string.IsNullOrEmpty(t))
                                   .ToList();

            Game.Description = tbDescription.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
