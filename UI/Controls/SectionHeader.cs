using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class SectionHeader : UserControl
    {
        private readonly Label _lblTitle;
        private readonly Label _lblSubtitle;
        private readonly ModernButton _btnAction;

        public event EventHandler? ActionClicked;

        public SectionHeader()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(600, 48);
            Padding = new Padding(0, 4, 0, 4);
            BackColor = Color.Transparent;

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
                Text = "Section Title",
                Font = AppTheme.Current.Fonts.TitleMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 2)
            };
            tlp.Controls.Add(_lblTitle, 0, 0);

            _lblSubtitle = new Label
            {
                Text = "Subtitle or description text",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0)
            };
            tlp.Controls.Add(_lblSubtitle, 0, 1);

            _btnAction = new ModernButton
            {
                Text = "Action",
                Size = new Size(95, 30),
                IsPrimary = false,
                Visible = false
            };
            _btnAction.Click += (s, e) => ActionClicked?.Invoke(this, EventArgs.Empty);

            tlp.Controls.Add(_btnAction, 1, 0);
            tlp.SetRowSpan(_btnAction, 2);

            Controls.Add(tlp);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _lblTitle.Text;
            set => _lblTitle.Text = value;
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Subtitle
        {
            get => _lblSubtitle.Text;
            set => _lblSubtitle.Text = value;
        }

        public void SetAction(string text)
        {
            _btnAction.Text = text;
            _btnAction.Visible = !string.IsNullOrEmpty(text);
        }
    }
}
