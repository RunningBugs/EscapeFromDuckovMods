using System;
using Duckov.MiniGames.GoldMiner.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerShopUIEntry : MonoBehaviour
{
	[SerializeField]
	private NavEntry navEntry;

	[SerializeField]
	private VirtualCursorTarget VCT;

	[SerializeField]
	private GameObject mainLayout;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private string priceFormat = "0";

	[SerializeField]
	private GameObject priceIndicator;

	[SerializeField]
	private GameObject freeIndicator;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject soldIndicator;

	private GoldMinerShopUI master;

	public ShopEntity target;

	private void Awake()
	{
		if (!navEntry)
		{
			navEntry = GetComponent<NavEntry>();
		}
		NavEntry obj = navEntry;
		obj.onInteract = (Action<NavEntry>)Delegate.Combine(obj.onInteract, new Action<NavEntry>(OnInteract));
		VCT = GetComponent<VirtualCursorTarget>();
		if ((bool)VCT)
		{
			VCT.onEnter.AddListener(OnVCTEnter);
		}
	}

	private void OnVCTEnter()
	{
		master.hoveringEntry = this;
	}

	private void OnInteract(NavEntry entry)
	{
		master.target.Buy(target);
	}

	internal void Setup(GoldMinerShopUI master, ShopEntity target)
	{
		this.master = master;
		this.target = target;
		if (target == null || target.artifact == null)
		{
			SetupEmpty();
			return;
		}
		mainLayout.SetActive(value: true);
		nameText.text = target.artifact.DisplayName;
		icon.sprite = target.artifact.Icon;
		Refresh();
	}

	private void Refresh()
	{
		bool useTicket;
		int num = master.target.CalculateDealPrice(target, out useTicket);
		priceText.text = num.ToString(priceFormat);
		priceIndicator.SetActive(num > 0);
		freeIndicator.SetActive(num <= 0);
		soldIndicator.SetActive(target.sold);
	}

	private void SetupEmpty()
	{
		mainLayout.SetActive(value: false);
	}
}
