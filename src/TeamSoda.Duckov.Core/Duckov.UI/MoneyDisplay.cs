using Duckov.Economy;
using Saves;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class MoneyDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private string format = "n0";

	private void Awake()
	{
		EconomyManager.OnMoneyChanged += OnMoneyChanged;
		SavesSystem.OnSetFile += OnSaveFileChanged;
		Refresh();
	}

	private void OnDestroy()
	{
		EconomyManager.OnMoneyChanged -= OnMoneyChanged;
		SavesSystem.OnSetFile -= OnSaveFileChanged;
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void Refresh()
	{
		text.text = EconomyManager.Money.ToString(format);
	}

	private void OnMoneyChanged(long arg1, long arg2)
	{
		Refresh();
	}

	private void OnSaveFileChanged()
	{
		Refresh();
	}
}
