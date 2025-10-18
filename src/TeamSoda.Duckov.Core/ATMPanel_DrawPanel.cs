using System;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ATMPanel_DrawPanel : MonoBehaviour
{
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

	public event Action<ATMPanel_DrawPanel> onQuit;

	private void OnEnable()
	{
		EconomyManager.OnMoneyChanged += OnMoneyChanged;
		Refresh();
	}

	private void OnDisable()
	{
		EconomyManager.OnMoneyChanged -= OnMoneyChanged;
	}

	private void Awake()
	{
		inputPanel.onInputFieldValueChanged += OnInputValueChanged;
		inputPanel.maxFunction = delegate
		{
			long num = EconomyManager.Money;
			if (num > 10000000)
			{
				num = 10000000L;
			}
			return num;
		};
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		quitButton.onClick.AddListener(OnQuitButtonClicked);
	}

	private void OnQuitButtonClicked()
	{
		this.onQuit?.Invoke(this);
	}

	private void OnMoneyChanged(long arg1, long arg2)
	{
		Refresh();
	}

	private void OnConfirmButtonClicked()
	{
		if (inputPanel.Value <= 0)
		{
			inputPanel.Clear();
			return;
		}
		long num = EconomyManager.Money;
		if (num > 10000000)
		{
			num = 10000000L;
		}
		if (inputPanel.Value <= num)
		{
			DrawTask(inputPanel.Value).Forget();
		}
	}

	private async UniTask DrawTask(long value)
	{
		if (await ATMPanel.Draw(value))
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
		bool flag = EconomyManager.Money >= inputPanel.Value;
		flag &= inputPanel.Value <= 10000000;
		flag &= inputPanel.Value >= 0;
		insufficientIndicator.SetActive(!flag);
	}

	internal void Show()
	{
		fadeGroup.Show();
	}

	internal void Hide(bool skip)
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
}
