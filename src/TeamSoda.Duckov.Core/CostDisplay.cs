using Duckov.Economy;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostDisplay : MonoBehaviour
{
	[SerializeField]
	private GameObject moneyContainer;

	[SerializeField]
	private GameObject itemsContainer;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image moneyBackground;

	[SerializeField]
	private TextMeshProUGUI money;

	[SerializeField]
	private ItemAmountDisplay itemAmountTemplate;

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color enoughColor;

	[SerializeField]
	private Color money_normalColor;

	[SerializeField]
	private Color money_enoughColor;

	private PrefabPool<ItemAmountDisplay> _itemPool;

	private Cost cost;

	private PrefabPool<ItemAmountDisplay> ItemPool
	{
		get
		{
			if (_itemPool == null)
			{
				_itemPool = new PrefabPool<ItemAmountDisplay>(itemAmountTemplate);
			}
			return _itemPool;
		}
	}

	private void OnEnable()
	{
		EconomyManager.OnMoneyChanged += OnMoneyChanged;
		ItemUtilities.OnPlayerItemOperation += OnItemOperation;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDisable()
	{
		EconomyManager.OnMoneyChanged -= OnMoneyChanged;
		ItemUtilities.OnPlayerItemOperation -= OnItemOperation;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		RefreshBackground();
	}

	private void OnItemOperation()
	{
		RefreshBackground();
	}

	private void RefreshBackground()
	{
		if (!(background == null))
		{
			background.color = (cost.Enough ? enoughColor : normalColor);
		}
	}

	private void OnMoneyChanged(long arg1, long arg2)
	{
		RefreshMoneyBackground();
		RefreshBackground();
	}

	public void Setup(Cost cost, int multiplier = 1)
	{
		this.cost = cost;
		moneyContainer.SetActive(cost.money > 0);
		money.text = (cost.money * multiplier).ToString("n0");
		itemsContainer.SetActive(cost.items != null && cost.items.Length != 0);
		ItemPool.ReleaseAll();
		if (cost.items != null)
		{
			Cost.ItemEntry[] items = cost.items;
			for (int i = 0; i < items.Length; i++)
			{
				Cost.ItemEntry itemEntry = items[i];
				ItemAmountDisplay itemAmountDisplay = ItemPool.Get();
				itemAmountDisplay.Setup(itemEntry.id, itemEntry.amount * multiplier);
				itemAmountDisplay.transform.SetAsLastSibling();
			}
		}
		RefreshMoneyBackground();
		RefreshBackground();
	}

	private void RefreshMoneyBackground()
	{
		bool flag = cost.money <= EconomyManager.Money;
		moneyBackground.color = (flag ? money_enoughColor : money_normalColor);
	}

	internal void Clear()
	{
		cost = default(Cost);
		moneyContainer.SetActive(value: false);
		ItemPool.ReleaseAll();
	}
}
