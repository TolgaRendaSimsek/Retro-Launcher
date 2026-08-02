using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Resources.Localization
{
    public class LanguageSettings
    {
        public string SelectedLanguage { get; set; } = "en";
    }

    public class LocalizationManager
    {
        private static readonly string SettingsPath = ApplicationPaths.LanguageSettingsJson;
        private static readonly string LocalesDir = Path.Combine(AppContext.BaseDirectory, "locales");
        private static readonly object FileLock = new object();

        private static LocalizationManager? _instance;
        private LanguageSettings _settings = new();
        private Dictionary<string, string> _translations = new();
        private Dictionary<string, string> _fallbackTranslations = new();

        public event EventHandler? LanguageChanged;

        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }

        private LocalizationManager()
        {
            EnsureLocalesDirectory();
            LoadSelectedLanguage();
            LoadFallbackLanguage();
            LoadLanguage(_settings.SelectedLanguage);
        }

        public string CurrentLanguage => _settings.SelectedLanguage;

        private void EnsureLocalesDirectory()
        {
            try
            {
                if (!Directory.Exists(LocalesDir))
                {
                    Directory.CreateDirectory(LocalesDir);
                }

                // Write default English file if missing
                string enPath = Path.Combine(LocalesDir, "en.json");
                if (!File.Exists(enPath))
                {
                    var defaultEn = GetDefaultEnglishTranslations();
                    string json = JsonSerializer.Serialize(defaultEn, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(enPath, json);
                }

                // Write default Turkish file if missing
                string trPath = Path.Combine(LocalesDir, "tr.json");
                if (!File.Exists(trPath))
                {
                    var defaultTr = GetDefaultTurkishTranslations();
                    string json = JsonSerializer.Serialize(defaultTr, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(trPath, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating locales directory: {ex.Message}");
            }
        }

        public void LoadSelectedLanguage()
        {
            lock (FileLock)
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        string json = File.ReadAllText(SettingsPath);
                        var s = JsonSerializer.Deserialize<LanguageSettings>(json);
                        if (s != null)
                        {
                            _settings = s;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading language settings: {ex.Message}");
                }
                _settings = new LanguageSettings { SelectedLanguage = "en" };
            }
        }

        public void SaveSelectedLanguage(string languageCode)
        {
            lock (FileLock)
            {
                try
                {
                    _settings.SelectedLanguage = languageCode;
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_settings, options);
                    File.WriteAllText(SettingsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving language settings: {ex.Message}");
                }
            }
        }

        private void LoadFallbackLanguage()
        {
            string path = Path.Combine(LocalesDir, "en.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        _fallbackTranslations = dict;
                    }
                }
                catch
                {
                    _fallbackTranslations = GetDefaultEnglishTranslations();
                }
            }
            else
            {
                _fallbackTranslations = GetDefaultEnglishTranslations();
            }
        }

        public void LoadLanguage(string languageCode)
        {
            string path = Path.Combine(LocalesDir, $"{languageCode}.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        _translations = dict;
                        SaveSelectedLanguage(languageCode);
                        LanguageChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading language file: {ex.Message}");
                }
            }

            // Fallback
            _translations = new Dictionary<string, string>(_fallbackTranslations);
            SaveSelectedLanguage(languageCode);
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GetText(string key)
        {
            if (_translations.TryGetValue(key, out string? value))
            {
                return value;
            }
            if (_fallbackTranslations.TryGetValue(key, out string? fallbackValue))
            {
                return fallbackValue;
            }
            return key; // Return the key itself as a fallback
        }

        public void ApplyLanguage(Form form)
        {
            // Translate form title
            string formKey = form.Name;
            if (_translations.TryGetValue(formKey, out string? formTitle))
            {
                form.Text = formTitle;
            }
            else if (_fallbackTranslations.TryGetValue(formKey, out string? fallbackFormTitle))
            {
                form.Text = fallbackFormTitle;
            }

            // Translate controls recursively
            foreach (Control control in form.Controls)
            {
                ApplyLanguageToControl(control);
            }
        }

        private void ApplyLanguageToControl(Control control)
        {
            string key = control.Name;
            if (!string.IsNullOrEmpty(key))
            {
                string translatedText = GetText(key);
                if (translatedText != key)
                {
                    if (control is TextBox tb)
                    {
                        tb.PlaceholderText = translatedText;
                    }
                    else
                    {
                        control.Text = translatedText;
                    }
                }
            }

            // Recurse descendants
            foreach (Control child in control.Controls)
            {
                ApplyLanguageToControl(child);
            }
        }

        private Dictionary<string, string> GetDefaultEnglishTranslations()
        {
            return new Dictionary<string, string>
            {
                { "MainForm", "RetroLauncher - Classic Game Library" },
                { "btnPlay", "▶  PLAY" },
                { "btnAddGame", "➕  Add Game" },
                { "btnManageEmulators", "⚙️  Manage Emulators" },
                { "btnProfile", "👤  Profile & Friends" },
                { "btnAppearance", "🎨  Theme" },
                { "btnManageSaves", "💾  Manage Saves" },
                { "btnManageScreenshots", "📸  Manage Screenshots" },
                { "btnManageVideos", "📹  Manage Videos" },
                { "btnManageControllers", "🎮  Manage Controllers" },
                { "btnLanguageSettings", "🌐  Language" },
                { "lblSidebarHeader", "CONSOLES" },
                { "tbSearch", "Search library..." },
                
                { "lblGamesHeader", "GAMES" },
                { "lblVideosHeader", "RECORDED CLIPS" },
                { "lblPreviewHeader", "CLIP OPERATIONS" },
                
                { "ControllerManagerForm", "Controller Settings Manager" },
                { "lblDevicesHeader", "CONNECTED CONTROLLERS" },
                { "btnScan", "🔄  Scan Devices" },
                { "lblProfilesHeader", "PROFILES" },
                { "btnCreate", "➕  Create Profile" },
                { "btnDelete", "🗑️  Delete" },
                { "lblConfigHeader", "PROFILE MAPPING & ASSIGNMENT" },
                { "btnSaveProfile", "💾  Save Profile Changes" },
                { "btnTestInput", "🎮  Test Input" },
                
                { "ControllerTestInputForm", "Gamepad Input Diagnostic Test" },
                { "lblChoose", "Select Controller to Test:" },
                { "gbLiveState", "LIVE INPUT STATE" },
                { "lblPOVState", "D-Pad / POV: Centered" },
                { "btnCloseTest", "Close" },
                { "btnClose", "Close" }
            };
        }

        private Dictionary<string, string> GetDefaultTurkishTranslations()
        {
            return new Dictionary<string, string>
            {
                { "MainForm", "RetroLauncher - Klasik Oyun Kütüphanesi" },
                { "btnPlay", "▶  OYNA" },
                { "btnAddGame", "➕  Oyun Ekle" },
                { "btnManageEmulators", "⚙️  Emülatörler" },
                { "btnProfile", "👤  Profil & Arkadaşlar" },
                { "btnAppearance", "🎨  Tema" },
                { "btnManageSaves", "💾  Kayıtları Yönet" },
                { "btnManageScreenshots", "📸  Ekran Görüntüleri" },
                { "btnManageVideos", "📹  Videoları Yönet" },
                { "btnManageControllers", "🎮  Kontrolcüler" },
                { "btnLanguageSettings", "🌐  Dil Ayarı" },
                { "lblSidebarHeader", "KONSOL" },
                { "tbSearch", "Kütüphanede ara..." },
                
                { "lblGamesHeader", "OYUNLAR" },
                { "lblVideosHeader", "KAYITLI VİDEOLAR" },
                { "lblPreviewHeader", "VİDEO İŞLEMLERİ" },
                
                { "ControllerManagerForm", "Kontrolcü Ayarları Yöneticisi" },
                { "lblDevicesHeader", "BAĞLI KONTROLCÜLER" },
                { "btnScan", "🔄  Cihazları Tara" },
                { "lblProfilesHeader", "PROFİLLER" },
                { "btnCreate", "➕  Profil Oluştur" },
                { "btnDelete", "🗑️  Sil" },
                { "lblConfigHeader", "PROFİL EŞLEŞTİRME VE ATAMA" },
                { "btnSaveProfile", "💾  Profil Değişikliklerini Kaydet" },
                { "btnTestInput", "🎮  Girdiyi Test Et" },
                
                { "ControllerTestInputForm", "Kontrolcü Tuş Test Paneli" },
                { "lblChoose", "Test Edilecek Kontrolcüyü Seçin:" },
                { "gbLiveState", "ANLIK TUŞ DURUMU" },
                { "lblPOVState", "D-Pad / POV: Ortalı" },
                { "btnCloseTest", "Kapat" },
                { "btnClose", "Kapat" }
            };
        }
    }
}
