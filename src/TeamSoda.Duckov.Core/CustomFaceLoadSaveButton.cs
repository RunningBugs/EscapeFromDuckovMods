using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomFaceLoadSaveButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Color selectedColor;

	public Color unselectedColor;

	public Image image;

	public CustomFaceSaveLoad master;

	public int index;

	public TextMeshProUGUI text;

	private void Awake()
	{
	}

	public void Init(CustomFaceSaveLoad _master, int _index, string name)
	{
		text.text = name;
		master = _master;
		index = _index;
	}

	public void SetSelection(bool selected)
	{
		image.color = (selected ? selectedColor : unselectedColor);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		master.SetSlotAndLoad(index);
	}
}
