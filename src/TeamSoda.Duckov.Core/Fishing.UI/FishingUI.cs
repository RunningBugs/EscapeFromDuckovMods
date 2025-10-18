using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;

namespace Fishing.UI;

public class FishingUI : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private BaitSelectPanel baitSelectPanel;

	[SerializeField]
	private ConfirmPanel confirmPanel;

	protected override void Awake()
	{
		base.Awake();
		Action_Fishing.OnPlayerStartSelectBait += OnStartSelectBait;
		Action_Fishing.OnPlayerStopCatching += OnStopCatching;
		Action_Fishing.OnPlayerStopFishing += OnStopFishing;
	}

	protected override void OnDestroy()
	{
		Action_Fishing.OnPlayerStopFishing -= OnStopFishing;
		Action_Fishing.OnPlayerStartSelectBait -= OnStartSelectBait;
		Action_Fishing.OnPlayerStopCatching -= OnStopCatching;
		base.OnDestroy();
	}

	internal override void TryQuit()
	{
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		Debug.Log("Open Fishing Panel");
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void OnStopFishing(Action_Fishing fishing)
	{
		baitSelectPanel.NotifyStop();
		confirmPanel.NotifyStop();
		Close();
	}

	private void OnStartSelectBait(Action_Fishing fishing, ICollection<Item> availableBaits, Func<Item, bool> baitSelectionResultCallback)
	{
		SelectBaitTask(availableBaits, baitSelectionResultCallback).Forget();
	}

	private async UniTask SelectBaitTask(ICollection<Item> availableBaits, Func<Item, bool> baitSelectionResultCallback)
	{
		Open();
		await baitSelectPanel.DoBaitSelection(availableBaits, baitSelectionResultCallback);
		Close();
	}

	private void OnStopCatching(Action_Fishing fishing, Item catchedItem, Action<bool> confirmCallback)
	{
		ConfirmTask(catchedItem, confirmCallback).Forget();
	}

	private async UniTask ConfirmTask(Item catchedItem, Action<bool> confirmCallback)
	{
		Open();
		await confirmPanel.DoConfirmDialogue(catchedItem, confirmCallback);
		Close();
	}
}
