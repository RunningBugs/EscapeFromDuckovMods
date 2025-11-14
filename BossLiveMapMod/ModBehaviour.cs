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
    public enum CharacterRole
    {
        Boss,
        NonBoss,
        None
    }

    public enum TeamRelation
    {
        SameTeam,
        Neutral,
        Hostile,
        Unknown
    }

    public static class MarkerVisuals
    {
        public static Sprite GetMarkerIcon(CharacterRole role, TeamRelation relation)
        {
            var icons = MapMarkerManager.Icons;
            if (icons == null)
                return TryGetSelectedIcon();

            var targetIndex = GetIconIndex(role, relation);
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

        public static Color GetMarkerColor(CharacterRole role, TeamRelation relation)
        {
            switch (relation)
            {
                case TeamRelation.SameTeam:
                    return ApplyRoleTint(role, new Color(0.3f, 0.85f, 0.3f));
                case TeamRelation.Neutral:
                    return ApplyRoleTint(role, new Color(1f, 0.9f, 0.3f));
                case TeamRelation.Hostile:
                    return ApplyRoleTint(role, new Color(1f, 0.3f, 0.3f));
                default:
                    return ApplyRoleTint(role, Color.red);
            }
        }

        private static Color ApplyRoleTint(CharacterRole role, Color baseColor)
        {
            return role == CharacterRole.Boss ? baseColor : ModBehaviour.AdjustNonBossColor(baseColor);
        }

        private static int? GetIconIndex(CharacterRole role, TeamRelation relation)
        {
            if (role == CharacterRole.Boss)
                return 3;

            return relation switch
            {
                TeamRelation.SameTeam => 1,
                TeamRelation.Neutral => 6,
                TeamRelation.Hostile => 2,
                _ => 2
            };
        }

        private static Sprite TryGetSelectedIcon()
        {
            try { return MapMarkerManager.SelectedIcon; }
            catch { return null; }
        }
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
            public CharacterRole Role;
            public TeamRelation Relation;
            public string DisplayName;
            public bool IsActive; // Whether character is active (within 100 distance)
            public bool HasPreexistingPoi; // Cached flag to avoid GetComponent calls
            public Teams LastKnownTeam;
            public Action<Teams> TeamChangedHandler;
            public Color LastAppliedColor;
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

        private CharacterMainControl _mainCharacter;
        private Teams _playerTeam = Teams.player;

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

        private readonly struct CharacterClassification
        {
            public CharacterClassification(CharacterRole role, TeamRelation relation)
            {
                Role = role;
                Relation = relation;
            }

            public CharacterRole Role { get; }
            public TeamRelation Relation { get; }
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
            RefreshMainCharacterReference();
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
            if (_mainCharacter != null)
            {
                try { _mainCharacter.OnTeamChanged -= OnMainCharacterTeamChanged; }
                catch { }
                _mainCharacter = null;
            }

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
            RefreshMainCharacterReference();
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
                UnsubscribeFromTeamChanges(marker);
                if (marker.Poi != null)
                {
                    PointsOfInterests.Unregister(marker.Poi);
                }
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
                RefreshMainCharacterReference();
                RefreshAllMarkerRelations(forceVisualUpdate: false);
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
                        var classification = ClassifyCharacter(character);
                        if (classification.Role == CharacterRole.Boss)
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

        private CharacterClassification ClassifyCharacter(CharacterMainControl character)
        {
            var role = GetCharacterRole(character);
            var relation = GetTeamRelation(character);
            return new CharacterClassification(role, relation);
        }

        private static CharacterRole GetCharacterRole(CharacterMainControl character)
        {
            if (character == null)
                return CharacterRole.None;

            var preset = character.characterPreset;
            if (preset != null && preset.characterIconType == CharacterIconTypes.boss)
                return CharacterRole.Boss;

            return CharacterRole.NonBoss;
        }

        private TeamRelation GetTeamRelation(CharacterMainControl character)
        {
            if (character == null)
                return TeamRelation.Unknown;

            if (_mainCharacter == null)
                return TeamRelation.Hostile;

            var targetTeam = character.Team;
            var playerTeam = _playerTeam;

            if (targetTeam == playerTeam)
                return TeamRelation.SameTeam;

            bool playerSeesEnemy = SafeIsEnemy(playerTeam, targetTeam);
            bool targetSeesEnemy = SafeIsEnemy(targetTeam, playerTeam);

            if (!playerSeesEnemy && !targetSeesEnemy)
                return TeamRelation.Neutral;

            return TeamRelation.Hostile;
        }

        private static bool SafeIsEnemy(Teams selfTeam, Teams targetTeam)
        {
            try
            {
                return Team.IsEnemy(selfTeam, targetTeam);
            }
            catch
            {
                return selfTeam != targetTeam;
            }
        }

        private void RefreshMainCharacterReference()
        {
            CharacterMainControl current = null;
            try { current = CharacterMainControl.Main; }
            catch { }

            if (current == null)
            {
                try { current = LevelManager.Instance?.MainCharacter; }
                catch { }
            }

            if (ReferenceEquals(current, _mainCharacter))
            {
                if (_mainCharacter != null)
                    _playerTeam = _mainCharacter.Team;
                return;
            }

            if (_mainCharacter != null)
            {
                try { _mainCharacter.OnTeamChanged -= OnMainCharacterTeamChanged; }
                catch { }
            }

            _mainCharacter = current;
            _playerTeam = _mainCharacter != null ? _mainCharacter.Team : Teams.player;

            if (_mainCharacter != null)
            {
                try { _mainCharacter.OnTeamChanged += OnMainCharacterTeamChanged; }
                catch { }
            }

            RefreshAllMarkerRelations(forceVisualUpdate: true);
        }

        private void OnMainCharacterTeamChanged(Teams team)
        {
            _playerTeam = team;
            RefreshAllMarkerRelations(forceVisualUpdate: true);
        }

        private void RefreshAllMarkerRelations(bool forceVisualUpdate)
        {
            if (_markers.Count == 0)
                return;

            List<CharacterMainControl> stale = null;

            foreach (var kv in _markers)
            {
                var character = kv.Key;
                var marker = kv.Value;
                if (marker == null || character == null)
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(character);
                    continue;
                }

                var classification = ClassifyCharacter(character);
                if (!ShouldTrack(classification.Role, classification.Relation, character))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(character);
                    continue;
                }

                var relationChanged = marker.Relation != classification.Relation || marker.Role != classification.Role;
                if (!forceVisualUpdate && !relationChanged)
                    continue;

                var displayName = GetDisplayName(character);
                var isActive = IsCharacterActive(character);
                UpdateMarker(marker, classification.Role, classification.Relation, displayName, isActive, ModConfig.ShowLivePositions, forceVisualUpdate);
            }

            if (stale != null)
            {
                foreach (var character in stale)
                {
                    DestroyMarker(character);
                }
            }
        }

        private void SubscribeToTeamChanges(CharacterMarker marker)
        {
            if (marker?.Character == null)
                return;

            UnsubscribeFromTeamChanges(marker);

            Action<Teams> handler = null;
            handler = newTeam => OnTrackedCharacterTeamChanged(marker, newTeam);
            marker.TeamChangedHandler = handler;
            marker.LastKnownTeam = marker.Character.Team;

            try { marker.Character.OnTeamChanged += handler; }
            catch { marker.TeamChangedHandler = null; }
        }

        private void UnsubscribeFromTeamChanges(CharacterMarker marker)
        {
            if (marker == null)
                return;

            if (marker.TeamChangedHandler != null && marker.Character != null)
            {
                try { marker.Character.OnTeamChanged -= marker.TeamChangedHandler; }
                catch { }
            }

            marker.TeamChangedHandler = null;
        }

        private void OnTrackedCharacterTeamChanged(CharacterMarker marker, Teams newTeam)
        {
            if (marker?.Character == null)
                return;

            marker.LastKnownTeam = newTeam;

            var classification = ClassifyCharacter(marker.Character);
            if (!ShouldTrack(classification.Role, classification.Relation, marker.Character))
            {
                DestroyMarker(marker.Character);
                return;
            }

            var displayName = GetDisplayName(marker.Character);
            var isActive = IsCharacterActive(marker.Character);
            UpdateMarker(marker, classification.Role, classification.Relation, displayName, isActive, ModConfig.ShowLivePositions, forceVisualUpdate: true);
        }

        private void AddOrUpdateMarker(CharacterMainControl character)
        {
            if (!IsCharacterValid(character, out bool hasPreexistingPoi))
                return;

            // Don't create markers for characters that already have a POI component
            if (hasPreexistingPoi)
                return;

            var classification = ClassifyCharacter(character);
            if (!ShouldTrack(classification.Role, classification.Relation, character))
                return;

            var displayName = GetDisplayName(character);
            bool isActive = IsCharacterActive(character);
            // If marker already exists, update it only if Live is ON
            if (_markers.TryGetValue(character, out var marker))
            {
                var allowPositionUpdate = ModConfig.ShowLivePositions;
                UpdateMarker(marker, classification.Role, classification.Relation, displayName, isActive, allowPositionUpdate);
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
                Role = classification.Role,
                Relation = classification.Relation,
                DisplayName = displayName,
                HasPreexistingPoi = hasPreexistingPoi,
                LastKnownTeam = character.Team
            };

            _markers[character] = marker;
            SubscribeToTeamChanges(marker);

            UpdateMarker(marker, classification.Role, classification.Relation, displayName, isActive, allowPositionUpdate: true, forceVisualUpdate: true);
        }

        private void UpdateMarker(CharacterMarker marker, CharacterRole role, TeamRelation relation, string displayName, bool isActive, bool allowPositionUpdate, bool forceVisualUpdate = false)
        {
            if (marker?.MarkerObject == null || marker.Poi == null || marker.Character == null)
                return;

            // Keep GameObject name updated (displayName may be empty depending on config)
            marker.MarkerObject.name = $"CharacterMarker:{displayName}";
            // Update position only when allowed (Live toggle controls per-frame updates)
            if (allowPositionUpdate && marker.Character != null && marker.Character.transform != null)
                marker.MarkerObject.transform.position = marker.Character.transform.position;
            if (!forceVisualUpdate && marker.Role == role && marker.Relation == relation && marker.DisplayName == displayName && marker.IsActive == isActive)
                return;

            marker.Role = role;
            marker.Relation = relation;
            marker.DisplayName = displayName;
            marker.IsActive = isActive;
            marker.LastKnownTeam = marker.Character.Team;

            // Marker color respects global transparency setting (mod config)
            var color = MarkerVisuals.GetMarkerColor(role, relation);
            const float baseAlpha = 1f;
            color.a = baseAlpha * ModConfig.Transparency;
            bool colorChanged = forceVisualUpdate || !ColorsApproximatelyEqual(marker.LastAppliedColor, color);

            if (colorChanged)
            {
                marker.Poi.Color = color;
                marker.Poi.ShadowColor = Color.clear;
                marker.Poi.ShadowDistance = 0f;

                // Respect config: show names or hide them
                var nameToUse = ModConfig.ShowMarkerNames ? displayName : string.Empty;
                // Use followActiveScene: true so POI system tracks the game object
                // The Live toggle controls per-frame updates in LateUpdate()
                marker.Poi.Setup(MarkerVisuals.GetMarkerIcon(role, relation), nameToUse, followActiveScene: true);

                // Always show markers (they show last known position when Live is off)
                marker.Poi.HideIcon = false;
                marker.LastAppliedColor = color;
            }
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
                if (!IsCharacterValidLightweight(character, entry))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(kv.Key);
                    continue;
                }

                var classification = ClassifyCharacter(character);
                entry.Role = classification.Role;
                entry.Relation = classification.Relation;

                if (!ShouldTrack(classification.Role, classification.Relation, character))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(kv.Key);
                    continue;
                }

                bool isActive = IsCharacterActive(character);
                bool allowPositionUpdate = ModConfig.ShowLivePositions;
                bool skipPositionUpdate = allowPositionUpdate &&
                                          classification.Role == CharacterRole.NonBoss &&
                                          classification.Relation == TeamRelation.Hostile &&
                                          !isActive;
                var applyPositionUpdate = skipPositionUpdate ? false : allowPositionUpdate;

                UpdateMarker(entry, classification.Role, classification.Relation, entry.DisplayName, isActive, applyPositionUpdate);
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

            UnsubscribeFromTeamChanges(entry);
            _markers.Remove(character);

            // If this was a boss entry, mark corresponding BossList entries as dead (strike-through in UI)
            try
            {
                if (entry != null && entry.Role == CharacterRole.Boss && !string.IsNullOrEmpty(entry.DisplayName))
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

            if (entry.Poi != null)
            {
                PointsOfInterests.Unregister(entry.Poi);
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

        private static bool ColorsApproximatelyEqual(Color a, Color b)
        {
            const float epsilon = 0.01f;
            return Mathf.Abs(a.r - b.r) < epsilon &&
                   Mathf.Abs(a.g - b.g) < epsilon &&
                   Mathf.Abs(a.b - b.b) < epsilon &&
                   Mathf.Abs(a.a - b.a) < epsilon;
        }

        private static bool ShouldTrack(CharacterRole role, TeamRelation relation, CharacterMainControl character)
        {
            if (role == CharacterRole.Boss)
                return true;

            if (role != CharacterRole.NonBoss)
                return false;

            if (relation == TeamRelation.SameTeam || relation == TeamRelation.Neutral)
                return true;

            if (relation == TeamRelation.Hostile || relation == TeamRelation.Unknown)
            {
                if (!ModConfig.ShowAllEnemies)
                    return false;

                if (ModConfig.ShowNearbyOnly)
                    return IsCharacterActive(character);

                return true;
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
