using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class ItemModifierEntry : MonoBehaviour, IPoolable
{
	private ModifierDescription target;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI value;

	[SerializeField]
	private Color color_Neutral;

	[SerializeField]
	private Color color_Positive;

	[SerializeField]
	private Color color_Negative;

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
	}

	internal void Setup(ModifierDescription target)
	{
		UnregisterEvents();
		this.target = target;
		Refresh();
		RegisterEvents();
	}

	private void Refresh()
	{
		displayName.text = target.DisplayName;
		StatInfoDatabase.Entry entry = StatInfoDatabase.Get(target.Key);
		value.text = target.GetDisplayValueString(entry.DisplayFormat);
		Color color = color_Neutral;
		Polarity polarity = entry.polarity;
		if (target.Value != 0f)
		{
			switch (polarity)
			{
			case Polarity.Negative:
				color = ((target.Value < 0f) ? color_Positive : color_Negative);
				break;
			case Polarity.Positive:
				color = ((target.Value > 0f) ? color_Positive : color_Negative);
				break;
			}
		}
		value.color = color;
	}

	private void RegisterEvents()
	{
		_ = target;
	}

	private void UnregisterEvents()
	{
		_ = target;
	}
}
