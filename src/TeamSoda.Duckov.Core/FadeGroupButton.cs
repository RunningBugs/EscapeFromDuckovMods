using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

public class FadeGroupButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup closeOnClick;

	[SerializeField]
	private FadeGroup openOnClick;

	[SerializeField]
	private bool triggerWhenCancel;

	private void OnEnable()
	{
		UIInputManager.OnCancel += OnCancel;
	}

	private void OnDisable()
	{
		UIInputManager.OnCancel -= OnCancel;
	}

	private void OnCancel(UIInputEventData data)
	{
		if (base.isActiveAndEnabled && !data.Used && triggerWhenCancel)
		{
			Execute();
			data.Use();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Execute();
	}

	private void Execute()
	{
		if ((bool)closeOnClick)
		{
			closeOnClick.Hide();
		}
		if ((bool)openOnClick)
		{
			openOnClick.Show();
		}
	}
}
