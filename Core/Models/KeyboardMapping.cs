using System.Windows.Forms;
using RetroLauncher.Core.Enums;

namespace RetroLauncher.Core.Models
{
    public sealed class KeyboardMapping
    {
        public VirtualControllerAction Action { get; init; }
        public Keys? Key { get; set; }
    }
}
