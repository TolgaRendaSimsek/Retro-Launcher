using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class SearchBox : UserControl
    {
        private readonly TextBox _textBox;
        private readonly Label _lblIcon;

        public event EventHandler? SearchTextChanged;

        public SearchBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(240, 34);
            Padding = new Padding(6, 4, 6, 4);
            BackColor = AppTheme.Current.Colors.SurfaceCard;

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _lblIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 9F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            tlp.Controls.Add(_lblIcon, 0, 0);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = AppTheme.Current.Colors.SurfaceCard,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodyMedium,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 0)
            };
            _textBox.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);

            tlp.Controls.Add(_textBox, 1, 0);
            Controls.Add(tlp);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SearchText
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PlaceholderText
        {
            get => _textBox.PlaceholderText;
            set => _textBox.PlaceholderText = value;
        }
    }
}
