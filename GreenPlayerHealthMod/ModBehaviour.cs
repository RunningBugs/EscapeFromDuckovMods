using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GreenPlayerHealthMod
{
    /// <summary>
    /// Colors the local health bar green.
    /// </summary>
    public sealed class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static readonly Color LocalPlayerColor = Color.green;

        private static Health _cachedLocalHealth;
        private static CharacterMainControl _cachedLocalCharacter;
        private static HealthBar _cachedLocalBar;

        private void OnEnable()
        {
            Health.OnRequestHealthBar += OnRequestHealthBar;
        }

        private void OnDisable()
        {
            Health.OnRequestHealthBar -= OnRequestHealthBar;
        }

        private async void OnRequestHealthBar(Health health)
        {
            if (!IsLocalHealth(health))
            {
                return;
            }

            HealthBar bar = _cachedLocalBar;
            if (bar == null || bar.target != health)
            {
                bar = await ResolveHealthBarAsync(health);
                if (bar == null)
                {
                    return;
                }

                _cachedLocalBar = bar;
            }

            HealthBarTint.Apply(bar, LocalPlayerColor);
        }

        private static void ResetLocalCache()
        {
            _cachedLocalHealth = null;
            _cachedLocalCharacter = null;
            _cachedLocalBar = null;
        }

        private static async UniTask<HealthBar> ResolveHealthBarAsync(Health health)
        {
            if (health == null)
            {
                return null;
            }

            HealthBar bar = FindHealthBar(health);
            if (bar != null)
            {
                return bar;
            }

            for (int attempt = 0; attempt < 4; attempt++)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                bar = FindHealthBar(health);
                if (bar != null)
                {
                    return bar;
                }
            }

            return null;
        }

        private static HealthBar FindHealthBar(Health health)
        {
            HealthBarManager manager = HealthBarManager.Instance;
            if (manager == null)
            {
                return null;
            }

            HealthBar[] bars = manager.GetComponentsInChildren<HealthBar>(true);
            for (int i = 0; i < bars.Length; i++)
            {
                HealthBar candidate = bars[i];
                if (candidate != null && candidate.target == health)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsLocalHealth(Health health)
        {
            if (health == null)
            {
                return false;
            }

            if (_cachedLocalHealth == health)
            {
                if (_cachedLocalCharacter != null && _cachedLocalCharacter != CharacterMainControl.Main)
                {
                    ResetLocalCache();
                    return false;
                }

                return true;
            }

            if (!health.IsMainCharacterHealth)
            {
                return false;
            }

            CharacterMainControl character = TryResolveCharacter(health);
            if (character != null && character != CharacterMainControl.Main)
            {
                return false;
            }

            _cachedLocalHealth = health;
            _cachedLocalCharacter = character;
            _cachedLocalBar = null;
            return true;
        }

        private static CharacterMainControl TryResolveCharacter(Health health)
        {
            if (health == null)
            {
                return null;
            }

            try
            {
                return health.TryGetCharacter();
            }
            catch
            {
                return null;
            }
        }

        private sealed class HealthBarTint : MonoBehaviour
        {
            private readonly List<Image> _targets = new List<Image>();
            private Color _color;
            private bool _dirty = true;

            public static void Apply(HealthBar bar, Color color)
            {
                if (bar == null)
                {
                    return;
                }

                HealthBarTint tint = bar.GetComponent<HealthBarTint>();
                if (tint == null)
                {
                    tint = bar.gameObject.AddComponent<HealthBarTint>();
                }

                tint.SetColor(color);
            }

            private void OnEnable()
            {
                _dirty = true;
            }

            private void OnDisable()
            {
                _targets.Clear();
                _dirty = true;
            }

            private void OnTransformChildrenChanged()
            {
                _dirty = true;
            }

            private void LateUpdate()
            {
                if (_dirty)
                {
                    RefreshTargets();
                }

                ApplyColor();
            }

            private void SetColor(Color color)
            {
                _color = color;
                _dirty = true;
            }

            private void RefreshTargets()
            {
                _targets.Clear();
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (IsFillCandidate(image))
                    {
                        _targets.Add(image);
                    }
                }

                _dirty = false;
            }

            private void ApplyColor()
            {
                if (_targets.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < _targets.Count; i++)
                {
                    Image image = _targets[i];
                    if (image == null)
                    {
                        _dirty = true;
                        continue;
                    }

                    Color current = image.color;
                    Color next = new Color(_color.r, _color.g, _color.b, current.a);
                    if (!Approximately(current, next))
                    {
                        image.color = next;
                    }
                }
            }

            private static bool IsFillCandidate(Image image)
            {
                if (image == null)
                {
                    return false;
                }

                string name = image.name;
                if (!string.IsNullOrEmpty(name) &&
                    name.IndexOf("fill", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                return image.type == Image.Type.Filled;
            }

            private static bool Approximately(Color lhs, Color rhs)
            {
                return Mathf.Approximately(lhs.r, rhs.r) &&
                       Mathf.Approximately(lhs.g, rhs.g) &&
                       Mathf.Approximately(lhs.b, rhs.b) &&
                       Mathf.Approximately(lhs.a, rhs.a);
            }
        }
    }
}
