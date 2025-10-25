using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BossLiveMapMod
{
    internal static class ModLocalization
    {
        private const string DataFileName = "Lang.ini";

        private static readonly Dictionary<string, Dictionary<SystemLanguage, string>> _entries = new Dictionary<string, Dictionary<SystemLanguage, string>>();

        private static string _loadedFrom;
        private static bool _loaded;

        internal static void Initialize(string modFolder)
        {
            if (string.IsNullOrEmpty(modFolder))
                return;

            if (_loaded && string.Equals(_loadedFrom, modFolder, StringComparison.OrdinalIgnoreCase))
                return;

            _loaded = true;
            _loadedFrom = modFolder;
            _entries.Clear();

            string path = Path.Combine(modFolder, DataFileName);
            if (!File.Exists(path))
            {
                Debug.Log($"[BossLiveMapMod] {DataFileName} not found at {path}, using default labels");
                return;
            }

            try
            {
                ParseIni(path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BossLiveMapMod] Failed to read {DataFileName}: {ex}");
            }
        }

        internal static string GetText(string key, SystemLanguage language, string fallback)
        {
            // Try to get from loaded entries
            if (!string.IsNullOrEmpty(key) && _entries.TryGetValue(key, out var overrides))
            {
                if (TryResolve(overrides, language, out string text))
                    return text;

                // Fallback to English if current language not found
                if (language != SystemLanguage.English && TryResolve(overrides, SystemLanguage.English, out text))
                    return text;
            }

            // Use provided fallback
            return fallback ?? key;
        }

        private static void ParseIni(string path)
        {
            string currentKey = null;
            Dictionary<SystemLanguage, string> currentMap = null;

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith(";"))
                    continue;

                // Section header [key]
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentKey = line.Substring(1, line.Length - 2).Trim();
                    if (string.IsNullOrEmpty(currentKey))
                    {
                        currentMap = null;
                        continue;
                    }

                    currentMap = new Dictionary<SystemLanguage, string>();
                    _entries[currentKey] = currentMap;
                    continue;
                }

                // Key=Value pair
                int equalsIndex = line.IndexOf('=');
                if (equalsIndex <= 0 || currentMap == null)
                    continue;

                string langToken = line.Substring(0, equalsIndex).Trim();
                string value = line.Substring(equalsIndex + 1).Trim();

                if (Enum.TryParse(langToken, ignoreCase: true, out SystemLanguage language))
                {
                    currentMap[language] = value.Replace(@"\n", "\n");
                }
                else
                {
                    Debug.LogWarning($"[BossLiveMapMod] Unknown language token '{langToken}' in {DataFileName}");
                }
            }
        }

        private static bool TryResolve(Dictionary<SystemLanguage, string> map, SystemLanguage language, out string text)
        {
            text = null;
            if (map == null)
                return false;

            if (map.TryGetValue(language, out text) && !string.IsNullOrEmpty(text))
                return true;

            return false;
        }
    }
}
