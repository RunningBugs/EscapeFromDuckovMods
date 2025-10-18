using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerMoneyDisplay : MonoBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private string format = "$0";

	private void Update()
	{
		text.text = master.Money.ToString(format);
	}
}
