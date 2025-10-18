using System;
using System.Reflection;
using System.Collections;
using Duckov.Modding;
using Duckov.Weathers;
using Saves;
using UnityEngine;
using Duckov.UI;

namespace RainNowMod
{
    // info.ini name => namespace => ModBehaviour class
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string SaveKeyActive = "RainNowMod_Active";
        private const string SaveKeyEndTicks = "RainNowMod_EndTicks";
        private const string SaveKeyOrig = "RainNowMod_Orig"; // pack originals as string

        private bool _overrideActive;
        private long _overrideEndTicks;
        private bool _overrideApplied;

        private float _origRainyThreshold;
        private float _origCloudyThreshold;
        private float _origOffset;
        private float _origContrast;

        private const float NotificationDuration = 2.8f;
        private const float NotificationDurationIfPending = 1.6f;

        private static readonly BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        void Awake()
        {
            Debug.Log("RainNowMod loaded (F8: rain 12h, F9: cancel)");
        }

        void OnEnable()
        {
            LoadState();
            ReapplyOverrideIfNeeded();
            LevelManager.OnLevelInitialized += OnLevelInitialized;
        }

        void OnDisable()
        {
            LevelManager.OnLevelInitialized -= OnLevelInitialized;
            // Restore on disable to avoid leaving modified values behind
            if (_overrideActive)
            {
                RestoreOriginalThresholds();
                _overrideActive = false;
                _overrideApplied = false;
                SaveState();
            }
        }

        private void OnLevelInitialized()
        {
            ReapplyOverrideIfNeeded();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                BeginRainOverride(hours: 12);
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                EndRainOverride();
            }

