using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class InventoryEntryTradingPriceDisplay : MonoBehaviour
{
	[SerializeField]
	private InventoryEntry master;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private bool selling = true;

	[SerializeField]
	private string moneyFormat = "n0";

	public bool Selling
	{
		get
		{
			return selling;
		}
		set
		{
			selling = value;
		}
	}

	private void Awake()
	{
		master.onRefresh += OnRefresh;
		TradingUIUtilities.OnActiveMerchantChanged += OnActiveMerchantChanged;
	}

	private void OnActiveMerchantChanged(IMerchant merchant)
	{
		Refresh();
	}

	private void Start()
	{
		Refresh();
	}

	private void OnDestroy()
	{
		if (master != null)
		{
			master.onRefresh -= OnRefresh;
		}
		TradingUIUtilities.OnActiveMerchantChanged -= OnActiveMerchantChanged;
	}

	private void OnRefresh(InventoryEntry entry)
	{
		Refresh();
	}

	private void Refresh()
	{
		Item item = master?.Content;
		if (item != null)
		{
			canvasGroup.alpha = 1f;
			string text = GetPrice(item).ToString(moneyFormat);
			priceText.text = text;
		}
		else
		{
			canvasGroup.alpha = 0f;
		}
	}

	private int GetPrice(Item content)
	{
		if (content == null)
		{
			return 0;
		}
		int value = content.Value;
		if (TradingUIUtilities.ActiveMerchant == null)
		{
			return value;
		}
		return TradingUIUtilities.ActiveMerchant.ConvertPrice(content, selling);
	}
}
