using Duckov.Economy;
using Duckov.UI.Animations;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormulasDetailsDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private FadeGroup placeHolderFadeGroup;

	[SerializeField]
	private Sprite unknownImage;

	private CraftingFormula formula;

	private void SetupEmpty()
	{
		contentFadeGroup.Hide();
		placeHolderFadeGroup.Show();
	}

	private void SetupFormula(CraftingFormula formula)
	{
		this.formula = formula;
		RefreshContent();
		contentFadeGroup.Show();
		placeHolderFadeGroup.Hide();
	}

	private void RefreshContent()
	{
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(formula.result.id);
		nameText.text = metaData.DisplayName;
		descriptionText.text = metaData.Description;
		image.sprite = metaData.icon;
		costDisplay.Setup(formula.cost);
	}

	public void Setup(CraftingFormula? formula)
	{
		if (!formula.HasValue)
		{
			SetupEmpty();
		}
		else if (!CraftingManager.IsFormulaUnlocked(formula.Value.id))
		{
			SetupUnknown();
		}
		else
		{
			SetupFormula(formula.Value);
		}
	}

	private void SetupUnknown()
	{
		nameText.text = "???";
		descriptionText.text = "???";
		image.sprite = unknownImage;
		contentFadeGroup.Show();
		placeHolderFadeGroup.Hide();
		costDisplay.Setup(default(Cost));
	}
}
