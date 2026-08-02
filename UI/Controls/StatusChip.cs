using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public enum StatusType
    {
        Success,
        Warning,
        Error,
        Info
    }

    public class StatusChip : Control
    {
        private StatusType _statusType = StatusType.Info;

        public StatusChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(95, 24);
            Font = AppTheme.Current.Fonts.BadgeFont;
            Margin = new Padding(2);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StatusType StatusType
        {
            get => _statusType;
            set { _statusType = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color bg, fg;
            switch (_statusType)
            {
                case StatusType.Success:
                    bg = AppTheme.Current.Colors.StatusSuccessBg;
                    fg = AppTheme.Current.Colors.StatusSuccess;
                    break;
                case StatusType.Warning:
                    bg = AppTheme.Current.Colors.StatusWarningBg;
                    fg = AppTheme.Current.Colors.StatusWarning;
                    break;
                case StatusType.Error:
                    bg = AppTheme.Current.Colors.StatusErrorBg;
                    fg = AppTheme.Current.Colors.StatusError;
                    break;
                default:
                    bg = AppTheme.Current.Colors.StatusInfoBg;
                    fg = AppTheme.Current.Colors.StatusInfo;
                    break;
            }

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using GraphicsPath path = GetRoundedPath(rect, Height / 2);
            using (SolidBrush brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }
            using (Pen pen = new Pen(fg, 1F))
            {
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, rect, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
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
