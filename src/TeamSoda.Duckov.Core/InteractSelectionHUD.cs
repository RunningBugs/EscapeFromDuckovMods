using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class InteractSelectionHUD : MonoBehaviour
{
	private InteractableBase interactable;

	public GameObject selectIndicator;

	public TextMeshProUGUI text;

	public ProceduralImage background;

	public Color selectedColor;

	public Color unselectedColor;

	public CanvasGroup requireCanvasGroup;

	public ProceduralImage requireItemBackgroundImage;

	public TextMeshProUGUI requireText;

	[LocalizationKey("UI")]
	public string requirItemTextKey = "UI_RequireItem";

	[LocalizationKey("UI")]
	public string requirUseItemTextKey = "UI_RequireUseItem";

	public Image requirementIcon;

	public Color hasRequireItemColor;

	public Color noRequireItemColor;

	private bool selecting;

	public UnityEvent OnSelectedEvent;

	public GameObject selectionPoint;

	public GameObject upDownIndicator;

	private bool hasUpDown;

	public InteractableBase InteractTarget => interactable;

	public void SetInteractable(InteractableBase _interactable, bool _hasUpDown)
	{
		interactable = _interactable;
		text.text = interactable.GetInteractName();
		UpdateRequireItem(interactable);
		selectionPoint.SetActive(_hasUpDown);
		hasUpDown = _hasUpDown;
	}

	private void UpdateRequireItem(InteractableBase interactable)
	{
		if ((bool)interactable && interactable.requireItem)
		{
			requireCanvasGroup.alpha = 1f;
			CharacterMainControl mainCharacter = LevelManager.Instance.MainCharacter;
			bool num = interactable.whenToUseRequireItem != InteractableBase.WhenToUseRequireItemTypes.None;
			string text = (num ? requirUseItemTextKey.ToPlainText() : requirItemTextKey.ToPlainText());
			requireText.text = text + " " + interactable.GetRequiredItemName();
			if (num)
			{
				requireText.text += " x1";
			}
			requirementIcon.sprite = interactable.GetRequireditemIcon();
			if (interactable.TryGetRequiredItem(mainCharacter).hasItem)
			{
				requireItemBackgroundImage.color = hasRequireItemColor;
			}
			else
			{
				requireItemBackgroundImage.color = noRequireItemColor;
			}
		}
		else
		{
			requireCanvasGroup.alpha = 0f;
		}
	}

	public void SetSelection(bool _select)
	{
		selecting = _select;
		selectIndicator.SetActive(selecting);
		upDownIndicator.SetActive(selecting && hasUpDown);
		selectionPoint.SetActive(!selecting && hasUpDown);
		if (_select)
		{
			OnSelectedEvent?.Invoke();
			background.color = selectedColor;
		}
		else
		{
			background.color = unselectedColor;
		}
	}
}
