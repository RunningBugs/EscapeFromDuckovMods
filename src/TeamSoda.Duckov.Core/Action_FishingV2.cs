using System;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Quests.Conditions;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using UnityEngine;

public class Action_FishingV2 : CharacterActionBase
{
	public enum FishingStates
	{
		non,
		throwing,
		waiting,
		ring,
		cancleBack,
		successBack,
		failBack
	}

	public InteractableBase interactable;

	public Transform baitVisual;

	public TrailRenderer baitTrail;

	public Canvas fishingHudCanvas;

	public Transform targetPoint;

	public Transform bucketPoint;

	[LocalizationKey("Default")]
	public string noRodText = "Pop_NoRod";

	[LocalizationKey("Default")]
	public string noBaitText = "Pop_NoBait";

	[LocalizationKey("Default")]
	public string gotFishText = "Notify_GotFish";

	[LocalizationKey("Default")]
	public string failText = "Notify_FishRunAway";

	private FishingRod rod;

	private ItemAgent rodAgent;

	private Item baitItem;

	public Animator ringAnimator;

	public Vector2 waitTimeRange = new Vector2(3f, 9f);

	private float waitTime;

	public Vector2 scaleRange = new Vector2(0.5f, 3f);

	public Vector2 successRange = new Vector2(0.75f, 1.1f);

	private float ringScaling = 2.5f;

	private float stateTimer;

	private bool catchInput;

	public Transform scaleRing;

	public LineRenderer lineRenderer;

	public float throwStartTime = 0.1f;

	public float outTime;

	public AnimationCurve outYCurve;

	public ParticleSystem waveParticle;

	public GameObject dropParticle;

	public GameObject bucketParticle;

	public InteractableLootbox lootbox;

	private bool hookFxSpawned;

	public GameObject hookFx;

	public float backTime;

	public AnimationCurve backYCurve;

	private Vector3 hookStartPoint;

	public GameObject gotFx;

	public FishSpawner lootSpawner;

	private Item currentFish;

	private float luck = 1f;

	private float scaleTime;

	private float scaleTimeFactor = 1.25f;

	private int fishingTimeHash = "FishingTime".GetHashCode();

	private int fishingDifficultyHash = "FishingDifficulty".GetHashCode();

	private int fishingQualityFactorHash = "FishingQualityFactor".GetHashCode();

	private Slot characterMeleeWeaponSlot;

	private string currentStateInfo;

	private string throwSoundKey = "SFX/Actions/Fishing_Throw";

	private string startFishingSoundKey = "SFX/Actions/Fishing_Start";

	private string pulloutSoundKey = "SFX/Actions/Fishing_PullOut";

	private string baitSoundKey = "SFX/Actions/Fishing_Bait";

	private string successSoundKey = "SFX/Actions/Fishing_Success";

	private string failSoundKey = "SFX/Actions/Fishing_Failed";

	public FishingStates fishingState = FishingStates.waiting;

	private bool needStopAction;

