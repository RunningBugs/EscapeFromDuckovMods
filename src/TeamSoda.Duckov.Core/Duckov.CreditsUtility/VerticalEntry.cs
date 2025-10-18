using UnityEngine;
using UnityEngine.UI;

namespace Duckov.CreditsUtility;

public class VerticalEntry : MonoBehaviour
{
	[SerializeField]
	private VerticalLayoutGroup layoutGroup;

	[SerializeField]
	private LayoutElement layoutElement;

	public void Setup(params string[] args)
	{
	}

	public void SetLayoutSpacing(float spacing)
	{
		layoutGroup.spacing = spacing;
	}

	public void SetPreferredWidth(float width)
	{
		layoutElement.preferredWidth = width;
	}
}
