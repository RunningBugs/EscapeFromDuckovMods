using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class KontextMenuEntry : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private float delayByIndex = 0.1f;

	[SerializeField]
	private List<FadeElement> fadeInElements;

	private KontextMenu menu;

	private KontextMenuDataEntry target;

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		target = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (menu != null)
		{
			menu.InstanceHide();
		}
		if (target != null)
		{
			target.action?.Invoke();
		}
	}

	public void Setup(KontextMenu menu, int index, KontextMenuDataEntry data)
	{
		this.menu = menu;
		target = data;
		if ((bool)icon)
		{
			if ((bool)data.icon)
			{
				icon.sprite = data.icon;
				icon.gameObject.SetActive(value: true);
			}
			else
			{
				icon.gameObject.SetActive(value: false);
			}
		}
		if ((bool)text)
		{
			if (!string.IsNullOrEmpty(target.text))
			{
				text.text = target.text;
				text.gameObject.SetActive(value: true);
			}
			else
			{
				text.gameObject.SetActive(value: false);
			}
		}
		foreach (FadeElement fadeInElement in fadeInElements)
		{
			fadeInElement.SkipHide();
			fadeInElement.Show(delayByIndex * (float)index).Forget();
		}
	}
}
