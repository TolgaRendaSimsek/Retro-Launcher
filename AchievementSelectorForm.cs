using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class AchievementSelectorForm : Form
    {
        public Achievement? SelectedAchievement { get; private set; }
        private readonly List<Achievement> _availableAchievements;
        private Panel? _selectedPanel = null;
        private readonly GameLibraryManager _libraryManager = new();

        public AchievementSelectorForm(List<Achievement> unlockedAchievements, List<string> alreadyShowcasedIds)
        {
            InitializeComponent();

            // Filter out achievements that are already in the showcase
            _availableAchievements = unlockedAchievements
                .Where(a => !alreadyShowcasedIds.Contains(a.Id))
                .ToList();

            SetupEvents();
            PopulateList();
        }

        private void SetupEvents()
        {
            btnSelect.Click += btnSelect_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Hover effects
            SetupHover(btnSelect, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnCancel, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void PopulateList()
        {
            flpAchievements.Controls.Clear();

            if (_availableAchievements.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "No unlocked achievements available to showcase.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font(this.Font.Name, 9.5F, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(380, 100),
                    Margin = new Padding(10, 50, 10, 10)
                };
                flpAchievements.Controls.Add(lblEmpty);
                btnSelect.Enabled = false;
                return;
            }

            foreach (var achievement in _availableAchievements)
            {
                Panel itemPanel = CreateAchievementItemPanel(achievement);
                flpAchievements.Controls.Add(itemPanel);
            }
        }

        private Panel CreateAchievementItemPanel(Achievement achievement)
        {
            var game = _libraryManager.Games.FirstOrDefault(g => g.Id == achievement.GameId);
            string gameTitle = game?.Title ?? "Unknown Game";

            Panel pnl = new Panel
            {
                Size = new Size(380, 64),
                BackColor = Color.FromArgb(30, 30, 36),
                Margin = new Padding(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                Tag = achievement
            };

            // Achievement Icon
            PictureBox pbIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(8, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = MediaManager.GetImageOrPlaceholder(achievement.IconPath, "icon")
            };

            // Title Label
            Label lblTitle = new Label
            {
                Text = achievement.Title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(64, 6),
                Size = new Size(240, 18),
                AutoEllipsis = true
            };

            // Game Badge & Points
            Label lblSub = new Label
            {
                Text = $"{gameTitle} • {achievement.Points} pts • {achievement.Rarity}",
                ForeColor = Color.FromArgb(99, 102, 241),
                Font = new Font("Segoe UI Symbol", 8.2F, FontStyle.Italic),
                Location = new Point(64, 25),
                Size = new Size(300, 16),
                AutoEllipsis = true
            };

            // Description Label
            Label lblDesc = new Label
            {
                Text = achievement.Description,
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8.2F),
                Location = new Point(64, 42),
                Size = new Size(300, 16),
                AutoEllipsis = true
            };

            pnl.Controls.Add(pbIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblSub);
            pnl.Controls.Add(lblDesc);

            // Click Handlers
            MouseEventHandler selectHandler = (s, e) =>
            {
                SelectPanel(pnl);
            };

            pnl.MouseClick += selectHandler;
            pbIcon.MouseClick += selectHandler;
            lblTitle.MouseClick += selectHandler;
            lblSub.MouseClick += selectHandler;
            lblDesc.MouseClick += selectHandler;

            // Double Click to Select directly
            EventHandler doubleClickHandler = (s, e) =>
            {
                SelectedAchievement = achievement;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            pnl.DoubleClick += doubleClickHandler;
            pbIcon.DoubleClick += doubleClickHandler;
            lblTitle.DoubleClick += doubleClickHandler;
            lblSub.DoubleClick += doubleClickHandler;
            lblDesc.DoubleClick += doubleClickHandler;

            // Hover transitions
            pnl.MouseEnter += (s, e) => { if (pnl != _selectedPanel) pnl.BackColor = Color.FromArgb(44, 44, 52); };
            pnl.MouseLeave += (s, e) => { if (pnl != _selectedPanel) pnl.BackColor = Color.FromArgb(30, 30, 36); };

            return pnl;
        }

        private void SelectPanel(Panel panel)
        {
            if (_selectedPanel != null)
            {
                _selectedPanel.BackColor = Color.FromArgb(30, 30, 36);
            }

            _selectedPanel = panel;
            _selectedPanel.BackColor = Color.FromArgb(55, 48, 163); // Indigo selected background
            SelectedAchievement = _selectedPanel.Tag as Achievement;
        }

        private void btnSelect_Click(object? sender, EventArgs e)
        {
            if (SelectedAchievement != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select an achievement first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetupHover(Button btn, Color normalColor, Color hoverColor)
        {
            btn.BackColor = normalColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }
    }
}
