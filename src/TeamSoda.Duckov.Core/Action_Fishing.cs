using System;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using FX;
using ItemStatsSystem;
using UnityEngine;

public class Action_Fishing : CharacterActionBase
{
	public enum FishingStates
	{
		notStarted,
		intro,
		selectingBait,
		fishing,
		catching,
		over
	}

	[SerializeField]
	private CinemachineVirtualCamera fishingCamera;

	private FishingRod fishingRod;

	[SerializeField]
	private FishingPoint fishingPoint;

	[SerializeField]
	private float introTime = 0.2f;

	private float fishingWaitTime = 2f;

	private float catchTime = 0.5f;

	private Item bait;

	private Transform socket;

	[SerializeField]
	[ItemTypeID]
	private int testCatchItem;

	private Item catchedItem;

	private bool quit;

	private UniTask currentTask;

	private bool catchInput;

	private bool resultConfirmed;

	private bool continueFishing;

	private FishingStates fishingState;

	private int fishingTaskToken;

	public FishingStates FishingState => fishingState;

	public static event Action<Action_Fishing, ICollection<Item>, Func<Item, bool>> OnPlayerStartSelectBait;

	public static event Action<Action_Fishing> OnPlayerStartFishing;

	public static event Action<Action_Fishing, float, Func<float>> OnPlayerStartCatching;

	public static event Action<Action_Fishing, Item, Action<bool>> OnPlayerStopCatching;

	public static event Action<Action_Fishing> OnPlayerStopFishing;

	private void Awake()
	{
		fishingCamera.gameObject.SetActive(value: false);
	}

