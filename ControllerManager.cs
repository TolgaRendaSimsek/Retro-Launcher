using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher
{
    public enum ControllerType
    {
        Xbox,
        PlayStation,
        Nintendo,
        EightBitDo,
        Generic
    }

    public class ControllerDevice
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public ControllerType Type { get; set; }
        public string Status { get; set; } = "Disconnected";
    }

    public class ControllerProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string ControllerTypeName { get; set; } = "Generic"; // Xbox, PlayStation, Nintendo, 8BitDo, Generic
        public string? TargetEmulatorId { get; set; }
        public string? TargetGameId { get; set; }
        public Dictionary<string, string> Mappings { get; set; } = new();
    }

    public class ControllerDatabase
    {
        public List<ControllerProfile> Profiles { get; set; } = new();
    }

    public class ControllerManager
    {
        private static readonly string ControllerJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "controller_profiles.json");
        private static readonly object FileLock = new object();

        private ControllerDatabase _db = new();
        private static ControllerManager? _instance;

        public static ControllerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ControllerManager();
                }
                return _instance;
            }
        }

        // Win32 struct and imports for joystick/gamepad detection
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct JOYCAPS
        {
            public ushort wMid;
            public ushort wPid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint wXmin;
            public uint wXmax;
            public uint wYmin;
            public uint wYmax;
            public uint wZmin;
            public uint wZmax;
            public uint wNumButtons;
            public uint wPeriodMin;
            public uint wPeriodMax;
            public uint wRmin;
            public uint wRmax;
            public uint wUmin;
            public uint wUmax;
            public uint wVmin;
            public uint wVmax;
            public uint wCaps;
            public uint wMaxAxes;
            public uint wNumAxes;
            public uint wMaxButtons;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szRegKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szOEMVxD;
        }

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int joyGetNumDevs();

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int joyGetDevCaps(IntPtr uJoyID, ref JOYCAPS pjc, int cbjc);

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYINFOEX
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwXpos;
            public uint dwYpos;
            public uint dwZpos;
            public uint dwRpos;
            public uint dwUpos;
            public uint dwVpos;
            public uint dwButtons;
            public uint dwButtonNumber;
            public uint dwPOV;
            public uint dwReserved1;
            public uint dwReserved2;
        }

        [DllImport("winmm.dll")]
        private static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

        private const int JOYERR_NOERROR = 0;

        public ControllerManager()
        {
            LoadProfiles();
        }

        public List<ControllerProfile> Profiles => _db.Profiles;

        public List<ControllerDevice> DetectConnectedControllers()
        {
            List<ControllerDevice> list = new List<ControllerDevice>();
            int maxDevices = joyGetNumDevs();

            // Windows joystick IDs typically range from 0 to 15
            int limit = Math.Min(maxDevices, 16);
            for (int i = 0; i < limit; i++)
            {
                JOYCAPS caps = new JOYCAPS();
                int result = joyGetDevCaps((IntPtr)i, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));
                if (result == JOYERR_NOERROR)
                {
                    string name = caps.szPname;
                    ControllerType type = ResolveTypeByName(name);
                    list.Add(new ControllerDevice
                    {
                        Id = i,
                        ProductName = name,
                        Type = type,
                        Status = "Connected"
                    });
                }
            }

            return list;
        }

        public string GetControllerName(int deviceId)
        {
            JOYCAPS caps = new JOYCAPS();
            int result = joyGetDevCaps((IntPtr)deviceId, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));
            if (result == JOYERR_NOERROR)
            {
                return caps.szPname;
            }
            return "Unknown Gamepad";
        }

        public bool GetJoystickState(int deviceId, ref JOYINFOEX info)
        {
            info.dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX));
            info.dwFlags = 0xFF; // JOY_RETURNALL
            int result = joyGetPosEx(deviceId, ref info);
            return result == JOYERR_NOERROR;
        }

        public ControllerProfile CreateControllerProfile(string name, string typeName)
        {
            var profile = new ControllerProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ControllerTypeName = typeName,
                Mappings = GetDefaultMappings()
            };

            lock (FileLock)
            {
                _db.Profiles.Add(profile);
            }
            SaveProfiles();
            return profile;
        }

        public bool EditControllerProfile(ControllerProfile profile)
        {
            lock (FileLock)
            {
                var existing = _db.Profiles.FirstOrDefault(p => p.Id == profile.Id);
                if (existing != null)
                {
                    existing.Name = profile.Name;
                    existing.ControllerTypeName = profile.ControllerTypeName;
                    existing.TargetEmulatorId = profile.TargetEmulatorId;
                    existing.TargetGameId = profile.TargetGameId;
                    existing.Mappings = new Dictionary<string, string>(profile.Mappings);
                    SaveProfiles();
                    return true;
                }
            }
            return false;
        }

        public bool DeleteControllerProfile(string profileId)
        {
            lock (FileLock)
            {
                var existing = _db.Profiles.FirstOrDefault(p => p.Id == profileId);
                if (existing != null)
                {
                    _db.Profiles.Remove(existing);
                    SaveProfiles();
                    return true;
                }
            }
            return false;
        }

        public bool AssignProfileToGame(string gameId, string? profileId)
        {
            lock (FileLock)
            {
                // Clear any existing assignment for this game
                foreach (var p in _db.Profiles)
                {
                    if (p.TargetGameId == gameId)
                    {
                        p.TargetGameId = null;
                    }
                }

                if (profileId != null)
                {
                    var target = _db.Profiles.FirstOrDefault(p => p.Id == profileId);
                    if (target != null)
                    {
                        target.TargetGameId = gameId;
                        target.TargetEmulatorId = null; // Mutually exclusive
                        SaveProfiles();
                        return true;
                    }
                }
                else
                {
                    SaveProfiles();
                    return true;
                }
            }
            return false;
        }

        public bool AssignProfileToEmulator(string emulatorId, string? profileId)
        {
            lock (FileLock)
            {
                // Clear any existing assignment for this emulator
                foreach (var p in _db.Profiles)
                {
                    if (p.TargetEmulatorId == emulatorId)
                    {
                        p.TargetEmulatorId = null;
                    }
                }

                if (profileId != null)
                {
                    var target = _db.Profiles.FirstOrDefault(p => p.Id == profileId);
                    if (target != null)
                    {
                        target.TargetEmulatorId = emulatorId;
                        target.TargetGameId = null; // Mutually exclusive
                        SaveProfiles();
                        return true;
                    }
                }
                else
                {
                    SaveProfiles();
                    return true;
                }
            }
            return false;
        }

        public void LoadProfiles()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(ControllerJsonPath))
                    {
                        string json = File.ReadAllText(ControllerJsonPath);
                        var db = JsonSerializer.Deserialize<ControllerDatabase>(json);
                        if (db != null)
                        {
                            _db = db;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading controller profiles: {ex.Message}");
                }
            }
        }

        public void SaveProfiles()
        {
            lock (FileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_db, options);
                    File.WriteAllText(ControllerJsonPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving controller profiles: {ex.Message}");
                }
            }
        }

        private ControllerType ResolveTypeByName(string name)
        {
            string lowerName = name.ToLowerInvariant();
            if (lowerName.Contains("xbox") || lowerName.Contains("xinput") || lowerName.Contains("360"))
            {
                return ControllerType.Xbox;
            }
            if (lowerName.Contains("sony") || lowerName.Contains("dualshock") || lowerName.Contains("dualsense") || lowerName.Contains("wireless controller") || lowerName.Contains("playstation"))
            {
                return ControllerType.PlayStation;
            }
            if (lowerName.Contains("switch") || lowerName.Contains("nintendo") || lowerName.Contains("pro controller"))
            {
                return ControllerType.Nintendo;
            }
            if (lowerName.Contains("8bitdo"))
            {
                return ControllerType.EightBitDo;
            }
            return ControllerType.Generic;
        }

        private Dictionary<string, string> GetDefaultMappings()
        {
            return new Dictionary<string, string>
            {
                { "Dpad_Up", "Button 0" },
                { "Dpad_Down", "Button 1" },
                { "Dpad_Left", "Button 2" },
                { "Dpad_Right", "Button 3" },
                { "A_Button", "Button 4" },
                { "B_Button", "Button 5" },
                { "X_Button", "Button 6" },
                { "Y_Button", "Button 7" },
                { "L1_Shoulder", "Button 8" },
                { "R1_Shoulder", "Button 9" },
                { "L2_Trigger", "Button 10" },
                { "R2_Trigger", "Button 11" },
                { "L3_Stick", "Button 12" },
                { "R3_Stick", "Button 13" },
                { "Start", "Button 14" },
                { "Select", "Button 15" }
            };
        }
    }
}
