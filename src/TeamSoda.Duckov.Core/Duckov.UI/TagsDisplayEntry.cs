using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class TagsDisplayEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ITooltipsProvider
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private TextMeshProUGUI text;

	private Tag target;

	public string GetTooltipsText()
	{
		if (target == null)
		{
			return "";
		}
		return target.Description;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(target == null) && target.ShowDescription)
		{
			Tooltips.NotifyEnterTooltipsProvider(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltips.NotifyExitTooltipsProvider(this);
	}

	private void OnDisable()
	{
		Tooltips.NotifyExitTooltipsProvider(this);
	}

	public void Setup(Tag tag)
	{
		target = tag;
		background.color = tag.Color;
		text.text = tag.DisplayName;
	}
}
