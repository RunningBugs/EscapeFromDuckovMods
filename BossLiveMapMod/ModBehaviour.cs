
using System;
using System.Collections.Generic;
using Duckov.MiniMaps;
using Duckov.MiniMaps.UI;
using Duckov.Scenes;
using Duckov.UI;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BossLiveMapMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private sealed class CharacterMarker
        {
            public CharacterMainControl Character;
            public GameObject MarkerObject;
            public SimplePointOfInterest Poi;
        }

        private readonly Dictionary<CharacterMainControl, CharacterMarker> _markers =
            new Dictionary<CharacterMainControl, CharacterMarker>();
        private readonly HashSet<GameObject> _ownedMarkerObjects = new HashSet<GameObject>();

        public static bool ShowNearbyEnemies = false;

        private static readonly FieldInfo CreatedCharactersField =
            typeof(CharacterSpawnerRoot).GetField("createdCharacters", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool _mapActive;

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
            ScanCharacters();
        }

        private void EndTracking()
        {
            if (!_mapActive)
                return;

            _mapActive = false;
            Health.OnDead -= OnAnyHealthDead;
            ResetMarkers();
        }


        private void ResetMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                if (marker?.MarkerObject != null)
                {
                    Destroy(marker.MarkerObject);
                }
            }
            _markers.Clear();

            foreach (var obj in _ownedMarkerObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _ownedMarkerObjects.Clear();
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
            CharacterSpawnerRoot[] roots;
            try
            {
                roots = Resources.FindObjectsOfTypeAll<CharacterSpawnerRoot>();
            }
            catch
            {
                yield break;
            }

            if (roots == null)
                yield break;

            foreach (var root in roots)
            {
                if (root == null)
                    continue;

                List<CharacterMainControl> list = null;
                try
                {
                    list = CreatedCharactersField?.GetValue(root) as List<CharacterMainControl>;
                }
                catch { }

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

        private void AddOrUpdateMarker(CharacterMainControl character)
        {
            if (character == null)
                return;

            bool isBoss = IsBoss(character);
            if (!isBoss)
            {
                if (!ShowNearbyEnemies)
                    return;
                if (!character.gameObject.activeInHierarchy)
                    return;
            }

            if (_markers.TryGetValue(character, out var marker))
            {
                marker.Character = character;
                UpdateMarker(marker, isBoss);
                return;
            }

            var markerObject = new GameObject($"CharacterMarker:{GetCharacterName(character)}");
            markerObject.transform.position = character.transform.position;
            if (MultiSceneCore.MainScene.HasValue)
            {
                SceneManager.MoveGameObjectToScene(markerObject, MultiSceneCore.MainScene.Value);
            }

            var poi = markerObject.AddComponent<SimplePointOfInterest>();
            poi.Setup(GetMarkerIcon(isBoss), GetCharacterName(character), followActiveScene: true);
            poi.Color = GetMarkerColor();
            poi.ShadowColor = Color.black;
            poi.ShadowDistance = 0f;

            marker = new CharacterMarker
            {
                Character = character,
                MarkerObject = markerObject,
                Poi = poi,
            };

            _markers[character] = marker;
            _ownedMarkerObjects.Add(markerObject);
        }

        private void UpdateMarker(CharacterMarker marker, bool? isBossOverride = null)
        {
            if (marker?.MarkerObject == null || marker.Poi == null || marker.Character == null)
                return;

            marker.MarkerObject.name = $"CharacterMarker:{GetCharacterName(marker.Character)}";
            marker.MarkerObject.transform.position = marker.Character.transform.position;
            bool isBoss = isBossOverride ?? IsBoss(marker.Character);
            marker.Poi.Setup(GetMarkerIcon(isBoss), GetCharacterName(marker.Character), followActiveScene: true);
            marker.Poi.Color = GetMarkerColor();
            marker.Poi.ShadowColor = Color.black;
            marker.Poi.ShadowDistance = 0f;
        }

        private void Update()
        {
            if (ModConfig.HasPendingUpdate)
            {
                ModConfig.ApplyPendingChanges();
                ShowNearbyEnemies = ModConfig.ShowNearbyEnemies;
                if (_mapActive)
                {
                    ResetMarkers();
                    ScanCharacters();
                }
            }
        }

        private void LateUpdate()
        {
            if (!_mapActive || _markers.Count == 0)
                return;

            List<CharacterMainControl> stale = null;

            foreach (var kv in _markers)
            {
                var entry = kv.Value;
                if (!IsCharacterValid(entry?.Character))
                {
                    stale ??= new List<CharacterMainControl>();
                    stale.Add(kv.Key);
                    continue;
                }

                UpdateMarker(entry);
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

            if (character.Health == null || character.Health.IsDead)
                return false;

            return true;
        }

        private void DestroyMarker(CharacterMainControl character)
        {
            if (character == null)
                return;

            if (_markers.TryGetValue(character, out var entry))
            {
                _markers.Remove(character);
                if (entry.MarkerObject != null)
                {
                    _ownedMarkerObjects.Remove(entry.MarkerObject);
                    Destroy(entry.MarkerObject);
                }
            }
        }

        private void OnAnyHealthDead(Health health, DamageInfo info)
        {
            if (!_mapActive || health == null)
                return;

            try
            {
                var character = health.TryGetCharacter();
                DestroyMarker(character);
            }
            catch { }
        }

        private static string GetCharacterName(CharacterMainControl character)
        {
            try
            {
                var name = character?.characterPreset?.DisplayName;
                return string.IsNullOrEmpty(name) ? "Character" : name;
            }
            catch { return "Character"; }
        }

        private static Sprite GetMarkerIcon(bool isBoss)
        {
            try
            {
                var icons = MapMarkerManager.Icons;
                if (icons != null)
                {
                    var index = isBoss ? 3 : 2;
                    if (icons.Count > index && icons[index] != null)
                        return icons[index];
                    foreach (var icon in icons)
                    {
                        if (icon != null)
                            return icon;
                    }
                }
            }
            catch { }

            try { return MapMarkerManager.SelectedIcon; } catch { }
            return null;
        }

        private static Color GetMarkerColor() => Color.red;

        // Placeholder for future filtering (keeping for compatibility)
        private static bool IsBoss(CharacterMainControl c)
        {
            try
            {
                if (c == null)
                    return false;

                var preset = c.characterPreset;
                if (preset == null)
                    return false;

                var icon = preset.GetCharacterIcon();
                if (icon != null && icon.name != null && icon.name.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                var field = typeof(CharacterRandomPreset).GetField("characterIconType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                {
                    var value = field.GetValue(preset);
                    if (value is CharacterIconTypes iconType && iconType == CharacterIconTypes.boss)
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
