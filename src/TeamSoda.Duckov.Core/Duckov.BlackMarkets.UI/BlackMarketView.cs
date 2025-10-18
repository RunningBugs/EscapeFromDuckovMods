using System;
using Duckov.UI;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.BlackMarkets.UI;

public class BlackMarketView : View
{
	public enum Mode
	{
		None,
		Demand,
		Supply
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private DemandPanel demandPanel;

	[SerializeField]
	private SupplyPanel supplyPanel;

	[SerializeField]
	private TextMeshProUGUI refreshETAText;

	[SerializeField]
	private TextMeshProUGUI refreshChanceText;

	[SerializeField]
	private Button btn_demandPanel;

	[SerializeField]
	private Button btn_supplyPanel;

	[SerializeField]
	private Button btn_refresh;

	[SerializeField]
	private GameObject refreshInteractableIndicator;

	private Mode mode;

	public static BlackMarketView Instance => View.GetViewInstance<BlackMarketView>();

	protected override bool ShowOpenCloseButtons => false;

	public BlackMarket Target { get; private set; }

	private bool ShowDemand => (Mode.Demand | mode) == mode;

	private bool ShowSupply => (Mode.Supply | mode) == mode;

	protected override void Awake()
	{
		base.Awake();
		btn_demandPanel.onClick.AddListener(delegate
		{
			SetMode(Mode.Demand);
		});
		btn_supplyPanel.onClick.AddListener(delegate
		{
			SetMode(Mode.Supply);
		});
		btn_refresh.onClick.AddListener(OnRefreshBtnClicked);
	}

	private void OnEnable()
	{
		BlackMarket.onRefreshChanceChanged += OnRefreshChanceChanced;
	}

	private void OnDisable()
	{
		BlackMarket.onRefreshChanceChanged -= OnRefreshChanceChanced;
	}

	private void OnRefreshChanceChanced(BlackMarket market)
	{
		RefreshRefreshButton();
	}

	private void RefreshRefreshButton()
	{
		if (Target == null)
		{
			refreshChanceText.text = "ERROR";
			refreshInteractableIndicator.SetActive(value: false);
		}
		int refreshChance = Target.RefreshChance;
		int maxRefreshChance = Target.MaxRefreshChance;
		refreshChanceText.text = $"{refreshChance}/{maxRefreshChance}";
		refreshInteractableIndicator.SetActive(refreshChance > 0);
	}

	private void OnRefreshBtnClicked()
	{
		if (!(Target == null))
		{
			Target.PayAndRegenerate();
		}
	}

	public static void Show(Mode mode)
	{
		if (!(Instance == null) && !(BlackMarket.Instance == null))
		{
			Instance.Setup(BlackMarket.Instance, mode);
			Instance.Open();
		}
	}

	private void Setup(BlackMarket target, Mode mode)
	{
		Target = target;
		demandPanel.Setup(target);
		supplyPanel.Setup(target);
		RefreshRefreshButton();
		SetMode(mode);
		Open();
	}

	private void SetMode(Mode mode)
	{
		this.mode = mode;
		demandPanel.gameObject.SetActive(ShowDemand);
		supplyPanel.gameObject.SetActive(ShowSupply);
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void Update()
	{
		if (!(Target == null))
		{
			int refreshChance = Target.RefreshChance;
			int maxRefreshChance = Target.MaxRefreshChance;
			string text;
			if (refreshChance < maxRefreshChance)
			{
				TimeSpan remainingTimeBeforeRefresh = Target.RemainingTimeBeforeRefresh;
				text = $"{Mathf.FloorToInt((float)remainingTimeBeforeRefresh.TotalHours):00}:{remainingTimeBeforeRefresh.Minutes:00}:{remainingTimeBeforeRefresh.Seconds:00}";
			}
			else
			{
				text = "--:--:--";
			}
			refreshETAText.text = text;
		}
	}
}
