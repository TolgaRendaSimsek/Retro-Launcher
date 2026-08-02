using System;
using System.Collections.Generic;

namespace RetroLauncher.Core.Models
{
    public class PlayerControllerConfig
    {
        public int PlayerIndex { get; set; } = 1; // 1 to 4
        public string ControllerType { get; set; } = "XInput"; // XInput, DirectInput, Keyboard, Disabled
        public string DeviceGuidOrName { get; set; } = "";
        
        // Calibration & Thresholds
        public float Deadzone { get; set; } = 0.15f; // 0.0 to 0.50
        public float Sensitivity { get; set; } = 1.0f; // 0.5 to 2.0
        public float TriggerThreshold { get; set; } = 0.10f; // 0.0 to 0.50
        
        // Axis Inversions
        public bool InvertLeftStickX { get; set; }
        public bool InvertLeftStickY { get; set; }
        public bool InvertRightStickX { get; set; }
        public bool InvertRightStickY { get; set; }
        
        // Force Feedback / Vibration
        public bool EnableRumble { get; set; } = true;
        public float RumbleStrength { get; set; } = 1.0f;

        // Mappings: Standard Key -> Input Binding
        public Dictionary<string, string> ButtonMappings { get; set; } = GetDefaultButtonMappings();

        public static Dictionary<string, string> GetDefaultButtonMappings()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Dpad_Up", "Pad_KeyUp" },
                { "Dpad_Down", "Pad_KeyDown" },
                { "Dpad_Left", "Pad_KeyLeft" },
                { "Dpad_Right", "Pad_KeyRight" },
                { "A_Button", "Button_A" },
                { "B_Button", "Button_B" },
                { "X_Button", "Button_X" },
                { "Y_Button", "Button_Y" },
                { "L1_Shoulder", "Button_L1" },
                { "R1_Shoulder", "Button_R1" },
                { "L2_Trigger", "Trigger_L2" },
                { "R2_Trigger", "Trigger_R2" },
                { "L3_Stick", "Button_L3" },
                { "R3_Stick", "Button_R3" },
                { "Start", "Button_Start" },
                { "Select", "Button_Select" },
                { "LeftStick_Up", "Axis_LY-" },
                { "LeftStick_Down", "Axis_LY+" },
                { "LeftStick_Left", "Axis_LX-" },
                { "LeftStick_Right", "Axis_LX+" },
                { "RightStick_Up", "Axis_RY-" },
                { "RightStick_Down", "Axis_RY+" },
                { "RightStick_Left", "Axis_RX-" },
                { "RightStick_Right", "Axis_RX+" }
            };
        }
    }

    public class GlobalHotkeysConfig
    {
        public string Pause { get; set; } = "P";
        public string SaveState { get; set; } = "F1";
        public string LoadState { get; set; } = "F3";
        public string FastForward { get; set; } = "Tab";
        public string Screenshot { get; set; } = "F12";
        public string ToggleMenu { get; set; } = "Escape";
    }

    public class GlobalControllerConfig
    {
        public bool AutoSyncOnLaunch { get; set; } = false;
        public GlobalHotkeysConfig Hotkeys { get; set; } = new();
        public List<PlayerControllerConfig> Players { get; set; } = new();

        public GlobalControllerConfig()
        {
            // Seed Players 1..4
            for (int i = 1; i <= 4; i++)
            {
                Players.Add(new PlayerControllerConfig
                {
                    PlayerIndex = i,
                    ControllerType = i == 1 ? "XInput" : "Disabled"
                });
            }
        }
    }
}
