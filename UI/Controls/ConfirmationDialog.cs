using System;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class ConfirmationDialog : Form
    {
        private readonly Label _lblTitle;
        private readonly Label _lblMessage;
        private readonly ModernButton _btnConfirm;
        private readonly ModernButton _btnCancel;

        public ConfirmationDialog(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel", bool isDestructive = false)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Size = new Size(420, 180);
            BackColor = AppTheme.Current.Colors.Surface;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20),
                Margin = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _lblTitle = new Label
            {
                Text = title,
                Font = AppTheme.Current.Fonts.TitleMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 8)
            };
            tlp.Controls.Add(_lblTitle, 0, 0);

            _lblMessage = new Label
            {
                Text = message,
                Font = AppTheme.Current.Fonts.BodyMedium,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 16)
            };
            tlp.Controls.Add(_lblMessage, 0, 1);

            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0)
            };

            _btnConfirm = new ModernButton
            {
                Text = confirmText,
                Size = new Size(100, 34),
                IsPrimary = !isDestructive,
                Margin = new Padding(6, 0, 0, 0)
            };
            if (isDestructive)
            {
                _btnConfirm.BackColor = AppTheme.Current.Colors.StatusError;
            }
            _btnConfirm.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            _btnCancel = new ModernButton
            {
                Text = cancelText,
                Size = new Size(90, 34),
                IsPrimary = false,
                Margin = new Padding(0)
            };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            flpButtons.Controls.Add(_btnConfirm);
            flpButtons.Controls.Add(_btnCancel);

            tlp.Controls.Add(flpButtons, 0, 2);
            Controls.Add(tlp);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using Pen pen = new Pen(AppTheme.Current.Colors.Border, 1F);
            e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        public static bool Confirm(IWin32Window owner, string title, string message, string confirmText = "Confirm", bool isDestructive = false)
        {
            using var dialog = new ConfirmationDialog(title, message, confirmText, "Cancel", isDestructive);
            return dialog.ShowDialog(owner) == DialogResult.OK;
        }
    }
}
