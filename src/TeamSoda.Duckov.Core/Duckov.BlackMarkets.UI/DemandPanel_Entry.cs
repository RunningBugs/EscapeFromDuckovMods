using System;
using Duckov.Utilities;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.BlackMarkets.UI;

public class DemandPanel_Entry : MonoBehaviour, IPoolable
{
	[SerializeField]
	private TextMeshProUGUI titleDisplay;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private TextMeshProUGUI moneyDisplay;

	[SerializeField]
	private GameObject remainingInfoContainer;

	[SerializeField]
	private TextMeshProUGUI remainingAmountDisplay;

	[SerializeField]
	private GameObject canInteractIndicator;

	[SerializeField]
	private GameObject outOfStockIndicator;

	[SerializeField]
	[LocalizationKey("UIText")]
	private string titleFormatKey_Normal = "BlackMarket_Demand_Title_Normal";

	[SerializeField]
	[LocalizationKey("UIText")]
	private string titleFormatKey_High = "BlackMarket_Demand_Title_High";

	[SerializeField]
	private Button dealButton;

	public BlackMarket.DemandSupplyEntry Target { get; private set; }

	private string TitleFormatKey
	{
		get
		{
			if (Target == null)
			{
				return "?";
			}
			if (Target.priceFactor >= 1.9f)
			{
				return titleFormatKey_High;
			}
			return titleFormatKey_Normal;
		}
	}

	private string TitleText
	{
		get
		{
			if (Target == null)
			{
				return "?";
			}
			return TitleFormatKey.ToPlainText().Format(new
			{
				itemName = Target.ItemDisplayName
			});
		}
	}

	public event Action<DemandPanel_Entry> onDealButtonClicked;

	private bool CanInteract()
	{
		if (Target == null)
		{
			return false;
		}
		if (Target.remaining <= 0)
		{
			return false;
		}
		return Target.SellCost.Enough;
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		if (Target != null)
		{
			Target.onChanged -= OnChanged;
		}
	}

	private void OnChanged(BlackMarket.DemandSupplyEntry entry)
	{
		Refresh();
	}

	public void OnDealButtonClicked()
	{
		this.onDealButtonClicked?.Invoke(this);
	}

	internal void Setup(BlackMarket.DemandSupplyEntry target)
	{
		if (target == null)
		{
			Debug.LogError("找不到对象", base.gameObject);
			return;
		}
		Target = target;
		costDisplay.Setup(target.SellCost);
		moneyDisplay.text = $"{target.TotalPrice}";
		titleDisplay.text = TitleText;
		Refresh();
		target.onChanged += OnChanged;
	}

	private void OnEnable()
	{
		ItemUtilities.OnPlayerItemOperation += Refresh;
	}

	private void OnDisable()
	{
		ItemUtilities.OnPlayerItemOperation -= Refresh;
	}

	private void Awake()
	{
		dealButton.onClick.AddListener(OnDealButtonClicked);
	}

	private void Refresh()
	{
		if (Target == null)
		{
			Debug.LogError("找不到对象", base.gameObject);
			return;
		}
		remainingAmountDisplay.text = $"{Target.Remaining}";
		bool active = CanInteract();
		canInteractIndicator.SetActive(active);
		bool active2 = Target.Remaining <= 0;
		outOfStockIndicator.SetActive(active2);
		remainingInfoContainer.SetActive(Target.remaining > 1);
	}
}
