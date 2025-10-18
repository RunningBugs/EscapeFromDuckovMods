using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiInteractionMenuButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private FadeGroup fadeGroup;

	private InteractableBase target;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		if (!(target == null))
		{
			CharacterMainControl.Main?.Interact(target);
		}
	}

	internal void Setup(InteractableBase target)
	{
		base.gameObject.SetActive(value: true);
		this.target = target;
		text.text = target.InteractName;
		fadeGroup.SkipHide();
	}

	internal void Show()
	{
		fadeGroup.Show();
	}

	internal void Hide()
	{
		fadeGroup.Hide();
	}
}
