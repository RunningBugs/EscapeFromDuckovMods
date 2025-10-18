using Duckov.MiniGames.GoldMiner.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.GoldMiner;

public class PassivePropDisplay : MonoBehaviour
{
	[SerializeField]
	private NavEntry navEntry;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI amounText;

	public RectTransform rectTransform { get; private set; }

	public NavEntry NavEntry => navEntry;

	public GoldMinerArtifact Target { get; private set; }

	internal void Setup(GoldMinerArtifact target, int amount)
	{
		Target = target;
		icon.sprite = target.Icon;
		rectTransform = base.transform as RectTransform;
		amounText.text = ((amount > 1) ? $"{amount}" : "");
	}
}