	public override ActionPriorities ActionPriority()
	{
		return ActionPriorities.Fishing;
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

	private void Awake()
	{
		interactable.OnInteractTimeoutEvent.AddListener(OnInteractTimeOut);
		interactable.finishWhenTimeOut = false;
		fishingHudCanvas.gameObject.SetActive(value: false);
		baitVisual.gameObject.SetActive(value: false);
		baitTrail.gameObject.SetActive(value: false);
		dropParticle.SetActive(value: false);
		bucketParticle.SetActive(value: false);
		gotFx.SetActive(value: false);
		SyncInteractable(CharacterMainControl.Main);
		CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent = (Action<CharacterMainControl, DuckovItemAgent>)Delegate.Combine(CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent, new Action<CharacterMainControl, DuckovItemAgent>(OnMainCharacterChangeItemAgent));
		TransToNon();
	}

	private void OnMainCharacterChangeItemAgent(CharacterMainControl character, DuckovItemAgent agent)
	{
		SyncInteractable(character);
	}

	private void SyncInteractable(CharacterMainControl character)
	{
		if (!character)
		{
			interactable.gameObject.SetActive(value: false);
			return;
		}
		DuckovItemAgent currentHoldItemAgent = character.CurrentHoldItemAgent;
		if (!currentHoldItemAgent)
		{
			interactable.gameObject.SetActive(value: false);
			return;
		}
		FishingRod component = currentHoldItemAgent.GetComponent<FishingRod>();
		interactable.gameObject.SetActive(component != null);
	}

	private void SetWaveEmissionRate(float rate)
	{
		ParticleSystem.EmissionModule emission = waveParticle.emission;
		emission.rateOverTime = rate;
	}

	private void OnDestroy()
	{
		if ((bool)interactable)
		{
			interactable.OnInteractTimeoutEvent.RemoveListener(OnInteractTimeOut);
		}
		CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent = (Action<CharacterMainControl, DuckovItemAgent>)Delegate.Remove(CharacterMainControl.OnMainCharacterChangeHoldItemAgentEvent, new Action<CharacterMainControl, DuckovItemAgent>(OnMainCharacterChangeItemAgent));
	}

	public void TryCatch()
	{
		Debug.Log("TryCatch");
		if (fishingState == FishingStates.waiting || fishingState == FishingStates.ring)
		{
			catchInput = true;
		}
	}

	private void OnInteractTimeOut(CharacterMainControl target, InteractableBase interactable)
	{
		interactable.StopInteract();
		target.StartAction(this);
	}

	public override bool IsReady()
	{
		return !base.Running;
	}

	protected override bool OnStart()
	{
		if (characterController == null)
		{
			StopAction();
		}
		waitTime = UnityEngine.Random.Range(waitTimeRange.x, waitTimeRange.y);
		ringAnimator.SetInteger("State", 0);
		rodAgent = characterController.CurrentHoldItemAgent;
		if (!rodAgent)
		{
			characterController.PopText(noRodText.ToPlainText());
			return false;
		}
		rod = rodAgent.GetComponent<FishingRod>();
		if (!rod)
		{
			characterController.PopText(noRodText.ToPlainText());
			return false;
		}
		baitItem = rod.Bait;
		if (!baitItem)
		{
			characterController.PopText(noBaitText.ToPlainText());
			return false;
		}
		characterController.characterModel.ForcePlayAttackAnimation();
		Vector3 direction = targetPoint.position - characterController.transform.position;
		direction.y = 0f;
		direction.Normalize();
		characterController.movementControl.ForceTurnTo(direction);
		fishingHudCanvas.worldCamera = Camera.main;
		fishingHudCanvas.gameObject.SetActive(value: true);
		hookStartPoint = rod.lineStart.position;
		TransToThrowing();
		return true;
	}

	protected override void OnStop()
	{
		TransToNon();
		fishingHudCanvas.gameObject.SetActive(value: false);
		ringAnimator.gameObject.SetActive(value: false);
		lineRenderer.gameObject.SetActive(value: false);
		baitVisual.gameObject.SetActive(value: false);
		gotFx.SetActive(value: false);
		SetWaveEmissionRate(0f);
		ringAnimator.SetInteger("State", 0);
		if ((bool)currentFish)
		{
			currentFish.DestroyTree();
			currentFish = null;
		}
	}

	private void SpawnDropParticle()
	{
		UnityEngine.Object.Instantiate(dropParticle, targetPoint).SetActive(value: true);
	}

	private void SpawnBucketParticle()
	{
		UnityEngine.Object.Instantiate(bucketParticle, bucketPoint).SetActive(value: true);
	}

	private void OnDisable()
	{
		if (base.Running)
		{
			StopAction();
		}
		fishingHudCanvas.gameObject.SetActive(value: false);
	}

	public override bool IsStopable()
	{
		return needStopAction;
	}

	private Vector3 GetHookOutPos(float lerpValue)
	{
		lerpValue = Mathf.Clamp01(lerpValue);
		Vector3 a = hookStartPoint;
		Vector3 position = targetPoint.position;
		Vector3 result = Vector3.Lerp(a, position, lerpValue);
		float y = Mathf.LerpUnclamped(position.y, a.y, outYCurve.Evaluate(lerpValue));
		result.y = y;
		return result;
	}

	private Vector3 GetHookBackPos(float lerpValue)
	{
		lerpValue = Mathf.Clamp01(lerpValue);
		Vector3 position = rod.lineStart.position;
		Vector3 position2 = targetPoint.position;
		Vector3 result = Vector3.Lerp(position2, position, lerpValue);
		float y = Mathf.LerpUnclamped(position2.y, position.y, backYCurve.Evaluate(lerpValue));
		result.y = y;
		return result;
	}

	protected override void OnUpdateAction(float deltaTime)
	{
		if (!characterController || !rod)
		{
			needStopAction = true;
			StopAction();
			return;
		}
		lineRenderer.SetPosition(0, rod.lineStart.position);
		Vector3 position = rod.lineStart.position;
		needStopAction = false;
		if (rod == null)
		{
			needStopAction = true;
			StopAction();
			return;
		}
		switch (fishingState)
		{
		case FishingStates.throwing:
			if (!baitItem || catchInput)
			{
				TransToCancleBack();
			}
			else if (stateTimer < throwStartTime)
			{
				hookStartPoint = rod.lineStart.position;
				position = hookStartPoint;
				baitTrail.Clear();
			}
			else if (stateTimer < outTime)
			{
				position = GetHookOutPos((stateTimer - throwStartTime) / (outTime - throwStartTime));
				if (!baitVisual.gameObject.activeInHierarchy)
				{
					baitVisual.gameObject.SetActive(value: true);
					baitTrail.gameObject.SetActive(value: true);
				}
				baitVisual.transform.position = position;
				baitTrail.transform.position = position;
			}
			else
			{
				TransToWaiting();
			}
			break;
		case FishingStates.waiting:
			if (catchInput)
			{
				TransToCancleBack();
				break;
			}
			position = targetPoint.position;
			baitVisual.transform.position = position;
			baitTrail.transform.position = position;
			if (stateTimer >= waitTime)
			{
				if (currentFish != null)
				{
					TransToRing();
				}
				else
				{
					characterController.PopText("Error:Spawn fish failed");
					TransToCancleBack();
				}
			}
			if (waitTime - stateTimer < 0.25f && !hookFxSpawned)
			{
				hookFxSpawned = true;
				SpawnHookFx();
			}
			break;
		case FishingStates.ring:
		{
			position = targetPoint.position;
			float num2 = Mathf.Lerp(scaleRange.y, scaleRange.x, 1f - stateTimer / scaleTime);
			scaleRing.localScale = Vector3.one * num2;
			if (catchInput)
			{
				if (num2 < successRange.x || num2 > successRange.y)
				{
					TransToFailBack();
					break;
				}
				TransToSuccessback();
			}
			if (stateTimer > scaleTime)
			{
				TransToFailBack();
			}
			break;
		}
		case FishingStates.cancleBack:
			position = GetHookBackPos(stateTimer / backTime);
			baitVisual.transform.position = position;
			baitTrail.transform.position = position;
			if (stateTimer > backTime)
			{
				needStopAction = true;
			}
			break;
		case FishingStates.successBack:
		{
			float num = 0.2f;
			if (!(stateTimer < num))
			{
				position = GetHookBackPos((stateTimer - num) / backTime);
				baitVisual.transform.position = position;
				baitTrail.transform.position = position;
				if (stateTimer - num > backTime)
				{
					needStopAction = true;
				}
			}
			break;
		}
		case FishingStates.failBack:
			position = GetHookBackPos(stateTimer / backTime);
			baitVisual.transform.position = position;
			baitTrail.transform.position = position;
			if (stateTimer > backTime)
			{
				NotificationText.Push(failText.ToPlainText());
				needStopAction = true;
			}
			break;
		}
		lineRenderer.SetPosition(1, position);
		catchInput = false;
		stateTimer += deltaTime;
		if (needStopAction)
		{
			baitVisual.gameObject.SetActive(value: false);
			baitTrail.gameObject.SetActive(value: false);
			baitTrail.Clear();
			StopAction();
		}
	}

	private void TransToNon()
	{
		fishingState = FishingStates.non;
		SetWaveEmissionRate(0f);
	}

	private void TransToThrowing()
	{
		AudioManager.Post(throwSoundKey, base.gameObject);
		stateTimer = 0f;
		lineRenderer.gameObject.SetActive(value: true);
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, rod.lineStart.position);
		lineRenderer.SetPosition(1, rod.lineStart.position);
		ringAnimator.gameObject.SetActive(value: true);
		ringAnimator.SetInteger("State", 0);
		fishingState = FishingStates.throwing;
	}

