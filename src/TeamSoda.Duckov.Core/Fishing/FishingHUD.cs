using System;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing;

public class FishingHUD : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Image countDownFill;

	[SerializeField]
	private FadeGroup succeedIndicator;

	[SerializeField]
	private FadeGroup failIndicator;

	private void Awake()
	{
		Action_Fishing.OnPlayerStartCatching += OnStartCatching;
		Action_Fishing.OnPlayerStopCatching += OnStopCatching;
		Action_Fishing.OnPlayerStopFishing += OnStopFishing;
	}

	private void OnDestroy()
	{
		Action_Fishing.OnPlayerStartCatching -= OnStartCatching;
		Action_Fishing.OnPlayerStopCatching -= OnStopCatching;
		Action_Fishing.OnPlayerStopFishing -= OnStopFishing;
	}

	private void OnStopFishing(Action_Fishing fishing)
	{
		fadeGroup.Hide();
	}

	private void OnStopCatching(Action_Fishing fishing, Item item, Action<bool> action)
	{
		StopCatchingTask(item, action).Forget();
	}

	private void OnStartCatching(Action_Fishing fishing, float totalTime, Func<float> currentTimeGetter)
	{
		CatchingTask(fishing, totalTime, currentTimeGetter).Forget();
	}

	private async UniTask CatchingTask(Action_Fishing fishing, float totalTime, Func<float> currentTimeGetter)
	{
		succeedIndicator.SkipHide();
		failIndicator.SkipHide();
		fadeGroup.Show();
		while (fishing.Running && fishing.FishingState == Action_Fishing.FishingStates.catching)
		{
			UpdateBar(totalTime, currentTimeGetter());
			await UniTask.Yield();
		}
		if (!fishing.Running)
		{
			fadeGroup.Hide();
		}
	}

	private void UpdateBar(float totalTime, float currentTime)
	{
		if (!(totalTime <= 0f))
		{
			float fillAmount = 1f - currentTime / totalTime;
			countDownFill.fillAmount = fillAmount;
		}
	}

	private async UniTask StopCatchingTask(Item item, Action<bool> confirmCallback)
	{
		if (item == null)
		{
			failIndicator.Show();
		}
		else
		{
			succeedIndicator.Show();
		}
		fadeGroup.Hide();
	}
}