            if (_overrideActive)
            {
                var now = GameClock.Now;
                if (now.Ticks >= _overrideEndTicks)
                {
                    EndRainOverride();
                }
            }
        }

        private void BeginRainOverride(int hours)
        {
            var now = GameClock.Now;
            _overrideEndTicks = (now + TimeSpan.FromHours(hours)).Ticks;

            // If not applied yet, capture originals and apply rainy threshold override
            if (!ApplyRainyThresholdOverride())
            {
                Debug.LogWarning("[RainNowMod] Failed to apply rainy threshold override (WeatherManager/Precipitation not found)");
                return;
            }
            _overrideActive = true;
            SaveState();
            ShowNotificationSafe($"Rain Now active for {hours}h. Ends at {(new DateTime(_overrideEndTicks)).ToLongTimeString()}");
        }

        private void EndRainOverride()
        {
            if (RestoreOriginalThresholds())
            {
                _overrideActive = false;
                _overrideApplied = false;
                SaveState();
                ShowNotificationSafe("Rain Now cancelled");
            }
        }

        private void ReapplyOverrideIfNeeded()
        {
            if (_overrideActive && GameClock.Now.Ticks < _overrideEndTicks)
            {
                if (!_overrideApplied)
                {
                    // Apply with stored originals
                    if (!ApplyRainyThresholdOverride(useStoredOriginals: true))
                    {
                        Debug.LogWarning("[RainNowMod] Reapply failed");
                    }
                }
            }
            else if (_overrideActive)
            {
                // expired
                EndRainOverride();
            }
        }

        private bool ApplyRainyThresholdOverride(bool useStoredOriginals = false)
        {
            var wm = WeatherManager.Instance;
            if (wm == null) return false;
            var precField = typeof(WeatherManager).GetField("precipitation", BF);
            if (precField == null) return false;
            var prec = precField.GetValue(wm);
            if (prec == null) return false;

            var t = prec.GetType();
            var fRainy = t.GetField("rainyThreshold", BF);
            var fCloudy = t.GetField("cloudyThreshold", BF);
            var fOffset = t.GetField("offset", BF);
            var fContrast = t.GetField("contrast", BF);
            if (fRainy == null || fCloudy == null || fOffset == null || fContrast == null) return false;

            if (!useStoredOriginals || !_overrideApplied)
            {
                _origRainyThreshold = (float)fRainy.GetValue(prec);
                _origCloudyThreshold = (float)fCloudy.GetValue(prec);
                _origOffset = (float)fOffset.GetValue(prec);
                _origContrast = (float)fContrast.GetValue(prec);
            }

            // Force rainy: set thresholds extremely low so Precipitation > rainyThreshold is always true
            // Keep storms intact (WeatherManager still returns Storm when active)
            float newRainy = -0.1f;   // anything >= 0 will be > -0.1
            float newCloudy = -0.2f;  // ensure order: cloudy < rainy

            fRainy.SetValue(prec, newRainy);
            fCloudy.SetValue(prec, newCloudy);
            // Do not change offset/contrast for this simpler mode

            _overrideApplied = true;
            return true;
        }

        private bool RestoreOriginalThresholds()
        {
            var wm = WeatherManager.Instance;
            if (wm == null) return false;
            var precField = typeof(WeatherManager).GetField("precipitation", BF);
            if (precField == null) return false;
            var prec = precField.GetValue(wm);
            if (prec == null) return false;

            var t = prec.GetType();
            var fRainy = t.GetField("rainyThreshold", BF);
            var fCloudy = t.GetField("cloudyThreshold", BF);
            var fOffset = t.GetField("offset", BF);
            var fContrast = t.GetField("contrast", BF);
            if (fRainy == null || fCloudy == null || fOffset == null || fContrast == null) return false;

            fRainy.SetValue(prec, _origRainyThreshold);
            fCloudy.SetValue(prec, _origCloudyThreshold);
            fOffset.SetValue(prec, _origOffset);
            fContrast.SetValue(prec, _origContrast);
            return true;
        }

        private void SaveState()
        {
            try
            {
                SavesSystem.SaveGlobal(SaveKeyActive, _overrideActive);
                SavesSystem.SaveGlobal(SaveKeyEndTicks, _overrideEndTicks);
                // Pack originals
                var packed = $"{_origRainyThreshold}|{_origCloudyThreshold}|{_origOffset}|{_origContrast}";
                SavesSystem.SaveGlobal(SaveKeyOrig, packed);
            }
            catch { }
        }

        private void LoadState()
        {
            try
            {
                _overrideActive = SavesSystem.LoadGlobal(SaveKeyActive, defaultValue: false);
            }
            catch { _overrideActive = false; }
            try
            {
                _overrideEndTicks = SavesSystem.LoadGlobal<long>(SaveKeyEndTicks);
            }
            catch { _overrideEndTicks = 0; }
            try
            {
                string packed = SavesSystem.LoadGlobal<string>(SaveKeyOrig);
                if (!string.IsNullOrEmpty(packed))
                {
                    var parts = packed.Split('|');
                    if (parts.Length == 4)
                    {
                        float.TryParse(parts[0], out _origRainyThreshold);
                        float.TryParse(parts[1], out _origCloudyThreshold);
                        float.TryParse(parts[2], out _origOffset);
                        float.TryParse(parts[3], out _origContrast);
                    }
                }
            }
            catch { }
        }

        private void ShowNotificationSafe(string message)
        {
            try
            {
                // Try to extend visibility by adjusting NotificationText durations temporarily
                var all = Resources.FindObjectsOfTypeAll(typeof(NotificationText));
                NotificationText instance = null;
                foreach (var o in all)
                {
                    if (o is NotificationText nt)
                    {
                        instance = nt;
                        break;
                    }
                }
                if (instance != null)
                {
                    var t = typeof(NotificationText);
                    var fDur = t.GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic);
                    var fDurPend = t.GetField("durationIfPending", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fDur != null && fDurPend != null)
                    {
                        float oldDur = (float)fDur.GetValue(instance);
                        float oldPend = (float)fDurPend.GetValue(instance);
                        fDur.SetValue(instance, NotificationDuration);
                        fDurPend.SetValue(instance, NotificationDurationIfPending);
                        NotificationText.Push(message);
                        // Restore after max duration
                        StartCoroutine(ResetNotificationDurations(instance, oldDur, oldPend, Mathf.Max(NotificationDuration, NotificationDurationIfPending) + 0.1f));
                        return;
                    }
                }
            }
            catch { }
            // Fallback
            try { NotificationText.Push(message); } catch { }
        }

        private IEnumerator ResetNotificationDurations(NotificationText instance, float oldDur, float oldPend, float waitSeconds)
        {
            yield return new WaitForSecondsRealtime(waitSeconds);
            try
            {
                var t = typeof(NotificationText);
                var fDur = t.GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic);
                var fDurPend = t.GetField("durationIfPending", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fDur != null) fDur.SetValue(instance, oldDur);
                if (fDurPend != null) fDurPend.SetValue(instance, oldPend);
            }
            catch { }
        }

        
    }
}
