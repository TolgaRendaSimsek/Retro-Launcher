using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class NavigationItem : Control
    {
        private bool _isSelected = false;
        private bool _isHovered = false;
        private string _icon = "🏠";
        private string _title = "Navigation";
        private string _targetPage = "";
        private ToolTip? _toolTip;

        public NavigationItem()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(200, 44);
            Margin = new Padding(4, 2, 4, 2);
            Cursor = Cursors.Hand;
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TargetPage
        {
            get => _targetPage;
            set { _targetPage = value; }
        }

        public void SetToolTip(string text)
        {
            _toolTip ??= new ToolTip();
            _toolTip.SetToolTip(this, text);
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
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            Color fill = _isSelected ? AppTheme.Current.Colors.SidebarItemSelected :
                         (_isHovered ? AppTheme.Current.Colors.SidebarItemHover : Color.Transparent);

            using (SolidBrush brush = new SolidBrush(fill))
            {
                g.FillRectangle(brush, rect);
            }

            // Draw active selection left indicator bar
            if (_isSelected)
            {
                using SolidBrush barBrush = new SolidBrush(AppTheme.Current.Colors.AccentPrimary);
                g.FillRectangle(barBrush, new Rectangle(0, 4, 4, Height - 8));
            }

            // Icon
            Font iconFont = new Font("Segoe UI Emoji", 11F, FontStyle.Regular);
            Color iconColor = _isSelected ? AppTheme.Current.Colors.AccentPrimary : AppTheme.Current.Colors.TextSecondary;
            TextRenderer.DrawText(g, _icon, iconFont, new Rectangle(14, 0, 30, Height), iconColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // Title
            Font textFont = _isSelected ? AppTheme.Current.Fonts.ButtonMedium : AppTheme.Current.Fonts.BodyMedium;
            Color textColor = _isSelected ? AppTheme.Current.Colors.TextPrimary : (_isHovered ? AppTheme.Current.Colors.TextPrimary : AppTheme.Current.Colors.TextSecondary);
            TextRenderer.DrawText(g, _title, textFont, new Rectangle(48, 0, Width - 52, Height), textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }
}
