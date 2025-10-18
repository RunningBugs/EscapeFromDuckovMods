using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Saves;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeleteSaveDataButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[SerializeField]
	private float totalTime = 3f;

	[SerializeField]
	private Image barFill;

	[SerializeField]
	private FadeGroup saveDeletedNotifierFadeGroup;

	private float timeWhenStartedHolding = float.MaxValue;

	private bool holding;

	private float TimeSinceStartedHolding => Time.unscaledTime - timeWhenStartedHolding;

	private float T
	{
		get
		{
			if (totalTime <= 0f)
			{
				return 1f;
			}
			return Mathf.Clamp01(TimeSinceStartedHolding / totalTime);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		holding = true;
		timeWhenStartedHolding = Time.unscaledTime;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		holding = false;
		timeWhenStartedHolding = float.MaxValue;
		RefreshProgressBar();
	}

	private void Start()
	{
		barFill.fillAmount = 0f;
	}

	private void Update()
	{
		if (holding)
		{
			RefreshProgressBar();
			if (T >= 1f)
			{
				Execute();
			}
		}
	}

	private void Execute()
	{
		holding = false;
		DeleteCurrentSaveData();
		RefreshProgressBar();
		NotifySaveDeleted().Forget();
	}

	private async UniTask NotifySaveDeleted()
	{
		await saveDeletedNotifierFadeGroup.ShowAndReturnTask();
		await UniTask.WaitForSeconds(2);
		await saveDeletedNotifierFadeGroup.HideAndReturnTask();
	}

	private void DeleteCurrentSaveData()
	{
		SavesSystem.DeleteCurrentSave();
	}

	private void RefreshProgressBar()
	{
		barFill.fillAmount = T;
	}
}
