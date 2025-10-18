using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class GameplayUIManager : MonoBehaviour
{
	private static GameplayUIManager instance;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private List<View> views = new List<View>();

	[SerializeField]
	private List<GameObject> setActiveOnAwake;

	private Dictionary<Type, View> viewDic = new Dictionary<Type, View>();

	private PrefabPool<ItemDisplay> itemDisplayPool;

	private PrefabPool<SlotDisplay> slotDisplayPool;

	private PrefabPool<InventoryEntry> inventoryEntryPool;

	[SerializeField]
	private SplitDialogue _splitDialogue;

	public static GameplayUIManager Instance => instance;

	public View ActiveView => View.ActiveView;

	public PrefabPool<ItemDisplay> ItemDisplayPool
	{
		get
		{
			if (itemDisplayPool == null)
			{
				itemDisplayPool = new PrefabPool<ItemDisplay>(GameplayDataSettings.UIPrefabs.ItemDisplay, base.transform);
			}
			return itemDisplayPool;
		}
	}

	public PrefabPool<SlotDisplay> SlotDisplayPool
	{
		get
		{
			if (slotDisplayPool == null)
			{
				slotDisplayPool = new PrefabPool<SlotDisplay>(GameplayDataSettings.UIPrefabs.SlotDisplay, base.transform);
			}
			return slotDisplayPool;
		}
	}

	public PrefabPool<InventoryEntry> InventoryEntryPool
	{
		get
		{
			if (inventoryEntryPool == null)
			{
				inventoryEntryPool = new PrefabPool<InventoryEntry>(GameplayDataSettings.UIPrefabs.InventoryEntry, base.transform);
			}
			return inventoryEntryPool;
		}
	}

	public SplitDialogue SplitDialogue => _splitDialogue;

	public static T GetViewInstance<T>() where T : View
	{
		if (Instance == null)
		{
			return null;
		}
		if (Instance.viewDic.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		View view = Instance.views.Find((View e) => e is T);
		if (view == null)
		{
			return null;
		}
		Instance.viewDic[typeof(T)] = view;
		return view as T;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogWarning("Duplicate Gameplay UI Manager detected!");
		}
		foreach (View view in views)
		{
			view.gameObject.SetActive(value: true);
		}
		foreach (GameObject item in setActiveOnAwake)
		{
			if (!(item == null))
			{
				item.gameObject.SetActive(value: true);
			}
		}
	}

	public static UniTask TemporaryHide()
	{
		if (Instance == null)
		{
			return UniTask.CompletedTask;
		}
		Instance.canvasGroup.blocksRaycasts = false;
		return Instance.fadeGroup.HideAndReturnTask();
	}

	public static UniTask ReverseTemporaryHide()
	{
		if (Instance == null)
		{
			return UniTask.CompletedTask;
		}
		Instance.canvasGroup.blocksRaycasts = true;
		return Instance.fadeGroup.ShowAndReturnTask();
	}
}
