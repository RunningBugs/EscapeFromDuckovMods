using System;
using System.IO;
using System.Threading;
using System.Globalization;
using UnityEngine;

namespace BossLiveMapMod
{
    internal static class ModConfig
    {
        private const string ConfigFileName = "config.ini";
        private static string _configPath;

        // Current (applied) settings
        internal static bool ShowAllEnemies { get; private set; } = true;
        internal static bool ShowLivePositions { get; private set; } = true;
        internal static bool ShowMarkers { get; private set; } = true;
        internal static bool ShowBossList { get; private set; } = true;
        internal static bool ShowNearbyEnemies { get; private set; } = false;
        internal static bool ShowNearbyOnly { get; private set; } = false;
        internal static bool ShowMarkerNames { get; private set; } = true;
        internal static float Transparency { get; private set; } = 1f; // 0..1
        internal static float UiScale { get; private set; } = 1f;      // 0.5..2.0
        internal static bool UiScaleAuto { get; private set; } = true;
        internal static float BossFontScale { get; private set; } = 1f; // 0.1..1.5

        // Pending values read from config file watcher (applied on next Update cycle)
        internal static bool PendingShowNearbyEnemies { get; private set; }
        internal static bool PendingShowAllEnemies { get; private set; }
        internal static bool PendingShowLivePositions { get; private set; }
        internal static bool PendingShowMarkers { get; private set; }
        internal static bool PendingShowBossList { get; private set; }
        internal static bool PendingShowNearbyOnly { get; private set; }
        internal static bool PendingShowMarkerNames { get; private set; }
        internal static float PendingTransparency { get; private set; }
        internal static float PendingUiScale { get; private set; }
        internal static bool PendingUiScaleAuto { get; private set; }
        internal static float PendingBossFontScale { get; private set; }

        internal static bool HasPendingUpdate { get; private set; }

        private static FileSystemWatcher _watcher;
        private static readonly object SyncRoot = new object();
        private static bool _suppressWatcher;
        private static DateTime _lastWriteTimeUtc = DateTime.MinValue;

        internal static void Load()
        {
            try
            {
                EnsurePath();

                // Initialize pending with current defaults
                PendingShowNearbyEnemies = ShowNearbyEnemies;
                PendingShowAllEnemies = ShowAllEnemies;
                PendingShowLivePositions = ShowLivePositions;
                PendingShowMarkers = ShowMarkers;
                PendingShowBossList = ShowBossList;
                PendingShowNearbyOnly = ShowNearbyOnly;
                PendingShowMarkerNames = ShowMarkerNames;
                PendingTransparency = Transparency;
                PendingUiScale = UiScale;
                PendingUiScaleAuto = UiScaleAuto;
                PendingBossFontScale = BossFontScale;

                if (!File.Exists(_configPath))
                {
                    // Write defaults
                    Save();
                }
                else
                {
                    if (TryReadConfig(out var parsedNearby, out var parsedAll, out var parsedLive, out var parsedMarkers, out var parsedBossList, out var parsedNearbyOnly, out var parsedMarkerNames, out var parsedTransparency, out var parsedUiScale, out var parsedUiScaleAuto, out var parsedBossFontScale))
                    {
                        PendingShowNearbyEnemies = parsedNearby;
                        PendingShowAllEnemies = parsedAll;
                        PendingShowLivePositions = parsedLive;
                        PendingShowMarkers = parsedMarkers;
                        PendingShowBossList = parsedBossList;
                        PendingShowNearbyOnly = parsedNearbyOnly;
                        PendingShowMarkerNames = parsedMarkerNames;
                        PendingTransparency = parsedTransparency;
                        PendingUiScale = parsedUiScale;
                        PendingUiScaleAuto = parsedUiScaleAuto;
                        PendingBossFontScale = parsedBossFontScale;
                    }
                }

                // Reflect pending to current for initial load (Defer the "apply" semantics to the mod update loop if desired).
                ShowNearbyEnemies = PendingShowNearbyEnemies;
                ShowAllEnemies = PendingShowAllEnemies;
                ShowLivePositions = PendingShowLivePositions;
                ShowMarkers = PendingShowMarkers;
                ShowBossList = PendingShowBossList;
                ShowNearbyOnly = PendingShowNearbyOnly;
                ShowMarkerNames = PendingShowMarkerNames;
                Transparency = PendingTransparency;
                UiScale = PendingUiScale;
                UiScaleAuto = PendingUiScaleAuto;
                BossFontScale = PendingBossFontScale;
                if (UiScaleAuto)
                {
                    var autoScale = ComputeAutoUiScale();
                    UiScale = autoScale;
                    PendingUiScale = autoScale;
                }

                _lastWriteTimeUtc = File.Exists(_configPath) ? File.GetLastWriteTimeUtc(_configPath) : DateTime.MinValue;

                HasPendingUpdate = false;
                EnsureWatcher();
            }
            catch
            {
                // ignore and keep defaults
            }
        }

