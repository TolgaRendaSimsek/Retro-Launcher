using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.Core.Enums;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class KeyCaptureControl : Control
    {
        private Keys? _selectedKey;
        private Keys? _previousKey;
        private bool _isCapturing;
        private bool _isHovered;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VirtualControllerAction Action { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Keys? SelectedKey
        {
            get => _selectedKey;
            set
            {
                _selectedKey = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCapturing
        {
            get => _isCapturing;
            private set
            {
                _isCapturing = value;
                Invalidate();
            }
        }

        public event EventHandler<Keys?>? KeyCaptured;

        public KeyCaptureControl()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            this.Size = new Size(180, 30);
            this.Cursor = Cursors.Hand;
            this.TabStop = true;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (_isCapturing)
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.Focus();
            StartCapture();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (_isCapturing)
            {
                CancelCapture();
            }
            Invalidate();
        }

        public void StartCapture()
        {
            if (_isCapturing) return;
            _previousKey = _selectedKey;
            IsCapturing = true;
        }

        public void CancelCapture()
        {
            if (!_isCapturing) return;
            _selectedKey = _previousKey;
            IsCapturing = false;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (_isCapturing)
            {
                e.IsInputKey = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_isCapturing)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                Keys pressed = e.KeyCode;

                if (pressed == Keys.Escape)
                {
                    CancelCapture();
                    return;
                }

                if (pressed == Keys.Back || pressed == Keys.Delete)
                {
                    _selectedKey = null;
                }
                else
                {
                    _selectedKey = pressed;
                }

                IsCapturing = false;
                KeyCaptured?.Invoke(this, _selectedKey);
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color bgColor = AppTheme.Current.Colors.Surface;
            Color borderColor = AppTheme.Current.Colors.Border;
            Color textColor = AppTheme.Current.Colors.TextPrimary;

            if (_isCapturing)
            {
                bgColor = Color.FromArgb(40, 99, 102, 241); // Glowing accent background
                borderColor = AppTheme.Current.Colors.AccentPrimary;
                textColor = Color.FromArgb(165, 180, 252);
            }
            else if (this.Focused || _isHovered)
            {
                borderColor = AppTheme.Current.Colors.AccentPrimary;
            }

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, rect);
            }

            using (var pen = new Pen(borderColor, _isCapturing ? 2f : 1f))
            {
                g.DrawRectangle(pen, rect);
            }

            string textToDisplay = _isCapturing ? "⏳ Press a key..." : FormatKeyDisplay(_selectedKey);
            Font fontToUse = _isCapturing ? new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold) : AppTheme.Current.Fonts.BodySmall;

            TextRenderer.DrawText(
                g,
                textToDisplay,
                fontToUse,
                rect,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
            );
        }

        public static string FormatKeyDisplay(Keys? key)
        {
            if (key == null) return "[ Unassigned ]";

            return key.Value switch
            {
                Keys.Space => "Space",
                Keys.Return => "Enter",
                Keys.Back => "Backspace",
                Keys.Escape => "Escape",
                Keys.Tab => "Tab",
                Keys.LShiftKey => "Left Shift",
                Keys.RShiftKey => "Right Shift",
                Keys.ShiftKey => "Shift",
                Keys.LControlKey => "Left Ctrl",
                Keys.RControlKey => "Right Ctrl",
                Keys.ControlKey => "Ctrl",
                Keys.LMenu => "Left Alt",
                Keys.RMenu => "Right Alt",
                Keys.Menu => "Alt",
                Keys.Up => "Up Arrow",
                Keys.Down => "Down Arrow",
                Keys.Left => "Left Arrow",
                Keys.Right => "Right Arrow",
                Keys.D0 => "0",
                Keys.D1 => "1",
                Keys.D2 => "2",
                Keys.D3 => "3",
                Keys.D4 => "4",
                Keys.D5 => "5",
                Keys.D6 => "6",
                Keys.D7 => "7",
                Keys.D8 => "8",
                Keys.D9 => "9",
                Keys.NumPad0 => "Numpad 0",
                Keys.NumPad1 => "Numpad 1",
                Keys.NumPad2 => "Numpad 2",
                Keys.NumPad3 => "Numpad 3",
                Keys.NumPad4 => "Numpad 4",
                Keys.NumPad5 => "Numpad 5",
                Keys.NumPad6 => "Numpad 6",
                Keys.NumPad7 => "Numpad 7",
                Keys.NumPad8 => "Numpad 8",
                Keys.NumPad9 => "Numpad 9",
                _ => key.Value.ToString()
            };
        }
    }
}