	public override bool CanEditInventory()
	{
		return false;
	}

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Fishing;
	}

	protected override bool OnStart()
	{
		if (!characterController)
		{
			return false;
		}
		fishingCamera.gameObject.SetActive(value: true);
		fishingRod = characterController.CurrentHoldItemAgent.GetComponent<FishingRod>();
		bool result = fishingRod != null;
		currentTask = Fishing();
		InputManager.OnInteractButtonDown = (Action)Delegate.Remove(InputManager.OnInteractButtonDown, new Action(OnCatchButton));
		InputManager.OnInteractButtonDown = (Action)Delegate.Combine(InputManager.OnInteractButtonDown, new Action(OnCatchButton));
		UIInputManager.OnCancel -= UIOnCancle;
		UIInputManager.OnCancel += UIOnCancle;
		return result;
	}

	private void OnCatchButton()
	{
		if (fishingState == FishingStates.catching)
		{
			catchInput = true;
		}
	}

	private void UIOnCancle(UIInputEventData data)
	{
		data.Use();
		Quit();
	}

	protected override void OnStop()
	{
		base.OnStop();
		fishingState = FishingStates.notStarted;
		Action_Fishing.OnPlayerStopFishing?.Invoke(this);
		InputManager.OnInteractButtonDown = (Action)Delegate.Remove(InputManager.OnInteractButtonDown, new Action(OnCatchButton));
		UIInputManager.OnCancel -= UIOnCancle;
		fishingCamera.gameObject.SetActive(value: false);
	}

	public override bool CanControlAim()
	{
		return false;
	}

	public override bool CanMove()
	{
		return false;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanUseHand()
	{
		return false;
	}

	public override bool IsReady()
	{
		return true;
	}

	private int NewToken()
	{
		fishingTaskToken++;
		fishingTaskToken %= 1000;
		return fishingTaskToken;
	}

	private async UniTask Fishing()
	{
		int token = NewToken();
		quit = false;
		fishingState = FishingStates.intro;
		await UniTask.WaitForSeconds(introTime);
		while (IsTaskValid())
		{
			await UniTask.WaitForEndOfFrame(this);
			await SingleFishingLoop(IsTaskValid);
		}
		bool IsTaskValid()
		{
			bool flag = true;
			if (!characterController)
			{
				flag = false;
			}
			if (!base.Running)
			{
				flag = false;
			}
			if (quit)
			{
				flag = false;
			}
			if (token != fishingTaskToken)
			{
				flag = false;
			}
			if (!flag)
			{
				Debug.Log($"钓鱼终止：当前状态：{fishingState}");
			}
			return flag;
		}
	}

	private async UniTask SingleFishingLoop(Func<bool> IsTaskValid)
	{
		if (IsTaskValid())
		{
			fishingState = FishingStates.selectingBait;
			bool flag = await WaitForSelectBait();
			if (IsTaskValid() && flag)
			{
				fishingState = FishingStates.fishing;
				Action_Fishing.OnPlayerStartFishing?.Invoke(this);
				await UniTask.WaitForSeconds(fishingWaitTime);
				if (IsTaskValid())
				{
					bool flag2 = await Catching(IsTaskValid);
					if (IsTaskValid())
					{
						fishingState = FishingStates.over;
						resultConfirmed = false;
						continueFishing = false;
						if (flag2)
						{
							Item arg = await ItemAssetsCollection.InstantiateAsync(testCatchItem);
							Action_Fishing.OnPlayerStopCatching?.Invoke(this, arg, ResultConfirm);
							PopText.Pop("成功", base.transform.position, Color.white, 1f);
						}
						else
						{
							Action_Fishing.OnPlayerStopCatching?.Invoke(this, null, ResultConfirm);
							PopText.Pop("失败", base.transform.position, Color.white, 1f);
						}
						await UniTask.WaitUntil(() => quit || resultConfirmed);
						if (IsTaskValid() && continueFishing)
						{
							fishingState = FishingStates.notStarted;
							return;
						}
					}
				}
			}
		}
		fishingState = FishingStates.notStarted;
		quit = true;
		if (base.Running)
		{
			StopAction();
		}
	}

	private void ResultConfirm(bool _continueFishing)
	{
		resultConfirmed = true;
		continueFishing = _continueFishing;
	}

	private async UniTask<bool> Catching(Func<bool> IsTaskValid)
	{
		catchInput = false;
		fishingState = FishingStates.catching;
		PopText.Pop($"FFFF,控制:{InputManager.InputActived}", base.transform.position, Color.white, 1f);
		float currentTime = 0f;
		Action_Fishing.OnPlayerStartCatching?.Invoke(this, catchTime, () => currentTime);
		await UniTask.WaitForEndOfFrame(this);
		float startCatchTime = Time.time;
		bool catchOver = false;
		while (!catchOver)
		{
			currentTime = Time.time - startCatchTime;
			if (!IsTaskValid())
			{
				return false;
			}
			if (catchInput && currentTime < catchTime)
			{
				Debug.Log("catch");
				return true;
			}
			if (currentTime >= catchTime)
			{
				return false;
			}
			await UniTask.WaitForEndOfFrame(this);
		}
		return false;
	}

	private async UniTask<bool> WaitForSelectBait()
	{
		bait = null;
		Action_Fishing.OnPlayerStartSelectBait?.Invoke(this, GetAllBaits(), SelectBaitAndStartFishing);
		await UniTask.WaitUntil(() => quit || bait != null);
		if (quit)
		{
			return false;
		}
		return true;
	}

	public List<Item> GetAllBaits()
	{
		List<Item> list = new List<Item>();
		if (!characterController)
		{
			return list;
		}
		foreach (Item item in characterController.CharacterItem.Inventory)
		{
			if (item.Tags.Contains(GameplayDataSettings.Tags.Bait))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void CatchButton()
	{
	}

	public void Quit()
	{
		Debug.Log("Quit");
		quit = true;
	}

	private bool SelectBaitAndStartFishing(Item _bait)
	{
		if (_bait == null)
		{
			Debug.Log("鱼饵选了个null, 退出");
			Quit();
			return false;
		}
		if (!_bait.Tags.Contains(GameplayDataSettings.Tags.Bait))
		{
			Quit();
			return false;
		}
		bait = _bait;
		return true;
	}

	private void OnDestroy()
	{
		if (base.Running)
		{
			Action_Fishing.OnPlayerStopFishing?.Invoke(this);
		}
		InputManager.OnInteractButtonDown = (Action)Delegate.Remove(InputManager.OnInteractButtonDown, new Action(OnCatchButton));
		UIInputManager.OnCancel -= UIOnCancle;
	}
}
