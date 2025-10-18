using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class TooltipsProvider : MonoBehaviour, ITooltipsProvider, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string text;

	public string GetTooltipsText()
	{
		return text;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(text))
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
}
