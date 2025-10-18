using UnityEngine;
using UnityEngine.UI;

namespace Duckov.CreditsUtility;

public class EmptyEntry : MonoBehaviour
{
	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private float defaultWidth;

	[SerializeField]
	private float defaultHeight;

	public void Setup(params string[] args)
	{
		layoutElement.preferredWidth = defaultWidth;
		layoutElement.preferredHeight = defaultHeight;
		if (args == null)
		{
			return;
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (i == 1)
			{
				TrySetWidth(args[i]);
			}
			if (i == 2)
			{
				TrySetHeight(args[i]);
			}
		}
	}

	private void TrySetWidth(string v)
	{
		if (float.TryParse(v, out var result))
		{
			layoutElement.preferredWidth = result;
		}
	}

	private void TrySetHeight(string v)
	{
		if (float.TryParse(v, out var result))
		{
			layoutElement.preferredHeight = result;
		}
	}
}
