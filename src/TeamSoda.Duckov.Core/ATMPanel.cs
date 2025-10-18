using System;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ATMPanel : MonoBehaviour
{
	private const int CashItemTypeID = 451;

	[SerializeField]
	private TextMeshProUGUI balanceAmountText;

	[SerializeField]
	private TextMeshProUGUI cashAmountText;

	[SerializeField]
	private Button btnSelectSave;

	[SerializeField]
	private Button btnSelectDraw;

	[SerializeField]
	private FadeGroup selectPanel;

	[SerializeField]
	private ATMPanel_SavePanel savePanel;

	[SerializeField]
	private ATMPanel_DrawPanel drawPanel;

	private int _cachedCashAmount = -1;

	private static bool drawingMoney;

	public const long MaxDrawAmount = 10000000L;

	private int CashAmount
	{
		get
		{
			if (_cachedCashAmount < 0)
			{
				_cachedCashAmount = ItemUtilities.GetItemCount(451);
			}
			return _cachedCashAmount;
		}
	}

	private void Awake()
	{
		btnSelectSave.onClick.AddListener(ShowSavePanel);
		btnSelectDraw.onClick.AddListener(ShowDrawPanel);
		savePanel.onQuit += SavePanel_onQuit;
		drawPanel.onQuit += DrawPanel_onQuit;
	}

	private void DrawPanel_onQuit(ATMPanel_DrawPanel panel)
	{
		ShowSelectPanel();
	}

	private void SavePanel_onQuit(ATMPanel_SavePanel obj)
	{
		ShowSelectPanel();
	}

	private void HideAllPanels(bool skip = false)
	{
		if (skip)
		{
			selectPanel.SkipHide();
		}
		else
		{
			selectPanel.Hide();
		}
		savePanel.Hide(skip);
		drawPanel.Hide(skip);
	}

	public void ShowSelectPanel(bool skipHideOthers = false)
	{
		HideAllPanels(skipHideOthers);
		selectPanel.Show();
	}

	public void ShowDrawPanel()
	{
		HideAllPanels();
		drawPanel.Show();
	}

	public void ShowSavePanel()
	{
		HideAllPanels();
		savePanel.Show();
	}

	private void OnEnable()
	{
		EconomyManager.OnMoneyChanged += OnMoneyChanged;
		ItemUtilities.OnPlayerItemOperation += OnPlayerItemOperation;
		RefreshCash();
		RefreshBalance();
		ShowSelectPanel();
	}

	private void OnDisable()
	{
		EconomyManager.OnMoneyChanged -= OnMoneyChanged;
		ItemUtilities.OnPlayerItemOperation -= OnPlayerItemOperation;
	}

	private void OnPlayerItemOperation()
	{
		RefreshCash();
	}

	private void OnMoneyChanged(long oldMoney, long changedMoney)
	{
		RefreshBalance();
	}

	private void RefreshCash()
	{
		_cachedCashAmount = ItemUtilities.GetItemCount(451);
		cashAmountText.text = $"{CashAmount:n0}";
	}

	private void RefreshBalance()
	{
		balanceAmountText.text = $"{EconomyManager.Money:n0}";
	}

	public static async UniTask<bool> Draw(long amount)
	{
		if (drawingMoney)
		{
			return false;
		}
		if (amount > 10000000)
		{
			Debug.LogError($"Drawing amount {amount} greater than max draw amount {10000000L}. Clamping draw amount down.");
			amount = 10000000L;
		}
		drawingMoney = true;
		try
		{
			Cost cost = new Cost(amount);
			if (!cost.Enough)
			{
				return false;
			}
			await new Cost((451, amount)).Return(directToBuffer: false, toPlayerInventory: true);
			cost.Pay(accountAvaliable: true, cashAvaliable: false);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		drawingMoney = false;
		return true;
	}

	public static bool Save(long amount)
	{
		Cost cost = new Cost(0L, new(int, long)[1] { (451, amount) });
		if (!cost.Pay(accountAvaliable: false))
		{
			return false;
		}
		EconomyManager.Add(amount);
		return true;
	}
}
