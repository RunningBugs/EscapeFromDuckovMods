using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReloadHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	public Button button;

	private bool reloadable;

	public UnityEvent OnShowEvent;

	public UnityEvent OnHideEvent;

	private int frame;

	private void Update()
	{
		if (characterMainControl == null)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if (characterMainControl == null)
			{
				return;
			}
			button.onClick.AddListener(Reload);
		}
		reloadable = characterMainControl.GetGunReloadable();
		if (reloadable != button.interactable)
		{
			button.interactable = reloadable;
			if (reloadable)
			{
				OnShowEvent?.Invoke();
			}
			else
			{
				OnHideEvent?.Invoke();
			}
		}
		frame++;
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	private void Reload()
	{
		if ((bool)characterMainControl)
		{
			characterMainControl.TryToReload();
		}
	}
}
