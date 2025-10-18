using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.ProceduralImage;

public class CustomFaceTabs : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public CustomFaceUI master;

	public List<GameObject> panels;

	public ProceduralImage background;

	public Color normalColor;

	public Color selectedColor;

	public void OnPointerClick(PointerEventData eventData)
	{
		master.SelectTab(this);
	}

	public void SetSelectVisual(bool selected)
	{
		background.color = (selected ? selectedColor : normalColor);
	}
}
