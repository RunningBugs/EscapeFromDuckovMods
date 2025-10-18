using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftView_ListEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color selectedColor;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image background;

	[SerializeField]
	private TextMeshProUGUI nameText;

	public CraftView Master { get; private set; }

	public CraftingFormula Formula { get; private set; }

	public void Setup(CraftView master, CraftingFormula formula)
	{
		Master = master;
		Formula = formula;
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(Formula.result.id);
		icon.sprite = metaData.icon;
		nameText.text = $"{metaData.DisplayName} x{formula.result.amount}";
		Refresh();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Master?.SetSelection(this);
	}

	internal void NotifyUnselected()
	{
		Refresh();
	}

	internal void NotifySelected()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!(Master == null))
		{
			bool flag = Master.GetSelection() == this;
			background.color = (flag ? selectedColor : normalColor);
		}
	}
}
