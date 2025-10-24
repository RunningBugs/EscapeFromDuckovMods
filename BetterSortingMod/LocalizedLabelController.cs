using System.Collections.Generic;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace BetterSortingMod;

internal class LocalizedLabelController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI target;

    private readonly Dictionary<SystemLanguage, string> fallback = new Dictionary<SystemLanguage, string>();

    private string key;

    private bool subscribed;

    internal void Configure(string key, IReadOnlyDictionary<SystemLanguage, string> defaults)
    {
        this.key = key;
        fallback.Clear();
        if (defaults != null)
        {
            foreach (KeyValuePair<SystemLanguage, string> item in defaults)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    fallback[item.Key] = item.Value;
                }
            }
        }
        EnsureTarget();
        Subscribe();
        Apply(LocalizationManager.Initialized ? LocalizationManager.CurrentLanguage : SystemLanguage.English);
    }

    private void OnEnable()
    {
        EnsureTarget();
        Apply(LocalizationManager.Initialized ? LocalizationManager.CurrentLanguage : SystemLanguage.English);
    }

    private void OnDestroy()
    {
        if (subscribed)
        {
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;
            subscribed = false;
        }
    }

    private void OnLanguageChanged(SystemLanguage language)
    {
        Apply(language);
    }

    private void Apply(SystemLanguage language)
    {
        if (target == null)
        {
            return;
        }
        target.text = ModLocalization.GetText(key, language, fallback);
    }

    private void EnsureTarget()
    {
        if (target == null)
        {
            target = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Subscribe()
    {
        if (!subscribed)
        {
            LocalizationManager.OnSetLanguage += OnLanguageChanged;
            subscribed = true;
        }
    }
}
