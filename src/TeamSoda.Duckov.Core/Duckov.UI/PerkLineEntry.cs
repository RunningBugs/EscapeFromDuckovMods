using Duckov.PerkTrees;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class PerkLineEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	private RectTransform _rectTransform;

	private PerkLevelLineNode target;

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	internal void Setup(PerkTreeView perkTreeView, PerkLevelLineNode cur)
	{
		target = cur;
		label.text = target.DisplayName;
	}

	internal Vector2 GetLayoutPosition()
	{
		if (target == null)
		{
			return Vector2.zero;
		}
		return target.cachedPosition;
	}
}
