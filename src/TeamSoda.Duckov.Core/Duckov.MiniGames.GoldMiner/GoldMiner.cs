using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using Saves;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMiner : MiniGameBehaviour
{
	[SerializeField]
	private Hook hook;

	[SerializeField]
	private GoldMinerShop shop;

	[SerializeField]
	private LevelSettlementUI settlementUI;

	[SerializeField]
	private GameObject titleScreen;

	[SerializeField]
	private GameObject gameoverScreen;

	[SerializeField]
	private GoldMiner_PopText popText;

	[SerializeField]
	private Transform levelLayout;

	[SerializeField]
	private Bounds bounds;

	[SerializeField]
	private Bomb bombPrefab;

	[SerializeField]
	private RandomContainer<GoldMinerEntity> entities;

	[SerializeField]
	private List<GoldMinerArtifact> artifactPrefabs = new List<GoldMinerArtifact>();

	private ReadOnlyCollection<GoldMinerArtifact> artifactPrefabs_ReadOnly;

	public Action<GoldMiner> onLevelBegin;

	public Action<GoldMiner> onLevelEnd;

	public Action<GoldMiner> onShopBegin;

	public Action<GoldMiner> onShopEnd;

	public Action<GoldMiner> onEarlyLevelPlayTick;

	public Action<GoldMiner> onLateLevelPlayTick;

	public Action<GoldMiner, Hook> onHookLaunch;

	public Action<GoldMiner, Hook> onHookBeginRetrieve;

	public Action<GoldMiner, Hook> onHookEndRetrieve;

	public Action<GoldMiner, Hook, GoldMinerEntity> onHookAttach;

	public Action<GoldMiner, GoldMinerEntity> onResolveEntity;

	public Action<GoldMiner, GoldMinerEntity> onAfterResolveEntity;

	public Action<GoldMiner> onArtifactChange;

	private const string HighLevelSaveKey = "MiniGame/GoldMiner/HighLevel";

	private bool titleConfirmed;

	private bool gameOverConfirmed;

	public List<GoldMinerEntity> activeEntities = new List<GoldMinerEntity>();

	private bool levelPlaying;

	public List<GoldMinerEntity> resolvedEntities = new List<GoldMinerEntity>();

	private bool launchHook;

	public Hook Hook => hook;

	public Bounds Bounds => bounds;

	public int Money
	{
		get
		{
			if (run == null)
			{
				return 0;
			}
			return run.money;
		}
	}

	public ReadOnlyCollection<GoldMinerArtifact> ArtifactPrefabs
	{
		get
		{
			if (artifactPrefabs_ReadOnly == null)
			{
				artifactPrefabs_ReadOnly = new ReadOnlyCollection<GoldMinerArtifact>(artifactPrefabs);
			}
			return artifactPrefabs_ReadOnly;
		}
	}

	public static int HighLevel
	{
		get
		{
			return SavesSystem.Load<int>("MiniGame/GoldMiner/HighLevel");
		}
		set
		{
			SavesSystem.Save("MiniGame/GoldMiner/HighLevel", value);
		}
	}

	public GoldMinerRunData run { get; private set; }

	private bool ShouldQuit
	{
		get
		{
			if (base.gameObject == null)
			{
				return true;
			}
			return false;
		}
	}

	public float GlobalPriceFactor => 1f;

	public static event Action<int> OnLevelClear;

	private void Awake()
	{
		Hook.OnBeginRetrieve += OnHookBeginRetrieve;
		Hook.OnEndRetrieve += OnHookEndRetrieve;
		Hook.OnLaunch += OnHookLaunch;
		Hook.OnResolveTarget += OnHookResolveEntity;
		Hook.OnAttach += OnHookAttach;
	}

	protected override void Start()
	{
		base.Start();
		hook.BeginSwing();
		Main().Forget();
	}

	internal bool PayMoney(int price)
	{
		if (run.money < price)
		{
			return false;
		}
		run.money -= price;
		return true;
	}

	private async UniTask Main()
	{
		await DoTitleScreen();
		while (!ShouldQuit)
		{
			Cleanup();
			int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			await Run(seed);
		}
	}

	private async UniTask DoTitleScreen()
	{
		titleConfirmed = false;
		if ((bool)titleScreen)
		{
			titleScreen.SetActive(value: true);
		}
		while (!titleConfirmed)
		{
			await UniTask.Yield();
			if (base.Game.GetButtonDown(MiniGame.Button.A) || base.Game.GetButtonDown(MiniGame.Button.Start))
			{
				titleConfirmed = true;
			}
		}
		if ((bool)titleScreen)
		{
			titleScreen.SetActive(value: false);
		}
	}

	private async UniTask DoGameOver()
	{
		Debug.Log("Game Over");
	}

	public void Cleanup()
	{
		if (run != null)
		{
			run.Cleanup();
		}
	}

	private void GenerateLevel()
	{
		for (int i = 0; i < activeEntities.Count; i++)
		{
			GoldMinerEntity goldMinerEntity = activeEntities[i];
			if (!(goldMinerEntity == null))
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(goldMinerEntity.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(goldMinerEntity.gameObject);
				}
			}
		}
		activeEntities.Clear();
		for (int j = 0; j < resolvedEntities.Count; j++)
		{
			GoldMinerEntity goldMinerEntity2 = activeEntities[j];
			if (!(goldMinerEntity2 == null))
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(goldMinerEntity2.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(goldMinerEntity2.gameObject);
				}
			}
		}
		resolvedEntities.Clear();
		int seed = run.levelRandom.Next();
		System.Random levelGenRandom = new System.Random(seed);
		int minValue = 10;
		int maxValue = 20;
		int num = levelGenRandom.Next(minValue, maxValue);
		for (int k = 0; k < num; k++)
		{
			GoldMinerEntity random = entities.GetRandom(levelGenRandom);
			Generate(random);
		}
		for (float num2 = run.extraRocks; num2 > 0f; num2 -= 1f)
		{
			if (num2 > 1f || levelGenRandom.NextDouble() < (double)num2)
			{
				GoldMinerEntity random2 = entities.GetRandom(levelGenRandom, (GoldMinerEntity e) => e.tags.Contains(GoldMinerEntity.Tag.Rock));
				Generate(random2);
			}
		}
		for (float num3 = run.extraGold; num3 > 0f; num3 -= 1f)
		{
			if (num3 > 1f || levelGenRandom.NextDouble() < (double)num3)
			{
				GoldMinerEntity random3 = entities.GetRandom(levelGenRandom, (GoldMinerEntity e) => e.tags.Contains(GoldMinerEntity.Tag.Gold));
				Generate(random3);
			}
		}
		for (float num4 = run.extraDiamond; num4 > 0f; num4 -= 1f)
		{
			if (num4 > 1f || levelGenRandom.NextDouble() < (double)num4)
			{
				GoldMinerEntity random4 = entities.GetRandom(levelGenRandom, (GoldMinerEntity e) => e.tags.Contains(GoldMinerEntity.Tag.Diamond));
				Generate(random4);
			}
		}
		run.shopRandom = new System.Random(run.seed + levelGenRandom.Next());
		void Generate(GoldMinerEntity entityPrefab)
		{
			if (!(entityPrefab == null))
			{
				Vector2 posNormalized = new Vector2((float)levelGenRandom.NextDouble(), (float)levelGenRandom.NextDouble());
				GoldMinerEntity goldMinerEntity3 = UnityEngine.Object.Instantiate(entityPrefab, levelLayout);
				Vector3 localPosition = NormalizedPosToLocalPos(posNormalized);
				Quaternion localRotation = Quaternion.AngleAxis((float)levelGenRandom.NextDouble() * 360f, Vector3.forward);
				goldMinerEntity3.transform.localPosition = localPosition;
				goldMinerEntity3.transform.localRotation = localRotation;
				goldMinerEntity3.SetMaster(this);
				activeEntities.Add(goldMinerEntity3);
			}
		}
	}

	private Vector3 NormalizedPosToLocalPos(Vector2 posNormalized)
	{
		float x = Mathf.Lerp(bounds.min.x, bounds.max.x, posNormalized.x);
		float y = Mathf.Lerp(bounds.min.y, bounds.max.y, posNormalized.y);
		return new Vector3(x, y, 0f);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = levelLayout.localToWorldMatrix;
		Gizmos.DrawWireCube(bounds.center, bounds.extents * 2f);
	}

	private async UniTask Run(int seed = 0)
	{
		run = new GoldMinerRunData(this, seed);
		while (true)
		{
			await DoLevel();
			if (!(await SettleLevel()))
			{
				break;
			}
			if (run.level > HighLevel)
			{
				HighLevel = run.level;
			}
			await DoShop();
			run.level++;
		}
		await DoGameOver();
	}

	private async UniTask<bool> SettleLevel()
	{
		int moneySum = 0;
		float factor = run.scoreFactorBase.Value + run.levelScoreFactor;
		int targetScore = run.targetScore;
		settlementUI.Reset();
		settlementUI.SetTargetScore(targetScore);
		settlementUI.Show();
		await UniTask.WaitForSeconds(0.5f);
		foreach (GoldMinerEntity resolvedEntity in resolvedEntities)
		{
			int value = resolvedEntity.Value;
			value = Mathf.CeilToInt((float)value * run.charm.Value);
			foreach (Func<int, int> item in run.settleValueProcessor)
			{
				value = item(value);
			}
			if (value != 0)
			{
				moneySum += value;
				int score = Mathf.CeilToInt((float)moneySum * factor);
				settlementUI.StepResolveEntity(resolvedEntity);
				settlementUI.Step(moneySum, factor, score);
				await UniTask.WaitForSeconds(0.2f);
			}
		}
		foreach (Func<float> additionalFactorFunc in run.additionalFactorFuncs)
		{
			float num = additionalFactorFunc();
			if (num != 0f)
			{
				factor += num;
				int score2 = Mathf.CeilToInt((float)moneySum * factor);
				settlementUI.Step(moneySum, factor, score2);
				await UniTask.WaitForSeconds(0.2f);
			}
		}
		if (moneySum < run.minMoneySum)
		{
			moneySum = run.minMoneySum;
		}
		int num2 = Mathf.CeilToInt((float)moneySum * factor);
		settlementUI.Step(moneySum, factor, num2);
		bool result = num2 >= targetScore;
		settlementUI.StepResult(result);
		if (!result)
		{
			for (int i = 0; i < run.forceLevelSuccessFuncs.Count; i++)
			{
				Func<bool> func = run.forceLevelSuccessFuncs[i];
				if (func != null && func())
				{
					result = true;
					settlementUI.StepResult(result);
					break;
				}
			}
		}
		run.money += moneySum;
		while (!base.Game.GetButton(MiniGame.Button.A))
		{
			await UniTask.Yield();
		}
		settlementUI.Hide();
		if (result)
		{
			GoldMiner.OnLevelClear?.Invoke(run.level);
		}
		return result;
	}

	private async UniTask DoLevel()
	{
		Hook.Reset();
		resolvedEntities.Clear();
		GenerateLevel();
		run.levelScoreFactor = 0f;
		run.stamina = run.maxStamina.Value;
		int level = run.level;
		run.targetScore = Mathf.CeilToInt(40.564f * Mathf.Exp(0.2118f * (float)(level + 1))) * 10;
		onLevelBegin?.Invoke(this);
		popText.Pop($"LEVEL {run.level + 1}", hook.Axis.position);
		await UniTask.WaitForSeconds(0.5f);
		popText.Pop("Begin!", hook.Axis.position);
		levelPlaying = true;
		launchHook = false;
		while (!IsLevelOver())
		{
			await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
		}
		onLevelEnd?.Invoke(this);
		levelPlaying = false;
		if ((bool)Hook.GrabbingTarget)
		{
			Hook.ReleaseClaw();
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (levelPlaying)
		{
			UpdateLevelPlaying(deltaTime);
		}
	}

	private void UpdateLevelPlaying(float deltaTime)
	{
		onEarlyLevelPlayTick?.Invoke(this);
		Hook.SetParameters(run.GameSpeedFactor, run.emptySpeed.Value, run.strength.Value);
		Hook.Tick(deltaTime);
		switch (Hook.Status)
		{
		case Hook.HookStatus.Swinging:
			if (launchHook)
			{
				Hook.Launch();
			}
			break;
		case Hook.HookStatus.Retrieving:
			run.stamina -= deltaTime * run.staminaDrain.Value;
			break;
		}
		onLateLevelPlayTick?.Invoke(this);
		launchHook = false;
	}

	public void LaunchHook()
	{
		launchHook = true;
	}

	private bool IsLevelOver()
	{
		activeEntities.RemoveAll((GoldMinerEntity e) => e == null);
		if (activeEntities.Count <= 0)
		{
			return true;
		}
		if (hook.Status == Hook.HookStatus.Swinging && run.stamina <= 0f)
		{
			return true;
		}
		if (Hook.Status == Hook.HookStatus.Retrieving && run.stamina < 0f - run.extraStamina.Value)
		{
			return true;
		}
		return false;
	}

	private async UniTask DoShop()
	{
		onShopBegin?.Invoke(this);
		await shop.Execute();
		onShopEnd?.Invoke(this);
	}

	private void OnHookResolveEntity(Hook hook, GoldMinerEntity entity)
	{
		entity.NotifyResolved(this);
		entity.gameObject.SetActive(value: false);
		activeEntities.Remove(entity);
		resolvedEntities.Add(entity);
		if (run.IsRock(entity))
		{
			entity.Value = Mathf.CeilToInt((float)entity.Value * run.rockValueFactor.Value);
		}
		if (run.IsGold(entity))
		{
			entity.Value = Mathf.CeilToInt((float)entity.Value * run.goldValueFactor.Value);
		}
		popText.Pop($"${entity.Value}", hook.Axis.position);
		onResolveEntity?.Invoke(this, entity);
		onAfterResolveEntity?.Invoke(this, entity);
	}

	private void OnHookBeginRetrieve(Hook hook)
	{
		onHookBeginRetrieve?.Invoke(this, hook);
	}

	private void OnHookEndRetrieve(Hook hook)
	{
		onHookEndRetrieve?.Invoke(this, hook);
		if (run.StrengthPotionActivated)
		{
			run.DeactivateStrengthPotion();
		}
	}

	private void OnHookLaunch(Hook hook)
	{
		onHookLaunch?.Invoke(this, hook);
		if (run.EagleEyeActivated)
		{
			run.DeactivateEagleEye();
		}
	}

	private void OnHookAttach(Hook hook, GoldMinerEntity entity)
	{
		onHookAttach?.Invoke(this, hook, entity);
	}

	public bool UseStrengthPotion()
	{
		if (run.strengthPotion <= 0)
		{
			return false;
		}
		if (run.StrengthPotionActivated)
		{
			return false;
		}
		run.strengthPotion--;
		run.ActivateStrengthPotion();
		return true;
	}

	public bool UseEagleEyePotion()
	{
		if (run.eagleEyePotion <= 0)
		{
			return false;
		}
		if (run.EagleEyeActivated)
		{
			return false;
		}
		run.eagleEyePotion--;
		run.ActivateEagleEye();
		return true;
	}

	public GoldMinerArtifact GetArtifactPrefab(string id)
	{
		return artifactPrefabs.Find((GoldMinerArtifact e) => e != null && e.ID == id);
	}

	internal bool UseBomb()
	{
		if (run.bomb <= 0)
		{
			return false;
		}
		run.bomb--;
		UnityEngine.Object.Instantiate(bombPrefab, hook.Axis.transform.position, Quaternion.FromToRotation(Vector3.up, -hook.Axis.transform.up), base.transform);
		return true;
	}

	internal void NotifyArtifactChange()
	{
		onArtifactChange?.Invoke(this);
	}
}
