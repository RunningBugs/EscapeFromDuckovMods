using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner.UI;

public class NavEntry : MonoBehaviour
{
	public GameObject selectedIndicator;

	public Action<NavEntry> onInteract;

	public Action<NavEntry> onTrySelectThis;

	public NavGroup masterGroup;

	public VirtualCursorTarget VCT;

	public bool selectionState { get; private set; }

	private void Awake()
	{
		if (masterGroup == null)
		{
			masterGroup = GetComponentInParent<NavGroup>();
		}
		VCT = GetComponent<VirtualCursorTarget>();
		if ((bool)VCT)
		{
			VCT.onEnter.AddListener(TrySelectThis);
			VCT.onClick.AddListener(Interact);
		}
	}

	private void Interact()
	{
		NotifyInteract();
	}

	public void NotifySelectionState(bool value)
	{
		selectionState = value;
		selectedIndicator.SetActive(selectionState);
	}

	internal void NotifyInteract()
	{
		onInteract?.Invoke(this);
	}

	public void TrySelectThis()
	{
		if (!(masterGroup == null))
		{
			masterGroup.TrySelect(this);
		}
	}
}
