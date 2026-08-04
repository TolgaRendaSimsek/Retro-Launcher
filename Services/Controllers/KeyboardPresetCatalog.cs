using System.Collections.Generic;
using System.Windows.Forms;
using RetroLauncher.Core.Enums;
using RetroLauncher.Core.Models;

namespace RetroLauncher.Services.Controllers
{
    public static class KeyboardPresetCatalog
    {
        public const string ModernWASD = "Modern WASD";
        public const string ArrowKeys = "Arrow Keys";
        public const string Custom = "Custom";

        public static List<string> GetPresetNames()
        {
            return new List<string> { ModernWASD, ArrowKeys, Custom };
        }

        public static List<KeyboardMapping> GetPresetMappings(string presetName)
        {
            return presetName switch
            {
                ArrowKeys => GetArrowKeysPreset(),
                Custom => new List<KeyboardMapping>(),
                _ => GetModernWASDPreset()
            };
        }

        public static List<KeyboardMapping> GetModernWASDPreset()
        {
            return new List<KeyboardMapping>
            {
                new() { Action = VirtualControllerAction.DPadUp, Key = Keys.W },
                new() { Action = VirtualControllerAction.DPadDown, Key = Keys.S },
                new() { Action = VirtualControllerAction.DPadLeft, Key = Keys.A },
                new() { Action = VirtualControllerAction.DPadRight, Key = Keys.D },

                new() { Action = VirtualControllerAction.LeftStickUp, Key = Keys.W },
                new() { Action = VirtualControllerAction.LeftStickDown, Key = Keys.S },
                new() { Action = VirtualControllerAction.LeftStickLeft, Key = Keys.A },
                new() { Action = VirtualControllerAction.LeftStickRight, Key = Keys.D },

                new() { Action = VirtualControllerAction.FaceSouth, Key = Keys.Space },
                new() { Action = VirtualControllerAction.FaceEast, Key = Keys.LShiftKey },
                new() { Action = VirtualControllerAction.FaceWest, Key = Keys.E },
                new() { Action = VirtualControllerAction.FaceNorth, Key = Keys.Q },

                new() { Action = VirtualControllerAction.L1, Key = Keys.R },
                new() { Action = VirtualControllerAction.R1, Key = Keys.F },
                new() { Action = VirtualControllerAction.L2, Key = Keys.Z },
                new() { Action = VirtualControllerAction.R2, Key = Keys.C },
                new() { Action = VirtualControllerAction.L3, Key = Keys.V },
                new() { Action = VirtualControllerAction.R3, Key = Keys.B },

                new() { Action = VirtualControllerAction.Start, Key = Keys.Return },
                new() { Action = VirtualControllerAction.Select, Key = Keys.Back },

                new() { Action = VirtualControllerAction.Pause, Key = Keys.Escape },
                new() { Action = VirtualControllerAction.FastForward, Key = Keys.Tab },
                new() { Action = VirtualControllerAction.SaveState, Key = Keys.F1 },
                new() { Action = VirtualControllerAction.LoadState, Key = Keys.F3 },
                new() { Action = VirtualControllerAction.Screenshot, Key = Keys.F12 },
                new() { Action = VirtualControllerAction.ToggleMenu, Key = Keys.F10 }
            };
        }

        public static List<KeyboardMapping> GetArrowKeysPreset()
        {
            return new List<KeyboardMapping>
            {
                new() { Action = VirtualControllerAction.DPadUp, Key = Keys.Up },
                new() { Action = VirtualControllerAction.DPadDown, Key = Keys.Down },
                new() { Action = VirtualControllerAction.DPadLeft, Key = Keys.Left },
                new() { Action = VirtualControllerAction.DPadRight, Key = Keys.Right },

                new() { Action = VirtualControllerAction.LeftStickUp, Key = Keys.Up },
                new() { Action = VirtualControllerAction.LeftStickDown, Key = Keys.Down },
                new() { Action = VirtualControllerAction.LeftStickLeft, Key = Keys.Left },
                new() { Action = VirtualControllerAction.LeftStickRight, Key = Keys.Right },

                new() { Action = VirtualControllerAction.FaceSouth, Key = Keys.Z },
                new() { Action = VirtualControllerAction.FaceEast, Key = Keys.X },
                new() { Action = VirtualControllerAction.FaceWest, Key = Keys.A },
                new() { Action = VirtualControllerAction.FaceNorth, Key = Keys.S },

                new() { Action = VirtualControllerAction.L1, Key = Keys.Q },
                new() { Action = VirtualControllerAction.R1, Key = Keys.W },
                new() { Action = VirtualControllerAction.L2, Key = Keys.E },
                new() { Action = VirtualControllerAction.R2, Key = Keys.R },

                new() { Action = VirtualControllerAction.Start, Key = Keys.Return },
                new() { Action = VirtualControllerAction.Select, Key = Keys.RShiftKey },

                new() { Action = VirtualControllerAction.Pause, Key = Keys.Escape },
                new() { Action = VirtualControllerAction.FastForward, Key = Keys.Tab },
                new() { Action = VirtualControllerAction.SaveState, Key = Keys.F1 },
                new() { Action = VirtualControllerAction.LoadState, Key = Keys.F3 },
                new() { Action = VirtualControllerAction.Screenshot, Key = Keys.F12 },
                new() { Action = VirtualControllerAction.ToggleMenu, Key = Keys.F10 }
            };
        }

        public static string GetActionDisplayName(VirtualControllerAction action)
        {
            return action switch
            {
                VirtualControllerAction.DPadUp => "D-Pad Up",
                VirtualControllerAction.DPadDown => "D-Pad Down",
                VirtualControllerAction.DPadLeft => "D-Pad Left",
                VirtualControllerAction.DPadRight => "D-Pad Right",
                VirtualControllerAction.LeftStickUp => "Left Stick Up",
                VirtualControllerAction.LeftStickDown => "Left Stick Down",
                VirtualControllerAction.LeftStickLeft => "Left Stick Left",
                VirtualControllerAction.LeftStickRight => "Left Stick Right",
                VirtualControllerAction.RightStickUp => "Right Stick Up",
                VirtualControllerAction.RightStickDown => "Right Stick Down",
                VirtualControllerAction.RightStickLeft => "Right Stick Left",
                VirtualControllerAction.RightStickRight => "Right Stick Right",
                VirtualControllerAction.FaceSouth => "A / Cross / Confirm",
                VirtualControllerAction.FaceEast => "B / Circle / Back",
                VirtualControllerAction.FaceWest => "X / Square",
                VirtualControllerAction.FaceNorth => "Y / Triangle",
                VirtualControllerAction.L1 => "L1 / LB",
                VirtualControllerAction.R1 => "R1 / RB",
                VirtualControllerAction.L2 => "L2 / LT",
                VirtualControllerAction.R2 => "R2 / RT",
                VirtualControllerAction.L3 => "L3 / Left Thumb",
                VirtualControllerAction.R3 => "R3 / Right Thumb",
                VirtualControllerAction.Start => "Start / Pause Game",
                VirtualControllerAction.Select => "Select / Back",
                VirtualControllerAction.Pause => "Pause Launcher Hotkey",
                VirtualControllerAction.SaveState => "Save State Hotkey",
                VirtualControllerAction.LoadState => "Load State Hotkey",
                VirtualControllerAction.FastForward => "Fast Forward Hotkey",
                VirtualControllerAction.Screenshot => "Screenshot Hotkey",
                VirtualControllerAction.ToggleMenu => "Toggle Menu Hotkey",
                _ => action.ToString()
            };
        }
    }
}
