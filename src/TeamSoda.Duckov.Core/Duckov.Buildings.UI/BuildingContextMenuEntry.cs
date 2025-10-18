using System;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Buildings.UI;

public class BuildingContextMenuEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKey;

	public event Action<BuildingContextMenuEntry> onPointerClick;

	private void OnEnable()
	{
		text.text = textKey.ToPlainText();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.onPointerClick?.Invoke(this);
	}
}
