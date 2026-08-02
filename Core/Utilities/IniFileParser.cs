using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetroLauncher.Core.Utilities
{
    public static class IniFileParser
    {
        public static Dictionary<string, Dictionary<string, string>> ParseFile(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(filePath)) return result;

            string currentSection = "General";
            result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string[] lines = File.ReadAllLines(filePath);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    if (!result.ContainsKey(currentSection))
                    {
                        result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    continue;
                }

                int idx = line.IndexOf('=');
                if (idx > 0)
                {
                    string key = line.Substring(0, idx).Trim();
                    string value = line.Substring(idx + 1).Trim();
                    result[currentSection][key] = value;
                }
            }

            return result;
        }

        public static void WriteFile(string filePath, Dictionary<string, Dictionary<string, string>> iniData)
        {
            // Read existing lines if present to preserve exact structure, comments, and unrelated sections
            var sb = new StringBuilder();
            var handledSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var handledKeys = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                string currentSection = "General";

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        handledSections.Add(currentSection);
                        if (!handledKeys.ContainsKey(currentSection))
                        {
                            handledKeys[currentSection] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        sb.AppendLine(line);
                        continue;
                    }

                    int idx = trimmed.IndexOf('=');
                    if (idx > 0)
                    {
                        string key = trimmed.Substring(0, idx).Trim();
                        if (iniData.ContainsKey(currentSection) && iniData[currentSection].ContainsKey(key))
                        {
                            sb.AppendLine($"{key} = {iniData[currentSection][key]}");
                            handledKeys[currentSection].Add(key);
                            continue;
                        }
                    }

                    sb.AppendLine(line);
                }
            }

            // Append any new sections or keys not present in existing file
            foreach (var secPair in iniData)
            {
                string section = secPair.Key;
                if (!handledSections.Contains(section))
                {
                    sb.AppendLine();
                    sb.AppendLine($"[{section}]");
                }

                if (!handledKeys.ContainsKey(section))
                {
                    handledKeys[section] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                foreach (var keyPair in secPair.Value)
                {
                    if (!handledKeys[section].Contains(keyPair.Key))
                    {
                        sb.AppendLine($"{keyPair.Key} = {keyPair.Value}");
                        handledKeys[section].Add(keyPair.Key);
                    }
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
