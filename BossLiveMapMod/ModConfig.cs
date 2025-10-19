using System;
using System.IO;
using System.Threading;

namespace BossLiveMapMod
{
    internal static class ModConfig
    {
        private const string ConfigFileName = "config.ini";
        private static string _configPath;

        internal static bool ShowNearbyEnemies { get; private set; } = false;
        internal static bool HasPendingUpdate { get; private set; }
        internal static bool PendingShowNearbyEnemies { get; private set; }

        private static FileSystemWatcher _watcher;
        private static readonly object SyncRoot = new object();
        private static bool _suppressWatcher;
        private static DateTime _lastWriteTimeUtc = DateTime.MinValue;

        internal static void Load()
        {
            try
            {
                EnsurePath();
                bool newValue = ShowNearbyEnemies;

                if (!File.Exists(_configPath))
                {
                    Save();
                    newValue = ShowNearbyEnemies;
                }
                else if (TryReadConfig(out var parsedValue))
                {
                    newValue = parsedValue;
                }

                ShowNearbyEnemies = newValue;
                _lastWriteTimeUtc = File.Exists(_configPath) ? File.GetLastWriteTimeUtc(_configPath) : DateTime.MinValue;
                ShowNearbyEnemies = newValue;
                PendingShowNearbyEnemies = newValue;
                HasPendingUpdate = true;
                EnsureWatcher();

                PendingShowNearbyEnemies = newValue;
                HasPendingUpdate = true;
            }
            catch
            {
                // ignore and keep defaults
            }
        }

        internal static void SetShowNearbyEnemies(bool value)
        {
            if (ShowNearbyEnemies == value)
            {
                return;
            }
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
                File.WriteAllLines(_configPath, new[]
                {
                    "# BossLiveMapMod configuration",
                    $"ShowNearbyEnemies={ShowNearbyEnemies}"
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
            {
                return;
            }
            var assemblyLocation = typeof(ModBehaviour).Assembly.Location;
            var directory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(directory))
            {
                directory = AppContext.BaseDirectory ?? ".";
            }
            _configPath = Path.Combine(directory, ConfigFileName);
        }

        private static bool TryReadConfig(out bool showNearbyEnemies)
        {
            showNearbyEnemies = ShowNearbyEnemies;
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
                        {
                            continue;
                        }
                        var parts = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2)
                        {
                            continue;
                        }
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        if (string.Equals(key, "ShowNearbyEnemies", StringComparison.OrdinalIgnoreCase)
                            && bool.TryParse(value, out var parsed))
                        {
                            showNearbyEnemies = parsed;
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

        private static void EnsureWatcher()
        {
            if (_watcher != null)
            {
                return;
            }
            EnsurePath();
            var directory = Path.GetDirectoryName(_configPath);
            if (string.IsNullOrEmpty(directory))
            {
                directory = ".";
            }
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
                {
                    return;
                }
            }

            try
            {
                EnsurePath();
                if (!File.Exists(_configPath))
                {
                    return;
                }

                var writeTime = File.GetLastWriteTimeUtc(_configPath);
                if (writeTime <= _lastWriteTimeUtc)
                {
                    return;
                }
                _lastWriteTimeUtc = writeTime;

                if (TryReadConfig(out var newValue))
                {
                    PendingShowNearbyEnemies = newValue;
                    HasPendingUpdate = true;
                }
            }
            catch
            {
                // ignore watcher errors
            }
        }

        internal static void ApplyPendingChanges()
        {
            if (!HasPendingUpdate)
            {
                return;
            }
            ShowNearbyEnemies = PendingShowNearbyEnemies;
            HasPendingUpdate = false;
        }
    }
}
