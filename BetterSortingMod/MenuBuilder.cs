using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSortingMod
{
    /// <summary>
    /// MenuBuilder creates option buttons from a stripped template and a set of descriptors.
    /// - Assumes the template is already stripped of game-specific localizers / animations.
    /// - Ensures buttons are active and participate in layout (no collapse).
    /// - Wires localization via LocalizedLabelController + ModLocalization.
    /// </summary>
    internal static class MenuBuilder
    {
        /// <summary>
        /// Descriptor for a menu option.
        /// </summary>
        internal sealed class SortOption
        {
            public readonly string Key;
            public readonly IReadOnlyDictionary<SystemLanguage, string> Fallback;
            public readonly Action OnClick;

            public SortOption(string key, IReadOnlyDictionary<SystemLanguage, string> fallback, Action onClick)
            {
                Key = key ?? string.Empty;
                Fallback = fallback;
                OnClick = onClick;
            }
        }

        /// <summary>
        /// Build option buttons under parent from a stripped template.
        /// </summary>
        /// <param name="parent">Menu parent (VerticalLayoutGroup + ContentSizeFitter recommended)</param>
        /// <param name="template">Pre-stripped source button</param>
        /// <param name="options">Descriptors list</param>
        /// <param name="minWidth">Minimum option width</param>
        /// <param name="minHeight">
        /// Minimum option height; if null, will use template height or 52 as fallback
        /// </param>
        /// <returns>List of created option buttons</returns>
        internal static List<Button> BuildOptions(
            Transform parent,
            Button template,
            IEnumerable<SortOption> options,
            float minWidth = 220f,
            float? minHeight = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var created = new List<Button>(16);
            float baseH = ComputeBaseHeight(template, minHeight);

            foreach (var opt in options)
            {
                if (opt == null) continue;

                Button btn = UnityEngine.Object.Instantiate(template, parent);
                btn.gameObject.name = $"BetterSortOption_{(string.IsNullOrEmpty(opt.Key) ? "option" : opt.Key)}";

                // Ensure visible & interactive
                EnsureActiveHierarchy(btn.transform);
                btn.onClick.RemoveAllListeners();
                if (opt.OnClick != null)
                {
                    btn.onClick.AddListener(() => opt.OnClick());
                }

                // Layout
                var le = RequireLayoutElement(btn.gameObject);
                le.flexibleWidth = 0f;
                le.minWidth = Mathf.Max(le.minWidth, minWidth);
                le.preferredWidth = Mathf.Max(le.preferredWidth, minWidth);
                le.minHeight = Mathf.Max(le.minHeight, baseH);
                le.preferredHeight = Mathf.Max(le.preferredHeight, baseH);
                le.flexibleHeight = 0f;
                le.ignoreLayout = false;

                // Localization
                SetLocalizedLabel(btn, opt.Key, opt.Fallback);

                created.Add(btn);
            }

            return created;
        }

        private static float ComputeBaseHeight(Button template, float? minHeight)
        {
            if (minHeight.HasValue) return Mathf.Max(1f, minHeight.Value);
            var rect = template.GetComponent<RectTransform>();
            var h = (rect != null && rect.rect.height > 0f) ? rect.rect.height : 52f;
            return Mathf.Max(1f, h);
        }

        private static LayoutElement RequireLayoutElement(GameObject go)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            return le;
        }

        /// <summary>
        /// Ensure button hierarchy is active to avoid layout collapse in nested canvases / special views.
        /// </summary>
        private static void EnsureActiveHierarchy(Transform t)
        {
            if (t == null) return;
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            for (int i = 0; i < t.childCount; i++)
            {
                var ch = t.GetChild(i);
                if (ch != null && !ch.gameObject.activeSelf) ch.gameObject.SetActive(true);
            }
        }

        private static void SetLocalizedLabel(Button button, string key, IReadOnlyDictionary<SystemLanguage, string> fallback)
        {
            if (button == null) return;

            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            if (tmp == null) return;

            // Immediate fallback so label is visible even if localization events fire later
            string text = ModLocalization.GetText(key, Application.systemLanguage, fallback);
            if (string.IsNullOrEmpty(text))
            {
                text = ModLocalization.GetText(key, SystemLanguage.English, fallback);
            }
            if (!string.IsNullOrEmpty(text))
            {
                tmp.text = text;
            }
            tmp.raycastTarget = false;
            var c = tmp.color; c.a = 1f; tmp.color = c;

            var controller = tmp.GetComponent<LocalizedLabelController>();
            if (controller == null) controller = tmp.gameObject.AddComponent<LocalizedLabelController>();
            controller.Configure(key, fallback);
        }
    }
}
