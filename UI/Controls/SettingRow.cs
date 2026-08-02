using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class SettingRow : UserControl
    {
        private readonly Label _lblTitle;
        private readonly Label _lblDescription;
        private readonly Panel _pnlControlHost;

        public SettingRow()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(580, 56);
            Padding = new Padding(12, 8, 12, 8);
            Margin = new Padding(0, 0, 0, 8);
            BackColor = AppTheme.Current.Colors.SurfaceCard;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _lblTitle = new Label
            {
                Text = "Setting Title",
                Font = AppTheme.Current.Fonts.ButtonMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 2)
            };
            tlp.Controls.Add(_lblTitle, 0, 0);

            _lblDescription = new Label
            {
                Text = "Setting description and details",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0)
            };
            tlp.Controls.Add(_lblDescription, 0, 1);

            _pnlControlHost = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0)
            };
            tlp.Controls.Add(_pnlControlHost, 1, 0);
            tlp.SetRowSpan(_pnlControlHost, 2);

            Controls.Add(tlp);
        }

        public void Configure(string title, string description, Control inputControl)
        {
            _lblTitle.Text = title;
            _lblDescription.Text = description;

            _pnlControlHost.Controls.Clear();
            inputControl.Dock = DockStyle.Right;
            _pnlControlHost.Controls.Add(inputControl);
        }
    }
}