        // Public setters used by runtime UI. They immediately persist the change.
        internal static void SetShowNearbyEnemies(bool value)
        {
            if (ShowNearbyEnemies == value)
            {
                return;
            }
            ShowNearbyEnemies = value;
            PendingShowNearbyEnemies = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowAllEnemies(bool value)
        {
            if (ShowAllEnemies == value)
                return;
            ShowAllEnemies = value;
            PendingShowAllEnemies = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowLivePositions(bool value)
        {
            if (ShowLivePositions == value)
                return;
            ShowLivePositions = value;
            PendingShowLivePositions = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowMarkers(bool value)
        {
            if (ShowMarkers == value)
                return;
            ShowMarkers = value;
            PendingShowMarkers = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowBossList(bool value)
        {
            if (ShowBossList == value)
                return;
            ShowBossList = value;
            PendingShowBossList = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowNearbyOnly(bool value)
        {
            if (ShowNearbyOnly == value)
                return;
            ShowNearbyOnly = value;
            PendingShowNearbyOnly = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetShowMarkerNames(bool value)
        {
            if (ShowMarkerNames == value)
                return;
            ShowMarkerNames = value;
            PendingShowMarkerNames = value;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetTransparency(float value)
        {
            // clamp 0..1
            var clamped = Math.Max(0f, Math.Min(1f, value));
            if (Math.Abs(Transparency - clamped) < 0.0001f)
                return;
            Transparency = clamped;
            PendingTransparency = clamped;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetUiScale(float value)
        {
            // clamp 0.5..2.0
            var clamped = Math.Max(0.5f, Math.Min(2.0f, value));
            if (Math.Abs(UiScale - clamped) < 0.0001f)
                return;
            UiScale = clamped;
            PendingUiScale = clamped;
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetUiScaleAuto(bool value)
        {
            if (UiScaleAuto == value)
                return;
            UiScaleAuto = value;
            PendingUiScaleAuto = value;
            if (value)
            {
                var autoScale = ComputeAutoUiScale();
                UiScale = autoScale;
                PendingUiScale = autoScale;
            }
            HasPendingUpdate = true;
            Save();
        }

        internal static void SetBossFontScale(float value)
        {
            // clamp 0.1..1.5
            var clamped = Math.Max(0.1f, Math.Min(1.5f, value));
            if (Math.Abs(BossFontScale - clamped) < 0.0001f)
                return;
            BossFontScale = clamped;
            PendingBossFontScale = clamped;
            HasPendingUpdate = true;
            Save();
        }

        private static void Save()
        {
            try
            {
                EnsurePath();
                lock (SyncRoot)
                {
                    _suppressWatcher = true;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? string.Empty);

                // Use invariant culture for float formatting
                File.WriteAllLines(_configPath, new[]
                {
                    "# BossLiveMapMod configuration",
                    $"ShowNearbyEnemies={ShowNearbyEnemies}",
                    $"ShowAllEnemies={ShowAllEnemies}",
                    $"ShowLivePositions={ShowLivePositions}",
                    $"ShowMarkers={ShowMarkers}",
                    $"ShowBossList={ShowBossList}",
                    $"ShowNearbyOnly={ShowNearbyOnly}",
                    $"ShowMarkerNames={ShowMarkerNames}",
                    "Transparency=" + Transparency.ToString("0.00", CultureInfo.InvariantCulture),
                    "UiScale=" + UiScale.ToString("0.00", CultureInfo.InvariantCulture),
                    $"UiScaleAuto={UiScaleAuto}",
                    "BossFontScale=" + BossFontScale.ToString("0.00", CultureInfo.InvariantCulture),
                });

                _lastWriteTimeUtc = File.Exists(_configPath) ? File.GetLastWriteTimeUtc(_configPath) : DateTime.UtcNow;
            }
            catch
            {
                // ignore save failures
            }
            finally
            {
                lock (SyncRoot)
                {
                    _suppressWatcher = false;
                }
            }
        }

        private static void EnsurePath()
        {
            if (!string.IsNullOrEmpty(_configPath))
                return;

            var assemblyLocation = typeof(ModConfig).Assembly.Location;
            var directory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(directory))
            {
                directory = AppContext.BaseDirectory ?? ".";
            }
            _configPath = Path.Combine(directory, ConfigFileName);
        }

        // Parse the config file and return parsed values for all keys.
        private static bool TryReadConfig(out bool showNearbyEnemies, out bool showAllEnemies, out bool showLivePositions, out bool showMarkers, out bool showBossList, out bool showNearbyOnly, out bool showMarkerNames, out float transparency, out float uiScale, out bool uiScaleAuto, out float bossFontScale)
        {
            showNearbyEnemies = ShowNearbyEnemies;
            showAllEnemies = ShowAllEnemies;
            showLivePositions = ShowLivePositions;
            showMarkers = ShowMarkers;
            showBossList = ShowBossList;
            showNearbyOnly = ShowNearbyOnly;
            showMarkerNames = ShowMarkerNames;
            transparency = Transparency;
            uiScale = UiScale;
            uiScaleAuto = UiScaleAuto;
            bossFontScale = BossFontScale;

            EnsurePath();
            if (!File.Exists(_configPath))
                return false;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    foreach (var raw in File.ReadAllLines(_configPath))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;

                        var parts = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2)
                            continue;

                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        if (string.Equals(key, "ShowNearbyEnemies", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedNearby))
                        {
                            showNearbyEnemies = parsedNearby;
                        }
                        else if (string.Equals(key, "ShowAllEnemies", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedAll))
                        {
                            showAllEnemies = parsedAll;
                        }
                        else if (string.Equals(key, "ShowLivePositions", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedLive))
                        {
                            showLivePositions = parsedLive;
                        }
                        else if (string.Equals(key, "ShowMarkers", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedMarkers))
                        {
                            showMarkers = parsedMarkers;
                        }
                        else if (string.Equals(key, "ShowBossList", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedBossList))
                        {
                            showBossList = parsedBossList;
                        }
                        else if (string.Equals(key, "ShowNearbyOnly", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedNearbyOnly))
                        {
                            showNearbyOnly = parsedNearbyOnly;
                        }
                        else if (string.Equals(key, "Transparency", StringComparison.OrdinalIgnoreCase)
                            && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedTransparency))
                        {
                            // clamp
                            transparency = Math.Max(0f, Math.Min(1f, parsedTransparency));
                        }
                        else if (string.Equals(key, "UiScale", StringComparison.OrdinalIgnoreCase)
                            && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedUiScale))
                        {
                            uiScale = Math.Max(0.5f, Math.Min(2.0f, parsedUiScale));
                        }
                        else if (string.Equals(key, "UiScaleAuto", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedUiScaleAuto))
                        {
                            uiScaleAuto = parsedUiScaleAuto;
                        }
                        else if (string.Equals(key, "ShowMarkerNames", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsedShowMarkerNames))
                        {
                            showMarkerNames = parsedShowMarkerNames;
                        }
                        else if (string.Equals(key, "BossFontScale", StringComparison.OrdinalIgnoreCase)
                            && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedBossFontScale))
                        {
                            bossFontScale = Math.Max(0.1f, Math.Min(1.5f, parsedBossFontScale));
                        }
                    }
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static float ComputeAutoUiScale()
        {
            try
            {
                // Prefer DPI when available; fallback to resolution relative to 1080p/1920p
                float dpi = Screen.dpi;
                if (dpi > 0f)
                {
                    // Normalize to ~96 DPI baseline
                    return Mathf.Clamp(dpi / 96f, 0.75f, 2.0f);
                }
                float sx = Screen.width > 0 ? (Screen.width / 1920f) : 1f;
                float sy = Screen.height > 0 ? (Screen.height / 1080f) : 1f;
                return Mathf.Clamp(Mathf.Min(sx, sy), 0.75f, 2.0f);
            }
            catch
            {
                return 1f;
            }
        }

        private static void EnsureWatcher()
        {
            if (_watcher != null)
                return;

            EnsurePath();
            var directory = Path.GetDirectoryName(_configPath);
            if (string.IsNullOrEmpty(directory))
                directory = ".";

            var fileName = Path.GetFileName(_configPath);
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnConfigFileChanged;
            _watcher.Created += OnConfigFileChanged;
            _watcher.Renamed += OnConfigFileChanged;
        }

        private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            lock (SyncRoot)
            {
                if (_suppressWatcher)
                    return;
            }

            try
            {
                EnsurePath();
                if (!File.Exists(_configPath))
                    return;

                var writeTime = File.GetLastWriteTimeUtc(_configPath);
                if (writeTime <= _lastWriteTimeUtc)
                    return;

                _lastWriteTimeUtc = writeTime;

                if (TryReadConfig(out var parsedNearby, out var parsedAll, out var parsedLive, out var parsedMarkers, out var parsedBossList, out var parsedNearbyOnly, out var parsedMarkerNames, out var parsedTransparency, out var parsedUiScale, out var parsedUiScaleAuto, out var parsedBossFontScale))
                {
                    PendingShowNearbyEnemies = parsedNearby;
                    PendingShowAllEnemies = parsedAll;
                    PendingShowLivePositions = parsedLive;
                    PendingShowMarkers = parsedMarkers;
                    PendingShowBossList = parsedBossList;
                    PendingShowNearbyOnly = parsedNearbyOnly;
                    PendingShowMarkerNames = parsedMarkerNames;
                    PendingTransparency = parsedTransparency;
                    PendingBossFontScale = parsedBossFontScale;
                    PendingUiScale = parsedUiScale;
                    PendingUiScaleAuto = parsedUiScaleAuto;
                    HasPendingUpdate = true;
                }
            }
            catch
            {
                // ignore watcher errors
            }
        }

        // Called by the mod update loop to apply file-based changes atomically.
        internal static void ApplyPendingChanges()
        {
            if (!HasPendingUpdate)
                return;

            ShowNearbyEnemies = PendingShowNearbyEnemies;
            ShowAllEnemies = PendingShowAllEnemies;
            ShowLivePositions = PendingShowLivePositions;
            ShowMarkers = PendingShowMarkers;
            ShowBossList = PendingShowBossList;
            ShowNearbyOnly = PendingShowNearbyOnly;
            ShowMarkerNames = PendingShowMarkerNames;
            Transparency = PendingTransparency;
            BossFontScale = PendingBossFontScale;
            UiScaleAuto = PendingUiScaleAuto;
            if (UiScaleAuto)
            {
                UiScale = ComputeAutoUiScale();
                PendingUiScale = UiScale;
            }
            else
            {
                UiScale = PendingUiScale;
            }

            HasPendingUpdate = false;
        }
    }
}
