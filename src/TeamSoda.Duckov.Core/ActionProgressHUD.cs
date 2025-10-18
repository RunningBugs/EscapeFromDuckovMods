using Duckov;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI.ProceduralImage;

public class ActionProgressHUD : MonoBehaviour
{
	public CharacterActionBase.ActionPriorities specificActionType;

	public ProceduralImage fillImage;

	public CanvasGroup parentCanvasGroup;

	private CharacterMainControl characterMainControl;

	private IProgress currentProgressInterface;

	private float targetAlpha;

	private bool inProgress;

	public UnityEvent OnFinishEvent;

	[FormerlySerializedAs("cancleIndicator")]
	public GameObject stopIndicator;

	public bool InProgress => inProgress;

	public void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if ((bool)characterMainControl)
			{
				characterMainControl.OnActionStartEvent += OnActionStart;
				characterMainControl.OnActionProgressFinishEvent += OnActionFinish;
			}
		}
		inProgress = false;
		float num = 0f;
		if (currentProgressInterface as Object != null)
		{
			Progress progress = currentProgressInterface.GetProgress();
			inProgress = progress.inProgress;
			num = progress.progress;
			if (!inProgress)
			{
				currentProgressInterface = null;
			}
		}
		if (inProgress)
		{
			targetAlpha = 1f;
			fillImage.fillAmount = num;
			if (num >= 1f)
			{
				targetAlpha = 0f;
			}
		}
		else
		{
			targetAlpha = 0f;
		}
		parentCanvasGroup.alpha = Mathf.MoveTowards(parentCanvasGroup.alpha, targetAlpha, 8f * Time.deltaTime);
		if ((bool)stopIndicator && (bool)characterMainControl)
		{
			bool flag = false;
			CharacterActionBase currentAction = characterMainControl.CurrentAction;
			if ((bool)currentAction && currentAction.Running && currentAction.IsStopable())
			{
				flag = true;
			}
			if (flag != stopIndicator.activeSelf && targetAlpha != 0f)
			{
				stopIndicator.SetActive(flag);
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)characterMainControl)
		{
			characterMainControl.OnActionStartEvent -= OnActionStart;
			characterMainControl.OnActionProgressFinishEvent -= OnActionFinish;
		}
	}

	private void OnActionStart(CharacterActionBase action)
	{
		currentProgressInterface = action as IProgress;
		if (specificActionType != CharacterActionBase.ActionPriorities.Whatever && action.ActionPriority() != specificActionType)
		{
			currentProgressInterface = null;
		}
		if ((bool)action && !action.progressHUD)
		{
			currentProgressInterface = null;
		}
	}

	private void OnActionFinish(CharacterActionBase action)
	{
		OnFinishEvent?.Invoke();
		if ((bool)fillImage)
		{
			fillImage.fillAmount = 1f;
		}
	}
}
