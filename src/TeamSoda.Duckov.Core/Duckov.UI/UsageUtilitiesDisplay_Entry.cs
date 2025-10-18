using ItemStatsSystem;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class UsageUtilitiesDisplay_Entry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	public UsageBehavior Target { get; private set; }

	internal void Setup(UsageBehavior cur)
	{
		text.text = cur.DisplaySettings.Description;
	}
}
