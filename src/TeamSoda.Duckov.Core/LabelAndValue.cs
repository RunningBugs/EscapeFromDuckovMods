using ItemStatsSystem;
using TMPro;
using UnityEngine;

public class LabelAndValue : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI labelText;

	[SerializeField]
	private TextMeshProUGUI valueText;

	[SerializeField]
	private Color colorNeutral;

	[SerializeField]
	private Color colorPositive;

	[SerializeField]
	private Color colorNegative;

	public void Setup(string label, string value, Polarity valuePolarity)
	{
		labelText.text = label;
		valueText.text = value;
		Color color = colorNeutral;
		switch (valuePolarity)
		{
		case Polarity.Neutral:
			color = colorNeutral;
			break;
		case Polarity.Negative:
			color = colorNegative;
			break;
		case Polarity.Positive:
			color = colorPositive;
			break;
		}
		valueText.color = color;
	}
}
