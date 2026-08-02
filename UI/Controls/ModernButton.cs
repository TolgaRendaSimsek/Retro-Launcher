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
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = AppTheme.Current.Fonts.ButtonMedium;
            this.ForeColor = AppTheme.Current.Colors.TextPrimary;
            this.BackColor = AppTheme.Current.Colors.SurfaceCard;
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
            set { _cornerRadius = value; Invalidate(); }
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

            Color textColor = Enabled ? AppTheme.Current.Colors.TextPrimary : AppTheme.Current.Colors.TextDisabled;
            TextRenderer.DrawText(g, Text, Font, rect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
