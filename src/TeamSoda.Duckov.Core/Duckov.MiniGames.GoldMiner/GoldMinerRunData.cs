using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

[Serializable]
public class GoldMinerRunData
{
	public readonly GoldMiner master;

	public int money;

	public int bomb;

	public int strengthPotion;

	public int eagleEyePotion;

	public int shopTicket;

	public const int shopDefaultItemAmount = 3;

	public const int shopMaxItemAmount = 3;

	public int shopCapacity = 3;

	public float levelScoreFactor;

	public Stat maxStamina = new Stat("maxStamina", 15f);

	public Stat extraStamina = new Stat("extraStamina", 2f);

	public Stat staminaDrain = new Stat("staminaDrain", 1f);

	public Stat gameSpeedFactor = new Stat("gameSpeedFactor", 1f);

	public Stat emptySpeed = new Stat("emptySpeed", 300f);

	public Stat strength = new Stat("strength", 0f);

	public Stat scoreFactorBase = new Stat("scoreFactor", 1f);

	public Stat rockValueFactor = new Stat("rockValueFactor", 1f);

	public Stat goldValueFactor = new Stat("goldValueFactor", 1f);

	public Stat charm = new Stat("charm", 1f);

	public Stat shopRefreshPrice = new Stat("shopRefreshPrice", 100f);

	public Stat shopRefreshPriceIncrement = new Stat("shopRefreshPriceIncrement", 50f);

	public Stat shopRefreshChances = new Stat("shopRefreshChances", 2f);

	public Stat shopPriceCut = new Stat("shopPriceCut", 0.7f);

	public Stat defuse = new Stat("defuse", 0f);

	public float extraRocks;

	public float extraGold;

	public float extraDiamond;

	public List<GoldMinerArtifact> artifacts = new List<GoldMinerArtifact>();

	private Dictionary<string, int> artifactCount = new Dictionary<string, int>();

	private Modifier strengthPotionModifier;

	private Modifier eagleEyeModifier;

	internal int targetScore = 100;

	public List<Func<GoldMinerEntity, bool>> isGoldPredicators = new List<Func<GoldMinerEntity, bool>>();

	public List<Func<GoldMinerEntity, bool>> isRockPredicators = new List<Func<GoldMinerEntity, bool>>();

	public List<Func<float>> additionalFactorFuncs = new List<Func<float>>();

	public List<Func<int, int>> settleValueProcessor = new List<Func<int, int>>();

	public List<Func<bool>> forceLevelSuccessFuncs = new List<Func<bool>>();

	internal int minMoneySum;

	public int seed { get; private set; }

	public System.Random shopRandom { get; set; }

	public System.Random levelRandom { get; private set; }

	public float GameSpeedFactor => gameSpeedFactor.Value;

	public float stamina { get; set; }

	public bool gameOver { get; set; }

	public int level { get; set; }

	public bool StrengthPotionActivated { get; private set; }

	public bool EagleEyeActivated { get; private set; }

	public GoldMinerArtifact AttachArtifactFromPrefab(GoldMinerArtifact prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		GoldMinerArtifact goldMinerArtifact = UnityEngine.Object.Instantiate(prefab, master.transform);
		AttachArtifact(goldMinerArtifact);
		return goldMinerArtifact;
	}

	private void AttachArtifact(GoldMinerArtifact artifact)
	{
		if (artifactCount.ContainsKey(artifact.ID))
		{
			artifactCount[artifact.ID]++;
		}
		else
		{
			artifactCount[artifact.ID] = 1;
		}
		artifacts.Add(artifact);
		artifact.Attach(master);
		master.NotifyArtifactChange();
	}

	public bool DetachArtifact(GoldMinerArtifact artifact)
	{
		bool result = artifacts.Remove(artifact);
		artifact.Detatch(master);
		if (artifactCount.ContainsKey(artifact.ID))
		{
			artifactCount[artifact.ID]--;
		}
		else
		{
			Debug.LogError("Artifact counter error.", master);
		}
		master.NotifyArtifactChange();
		return result;
	}

	public int GetArtifactCount(string id)
	{
		if (artifactCount.TryGetValue(id, out var value))
		{
			return value;
		}
		return 0;
	}

	public GoldMinerRunData(GoldMiner master, int seed = 0)
	{
		this.master = master;
		if (seed == 0)
		{
			seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
		this.seed = seed;
		levelRandom = new System.Random(seed);
		strengthPotionModifier = new Modifier(ModifierType.Add, 100f, this);
		eagleEyeModifier = new Modifier(ModifierType.PercentageMultiply, -0.5f, this);
	}

	public void ActivateStrengthPotion()
	{
		if (!StrengthPotionActivated)
		{
			strength.AddModifier(strengthPotionModifier);
			StrengthPotionActivated = true;
		}
	}

	public void DeactivateStrengthPotion()
	{
		strength.RemoveModifier(strengthPotionModifier);
		StrengthPotionActivated = false;
	}

	public void ActivateEagleEye()
	{
		if (!EagleEyeActivated)
		{
			gameSpeedFactor.AddModifier(eagleEyeModifier);
			EagleEyeActivated = true;
		}
	}

	public void DeactivateEagleEye()
	{
		gameSpeedFactor.RemoveModifier(eagleEyeModifier);
		EagleEyeActivated = false;
	}

	internal void Cleanup()
	{
		foreach (GoldMinerArtifact artifact in artifacts)
		{
			if (!(artifact == null))
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(artifact.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(artifact.gameObject);
				}
			}
		}
	}

	public bool IsGold(GoldMinerEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		foreach (Func<GoldMinerEntity, bool> isGoldPredicator in isGoldPredicators)
		{
			if (isGoldPredicator(entity))
			{
				return true;
			}
		}
		return entity.tags.Contains(GoldMinerEntity.Tag.Gold);
	}

	public bool IsRock(GoldMinerEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		foreach (Func<GoldMinerEntity, bool> isGoldPredicator in isGoldPredicators)
		{
			if (isGoldPredicator(entity))
			{
				return true;
			}
		}
		return entity.tags.Contains(GoldMinerEntity.Tag.Rock);
	}

	internal bool IsPig(GoldMinerEntity entity)
	{
		return entity.tags.Contains(GoldMinerEntity.Tag.Pig);
	}
}
