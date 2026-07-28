using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RetroLauncher
{
    public interface IEmulatorPackageDefinitionProvider
    {
        IReadOnlyList<EmulatorPackageDefinition> GetAll();
        EmulatorPackageDefinition? GetById(string id);
        IReadOnlyList<EmulatorPackageDefinition> GetByConsole(string consoleName);
        void Validate(EmulatorPackageDefinition definition);
    }

    public class JsonEmulatorPackageDefinitionProvider : IEmulatorPackageDefinitionProvider
    {
        private readonly string _filePath;
        private readonly List<EmulatorPackageDefinition> _definitions = new();
        private bool _needsSave = false;

        public JsonEmulatorPackageDefinitionProvider(string? filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "emulator_definitions.json");
            LoadDefinitions();
        }

        private void LoadDefinitions()
        {
            try
            {
                bool loadedFromFile = false;
                if (File.Exists(_filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_filePath);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        };
                        options.Converters.Add(new JsonStringEnumConverter());

                        var list = JsonSerializer.Deserialize<List<EmulatorPackageDefinition>>(json, options);
                        if (list != null)
                        {
                            var verifiedList = new List<EmulatorPackageDefinition>();
                            foreach (var def in list)
                            {
                                try
                                {
                                    MigrateOldDirectory(def);
                                    Validate(def);
                                    // Check duplicates
                                    if (verifiedList.Any(x => string.Equals(x.Id, def.Id, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        RetroLogger.Log($"Duplicate emulator ID '{def.Id}' rejected in {_filePath}.", "WARNING");
                                        continue;
                                    }
                                    verifiedList.Add(def);
                                }
                                catch (Exception valEx)
                                {
                                    RetroLogger.Log($"Validation failed for emulator '{def.Id ?? "unknown"}' in {_filePath}: {valEx.Message}", "WARNING");
                                }
                            }
                            if (verifiedList.Count > 0)
                            {
                                _definitions.Clear();
                                _definitions.AddRange(verifiedList);
                                loadedFromFile = true;
                            }
                        }
                    }
                    catch (Exception fileEx)
                    {
                        RetroLogger.Log($"Failed to parse {_filePath}: {fileEx.Message}. Falling back to default definitions.", "WARNING");
                    }
                }

                if (!loadedFromFile)
                {
                    RetroLogger.Log("Using built-in fallback emulator definitions.");
                    var defaults = GetDefaultDefinitions();
                    _definitions.Clear();
                    _definitions.AddRange(defaults);
                    SaveDefinitions(); // Attempt to seed the directory
                }
                else if (_needsSave)
                {
                    RetroLogger.Log("Saving migrated emulator definitions back to disk.");
                    SaveDefinitions();
                }
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Error initializing emulator package definition provider: {ex.Message}", "ERROR");
            }
        }

        public void SaveDefinitions()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                string json = JsonSerializer.Serialize(_definitions, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Failed to save emulator definitions to {_filePath}: {ex.Message}", "WARNING");
            }
        }

        public IReadOnlyList<EmulatorPackageDefinition> GetAll() => _definitions.AsReadOnly();

        public EmulatorPackageDefinition? GetById(string id)
        {
            return _definitions.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<EmulatorPackageDefinition> GetByConsole(string consoleName)
        {
            return _definitions.Where(d => string.Equals(d.ConsoleName, consoleName, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }

        public void Validate(EmulatorPackageDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            // 1. Validate ID
            if (string.IsNullOrWhiteSpace(definition.Id))
            {
                throw new ArgumentException("Emulator ID cannot be empty.");
            }
            if (!Regex.IsMatch(definition.Id, "^[a-z0-9_-]+$"))
            {
                throw new ArgumentException($"Emulator ID '{definition.Id}' must be lowercase alphanumeric, containing only letters, numbers, hyphens, or underscores.");
            }

            // 2. Validate Display Name & Console
            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                throw new ArgumentException("Emulator DisplayName cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(definition.ConsoleName))
            {
                throw new ArgumentException("Emulator ConsoleName cannot be empty.");
            }

            // 3. Validate Repositories if GitHub-based source
            if (string.IsNullOrWhiteSpace(definition.GitHubOwner))
            {
                throw new ArgumentException("GitHubOwner is required.");
            }
            if (string.IsNullOrWhiteSpace(definition.GitHubRepository))
            {
                throw new ArgumentException("GitHubRepository is required.");
            }
            if (!Regex.IsMatch(definition.GitHubOwner, "^[a-zA-Z0-9-._]+$"))
            {
                throw new ArgumentException($"Invalid GitHubOwner '{definition.GitHubOwner}'.");
            }
            if (!Regex.IsMatch(definition.GitHubRepository, "^[a-zA-Z0-9-._]+$"))
            {
                throw new ArgumentException($"Invalid GitHubRepository '{definition.GitHubRepository}'.");
            }

            // 4. Validate Installation Directory (Zip Slip / Absolute check)
            if (string.IsNullOrWhiteSpace(definition.InstallDirectoryName))
            {
                throw new ArgumentException("InstallDirectoryName cannot be empty.");
            }
            if (definition.InstallDirectoryName.Contains("..") || 
                Path.IsPathRooted(definition.InstallDirectoryName) ||
                definition.InstallDirectoryName.Contains(":") ||
                definition.InstallDirectoryName.StartsWith("/") ||
                definition.InstallDirectoryName.StartsWith("\\"))
            {
                throw new ArgumentException($"InstallDirectoryName '{definition.InstallDirectoryName}' is unsafe or absolute.");
            }

            // 5. Validate Executable Candidates (Zip Slip / Absolute check)
            if (definition.ExecutableCandidates == null || definition.ExecutableCandidates.Count == 0)
            {
                throw new ArgumentException("ExecutableCandidates list cannot be empty.");
            }
            foreach (var exe in definition.ExecutableCandidates)
            {
                if (string.IsNullOrWhiteSpace(exe))
                {
                    throw new ArgumentException("Executable candidate name cannot be empty.");
                }
                if (exe.Contains("..") || 
                    Path.IsPathRooted(exe) || 
                    exe.Contains(":") || 
                    exe.StartsWith("/") || 
                    exe.StartsWith("\\"))
                {
                    throw new ArgumentException($"Executable candidate '{exe}' contains unsafe traversal path components.");
                }
            }

            // 6. Validate Launch Arguments Template to reject arbitrary executable command execution
            if (!string.IsNullOrEmpty(definition.LaunchArgumentsTemplate))
            {
                string dangerousPattern = @"[|&;$><`]";
                if (Regex.IsMatch(definition.LaunchArgumentsTemplate, dangerousPattern))
                {
                    throw new ArgumentException("LaunchArgumentsTemplate contains dangerous shell metacharacters.");
                }
            }
        }

        private List<EmulatorPackageDefinition> GetDefaultDefinitions()
        {
            return new List<EmulatorPackageDefinition>
            {
                new EmulatorPackageDefinition
                {
                    Id = "duckstation",
                    DisplayName = "DuckStation",
                    ConsoleName = "Sony PlayStation 1",
                    Description = "A Sony PlayStation 1 emulator focusing on playability, speed, and long-term maintainability.",
                    GitHubOwner = "stenzek",
                    GitHubRepository = "duckstation",
                    SupportedPlatforms = new List<string> { "Windows" },
                    IncludeAssetPatterns = new List<string> { "duckstation-windows-x64-release.zip", "duckstation" },
                    ExcludeAssetPatterns = new List<string>(),
                    SupportedArchiveTypes = new List<string> { "zip", "7z" },
                    ExecutableCandidates = new List<string> { "duckstation-qt-x64-ReleaseLTCG.exe", "duckstation-qt-x64-Release.exe", "duckstation-qt.exe", "duckstation.exe" },
                    InstallDirectoryName = "Emulators/DuckStation",
                    LaunchArgumentsTemplate = "-fullscreen -nogui \"{rom}\"",
                    RequiresBios = true,
                    BiosDirectoryCandidates = new List<string> { "bios" },
                    PreservedDirectories = new List<string> { "bios", "savestates", "memcards", "screenshots", "inis", "settings" },
                    PreservedFiles = new List<string> { "portable.txt" },

                    // UI Compatibility fields
                    SupportedRomExtensions = new List<string> { ".bin", ".cue", ".img", ".iso", ".chd", ".m3u", ".pbp" },
                    RequiresFirmware = false,
                    OfficialProjectUrl = "https://github.com/stenzek/duckstation",
                    OfficialDownloadUrl = "https://github.com/stenzek/duckstation/releases",
                    LicenseNoticeUrl = "https://github.com/stenzek/duckstation/blob/master/LICENSE",
                    SupportsPortableMode = true,
                    DefaultEnabled = true
                },
                new EmulatorPackageDefinition
                {
                    Id = "pcsx2",
                    DisplayName = "PCSX2",
                    ConsoleName = "Sony PlayStation 2",
                    Description = "A Sony PlayStation 2 emulator that aims to replicate the original experience.",
                    GitHubOwner = "PCSX2",
                    GitHubRepository = "pcsx2",
                    SupportedPlatforms = new List<string> { "Windows" },
                    IncludeAssetPatterns = new List<string> { "pcsx2-v2.*-windows-x64-Qt.7z", "pcsx2-*-windows-x64-Qt.7z", "pcsx2" },
                    ExcludeAssetPatterns = new List<string>(),
                    SupportedArchiveTypes = new List<string> { "7z", "zip" },
                    ExecutableCandidates = new List<string> { "pcsx2-qt.exe", "pcsx2.exe" },
                    InstallDirectoryName = "Emulators/PCSX2",
                    LaunchArgumentsTemplate = "-fullscreen \"{rom}\"",
                    RequiresBios = true,
                    BiosDirectoryCandidates = new List<string> { "bios" },
                    ReleaseChannel = EmulatorReleaseChannel.Dev,
                    PreservedDirectories = new List<string> { "bios", "memcards", "inis", "snaps", "savestates", "sstates", "games" },
                    PreservedFiles = new List<string> { "portable.txt" },

                    // UI Compatibility fields
                    SupportedRomExtensions = new List<string> { ".iso", ".bin", ".elf", ".chd", ".cso" },
                    RequiresFirmware = false,
                    OfficialProjectUrl = "https://pcsx2.net/",
                    OfficialDownloadUrl = "https://pcsx2.net/downloads/",
                    LicenseNoticeUrl = "https://github.com/PCSX2/pcsx2/blob/master/COPYING.GPLv3",
                    SupportsPortableMode = true,
                    DefaultEnabled = true
                },
                new EmulatorPackageDefinition
                {
                    Id = "rpcs3",
                    DisplayName = "RPCS3",
                    ConsoleName = "Sony PlayStation 3",
                    Description = "An open-source Sony PlayStation 3 emulator and debugger written in C++.",
                    GitHubOwner = "RPCS3",
                    GitHubRepository = "rpcs3",
                    SupportedPlatforms = new List<string> { "Windows" },
                    IncludeAssetPatterns = new List<string> { "rpcs3-v*_win64.7z", "rpcs3-*_win64.7z", "win64" },
                    ExcludeAssetPatterns = new List<string>(),
                    SupportedArchiveTypes = new List<string> { "7z", "zip" },
                    ExecutableCandidates = new List<string> { "rpcs3.exe" },
                    InstallDirectoryName = "Emulators/RPCS3",
                    LaunchArgumentsTemplate = "--fullscreen \"{rom}\"",
                    RequiresBios = false,
                    BiosDirectoryCandidates = new List<string>(),
                    PreservedDirectories = new List<string> { "dev_hdd0", "dev_flash", "GuiConfigs" },
                    PreservedFiles = new List<string> { "config.yml", "games.yml" },

                    // UI Compatibility fields
                    SupportedRomExtensions = new List<string> { ".bin", ".pkg", ".iso" },
                    RequiresFirmware = true,
                    OfficialProjectUrl = "https://rpcs3.net/",
                    OfficialDownloadUrl = "https://rpcs3.net/download",
                    LicenseNoticeUrl = "https://github.com/RPCS3/rpcs3/blob/master/LICENSE",
                    SupportsPortableMode = false,
                    DefaultEnabled = true
                }
            };
        }

        private void MigrateOldDirectory(EmulatorPackageDefinition def)
        {
            bool migrated = false;
            if (string.Equals(def.InstallDirectoryName, "Emulators/PS1", StringComparison.OrdinalIgnoreCase))
            {
                def.InstallDirectoryName = "Emulators/DuckStation";
                migrated = true;
            }
            else if (string.Equals(def.InstallDirectoryName, "Emulators/PS2", StringComparison.OrdinalIgnoreCase))
            {
                def.InstallDirectoryName = "Emulators/PCSX2";
                migrated = true;
            }
            else if (string.Equals(def.InstallDirectoryName, "Emulators/PS3", StringComparison.OrdinalIgnoreCase))
            {
                def.InstallDirectoryName = "Emulators/RPCS3";
                migrated = true;
            }

            if (def.PreservedDirectories == null || def.PreservedDirectories.Count == 0)
            {
                def.PreservedDirectories = GetDefaultPreservedDirectories(def.Id);
                migrated = true;
            }
            if (def.PreservedFiles == null || def.PreservedFiles.Count == 0)
            {
                def.PreservedFiles = GetDefaultPreservedFiles(def.Id);
                migrated = true;
            }

            if (migrated)
            {
                _needsSave = true;
            }
        }

        private List<string> GetDefaultPreservedDirectories(string id)
        {
            id = id.ToLowerInvariant();
            if (id == "pcsx2")
            {
                return new List<string> { "bios", "memcards", "inis", "snaps", "savestates", "sstates", "games" };
            }
            if (id == "duckstation")
            {
                return new List<string> { "bios", "savestates", "memcards", "screenshots", "inis", "settings" };
            }
            if (id == "rpcs3")
            {
                return new List<string> { "dev_hdd0", "dev_flash", "GuiConfigs" };
            }
            return new List<string> { "bios", "saves", "configs", "screenshots", "games", "roms" };
        }

        private List<string> GetDefaultPreservedFiles(string id)
        {
            id = id.ToLowerInvariant();
            if (id == "pcsx2" || id == "duckstation")
            {
                return new List<string> { "portable.txt" };
            }
            if (id == "rpcs3")
            {
                return new List<string> { "config.yml", "games.yml" };
            }
            return new List<string>();
        }
    }
}
