using System.Reflection;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GreenPlayerHealthMod
{
    /// <summary>
    /// Forces the main character health bar fill to use a solid green color.
    /// </summary>
    public sealed class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static readonly FieldInfo FillField =
            typeof(HealthBar).GetField("fill", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo GradientField =
            typeof(HealthBar).GetField("colorOverAmount", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo RefreshMethod =
            typeof(HealthBar).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly GradientColorKey[] GreenKeys =
        {
            new GradientColorKey(Color.green, 0f),
            new GradientColorKey(Color.green, 1f)
        };

        private static readonly GradientAlphaKey[] AlphaKeys =
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

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
            if (health == null || !health.IsMainCharacterHealth)
            {
                return;
            }

            // Give HealthBarManager a frame to spawn and configure the bar.
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            foreach (HealthBar bar in Object.FindObjectsOfType<HealthBar>())
            {
                if (bar.target != health)
                {
                    continue;
                }

                ForceGreen(bar);
                break;
            }
        }

        private static void ForceGreen(HealthBar bar)
        {
            if (GradientField?.GetValue(bar) is Gradient gradient)
            {
                gradient.colorKeys = GreenKeys;
                gradient.alphaKeys = AlphaKeys;
                GradientField.SetValue(bar, gradient);
            }

            if (FillField?.GetValue(bar) is Image fillImage)
            {
                fillImage.color = Color.green;
            }

            RefreshMethod?.Invoke(bar, null);
        }
    }
}
