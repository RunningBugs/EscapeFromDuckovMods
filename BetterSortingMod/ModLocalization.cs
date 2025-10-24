using System;
using System.Collections.Generic;
using System.IO;
using SodaCraft.Localizations;
using UnityEngine;

namespace BetterSortingMod;

internal static class ModLocalization
{
    private const string DataFileName = "Lang.ini";

    private static readonly Dictionary<string, Dictionary<SystemLanguage, string>> entries = new();

    private static string loadedFrom;

    private static bool loaded;

    internal static void Initialize(string modFolder)
    {
        if (string.IsNullOrEmpty(modFolder))
        {
            return;
        }
        if (loaded && string.Equals(loadedFrom, modFolder, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        loaded = true;
        loadedFrom = modFolder;
        entries.Clear();
        string path = Path.Combine(modFolder, DataFileName);
        if (!File.Exists(path))
        {
            return;
        }
        try
        {
            ParseIni(path);
        }
        catch (Exception ex)
        {
            Debug.LogError($"BetterSortingMod: Failed to read {DataFileName}: {ex}");
        }
    }

    internal static string GetText(string key, SystemLanguage language, IReadOnlyDictionary<SystemLanguage, string> fallback)
    {
        if (!string.IsNullOrEmpty(key) && entries.TryGetValue(key, out var overrides) && TryResolve(overrides, language, out string text))
        {

            return text;
        }
        if (TryResolve(fallback, language, out string fallbackText))
        {

            return fallbackText;
        }
        if (!string.IsNullOrEmpty(key) && entries.TryGetValue(key, out overrides) && TryResolve(overrides, SystemLanguage.English, out text))
        {

            return text;
        }
        if (TryResolve(fallback, SystemLanguage.English, out fallbackText))
        {

            return fallbackText;
        }

        return string.Empty;
    }

    private static void ParseIni(string path)
    {
        string currentKey = null;
        Dictionary<SystemLanguage, string> currentMap = null;
        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith(";"))
            {
                continue;
            }
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentKey = line.Substring(1, line.Length - 2).Trim();
                if (string.IsNullOrEmpty(currentKey))
                {
                    currentMap = null;
                    continue;
                }
                currentMap = new Dictionary<SystemLanguage, string>();
                entries[currentKey] = currentMap;
                continue;
            }
            int equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || currentMap == null)
            {
                continue;
            }
            string langToken = line.Substring(0, equalsIndex).Trim();
            string value = line.Substring(equalsIndex + 1).Trim();
            if (Enum.TryParse(langToken, ignoreCase: true, out SystemLanguage language))
            {
                currentMap[language] = value.Replace(@"\n", "\n");
            }
            else
            {
                Debug.LogWarning($"BetterSortingMod: Unknown language token '{langToken}' in {DataFileName}.");
            }
        }
    }

    private static bool TryResolve(IReadOnlyDictionary<SystemLanguage, string> map, SystemLanguage language, out string text)
    {
        text = null;
        if (map == null)
        {
            return false;
        }
        if (map.TryGetValue(language, out text) && !string.IsNullOrEmpty(text))
        {
            return true;
        }
        if (language != SystemLanguage.English && map.TryGetValue(SystemLanguage.English, out text) && !string.IsNullOrEmpty(text))
        {
            return true;
        }
        return false;
    }
}
