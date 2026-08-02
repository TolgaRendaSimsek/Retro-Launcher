using System;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class EmptyStatePanel : UserControl
    {
        private readonly Label _lblIcon;
        private readonly Label _lblTitle;
        private readonly Label _lblDescription;
        private readonly ModernButton _btnAction;

        public event EventHandler? ActionClicked;

        public EmptyStatePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(400, 220);
            Padding = new Padding(20);
            BackColor = AppTheme.Current.Colors.Surface;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _lblIcon = new Label
            {
                Text = "🎮",
                Font = new Font("Segoe UI Emoji", 28F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 6)
            };
            tlp.Controls.Add(_lblIcon, 0, 0);

            _lblTitle = new Label
            {
                Text = "No Items Found",
                Font = AppTheme.Current.Fonts.TitleMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 4)
            };
            tlp.Controls.Add(_lblTitle, 0, 1);

            _lblDescription = new Label
            {
                Text = "Get started by adding items or adjusting filters.",
                Font = AppTheme.Current.Fonts.BodyMedium,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 12)
            };
            tlp.Controls.Add(_lblDescription, 0, 2);

            _btnAction = new ModernButton
            {
                Text = "Action",
                IsPrimary = true,
                Size = new Size(130, 36),
                Anchor = AnchorStyles.Top
            };
            _btnAction.Click += (s, e) => ActionClicked?.Invoke(this, EventArgs.Empty);

            tlp.Controls.Add(_btnAction, 0, 3);
            Controls.Add(tlp);
        }

        public void Configure(string icon, string title, string description, string actionText = "")
        {
            _lblIcon.Text = icon;
            _lblTitle.Text = title;
            _lblDescription.Text = description;

            if (!string.IsNullOrEmpty(actionText))
            {
                _btnAction.Text = actionText;
                _btnAction.Visible = true;
            }
            else
            {
                _btnAction.Visible = false;
            }
        }
    }
}
