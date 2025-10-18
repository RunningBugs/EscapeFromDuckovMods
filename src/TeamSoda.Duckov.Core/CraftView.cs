using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;
using UnityEngine.UI;

public class CraftView : View, ISingleSelectionMenu<CraftView_ListEntry>
{
	[Serializable]
	public struct FilterInfo
	{
		[LocalizationKey("Default")]
		[SerializeField]
		public string displayNameKey;

		[SerializeField]
		public Sprite icon;

		[SerializeField]
		public Tag[] requireTags;
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private CraftView_ListEntry listEntryTemplate;

	private PrefabPool<CraftView_ListEntry> _listEntryPool;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private FadeGroup loadingIndicator;

	[SerializeField]
	private FadeGroup placeHolderFadeGroup;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private Color crafableColor;

	[SerializeField]
	private Color notCraftableColor;

	[SerializeField]
	private Image buttonImage;

	[SerializeField]
	private Button craftButton;

	[LocalizationKey("Default")]
	[SerializeField]
	private string notificationFormatKey;

	[SerializeField]
	private CraftViewFilterBtnEntry filterBtnTemplate;

	[SerializeField]
	private FilterInfo[] filters;

	private PrefabPool<CraftViewFilterBtnEntry> _filterBtnPool;

	private int currentFilterIndex;

	private bool crafting;

	private Predicate<CraftingFormula> predicate;

	private CraftView_ListEntry selectedEntry;

	private int refreshTaskToken;

	private Item tempItem;

	private static CraftView Instance => View.GetViewInstance<CraftView>();

	private PrefabPool<CraftView_ListEntry> ListEntryPool
	{
		get
		{
			if (_listEntryPool == null)
			{
				_listEntryPool = new PrefabPool<CraftView_ListEntry>(listEntryTemplate);
			}
			return _listEntryPool;
		}
	}

	private string NotificationFormat => notificationFormatKey.ToPlainText();

	private PrefabPool<CraftViewFilterBtnEntry> FilterBtnPool
	{
		get
		{
			if (_filterBtnPool == null)
			{
				_filterBtnPool = new PrefabPool<CraftViewFilterBtnEntry>(filterBtnTemplate);
			}
			return _filterBtnPool;
		}
	}

	private FilterInfo CurrentFilter
	{
		get
		{
			if (currentFilterIndex < 0 || currentFilterIndex >= filters.Length)
			{
				currentFilterIndex = 0;
			}
			return filters[currentFilterIndex];
		}
	}

	public void SetFilter(int index)
	{
		if (index >= 0 && index < filters.Length)
		{
			currentFilterIndex = index;
			selectedEntry = null;
			RefreshDetails();
			RefreshList(predicate);
			RefreshFilterButtons();
		}
	}

