using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class ModernButton : Button
    {
        private bool _isHovered = false;
        private bool _isPressed = false;
        private bool _isPrimary = false;
        private int _cornerRadius = 8;

        public ModernButton()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = AppTheme.Current.Fonts.ButtonMedium;
            this.ForeColor = AppTheme.Current.Colors.TextPrimary;
            this.BackColor = Color.Transparent;
            this.Size = new Size(120, 36);
            this.Cursor = Cursors.Hand;
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPrimary
        {
            get => _isPrimary;
            set { _isPrimary = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                UpdateRegion();
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width > 0 && Height > 0)
            {
                using var path = GetRoundedPath(new Rectangle(0, 0, Width, Height), _cornerRadius);
                this.Region = new Region(path);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            base.OnMouseDown(mevent);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            base.OnMouseUp(mevent);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using GraphicsPath path = GetRoundedPath(rect, _cornerRadius);

            Color fillBg = _isPrimary
                ? (_isPressed ? AppTheme.Current.Colors.AccentGradientEnd : (_isHovered ? AppTheme.Current.Colors.AccentHover : AppTheme.Current.Colors.AccentPrimary))
                : (_isPressed ? AppTheme.Current.Colors.SurfaceCardSelected : (_isHovered ? AppTheme.Current.Colors.SurfaceCardHover : AppTheme.Current.Colors.SurfaceCard));

            if (!Enabled)
            {
                fillBg = AppTheme.Current.Colors.Surface;
            }

            using (SolidBrush brush = new SolidBrush(fillBg))
            {
                g.FillPath(brush, path);
            }

            Color borderColor = _isHovered ? AppTheme.Current.Colors.BorderHover : AppTheme.Current.Colors.Border;
            using (Pen pen = new Pen(borderColor, 1F))
            {
                g.DrawPath(pen, path);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                Color textColor = Enabled ? (this.ForeColor) : AppTheme.Current.Colors.TextMuted;
                Rectangle textRect = new Rectangle(0, 0, Width, Height);
                if (_isPressed)
                {
                    textRect.Offset(1, 1);
                }
                TextRenderer.DrawText(g, Text, Font, textRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
