using Duckov.Utilities;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class ItemVariableEntry : MonoBehaviour, IPoolable
{
	private CustomData target;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI value;

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

	internal void Setup(CustomData target)
	{
		UnregisterEvents();
		this.target = target;
		Refresh();
		RegisterEvents();
	}

	private void Refresh()
	{
		displayName.text = target.DisplayName;
		value.text = target.GetValueDisplayString();
	}

	private void RegisterEvents()
	{
		if (target != null)
		{
			target.OnSetData += OnTargetSetData;
		}
	}

	private void UnregisterEvents()
	{
		if (target != null)
		{
			target.OnSetData -= OnTargetSetData;
		}
	}

	private void OnTargetSetData(CustomData data)
	{
		Refresh();
	}
}
