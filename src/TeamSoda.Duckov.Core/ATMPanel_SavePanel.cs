using System;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ATMPanel_SavePanel : MonoBehaviour
{
	private const int CashItemTypeID = 451;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private DigitInputPanel inputPanel;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private GameObject insufficientIndicator;

	[SerializeField]
	private Button quitButton;

	private int _cachedCashAmount = -1;

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

	public event Action<ATMPanel_SavePanel> onQuit;

	private void OnEnable()
	{
		ItemUtilities.OnPlayerItemOperation += OnPlayerItemOperation;
		RefreshCash();
		Refresh();
	}

	private void OnDisable()
	{
		ItemUtilities.OnPlayerItemOperation -= OnPlayerItemOperation;
	}

	private void OnPlayerItemOperation()
	{
		RefreshCash();
		Refresh();
	}

	private void RefreshCash()
	{
		_cachedCashAmount = ItemUtilities.GetItemCount(451);
	}

	private void Awake()
	{
		inputPanel.onInputFieldValueChanged += OnInputValueChanged;
		inputPanel.maxFunction = () => CashAmount;
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		quitButton.onClick.AddListener(OnQuitButtonClicked);
	}

	private void OnQuitButtonClicked()
	{
		this.onQuit?.Invoke(this);
	}

	private void OnConfirmButtonClicked()
	{
		if (inputPanel.Value <= 0)
		{
			inputPanel.Clear();
		}
		else if (inputPanel.Value <= CashAmount && ATMPanel.Save(inputPanel.Value))
		{
			inputPanel.Clear();
		}
	}

	private void OnInputValueChanged(string v)
	{
		Refresh();
	}

	private void Refresh()
	{
		bool flag = CashAmount >= inputPanel.Value;
		flag &= inputPanel.Value >= 0;
		insufficientIndicator.SetActive(!flag);
	}

	internal void Hide(bool skip = false)
	{
		if (skip)
		{
			fadeGroup.SkipHide();
		}
		else
		{
			fadeGroup.Hide();
		}
	}

	internal void Show()
	{
		fadeGroup.Show();
	}
}