	private static bool CheckFilter(CraftingFormula formula, FilterInfo filter)
	{
		if (filter.requireTags.Length == 0)
		{
			return true;
		}
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(formula.result.id);
		Tag[] requireTags = filter.requireTags;
		foreach (Tag value in requireTags)
		{
			if (metaData.tags.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		listEntryTemplate.gameObject.SetActive(value: false);
		craftButton.onClick.AddListener(OnCraftButtonClicked);
	}

	private void OnCraftButtonClicked()
	{
		CraftTask().Forget();
	}

	private async UniTask CraftTask()
	{
		if (!crafting && !(selectedEntry == null) && !(CraftingManager.Instance == null))
		{
			crafting = true;
			List<Item> list = await CraftingManager.Instance.Craft(selectedEntry.Formula.id);
			if (list != null)
			{
				foreach (Item item in list)
				{
					OnCraftFinished(item);
				}
			}
		}
		crafting = false;
	}

	private void OnCraftFinished(Item item)
	{
		if (!(item == null))
		{
			string displayName = item.DisplayName;
			NotificationText.Push(NotificationFormat.Format(new
			{
				itemDisplayName = displayName
			}));
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		SetFilter(0);
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	public static void SetupAndOpenView(Predicate<CraftingFormula> predicate)
	{
		if ((bool)Instance)
		{
			Instance.SetupAndOpen(predicate);
		}
	}

	public void SetupAndOpen(Predicate<CraftingFormula> predicate)
	{
		this.predicate = predicate;
		detailsFadeGroup.SkipHide();
		loadingIndicator.SkipHide();
		placeHolderFadeGroup.SkipShow();
		selectedEntry = null;
		RefreshDetails();
		RefreshList(predicate);
		RefreshFilterButtons();
		Open();
	}

	private void RefreshList(Predicate<CraftingFormula> predicate)
	{
		ListEntryPool.ReleaseAll();
		IEnumerable<string> unlockedFormulaIDs = CraftingManager.UnlockedFormulaIDs;
		FilterInfo currentFilter = CurrentFilter;
		bool flag = currentFilter.requireTags != null && currentFilter.requireTags.Length != 0;
		foreach (string item in unlockedFormulaIDs)
		{
			if (CraftingFormulaCollection.TryGetFormula(item, out var formula) && predicate(formula) && (!flag || CheckFilter(formula, currentFilter)))
			{
				ListEntryPool.Get().Setup(this, formula);
			}
		}
	}

	private int CountFilter(FilterInfo filter)
	{
		IEnumerable<string> unlockedFormulaIDs = CraftingManager.UnlockedFormulaIDs;
		bool flag = filter.requireTags != null && filter.requireTags.Length != 0;
		int num = 0;
		foreach (string item in unlockedFormulaIDs)
		{
			if (CraftingFormulaCollection.TryGetFormula(item, out var formula) && predicate(formula) && (!flag || CheckFilter(formula, filter)))
			{
				num++;
			}
		}
		return num;
	}

	private void RefreshFilterButtons()
	{
		FilterBtnPool.ReleaseAll();
		int num = 0;
		FilterInfo[] array = filters;
		foreach (FilterInfo filterInfo in array)
		{
			if (CountFilter(filterInfo) < 1)
			{
				num++;
				continue;
			}
			FilterBtnPool.Get().Setup(this, filterInfo, num, num == currentFilterIndex);
			num++;
		}
	}

	public CraftView_ListEntry GetSelection()
	{
		return selectedEntry;
	}

	public bool SetSelection(CraftView_ListEntry selection)
	{
		if (selectedEntry != null)
		{
			CraftView_ListEntry craftView_ListEntry = selectedEntry;
			selectedEntry = null;
			craftView_ListEntry.NotifyUnselected();
		}
		selectedEntry = selection;
		selectedEntry.NotifySelected();
		RefreshDetails();
		return true;
	}

	private void RefreshDetails()
	{
		RefreshTask(NewRefreshToken()).Forget();
	}

	private int NewRefreshToken()
	{
		int num;
		do
		{
			num = UnityEngine.Random.Range(0, int.MaxValue);
		}
		while (num == refreshTaskToken);
		refreshTaskToken = num;
		return num;
	}

	private async UniTask RefreshTask(int token)
	{
		if (selectedEntry == null)
		{
			detailsFadeGroup.Hide();
			loadingIndicator.Hide();
			placeHolderFadeGroup.Show();
			return;
		}
		detailsFadeGroup.Hide();
		placeHolderFadeGroup.Hide();
		loadingIndicator.Show();
		if (tempItem != null)
		{
			UnityEngine.Object.Destroy(tempItem);
		}
		CraftingFormula formula = selectedEntry.Formula;
		int itemID = formula.result.id;
		tempItem = await ItemAssetsCollection.InstantiateAsync(itemID);
		if (token != refreshTaskToken)
		{
			return;
		}
		if (tempItem == null)
		{
			Debug.LogError($"Failed to create item of id {itemID}");
			return;
		}
		if (tempItem.Stackable)
		{
			tempItem.StackCount = formula.result.amount;
		}
		buttonImage.color = (selectedEntry.Formula.cost.Enough ? crafableColor : notCraftableColor);
		detailsDisplay.Setup(tempItem);
		costDisplay.Setup(formula.cost);
		detailsFadeGroup.Show();
		loadingIndicator.Hide();
	}

	private void TestShow()
	{
		CraftingManager.UnlockFormula("Biscuit");
		CraftingManager.UnlockFormula("Character");
		SetupAndOpen((CraftingFormula e) => true);
	}
}
