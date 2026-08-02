using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RetroLauncher.Services
{
    public static class RomDetector
    {
        private static readonly string[] Consoles = new[]
        {
            "Sony PlayStation 1",
            "Sony PlayStation 2",
            "Sony PlayStation 3",
            "Sony PlayStation Portable (PSP)",
            "Game Boy",
            "Game Boy Color",
            "Game Boy Advance",
            "Nintendo Entertainment System (NES)",
            "Super Nintendo (SNES)",
            "Nintendo DS",
            "Nintendo 64",
            "Sega Genesis"
        };

        public static string? DetectConsole(string romPath)
        {
            if (string.IsNullOrEmpty(romPath)) return null;

            // Folder game check (e.g. PS3 directory game structure containing PS3_GAME)
            if (Directory.Exists(romPath))
            {
                string ps3GamePath = Path.Combine(romPath, "PS3_GAME");
                if (Directory.Exists(ps3GamePath))
                {
                    return "Sony PlayStation 3";
                }
                // Other directories are ambiguous
                return null;
            }

            string ext = Path.GetExtension(romPath).ToLower();

            return ext switch
            {
                // Unambiguous PS1 formats
                ".cue" or ".pbp" or ".m3u" => "Sony PlayStation 1",
                
                // Unambiguous PS3 formats
                ".pkg" or ".rap" => "Sony PlayStation 3",
                
                // Unambiguous Game Boy formats
                ".gb" => "Game Boy",
                ".gbc" => "Game Boy Color",
                ".gba" => "Game Boy Advance",

                // Unambiguous Nintendo DS format
                ".nds" => "Nintendo DS",
                
                // Unambiguous NES & SNES & N64 formats
                ".nes" => "Nintendo Entertainment System (NES)",
                ".sfc" or ".smc" => "Super Nintendo (SNES)",
                ".n64" or ".z64" => "Nintendo 64",

                // Unambiguous Sega formats
                ".md" or ".smd" => "Sega Genesis",

                // Ambiguous formats (like .iso, .chd, .cso, .bin which map to multiple consoles)
                // returning null triggers manual console choice
                _ => null
            };
        }

        public static void DetectConsoleAndEmulator(string romPath, IWin32Window parent, out string consoleName, out string emulatorPath)
        {
            consoleName = "";
            emulatorPath = "";

            string? detected = DetectConsole(romPath);

            if (detected != null)
            {
                consoleName = detected;
                emulatorPath = GetDefaultEmulatorForConsole(detected);
                return;
            }

            // Ambiguous extension or directory - Prompt the user to select the Console manually
            string ext = Directory.Exists(romPath) ? "Folder/Directory" : Path.GetExtension(romPath);
            if (string.IsNullOrEmpty(ext)) ext = "Unknown";

            string? chosenConsole = PromptForConsoleSelection(parent, ext);
            if (!string.IsNullOrEmpty(chosenConsole))
            {
                consoleName = chosenConsole;
                emulatorPath = GetDefaultEmulatorForConsole(chosenConsole);
            }
        }

        private static string GetDefaultEmulatorForConsole(string console)
        {
            var emuConfig = EmulatorManager.LoadConfig();
            if (emuConfig.DefaultEmulators.TryGetValue(console, out string? path))
            {
                return path;
            }
            return "";
        }

        private static string? PromptForConsoleSelection(IWin32Window parent, string ext)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 220;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Select Console Platform";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.FromArgb(24, 24, 28);
                prompt.ForeColor = Color.White;

                Label textLabel = new Label
                {
                    Left = 20,
                    Top = 15,
                    Width = 350,
                    Height = 40,
                    Text = $"The ROM format '{ext}' is ambiguous or unrecognized.\n\nPlease select the correct console platform manually:"
                };
                textLabel.ForeColor = Color.FromArgb(209, 213, 223);

                ComboBox comboBox = new ComboBox
                {
                    Left = 20,
                    Top = 65,
                    Width = 340,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                comboBox.BackColor = Color.FromArgb(44, 44, 52);
                comboBox.ForeColor = Color.White;
                comboBox.FlatStyle = FlatStyle.Flat;

                foreach (var console in Consoles)
                {
                    comboBox.Items.Add(console);
                }
                comboBox.SelectedIndex = 0;

                Button confirmation = new Button
                {
                    Text = "Select",
                    Left = 260,
                    Width = 100,
                    Top = 115,
                    Height = 35,
                    DialogResult = DialogResult.OK
                };
                confirmation.BackColor = Color.FromArgb(16, 185, 129);
                confirmation.ForeColor = Color.White;
                confirmation.FlatStyle = FlatStyle.Flat;
                confirmation.FlatAppearance.BorderSize = 0;

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(comboBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog(parent) == DialogResult.OK ? comboBox.SelectedItem?.ToString() : null;
            }
        }
    }
}
