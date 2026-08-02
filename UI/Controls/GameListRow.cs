using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RetroLauncher.UI.Controls
{
    public partial class GameListRow : UserControl
    {
        public event EventHandler? RowSelected;
        public Game Game { get; private set; }

        private bool _isSelected = false;
        private bool _isHovered = false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    this.Invalidate();
                }
            }
        }

        public GameListRow(Game game)
        {
            Game = game;
            InitializeComponent();
            SetupRow();
        }

        private void SetupRow()
        {
            pbThumb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPlatform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPlaytime.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            lblTitle.Text = Game.Title;
            lblPlatform.Text = Game.Platform;
            lblPlaytime.Text = $"Playtime: {Game.TotalPlaytimeMinutes} mins";

            // Load thumbnail using MediaManager
            pbThumb.Image = MediaManager.GetImageOrPlaceholder(Game.IconImagePath, "icon");

            // Bubble clicks
            EventHandler clickHandler = (s, e) => RowSelected?.Invoke(this, EventArgs.Empty);
            this.Click += clickHandler;
            pbThumb.Click += clickHandler;
            lblTitle.Click += clickHandler;
            lblPlatform.Click += clickHandler;
            lblPlaytime.Click += clickHandler;

            // Hover state tracking
            EventHandler hoverEnter = (s, e) =>
            {
                if (!_isHovered)
                {
                    _isHovered = true;
                    this.Invalidate();
                }
            };
            EventHandler hoverLeave = (s, e) =>
            {
                Point clientMouse = this.PointToClient(Cursor.Position);
                if (!this.ClientRectangle.Contains(clientMouse))
                {
                    _isHovered = false;
                    this.Invalidate();
                }
            };

            this.MouseEnter += hoverEnter;
            pbThumb.MouseEnter += hoverEnter;
            lblTitle.MouseEnter += hoverEnter;
            lblPlatform.MouseEnter += hoverEnter;
            lblPlaytime.MouseEnter += hoverEnter;

            this.MouseLeave += hoverLeave;
            pbThumb.MouseLeave += hoverLeave;
            lblTitle.MouseLeave += hoverLeave;
            lblPlatform.MouseLeave += hoverLeave;
            lblPlaytime.MouseLeave += hoverLeave;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Highlight Background and borders based on selection state
            Color bgColor = Color.FromArgb(24, 24, 28);
            Color borderColor = Color.FromArgb(36, 36, 42);

            if (IsSelected)
            {
                bgColor = Color.FromArgb(38, 38, 48);
                borderColor = Color.FromArgb(99, 102, 241); // Indigo
            }
            else if (_isHovered)
            {
                bgColor = Color.FromArgb(32, 32, 38);
                borderColor = Color.FromArgb(75, 85, 99); // Lighter Slate
            }

            // Fill row background
            using (var bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Draw selection borders
            using (var pen = new Pen(borderColor, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Draw Favorite Star (★)
            if (Game.IsFavorite)
            {
                using (Font starFont = new Font("Segoe UI Symbol", 12F, FontStyle.Bold))
                using (Brush starBrush = new SolidBrush(Color.FromArgb(245, 158, 11))) // Gold/Amber
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    // Place it near the right edge before status dot
                    Rectangle starRect = new Rectangle(Width - 85, 0, 30, Height);
                    e.Graphics.DrawString("★", starFont, starBrush, starRect, sf);
                }
            }

            // Draw Installation Status indicator dot
            Color statusColor = Game.IsInstalled ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
            using (var dotBrush = new SolidBrush(statusColor))
            {
                int dotSize = 10;
                int dotX = Width - 30;
                int dotY = (Height / 2) - (dotSize / 2);
                e.Graphics.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Tooltip or helper check for missing ROM
            if (!Game.IsInstalled && _isHovered)
            {
                using (Font warningFont = new Font("Segoe UI", 7F, FontStyle.Italic))
                using (Brush warningBrush = new SolidBrush(Color.FromArgb(239, 68, 68)))
                {
                    e.Graphics.DrawString("ROM Missing", warningFont, warningBrush, Width - 100, Height - 14);
                }
            }
        }
    }
}
