using Duckov.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.CreditsUtility;

public class ImageEntry : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private LayoutElement layoutElement;

	internal void Setup(string[] elements)
	{
		if (elements.Length < 2)
		{
			return;
		}
		for (int i = 0; i < elements.Length; i++)
		{
			switch (i)
			{
			case 1:
			{
				string text = elements[1];
				Sprite sprite = GameplayDataSettings.GetSprite(text);
				if (sprite == null)
				{
					Debug.LogError("Cannot find sprite:" + text);
				}
				else
				{
					image.sprite = sprite;
				}
				break;
			}
			case 2:
			{
				if (float.TryParse(elements[2], out var result2))
				{
					layoutElement.preferredHeight = result2;
				}
				break;
			}
			case 3:
			{
				if (float.TryParse(elements[2], out var result))
				{
					layoutElement.preferredWidth = result;
				}
				break;
			}
			}
		}
	}
}