	private void TransToWaiting()
	{
		if (baitItem == null)
		{
			needStopAction = true;
			StopAction();
		}
		AudioManager.Post(startFishingSoundKey, targetPoint.gameObject);
		hookFxSpawned = false;
		SpawnDropParticle();
		SetWaveEmissionRate(1.5f);
		stateTimer = 0f;
		ringAnimator.SetInteger("State", 0);
		luck = characterController.CharacterItem.GetStatValue(fishingQualityFactorHash);
		SpawnFish(luck).Forget();
		fishingState = FishingStates.waiting;
	}

	private void SpawnHookFx()
	{
		if (!(hookFx == null))
		{
			UnityEngine.Object.Instantiate(hookFx, targetPoint.position + Vector3.up * 3f, Quaternion.identity);
			AudioManager.Post(baitSoundKey, targetPoint.gameObject);
		}
	}

	private void TransToRing()
	{
		scaleTime = characterController.CharacterItem.GetStatValue(fishingTimeHash) * scaleTimeFactor;
		scaleTime = Mathf.Max(0.01f, scaleTime);
		float num = currentFish.GetStatValue(fishingDifficultyHash);
		if (num < 0.02f)
		{
			num = 1f;
		}
		scaleTime /= num;
		if (scaleTime > 7f)
		{
			scaleTime = 7f;
		}
		stateTimer = 0f;
		catchInput = false;
		fishingState = FishingStates.ring;
		ringAnimator.SetInteger("State", 1);
	}

