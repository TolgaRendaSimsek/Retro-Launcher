using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class ToastNotification : Form
    {
        private readonly System.Windows.Forms.Timer _dismissTimer;
        private readonly Label _lblIcon;
        private readonly Label _lblMessage;

        public ToastNotification(string message, StatusType type = StatusType.Success, int durationMs = 3000)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(320, 50);
            BackColor = AppTheme.Current.Colors.SurfaceCard;

            _lblIcon = new Label
            {
                Text = type switch { StatusType.Success => "✅", StatusType.Warning => "⚠️", StatusType.Error => "❌", _ => "ℹ️" },
                Font = new Font("Segoe UI Emoji", 12F),
                Size = new Size(36, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Left
            };
            Controls.Add(_lblIcon);

            _lblMessage = new Label
            {
                Text = message,
                Font = AppTheme.Current.Fonts.BodyMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 0, 8, 0)
            };
            Controls.Add(_lblMessage);

            _dismissTimer = new System.Windows.Forms.Timer { Interval = durationMs };
            _dismissTimer.Tick += (s, e) =>
            {
                _dismissTimer.Stop();
                Close();
            };
        }

        public static void ShowToast(Form owner, string message, StatusType type = StatusType.Success, int durationMs = 3000)
        {
            var toast = new ToastNotification(message, type, durationMs);
            if (owner != null && !owner.IsDisposed)
            {
                Point ownerLoc = owner.Location;
                toast.Location = new Point(ownerLoc.X + owner.Width - toast.Width - 24, ownerLoc.Y + owner.Height - toast.Height - 24);
            }
            toast.Show();
            toast._dismissTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            using Pen pen = new Pen(AppTheme.Current.Colors.AccentPrimary, 1.5F);
            g.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));
        }
    }
}
