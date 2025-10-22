using System;
using System.Collections.Generic;
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
        }

        /// <summary>
        /// Map a character to its marker.
        /// </summary>
        private readonly Dictionary<CharacterMainControl, CharacterMarker> _markers =
            new Dictionary<CharacterMainControl, CharacterMarker>();

        public static bool ShowNearbyEnemies = false;

        private bool _mapActive;
        private CharacterSpawnerRoot[] _cachedSpawnerRoots;
        private float _scanCooldown;
        private const float ScanIntervalSeconds = 0.5f;

        private void Awake()
        {
            Debug.Log("BossLiveMapMod loaded: live character markers enabled");
            ModConfig.Load();
            ShowNearbyEnemies = ModConfig.ShowNearbyEnemies;
        }

        private void OnEnable()
        {
            View.OnActiveViewChanged += OnActiveViewChanged;
            if (IsMapOpen())
            {
                BeginTracking();
            }
        }

        private void OnDisable()
        {
            View.OnActiveViewChanged -= OnActiveViewChanged;
            EndTracking();
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
            ResetMarkers();
            _mapActive = true;
            Health.OnDead += OnAnyHealthDead;
            _cachedSpawnerRoots = null;
            ScanCharacters();
            _scanCooldown = ScanIntervalSeconds;
        }

        private void EndTracking()
        {
            if (!_mapActive)
                return;

            _mapActive = false;
            Health.OnDead -= OnAnyHealthDead;
            _cachedSpawnerRoots = null;
            ResetMarkers();
        }


        private void ResetMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                DestroySafely(marker?.MarkerObject);
            }
            _markers.Clear();

        }

        private void ScanCharacters()
        {
            foreach (var character in EnumerateSpawnedCharacters())
            {
                AddOrUpdateMarker(character);
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
                    if (IsCharacterValid(character))
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
            if (character == null)
                return;

            var characterType = GetCharacterType(character);
            if (!ShouldTrack(characterType, character))
                return;

            var displayName = GetCharacterName(character);

            if (_markers.TryGetValue(character, out var marker))
            {
                UpdateMarker(marker, characterType, displayName);
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
            };

            _markers[character] = marker;

            UpdateMarker(marker, characterType, displayName, forceVisualUpdate: true);
        }

        private void UpdateMarker(CharacterMarker marker, CharacterType characterType, string displayName, bool forceVisualUpdate = false)
        {
            if (marker?.MarkerObject == null || marker.Poi == null || marker.Character == null)
                return;

            marker.MarkerObject.name = $"CharacterMarker:{displayName}";
            marker.MarkerObject.transform.position = marker.Character.transform.position;
            if (!forceVisualUpdate && marker.Type == characterType && marker.DisplayName == displayName)
                return;

            marker.Type = characterType;
            marker.DisplayName = displayName;
            var color = characterType.GetMarkerColor();
            color.a = 0.66f;
            marker.Poi.Color = color;
            marker.Poi.ShadowColor = Color.clear;
            marker.Poi.ShadowDistance = 0f;
            marker.Poi.Setup(characterType.GetMarkerIcon(), displayName, followActiveScene: true);
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
                if (!IsCharacterValid(character) || !ShouldTrack(entry.Type, character))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(kv.Key);
                    continue;
                }

                UpdateMarker(entry, entry.Type, entry.DisplayName);
            }

            if (stale != null)
            {
                foreach (var character in stale)
                {
                    DestroyMarker(character);
                }
            }
        }

        private static bool IsCharacterValid(CharacterMainControl character)
        {
            if (character == null)
                return false;

            var go = character.gameObject;
            if (!go.scene.IsValid() || !go.scene.isLoaded)
                return false;

            if (character.GetComponent<SimplePointOfInterest>() != null)
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
            if (entry.MarkerObject != null)
            {
                DestroySafely(entry.MarkerObject);
            }
        }

        private void OnAnyHealthDead(Health health, DamageInfo info)
        {
            if (!_mapActive || health == null)
                return;

            var character = health.TryGetCharacter();
            DestroyMarker(character);
        }

        private static string GetCharacterName(CharacterMainControl character)
        {
            var name = character?.characterPreset?.DisplayName;
            // var name = character?.characterPreset?.nameKey;
            return string.IsNullOrEmpty(name) ? "*" : name;
        }

        public static Color AdjustNonBossColor(Color baseColor) =>
            Color.Lerp(baseColor, Color.white, 0.35f);

        private static bool ShouldTrack(CharacterType type, CharacterMainControl character) =>
            type != CharacterType.Mobs || (ShowNearbyEnemies && character != null && character.gameObject.activeInHierarchy);

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
