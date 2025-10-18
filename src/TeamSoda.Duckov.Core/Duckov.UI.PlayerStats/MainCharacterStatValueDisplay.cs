using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI.PlayerStats;

public class MainCharacterStatValueDisplay : MonoBehaviour
{
	[SerializeField]
	private string statKey;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI valueText;

	[SerializeField]
	private string format = "{0:0.0}";

	private Stat target;

	private void OnEnable()
	{
		if (target == null)
		{
			target = CharacterMainControl.Main?.CharacterItem?.GetStat(statKey.GetHashCode());
		}
		Refresh();
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void AutoRename()
	{
		base.gameObject.name = "StatDisplay_" + statKey;
	}

	private void RegisterEvents()
	{
		if (target != null)
		{
			target.OnSetDirty += OnTargetDirty;
		}
	}

	private void UnregisterEvents()
	{
		if (target != null)
		{
			target.OnSetDirty -= OnTargetDirty;
		}
	}

	private void OnTargetDirty(Stat stat)
	{
		Refresh();
	}

	private void Refresh()
	{
		if (target != null)
		{
			displayNameText.text = target.DisplayName;
			float value = target.Value;
			valueText.text = string.Format(format, value);
		}
	}
}
