using System.Drawing;
using System.Windows.Forms;

namespace RetroLauncher.UI.Controls
{
    public class IconButton : ModernButton
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Icon { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconEmoji { get; set; } = "";

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (Icon != null)
            {
                Graphics g = pevent.Graphics;
                int iconSize = 18;
                int y = (Height - iconSize) / 2;
                g.DrawImage(Icon, new Rectangle(10, y, iconSize, iconSize));
            }
        }
    }
}
