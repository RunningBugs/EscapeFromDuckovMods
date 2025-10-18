using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;

public class Interact_CustomFace : InteractableBase
{
	public GameObject activePart;

	public CustomFaceUI customFaceUI;

	public CanvasGroupFade fade;

	public CustomFaceInstance customFaceInstance;

	public UnityEvent OnCustomFaceUiClosedEvent;

	[LocalizationKey("UIText")]
	public string OnCopyNotificationKey;

	[LocalizationKey("UIText")]
	public string OnPastySuccessNotificationKey;

	[LocalizationKey("UIText")]
	public string OnPastyFailedNotificationKey;

	public static event Action OnCustomFaceStartEvent;

	public static event Action OnCustomFaceFinishedEvent;

	protected override void Awake()
	{
		base.Awake();
		activePart.SetActive(value: false);
		customFaceUI.SetFace(customFaceInstance);
		fade.gameObject.SetActive(value: false);
	}

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		Show().Forget();
	}

	protected override void OnInteractStop()
	{
		Debug.Log("Stop custom face");
		Hide().Forget();
	}

	private async UniTaskVoid Show()
	{
		await BlackScreen.ShowAndReturnTask(null, 1f, 0.25f);
		if (!base.Interacting)
		{
			BlackScreen.HideAndReturnTask(null, 0f, 0.25f).Forget();
			return;
		}
		Interact_CustomFace.OnCustomFaceStartEvent?.Invoke();
		activePart.SetActive(value: true);
		InputManager.DisableInput(activePart);
		fade.Show().Forget();
		customFaceUI.canControl = true;
		customFaceInstance.LoadFromData(LevelManager.Instance.CustomFaceManager.LoadMainCharacterSetting());
		CharacterMainControl.Main.characterModel.gameObject.SetActive(value: false);
		LevelManager.Instance.PetCharacter.characterModel.gameObject.SetActive(value: false);
		await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true);
		await BlackScreen.HideAndReturnTask(null, 0f, 0.25f);
	}

	private async UniTaskVoid Hide()
	{
		Interact_CustomFace.OnCustomFaceFinishedEvent?.Invoke();
		await BlackScreen.ShowAndReturnTask(null, 0f, 0.25f);
		activePart.SetActive(value: false);
		InputManager.ActiveInput(activePart);
		customFaceUI.canControl = false;
		fade.Hide().Forget();
		CharacterMainControl.Main.characterModel.gameObject.SetActive(value: true);
		LevelManager.Instance.PetCharacter.characterModel.gameObject.SetActive(value: true);
		LevelManager.Instance.RefreshMainCharacterFace();
		CharacterMainControl.Main.SetAimPoint(customFaceInstance.transform.position + customFaceInstance.transform.forward * 100f);
		await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true);
		OnCustomFaceUiClosedEvent?.Invoke();
		await BlackScreen.HideAndReturnTask(null, 1f, 0.25f);
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = customFaceInstance.ConvertToSaveData().DataToJson();
		NotificationText.Push(OnCopyNotificationKey.ToPlainText());
	}

	public void PastyDataAndApply()
	{
		if (CustomFaceSettingData.JsonToData(GUIUtility.systemCopyBuffer, out var data))
		{
			customFaceInstance.LoadFromData(data);
			NotificationText.Push(OnPastySuccessNotificationKey.ToPlainText());
		}
		else
		{
			NotificationText.Push(OnPastyFailedNotificationKey.ToPlainText());
		}
	}
}
