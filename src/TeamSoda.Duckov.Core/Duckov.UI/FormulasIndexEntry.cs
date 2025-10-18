using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class FormulasIndexEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private Image image;

	[SerializeField]
	private string lockedText = "???";

	[SerializeField]
	private Sprite lockedImage;

	private FormulasIndexView master;

	private CraftingFormula formula;

	public CraftingFormula Formula => formula;

	private int ItemID => formula.result.id;

	private ItemMetaData Meta => ItemAssetsCollection.GetMetaData(ItemID);

	private bool Unlocked => CraftingManager.IsFormulaUnlocked(formula.id);

	public bool Valid => ItemID >= 0;

	public void OnPointerClick(PointerEventData eventData)
	{
		master.OnEntryClicked(this);
	}

	internal void Setup(FormulasIndexView master, CraftingFormula formula)
	{
		this.master = master;
		this.formula = formula;
		Refresh();
	}

	public void Refresh()
	{
		ItemMetaData meta = Meta;
		if (!Valid)
		{
			displayNameText.text = "! " + formula.id + " !";
			image.sprite = lockedImage;
		}
		else if (Unlocked)
		{
			displayNameText.text = $"{meta.DisplayName} x{formula.result.amount}";
			image.sprite = meta.icon;
		}
		else
		{
			displayNameText.text = lockedText;
			image.sprite = lockedImage;
		}
	}
}
