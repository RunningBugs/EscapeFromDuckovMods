using System;
using System.Collections.Generic;
using System.IO;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;
using Duckov.Scenes;
using Duckov.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BossLiveMapMod
{
    public enum CharacterType
    {
        Boss,
        Friend,
        Neutral,
        Mobs,
        None
    }

    public static class CharacterTypeExtensions
    {
        public static Sprite GetMarkerIcon(this CharacterType characterType)
        {
            var icons = MapMarkerManager.Icons;
            if (icons == null)
                return TryGetSelectedIcon();

            var targetIndex = GetIconIndex(characterType);
            if (targetIndex.HasValue)
            {
                var index = targetIndex.Value;
                if (index >= 0 && index < icons.Count)
                {
                    var icon = icons[index];
                    if (icon != null)
                        return icon;
                }
            }

            foreach (var icon in icons)
            {
                if (icon != null)
                    return icon;
            }

            return TryGetSelectedIcon();
        }

        private static Sprite TryGetSelectedIcon()
        {
            try { return MapMarkerManager.SelectedIcon; }
            catch { return null; }
        }

        private static int? GetIconIndex(CharacterType type) =>
            type switch
            {
                CharacterType.Friend => 0,
                CharacterType.Mobs => 2,
                CharacterType.Boss => 3,
                CharacterType.Neutral => 6,
                _ => null,
            };

        public static Color GetMarkerColor(this CharacterType characterType) =>
            characterType switch
            {
                CharacterType.Boss => Color.red,
                CharacterType.Friend => ModBehaviour.AdjustNonBossColor(new Color(0.3f, 0.85f, 0.3f)),
                CharacterType.Neutral => ModBehaviour.AdjustNonBossColor(new Color(1f, 0.9f, 0.3f)),
                CharacterType.Mobs => ModBehaviour.AdjustNonBossColor(new Color(1f, 0.3f, 0.3f)),
                _ => ModBehaviour.AdjustNonBossColor(Color.red),
            };
    }
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        /// <summary>
        /// The data structure for a tracked character marker.
        /// All information needed to render the marker on the map.
        /// </summary>
        private sealed class CharacterMarker
        {
            public CharacterMainControl Character;
            public GameObject MarkerObject;
            public SimplePointOfInterest Poi;
            public CharacterType Type;
            public string DisplayName;
            public bool IsActive; // Whether character is active (within 100 distance)
            public bool HasPreexistingPoi; // Cached flag to avoid GetComponent calls
        }

        /// <summary>
        /// Map a character to its marker.
        /// </summary>
        private readonly Dictionary<CharacterMainControl, CharacterMarker> _markers =
            new Dictionary<CharacterMainControl, CharacterMarker>();

        // Tracked boss instances (allow duplicates) and a view for UI with strike-through for dead ones.
        public sealed class BossEntry
        {
            public CharacterMainControl Character;
            public string DisplayName;
            public bool Alive;
        }

        // Backing list of boss instances (allow duplicates)
        public static readonly List<BossEntry> BossList = new List<BossEntry>();

        // Backing list of special preset instances (allow duplicates)
        public static readonly List<BossEntry> SpecialList = new List<BossEntry>();

        // Cached formatted boss names to avoid allocations every frame
        private static List<string> _cachedBossNames = new List<string>();
        private static int _cachedBossNamesHash = 0;

        // Cached formatted special names
        private static List<string> _cachedSpecialNames = new List<string>();
        private static int _cachedSpecialNamesHash = 0;

        // UI-facing list of strings (includes strike-through for dead bosses).
        public static List<string> BossNames
        {
            get
            {
                try
                {
                    lock (BossList)
                    {
                        int currentHash = ComputeBossListHash();
                        if (currentHash != _cachedBossNamesHash)
                        {
                            _cachedBossNames.Clear();
                            foreach (var be in BossList)
                            {
                                if (be == null) continue;
                                var name = be.DisplayName ?? string.Empty;
                                if (!be.Alive)
                                    _cachedBossNames.Add($"<s>{name}</s>");
                                else
                                    _cachedBossNames.Add(name);
                            }
                            _cachedBossNamesHash = currentHash;
                        }
                    }
                }
                catch { }
                return _cachedBossNames;
            }
        }

        // UI-facing list of strings for special presets
        public static List<string> SpecialNames
        {
            get
            {
                try
                {
                    lock (SpecialList)
                    {
                        int currentHash = ComputeSpecialListHash();
                        if (currentHash != _cachedSpecialNamesHash)
                        {
                            _cachedSpecialNames.Clear();
                            foreach (var be in SpecialList)
                            {
                                if (be == null) continue;
                                var name = be.DisplayName ?? string.Empty;
                                if (!be.Alive)
                                    _cachedSpecialNames.Add($"<s>{name}</s>");
                                else
                                    _cachedSpecialNames.Add(name);
                            }
                            _cachedSpecialNamesHash = currentHash;
                        }
                    }
                }
                catch { }
                return _cachedSpecialNames;
            }
        }

        private static int ComputeBossListHash()
        {
            unchecked
            {
                int hash = 17;
                foreach (var be in BossList)
                {
                    if (be == null) continue;
                    hash = hash * 31 + (be.Character?.GetHashCode() ?? 0);
                    hash = hash * 31 + (be.DisplayName?.GetHashCode() ?? 0);
                    hash = hash * 31 + (be.Alive ? 1 : 0);
                }
                return hash;
            }
        }

        private static int ComputeSpecialListHash()
        {
            unchecked
            {
                int hash = 17;
                foreach (var se in SpecialList)
                {
                    if (se == null) continue;
                    hash = hash * 31 + (se.Character?.GetHashCode() ?? 0);
                    hash = hash * 31 + (se.DisplayName?.GetHashCode() ?? 0);
                    hash = hash * 31 + (se.Alive ? 1 : 0);
                }
                return hash;
            }
        }

        public static bool ShowNearbyEnemies = false;

        private bool _mapActive;
        private CharacterSpawnerRoot[] _cachedSpawnerRoots;
        private float _scanCooldown;
        private const float ScanIntervalSeconds = 0.5f;

        // Special preset names loaded from text file (one name per line). Comparisons are case-insensitive.
        private static readonly HashSet<string> _specialPresetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Watcher for auto-reloading the special_presets.txt file
        private static FileSystemWatcher _specialPresetsWatcher;
        private static readonly object _specialPresetsWatcherLock = new object();
        private static DateTime _specialPresetsLastWriteUtc = DateTime.MinValue;

        private void LoadSpecialPresets(string modFolder)
        {
            try
            {
                if (string.IsNullOrEmpty(modFolder))
                    return;
                var filePath = Path.Combine(modFolder, "special_presets.txt");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(filePath);
                        lock (_specialPresetNames)
                        {
                            _specialPresetNames.Clear();
                            foreach (var raw in lines)
                            {
                                if (string.IsNullOrWhiteSpace(raw)) continue;
                                var t = raw.Trim();
                                if (t.StartsWith("#")) continue;
                                _specialPresetNames.Add(t);
                            }
                        }
                        // store last write time to avoid duplicate reloads
                        try { _specialPresetsLastWriteUtc = File.GetLastWriteTimeUtc(filePath); } catch { }
                    }
                    catch { /* ignore read errors */ }
                }

                // Ensure a watcher is watching the file so changes auto-reload
                try { EnsureSpecialPresetsWatcher(filePath); } catch { }
            }
            catch
            {
                // ignore load errors
            }
        }

        private void EnsureSpecialPresetsWatcher(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;
                var dir = Path.GetDirectoryName(filePath);
                var fname = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(fname))
                    return;

                lock (_specialPresetsWatcherLock)
                {
                    if (_specialPresetsWatcher != null)
                        return;

                    var watcher = new FileSystemWatcher(dir, fname)
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                        EnableRaisingEvents = true
                    };
                    watcher.Changed += OnSpecialPresetsFileChanged;
                    watcher.Created += OnSpecialPresetsFileChanged;
                    watcher.Renamed += OnSpecialPresetsFileChanged;
                    _specialPresetsWatcher = watcher;
                }
            }
            catch { /* ignore watcher errors */ }
        }

        private void OnSpecialPresetsFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Debounce using last write time
                var write = File.GetLastWriteTimeUtc(e.FullPath);
                if (write <= _specialPresetsLastWriteUtc)
                    return;
                _specialPresetsLastWriteUtc = write;

                // Reload from file directory (modFolder isn't strictly needed because we have full path)
                var modFolder = Path.GetDirectoryName(e.FullPath);
                LoadSpecialPresets(modFolder);
            }
            catch { /* ignore file watch handling errors */ }
        }

        private void Awake()
        {
            InitializeLocalization();
            ModConfig.Load();
            ShowNearbyEnemies = ModConfig.ShowNearbyEnemies;

        }

        private void InitializeLocalization()
        {
            try
            {
                var assemblyLocation = typeof(ModBehaviour).Assembly.Location;
                var modFolder = Path.GetDirectoryName(assemblyLocation);
                ModLocalization.Initialize(modFolder);
                // Load special preset tracking list (text file in the same folder as Lang.ini)
                try { LoadSpecialPresets(modFolder); } catch { }
            }
            catch
            {
                // Localization initialization failed
            }
        }



        private void OnEnable()
        {
            View.OnActiveViewChanged += OnActiveViewChanged;
            SceneLoader.onStartedLoadingScene += OnSceneStartedLoading;
            SceneLoader.onFinishedLoadingScene += OnSceneFinishedLoading;
            Health.OnDead += OnAnyHealthDead;
            if (IsMapOpen())
            {
                BeginTracking();
            }
        }

        private void OnDisable()
        {
            View.OnActiveViewChanged -= OnActiveViewChanged;
            SceneLoader.onStartedLoadingScene -= OnSceneStartedLoading;
            SceneLoader.onFinishedLoadingScene -= OnSceneFinishedLoading;
            Health.OnDead -= OnAnyHealthDead;
            EndTracking();

            // Dispose special presets watcher if present
            try
            {
                lock (_specialPresetsWatcherLock)
                {
                    if (_specialPresetsWatcher != null)
                    {
                        try
                        {
                            _specialPresetsWatcher.EnableRaisingEvents = false;
                        }
                        catch { }
                        try { _specialPresetsWatcher.Changed -= OnSpecialPresetsFileChanged; } catch { }
                        try { _specialPresetsWatcher.Created -= OnSpecialPresetsFileChanged; } catch { }
                        try { _specialPresetsWatcher.Renamed -= OnSpecialPresetsFileChanged; } catch { }
                        try { _specialPresetsWatcher.Dispose(); } catch { }
                        _specialPresetsWatcher = null;
                    }
                }
            }
            catch { }
        }

        private void OnSceneStartedLoading(SceneLoadingContext context)
        {
            // Clear markers when leaving the current scene
            ResetMarkers();
        }

        private void OnSceneFinishedLoading(SceneLoadingContext context)
        {
            // Scan once when entering scene to pre-populate markers
            ScanCharacters();
            _scanCooldown = ScanIntervalSeconds;
        }



        private static bool IsMapOpen()
        {
            var view = MiniMapView.Instance;
            return view != null && View.ActiveView == view;
        }

        private void OnActiveViewChanged()
        {
            if (IsMapOpen())
                BeginTracking();
            else
                EndTracking();
        }

        private void BeginTracking()
        {
            // Don't reset markers on map open - preserve last known positions when Live is OFF
            // ResetMarkers();
            _mapActive = true;
            // Ensure our runtime UI is present when the map opens
            MapViewUI.Ensure();
            _cachedSpawnerRoots = null;

            // Clean up any stale markers for dead characters before scanning
            RemoveStaleMarkers();

            ScanCharacters();
            _scanCooldown = ScanIntervalSeconds;
        }

        private void EndTracking()
        {
            if (!_mapActive)
                return;

            _mapActive = false;
            _cachedSpawnerRoots = null;
            // Don't reset markers on map close - preserve last known positions when Live is OFF
            // ResetMarkers();

            // Clear boss list when tracking ends (map closed)
            try
            {
                lock (BossList)
                {
                    BossList.Clear();
                }
            }
            catch { }
        }


        private void ResetMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                DestroySafely(marker?.MarkerObject);
            }
            _markers.Clear();

            // Clear boss list as markers and scene are being reset
            try
            {
                lock (BossList)
                {
                    BossList.Clear();
                }
            }
            catch { }
        }

        private void ScanCharacters()
        {
            // Rebuild boss list and special list from current spawned characters so it reflects current scene
            try
            {
                lock (BossList)
                {
                    BossList.Clear();
                }
                lock (SpecialList)
                {
                    SpecialList.Clear();
                }

                foreach (var character in EnumerateSpawnedCharacters())
                {
                    // RuntimeDumper.DumpCharacterFields(character); // disabled (too verbose). Uncomment for debugging.

                    // Ensure boss list stays up-to-date regardless of marker settings
                    try
                    {
                        var ct = GetCharacterType(character);
                        if (ct == CharacterType.Boss)
                        {
                            var displayName = GetDisplayName(character);
                            lock (BossList)
                            {
                                BossList.Add(new BossEntry { Character = character, DisplayName = displayName, Alive = true });
                            }
                        }

                        // Check if character's preset is in special presets list
                        try
                        {
                            var preset = character?.characterPreset;
                            if (preset != null)
                            {
                                string presetName = null;
                                try { presetName = preset.name; } catch { }
                                if (string.IsNullOrEmpty(presetName))
                                {
                                    try { presetName = preset.nameKey; } catch { }
                                }
                                if (string.IsNullOrEmpty(presetName))
                                {
                                    try { presetName = preset.DisplayName; } catch { }
                                }

                                if (!string.IsNullOrEmpty(presetName))
                                {
                                    bool isSpecial = false;
                                    lock (_specialPresetNames)
                                    {
                                        isSpecial = _specialPresetNames.Contains(presetName);
                                    }

                                    if (isSpecial)
                                    {
                                        var displayName = GetDisplayName(character);
                                        lock (SpecialList)
                                        {
                                            SpecialList.Add(new BossEntry { Character = character, DisplayName = displayName, Alive = true });
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    catch { }

                    AddOrUpdateMarker(character);
                }
            }
            catch
            {
                // Fallback: call original behavior if something goes wrong
                foreach (var character in EnumerateSpawnedCharacters())
                {
                    // RuntimeDumper.DumpCharacterFields(character); // disabled (too verbose). Uncomment for debugging.
                    AddOrUpdateMarker(character);
                }
            }
        }

        private IEnumerable<CharacterMainControl> EnumerateSpawnedCharacters()
        {
            var roots = GetSpawnerRoots();
            if (roots == null || roots.Length == 0)
                yield break;

            foreach (var root in roots)
            {
                var list = root?.createdCharacters;
                if (list == null)
                    continue;

                foreach (var character in list)
                {
                    if (IsCharacterValid(character, out _))
                    {
                        yield return character;
                    }
                }
            }
        }

        private CharacterSpawnerRoot[] GetSpawnerRoots()
        {
            if (_cachedSpawnerRoots == null || _cachedSpawnerRoots.Length == 0 || Array.Exists(_cachedSpawnerRoots, r => r == null))
            {
                _cachedSpawnerRoots = Resources.FindObjectsOfTypeAll<CharacterSpawnerRoot>() ?? Array.Empty<CharacterSpawnerRoot>();
            }

            return _cachedSpawnerRoots;
        }

        public static CharacterType GetCharacterType(CharacterMainControl c)
        {
            if (c == null)
                return CharacterType.None;

            var preset = c.characterPreset;
            return c.Team switch
            {
                Teams.player => CharacterType.Friend,
                Teams.all => CharacterType.Neutral,
                _ when preset != null && preset.characterIconType == CharacterIconTypes.boss
                    => CharacterType.Boss,
                _ => CharacterType.Mobs,
            };
        }

        private void AddOrUpdateMarker(CharacterMainControl character)
        {
            if (!IsCharacterValid(character, out bool hasPreexistingPoi))
                return;

            // Don't create markers for characters that already have a POI component
            if (hasPreexistingPoi)
                return;

            var characterType = GetCharacterType(character);
            if (!ShouldTrack(characterType, character))
                return;

            var displayName = GetDisplayName(character);
            bool isActive = IsCharacterActive(character);
            // If marker already exists, update it only if Live is ON
            if (_markers.TryGetValue(character, out var marker))
            {
                // When Live is OFF, skip position updates for existing markers
                if (!ModConfig.ShowLivePositions)
                    return;

                UpdateMarker(marker, characterType, displayName, isActive);
                return;
            }

            // Only create marker objects when Markers toggle is enabled; BossList is updated separately by ScanCharacters
            if (!ModConfig.ShowMarkers)
            {
                // Do not create marker objects, but boss list is handled in ScanCharacters to always reflect current scene
                return;
            }

            var markerObject = new GameObject($"CharacterMarker:{displayName}");
            markerObject.transform.position = character.transform.position;
            if (MultiSceneCore.MainScene.HasValue)
            {
                SceneManager.MoveGameObjectToScene(markerObject, MultiSceneCore.MainScene.Value);
            }

            var poi = markerObject.AddComponent<SimplePointOfInterest>();

            marker = new CharacterMarker
            {
                Character = character,
                MarkerObject = markerObject,
                Poi = poi,
                Type = characterType,
                DisplayName = displayName,
                HasPreexistingPoi = hasPreexistingPoi,
            };

            _markers[character] = marker;

            UpdateMarker(marker, characterType, displayName, isActive, forceVisualUpdate: true);
        }

        private void UpdateMarker(CharacterMarker marker, CharacterType characterType, string displayName, bool isActive, bool forceVisualUpdate = false)
        {
            if (marker?.MarkerObject == null || marker.Poi == null || marker.Character == null)
                return;

            // Keep GameObject name updated (displayName may be empty depending on config)
            marker.MarkerObject.name = $"CharacterMarker:{displayName}";
            // Always update position when called (whether from scan or per-frame update)
            if (marker.Character != null && marker.Character.transform != null)
                marker.MarkerObject.transform.position = marker.Character.transform.position;
            if (!forceVisualUpdate && marker.Type == characterType && marker.DisplayName == displayName && marker.IsActive == isActive)
                return;

            marker.Type = characterType;
            marker.DisplayName = displayName;
            marker.IsActive = isActive;

            // Marker color respects global transparency setting (mod config)
            var color = characterType.GetMarkerColor();
            const float baseAlpha = 1f;
            color.a = baseAlpha * ModConfig.Transparency;
            marker.Poi.Color = color;
            marker.Poi.ShadowColor = Color.clear;
            marker.Poi.ShadowDistance = 0f;

            // Respect config: show names or hide them
            var nameToUse = ModConfig.ShowMarkerNames ? displayName : string.Empty;
            // Use followActiveScene: true so POI system tracks the game object
            // The Live toggle controls per-frame updates in LateUpdate()
            marker.Poi.Setup(characterType.GetMarkerIcon(), nameToUse, followActiveScene: true);

            // Always show markers (they show last known position when Live is off)
            marker.Poi.HideIcon = false;
        }

        /// <summary>
        /// Check for configuration changes and only apply changes when config is changed.
        /// </summary>
        private void Update()
        {
            if (_mapActive && StepScanTimer())
            {
                ScanCharacters();
            }

            if (!ModConfig.HasPendingUpdate)
                return;

            ModConfig.ApplyPendingChanges();
            ShowNearbyEnemies = ModConfig.ShowNearbyEnemies;
            if (_mapActive)
            {
                ResetMarkers();
                ScanCharacters();
                _scanCooldown = ScanIntervalSeconds;
            }
        }

        /// <summary>
        /// When map is active, find invalid markers and remove them.
        /// </summary>
        private void LateUpdate()
        {
            if (!_mapActive || _markers.Count == 0)
                return;

            List<CharacterMainControl> stale = null;

            foreach (var kv in _markers)
            {
                var entry = kv.Value;
                var character = entry?.Character;

                // Use lightweight validation without GetComponent check
                if (!IsCharacterValidLightweight(character, entry) || !ShouldTrack(entry.Type, character))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(kv.Key);
                    continue;
                }

                // Skip per-frame position updates when Live is OFF
                if (!ModConfig.ShowLivePositions)
                    continue;

                // Only update position for active characters when Live is on
                // Inactive mobs don't need real-time tracking
                bool isActive = IsCharacterActive(character);
                if (entry.Type == CharacterType.Mobs && !isActive)
                    continue; // Skip per-frame updates for inactive mobs

                UpdateMarker(entry, entry.Type, entry.DisplayName, isActive);
            }

            if (stale != null)
            {
                foreach (var character in stale)
                {
                    DestroyMarker(character);
                }
            }
        }

        private static bool IsCharacterValid(CharacterMainControl character, out bool hasPreexistingPoi)
        {
            hasPreexistingPoi = false;

            if (character == null)
                return false;

            var go = character.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded)
                return false;

            // Quick preset-name override: if this character's preset name matches our special list, treat it as valid.
            try
            {
                var preset = character.characterPreset;
                if (preset != null)
                {
                    string presetName = null;

                    // Attempt to read common property names using reflection (safe fallbacks).
                    // If you later publicize the preset fields via Krafs.Publicizer, you can replace these
                    // reflection calls with direct property access for performance.
                    try
                    {
                        // Use publicized direct access to the preset name (Krafs.Publicizer makes private members accessible)
                        presetName = preset.name;
                    }
                    catch { /* ignore */ }

                    // Fallback: try common alternatives
                    if (string.IsNullOrEmpty(presetName))
                    {
                        try
                        {
                            // Publicized direct access to common alternative key
                            presetName = preset.nameKey;
                        }
                        catch { }
                    }

                    if (string.IsNullOrEmpty(presetName))
                    {
                        try
                        {
                            // Publicized direct access to DisplayName
                            presetName = preset.DisplayName;
                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(presetName))
                    {
                        try
                        {
                            lock (_specialPresetNames)
                            {
                                if (_specialPresetNames.Contains(presetName))
                                {
                                    // Force valid if preset is explicitly tracked
                                    return true;
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Only check GetComponent during initial scan, cache the result
            hasPreexistingPoi = character.GetComponent<SimplePointOfInterest>() != null;

            if (character.Health == null || character.Health.IsDead)
                return false;

            return true;
        }

        /// <summary>
        /// Lightweight validation for per-frame checks that avoids expensive GetComponent calls.
        /// Uses cached POI flag from the marker itself.
        /// </summary>
        private static bool IsCharacterValidLightweight(CharacterMainControl character, CharacterMarker marker)
        {
            if (character == null)
                return false;

            var go = character.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded)
                return false;

            // Skip GetComponent check - use cached flag
            if (marker.HasPreexistingPoi)
                return false;

            if (character.Health == null || character.Health.IsDead)
                return false;

            return true;
        }

        private void DestroyMarker(CharacterMainControl character)
        {
            if (character == null)
                return;

            if (!_markers.TryGetValue(character, out var entry))
                return;

            _markers.Remove(character);

            // If this was a boss entry, mark corresponding BossList entries as dead (strike-through in UI)
            try
            {
                if (entry != null && entry.Type == CharacterType.Boss && !string.IsNullOrEmpty(entry.DisplayName))
                {
                    lock (BossList)
                    {
                        for (int i = 0; i < BossList.Count; i++)
                        {
                            var be = BossList[i];
                            if (be != null && be.Character == character)
                            {
                                be.Alive = false;
                                // keep entry for display with strike-through
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore removal errors
            }

            // Also mark SpecialList entries as dead
            try
            {
                if (entry != null && !string.IsNullOrEmpty(entry.DisplayName))
                {
                    lock (SpecialList)
                    {
                        for (int i = 0; i < SpecialList.Count; i++)
                        {
                            var se = SpecialList[i];
                            if (se != null && se.Character == character)
                            {
                                se.Alive = false;
                                // keep entry for display with strike-through
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore removal errors
            }

            if (entry.MarkerObject != null)
            {
                DestroySafely(entry.MarkerObject);
            }
        }

        private void OnAnyHealthDead(Health health, DamageInfo info)
        {
            if (health == null)
                return;

            var character = health.TryGetCharacter();
            DestroyMarker(character);
        }

        private void RemoveStaleMarkers()
        {
            List<CharacterMainControl> stale = null;

            foreach (var kv in _markers)
            {
                var character = kv.Key;
                var marker = kv.Value;
                // Use lightweight validation for existing markers
                if (!IsCharacterValidLightweight(character, marker))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(character);
                }
            }

            if (stale != null)
            {
                foreach (var character in stale)
                {
                    DestroyMarker(character);
                }
            }
        }

        private static string GetDisplayName(CharacterMainControl character)
        {
            var name = character?.characterPreset?.DisplayName;
            // var name = character?.characterPreset?.nameKey;
            return string.IsNullOrEmpty(name) ? "*" : name;
        }

        public static Color AdjustNonBossColor(Color baseColor) =>
            Color.Lerp(baseColor, Color.white, 0.35f);

        private static bool ShouldTrack(CharacterType type, CharacterMainControl character)
        {
            // Always show: Boss, Neutral, Friend
            if (type == CharacterType.Boss || type == CharacterType.Neutral || type == CharacterType.Friend)
                return true;

            // Mobs: only show when "Mobs" toggle is enabled
            if (type == CharacterType.Mobs)
            {
                if (!ModConfig.ShowAllEnemies)
                    return false; // Mobs toggle is OFF

                // Mobs toggle is ON - check Nearby filter
                if (ModConfig.ShowNearbyOnly)
                    return IsCharacterActive(character); // Only show active mobs

                return true; // Show all mobs
            }

            return false;
        }

        private static bool IsCharacterActive(CharacterMainControl character)
        {
            // Characters are marked inactive by the game when > 100 distance from player
            return character != null && character.gameObject.activeInHierarchy;
        }

        private bool StepScanTimer()
        {
            _scanCooldown -= Time.deltaTime;
            if (_scanCooldown > 0f)
                return false;

            _scanCooldown = ScanIntervalSeconds;
            return true;
        }

        private static void DestroySafely(GameObject go)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        }

    }
}