	private void TransToCancleBack()
	{
		stateTimer = 0f;
		ringAnimator.SetInteger("State", 0);
		fishingState = FishingStates.cancleBack;
		SetWaveEmissionRate(0f);
		SpawnDropParticle();
		fishingHudCanvas.gameObject.SetActive(value: false);
		AudioManager.Post(pulloutSoundKey, targetPoint.gameObject);
	}

	private void TransToSuccessback()
	{
		stateTimer = 0f;
		ringAnimator.SetInteger("State", 2);
		AudioManager.Post(successSoundKey, targetPoint.gameObject);
		fishingState = FishingStates.successBack;
		SetWaveEmissionRate(0f);
		SpawnDropParticle();
		gotFx.SetActive(value: true);
		fishingHudCanvas.gameObject.SetActive(value: false);
		CatchFish().Forget();
		RequireHasFished.SetHasFished();
	}

	private void TransToFailBack()
	{
		stateTimer = 0f;
		ringAnimator.SetInteger("State", 3);
		AudioManager.Post(failSoundKey, targetPoint.gameObject);
		fishingState = FishingStates.failBack;
		SetWaveEmissionRate(0f);
		SpawnDropParticle();
		fishingHudCanvas.gameObject.SetActive(value: false);
	}

	private async UniTaskVoid SpawnFish(float luck)
	{
		if (!baitItem)
		{
			return;
		}
		int typeID = baitItem.TypeID;
		if (lootbox.Inventory.GetFirstEmptyPosition() == -1)
		{
			lootbox.Inventory.SetCapacity(lootbox.Inventory.Capacity + 5);
		}
		Item item = await lootSpawner.Spawn(typeID, luck);
		if (item == null)
		{
			return;
		}
		item.Inspected = true;
		currentFish = item;
		if ((bool)baitItem)
		{
			if (baitItem.Stackable)
			{
				baitItem.StackCount--;
			}
			else
			{
				baitItem.DestroyTree();
			}
		}
	}

	private async UniTaskVoid CatchFish()
	{
		if (!(currentFish == null))
		{
			string notify = gotFishText.ToPlainText() + " " + currentFish.DisplayName + "!";
			characterController.PickupItem(currentFish);
			currentFish = null;
			await UniTask.WaitForSeconds(0.65f);
			NotificationText.Push(notify);
		}
	}

	public override bool CanEditInventory()
	{
		return false;
	}
}
