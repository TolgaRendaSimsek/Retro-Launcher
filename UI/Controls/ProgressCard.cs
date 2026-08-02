using System;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class ProgressCard : UserControl
    {
        private readonly Label _lblTitle;
        private readonly Label _lblStatus;
        private readonly ProgressBar _pbProgress;
        private readonly ModernButton _btnCancel;
        private readonly ModernButton _btnDetails;

        public event EventHandler? CancelRequested;
        public event EventHandler? DetailsRequested;

        public ProgressCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(500, 100);
            Padding = new Padding(12);
            Margin = new Padding(8);
            BackColor = AppTheme.Current.Colors.SurfaceCard;
            Visible = false;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Margin = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _lblTitle = new Label
            {
                Text = "Installation in Progress...",
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 4)
            };
            tlp.Controls.Add(_lblTitle, 0, 0);

            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0)
            };
            _btnCancel = new ModernButton { Text = "Cancel", Size = new Size(70, 26), IsPrimary = false };
            _btnCancel.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);

            _btnDetails = new ModernButton { Text = "Details", Size = new Size(70, 26), IsPrimary = false };
            _btnDetails.Click += (s, e) => DetailsRequested?.Invoke(this, EventArgs.Empty);

            flpButtons.Controls.Add(_btnCancel);
            flpButtons.Controls.Add(_btnDetails);
            tlp.Controls.Add(flpButtons, 1, 0);

            _pbProgress = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 16,
                Margin = new Padding(0, 4, 0, 6)
            };
            tlp.Controls.Add(_pbProgress, 0, 1);
            tlp.SetColumnSpan(_pbProgress, 2);

            _lblStatus = new Label
            {
                Text = "Downloading archive... 0%",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0)
            };
            tlp.Controls.Add(_lblStatus, 0, 2);
            tlp.SetColumnSpan(_lblStatus, 2);

            Controls.Add(tlp);
        }

        public void UpdateProgress(string title, string status, int percent)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(title, status, percent)));
                return;
            }
            _lblTitle.Text = title;
            _lblStatus.Text = status;
            _pbProgress.Value = Math.Clamp(percent, 0, 100);
            Visible = true;
        }

        public void HideCard()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(HideCard));
                return;
            }
            Visible = false;
        }
    }
}
