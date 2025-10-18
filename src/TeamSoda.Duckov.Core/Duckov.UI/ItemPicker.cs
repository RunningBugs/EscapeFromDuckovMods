using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemPicker : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private ItemPickerEntry entryPrefab;

	[SerializeField]
	private Transform contentParent;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	private PrefabPool<ItemPickerEntry> _entryPool;

	private bool picking;

	private bool canceled;

	private bool confirmed;

	private Item pickedItem;

	public static ItemPicker Instance { get; private set; }

	private PrefabPool<ItemPickerEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<ItemPickerEntry>(entryPrefab, contentParent ? contentParent : base.transform, OnGetEntry);
			}
			return _entryPool;
		}
	}

	public bool Picking => picking;

	private void OnGetEntry(ItemPickerEntry entry)
	{
	}

	private async UniTask<Item> WaitForUserPick(ICollection<Item> candidates)
	{
		if (picking)
		{
			Debug.LogError("选择UI已被占用");
			return null;
		}
		picking = true;
		confirmed = false;
		canceled = false;
		pickedItem = null;
		base.gameObject.SetActive(value: true);
		fadeGroup.gameObject.SetActive(value: true);
		SetupUI(candidates);
		RectTransform obj = base.transform as RectTransform;
		obj.ForceUpdateRectTransforms();
		LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
		await UniTask.NextFrame();
		fadeGroup.Show();
		do
		{
			await UniTask.NextFrame();
		}
		while (!confirmed && !canceled);
		picking = false;
		fadeGroup.Hide();
		if (confirmed)
		{
			if (!candidates.Contains(pickedItem))
			{
				Debug.LogError("选出了意料之外的物品。");
			}
			return pickedItem;
		}
		_ = canceled;
		return null;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("场景中存在两个ItemPicker，请检查。");
		}
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(OnCancelButtonClicked);
	}

	private void OnCancelButtonClicked()
	{
		Cancel();
	}

	private void OnConfirmButtonClicked()
	{
		ConfirmPick(pickedItem);
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (!picking && fadeGroup.IsShown)
		{
			fadeGroup.Hide();
		}
	}

	public static async UniTask<Item> Pick(ICollection<Item> candidates)
	{
		if (Instance == null)
		{
			return null;
		}
		return await Instance.WaitForUserPick(candidates);
	}

	public void ConfirmPick(Item item)
	{
		confirmed = true;
		pickedItem = item;
	}

	public void Cancel()
	{
		canceled = true;
	}

	private void SetupUI(ICollection<Item> candidates)
	{
		EntryPool.ReleaseAll();
		foreach (Item candidate in candidates)
		{
			if (!(candidate == null))
			{
				ItemPickerEntry itemPickerEntry = EntryPool.Get();
				itemPickerEntry.Setup(this, candidate);
				itemPickerEntry.transform.SetAsLastSibling();
			}
		}
	}

	internal void NotifyEntryClicked(ItemPickerEntry itemPickerEntry, Item target)
	{
		pickedItem = target;
	}
}
