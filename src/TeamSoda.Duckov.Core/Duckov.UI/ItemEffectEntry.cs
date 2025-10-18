using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class ItemEffectEntry : MonoBehaviour, IPoolable
{
	private Effect target;

	[SerializeField]
	private TextMeshProUGUI text;

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
		target = null;
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	public void Setup(Effect target)
	{
		this.target = target;
		Refresh();
		RegisterEvents();
	}

	private void Refresh()
	{
		text.text = target.GetDisplayString();
	}

	private void RegisterEvents()
	{
	}

	private void UnregisterEvents()
	{
	}
}
