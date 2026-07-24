using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RetroLauncher
{
    public interface IEmulatorDefinitionProvider
    {
        IReadOnlyList<EmulatorDefinition> GetAll();
        EmulatorDefinition? GetById(string id);
        IReadOnlyList<EmulatorDefinition> GetByConsole(string consoleName);
        void Validate(EmulatorDefinition definition);
    }

    public class JsonEmulatorDefinitionProvider : IEmulatorDefinitionProvider
    {
        private readonly string _filePath;
        private readonly List<EmulatorDefinition> _definitions = new();

        public JsonEmulatorDefinitionProvider(string? filePath = null)
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

                        var list = JsonSerializer.Deserialize<List<EmulatorDefinition>>(json, options);
                        if (list != null)
                        {
                            var verifiedList = new List<EmulatorDefinition>();
                            foreach (var def in list)
                            {
                                try
                                {
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
                            _definitions.Clear();
                            _definitions.AddRange(verifiedList);
                            loadedFromFile = true;
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
            }
            catch (Exception ex)
            {
                RetroLogger.Log($"Error initializing emulator definition provider: {ex.Message}", "ERROR");
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

        public IReadOnlyList<EmulatorDefinition> GetAll() => _definitions.AsReadOnly();

        public EmulatorDefinition? GetById(string id)
        {
            return _definitions.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<EmulatorDefinition> GetByConsole(string consoleName)
        {
            return _definitions.Where(d => string.Equals(d.ConsoleName, consoleName, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }

        public void Validate(EmulatorDefinition definition)
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
            if (definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubLatestRelease ||
                definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubReleaseList ||
                definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubRollingTag ||
                definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubBinaryRepository)
            {
                if (string.IsNullOrWhiteSpace(definition.RepositoryOwner))
                {
                    throw new ArgumentException($"RepositoryOwner is required for source type '{definition.ReleaseSourceType}'.");
                }
                if (string.IsNullOrWhiteSpace(definition.RepositoryName))
                {
                    throw new ArgumentException($"RepositoryName is required for source type '{definition.ReleaseSourceType}'.");
                }
                if (!Regex.IsMatch(definition.RepositoryOwner, "^[a-zA-Z0-9-._]+$"))
                {
                    throw new ArgumentException($"Invalid RepositoryOwner '{definition.RepositoryOwner}'.");
                }
                if (!Regex.IsMatch(definition.RepositoryName, "^[a-zA-Z0-9-._]+$"))
                {
                    throw new ArgumentException($"Invalid RepositoryName '{definition.RepositoryName}'.");
                }
            }

            // 4. Validate Installation Directory (Zip Slip / Absolute check)
            if (string.IsNullOrWhiteSpace(definition.InstallationDirectoryName))
            {
                throw new ArgumentException("InstallationDirectoryName cannot be empty.");
            }
            if (definition.InstallationDirectoryName.Contains("..") || 
                Path.IsPathRooted(definition.InstallationDirectoryName) ||
                definition.InstallationDirectoryName.Contains(":") ||
                definition.InstallationDirectoryName.StartsWith("/") ||
                definition.InstallationDirectoryName.StartsWith("\\"))
            {
                throw new ArgumentException($"InstallationDirectoryName '{definition.InstallationDirectoryName}' is unsafe or absolute.");
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
            if (!string.IsNullOrEmpty(definition.LaunchArgumentTemplate))
            {
                string dangerousPattern = @"[|&;$><`]";
                if (Regex.IsMatch(definition.LaunchArgumentTemplate, dangerousPattern))
                {
                    throw new ArgumentException("LaunchArgumentTemplate contains dangerous shell metacharacters.");
                }
            }
        }

        private List<EmulatorDefinition> GetDefaultDefinitions()
        {
            return new List<EmulatorDefinition>
            {
                new EmulatorDefinition
                {
                    Id = "duckstation",
                    DisplayName = "DuckStation",
                    ConsoleName = "Sony PlayStation 1",
                    Description = "A Sony PlayStation 1 emulator focusing on playability, speed, and long-term maintainability.",
                    RepositoryOwner = "stenzek",
                    RepositoryName = "duckstation",
                    ReleaseSourceType = EmulatorReleaseSourceType.GitHubLatestRelease,
                    ReleaseChannel = EmulatorReleaseChannel.Stable,
                    SupportedOperatingSystems = new List<SupportedOperatingSystem> { SupportedOperatingSystem.Windows },
                    SupportedArchitectures = new List<CpuArchitecture> { CpuArchitecture.X64 },
                    SupportedRomExtensions = new List<string> { ".bin", ".cue", ".img", ".iso", ".chd", ".m3u", ".pbp" },
                    InstallationDirectoryName = "Emulators/PS1",
                    ExecutableCandidates = new List<string> { "duckstation-qt-x64-ReleaseLTCG.exe", "duckstation-nogui-x64-ReleaseLTCG.exe", "duckstation.exe" },
                    AssetSelectionRules = new List<string> { "duckstation-windows-x64-release.zip" },
                    ArchiveType = EmulatorArchiveType.Zip,
                    RequiresBios = true,
                    RequiresFirmware = false,
                    OfficialProjectUrl = "https://github.com/stenzek/duckstation",
                    OfficialDownloadUrl = "https://github.com/stenzek/duckstation/releases",
                    LicenseNoticeUrl = "https://github.com/stenzek/duckstation/blob/master/LICENSE",
                    LaunchArgumentTemplate = "-fullscreen \"{rom}\"",
                    SupportsPortableMode = true,
                    DefaultEnabled = true
                },
                new EmulatorDefinition
                {
                    Id = "pcsx2",
                    DisplayName = "PCSX2",
                    ConsoleName = "Sony PlayStation 2",
                    Description = "A Sony PlayStation 2 emulator that aims to replicate the original experience.",
                    RepositoryOwner = "PCSX2",
                    RepositoryName = "pcsx2",
                    ReleaseSourceType = EmulatorReleaseSourceType.GitHubLatestRelease,
                    ReleaseChannel = EmulatorReleaseChannel.Nightly,
                    SupportedOperatingSystems = new List<SupportedOperatingSystem> { SupportedOperatingSystem.Windows },
                    SupportedArchitectures = new List<CpuArchitecture> { CpuArchitecture.X64 },
                    SupportedRomExtensions = new List<string> { ".iso", ".bin", ".elf", ".chd", ".cso" },
                    InstallationDirectoryName = "Emulators/PS2",
                    ExecutableCandidates = new List<string> { "pcsx2-qt.exe", "pcsx2.exe" },
                    AssetSelectionRules = new List<string> { "pcsx2-v1.7.*-windows-x64-Qt.7z" },
                    ArchiveType = EmulatorArchiveType.SevenZip,
                    RequiresBios = true,
                    RequiresFirmware = false,
                    OfficialProjectUrl = "https://pcsx2.net/",
                    OfficialDownloadUrl = "https://pcsx2.net/downloads/",
                    LicenseNoticeUrl = "https://github.com/PCSX2/pcsx2/blob/master/COPYING.GPLv3",
                    LaunchArgumentTemplate = "-fullscreen \"{rom}\"",
                    SupportsPortableMode = true,
                    DefaultEnabled = true
                },
                new EmulatorDefinition
                {
                    Id = "rpcs3",
                    DisplayName = "RPCS3",
                    ConsoleName = "Sony PlayStation 3",
                    Description = "An open-source Sony PlayStation 3 emulator and debugger written in C++.",
                    RepositoryOwner = "RPCS3",
                    RepositoryName = "rpcs3-binaries-win",
                    ReleaseSourceType = EmulatorReleaseSourceType.GitHubBinaryRepository,
                    ReleaseChannel = EmulatorReleaseChannel.Nightly,
                    SupportedOperatingSystems = new List<SupportedOperatingSystem> { SupportedOperatingSystem.Windows },
                    SupportedArchitectures = new List<CpuArchitecture> { CpuArchitecture.X64 },
                    SupportedRomExtensions = new List<string> { ".bin", ".pkg", ".iso" },
                    InstallationDirectoryName = "Emulators/PS3",
                    ExecutableCandidates = new List<string> { "rpcs3.exe" },
                    AssetSelectionRules = new List<string> { "rpcs3-v0.0.*_win64.7z" },
                    ArchiveType = EmulatorArchiveType.SevenZip,
                    RequiresBios = false,
                    RequiresFirmware = true,
                    OfficialProjectUrl = "https://rpcs3.net/",
                    OfficialDownloadUrl = "https://rpcs3.net/download",
                    LicenseNoticeUrl = "https://github.com/RPCS3/rpcs3/blob/master/LICENSE",
                    LaunchArgumentTemplate = "--fullscreen \"{rom}\"",
                    SupportsPortableMode = true,
                    DefaultEnabled = true
                }
            };
        }
    }
}
