using System;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

public class TimeOfDayAlert : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	public TextMeshProUGUI text;

	[SerializeField]
	private ColorPunch blinkPunch;

	[LocalizationKey("Default")]
	public string nearNightKey = "TODAlert_NearNight";

	[LocalizationKey("Default")]
	public string inNightKey = "TODAlert_InNight";

	private float stayTime = 5f;

	private float timer;

	public static event Action OnAlertTriggeredEvent;

	private void Awake()
	{
		canvasGroup.alpha = 0f;
		OnAlertTriggeredEvent += OnAlertTriggered;
	}

	private void OnDestroy()
	{
		OnAlertTriggeredEvent -= OnAlertTriggered;
	}

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		if (!LevelManager.Instance.IsBaseLevel)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
		}
		if (timer <= 0f && canvasGroup.alpha > 0f)
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, 0.4f * Time.unscaledDeltaTime);
		}
	}

	private void OnAlertTriggered()
	{
		bool flag = false;
		float time = TimeOfDayController.Instance.Time;
		if (TimeOfDayController.Instance.AtNight)
		{
			flag = true;
			Debug.Log($"At Night,time:{time}");
			text.text = inNightKey.ToPlainText();
		}
		else if (TimeOfDayController.Instance.nightStart - time < 4f)
		{
			flag = true;
			Debug.Log($"Near Night,time:{time},night start:{TimeOfDayController.Instance.nightStart}");
			text.text = nearNightKey.ToPlainText();
		}
		if (flag)
		{
			canvasGroup.alpha = 1f;
			timer = stayTime;
			blinkPunch.Punch();
		}
	}

	public static void EnterAlertTrigger()
	{
		TimeOfDayAlert.OnAlertTriggeredEvent?.Invoke();
	}

	public static void LeaveAlertTrigger()
	{
	}
}
