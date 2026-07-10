using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace RetroLauncher
{
    public partial class GameCard : UserControl
    {
        public event EventHandler? CardSelected;
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
                    this.Invalidate(); // Force repaint to update border highlights
                }
            }
        }

        public GameCard(Game game)
        {
            Game = game;
            InitializeComponent();
            SetupCard();
        }

        private void SetupCard()
        {
            // Populate content
            lblTitle.Text = Game.Title;
            
            // Shorten platform name for clean display
            string shortConsole = Game.Platform;
            if (shortConsole.StartsWith("Sony PlayStation", StringComparison.OrdinalIgnoreCase))
            {
                shortConsole = "PS" + shortConsole.Substring("Sony PlayStation".Length).Trim();
            }
            lblConsole.Text = $"{shortConsole} • {Game.TotalPlaytimeMinutes}m";

            // Load cover image
            string resolvedPath = ResolvePath(Game.CoverImagePath);
            Image? img = LoadImageFromFile(resolvedPath);
            pbCover.Image = img ?? CreatePlaceholderImage(Game.Title);

            // Bubble up clicks on all children
            EventHandler clickHandler = (s, e) => CardSelected?.Invoke(this, EventArgs.Empty);
            this.Click += clickHandler;
            pbCover.Click += clickHandler;
            lblTitle.Click += clickHandler;
            lblConsole.Click += clickHandler;

            // Bind mouse hover states across child components
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
                // Ensure the cursor has actually left the bounds of the UserControl
                Point clientMouse = this.PointToClient(Cursor.Position);
                if (!this.ClientRectangle.Contains(clientMouse))
                {
                    _isHovered = false;
                    this.Invalidate();
                }
            };

            this.MouseEnter += hoverEnter;
            pbCover.MouseEnter += hoverEnter;
            lblTitle.MouseEnter += hoverEnter;
            lblConsole.MouseEnter += hoverEnter;

            this.MouseLeave += hoverLeave;
            pbCover.MouseLeave += hoverLeave;
            lblTitle.MouseLeave += hoverLeave;
            lblConsole.MouseLeave += hoverLeave;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Do not paint control backgrounds normally to support rounded corners
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Border color decision
            Color borderColor = Color.FromArgb(44, 44, 52); // Normal
            if (IsSelected)
            {
                borderColor = Color.FromArgb(99, 102, 241); // Selected Active (Indigo)
            }
            else if (_isHovered)
            {
                borderColor = Color.FromArgb(75, 85, 99); // Hover (Lighter Slate)
            }

            // Fill rounded card background
            using (var bgBrush = new SolidBrush(Color.FromArgb(28, 28, 34)))
            using (var path = GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 8))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            // Draw rounded border outline
            using (var pen = new Pen(borderColor, 2))
            using (var path = GetRoundedPath(new Rectangle(1, 1, Width - 3, Height - 3), 8))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (File.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(testPath2)) return testPath2;

            return testPath1;
        }

        private Image? LoadImageFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private Image CreatePlaceholderImage(string title)
        {
            Bitmap bmp = new Bitmap(180, 240);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 180, 240),
                    Color.FromArgb(40, 40, 50),
                    Color.FromArgb(20, 20, 25),
                    45f))
                {
                    g.FillRectangle(brush, 0, 0, 180, 240);
                }

                using (var pen = new Pen(Color.FromArgb(70, 75, 95), 2))
                {
                    g.DrawRectangle(pen, 1, 1, 178, 238);
                }

                using (var accentBrush = new SolidBrush(Color.FromArgb(99, 102, 241)))
                {
                    g.FillRectangle(accentBrush, 20, 40, 140, 8);
                    g.FillRectangle(accentBrush, 20, 55, 140, 3);
                }

                using (Font font = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.FromArgb(220, 225, 235)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(title, font, brush, new Rectangle(10, 80, 160, 100), sf);
                }

                using (Font miniFont = new Font("Segoe UI", 8, FontStyle.Bold))
                using (Brush badgeBg = new SolidBrush(Color.FromArgb(99, 102, 241)))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(badgeBg, 50, 200, 80, 20);
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("RETRO", miniFont, textBrush, new Rectangle(50, 200, 80, 20), sf);
                }
            }
            return bmp;
        }
    }
}
