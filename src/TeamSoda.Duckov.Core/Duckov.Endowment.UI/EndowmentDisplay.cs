using TMPro;
using UnityEngine;

namespace Duckov.Endowment.UI;

public class EndowmentDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI descriptionsText;

	private void Refresh()
	{
		EndowmentEntry current = EndowmentManager.Current;
		if (current == null)
		{
			displayNameText.text = "?";
			descriptionsText.text = "?";
		}
		else
		{
			displayNameText.text = current.DisplayName;
			descriptionsText.text = current.DescriptionAndEffects;
		}
	}

	private void OnEnable()
	{
		Refresh();
	}
}
