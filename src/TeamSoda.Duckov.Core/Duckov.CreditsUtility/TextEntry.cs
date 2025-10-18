using TMPro;
using UnityEngine;

namespace Duckov.CreditsUtility;

public class TextEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	public int defaultSize = 26;

	internal void Setup(string text, Color color, int size = -1, bool bold = false)
	{
		this.text.text = text;
		if (size < 0)
		{
			size = defaultSize;
		}
		this.text.color = color;
		this.text.fontSize = size;
		this.text.fontStyle = (FontStyles)((int)(this.text.fontStyle & ~FontStyles.Bold) | (bold ? 1 : 0));
	}
}
