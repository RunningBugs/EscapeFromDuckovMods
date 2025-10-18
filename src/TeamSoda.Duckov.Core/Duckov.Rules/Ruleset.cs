using System;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Rules;

[Serializable]
public class Ruleset
{
	[LocalizationKey("UIText")]
	[SerializeField]
	internal string displayNameKey;

	[SerializeField]
	private float damageFactor_ToPlayer = 1f;

	[SerializeField]
	private float enemyHealthFactor = 1f;

	[SerializeField]
	private bool spawnDeadBody = true;

	[SerializeField]
	private bool fogOfWar = true;

	[SerializeField]
	private bool advancedDebuffMode;

	[SerializeField]
	private int saveDeadbodyCount = 1;

	[Range(0f, 1f)]
	[SerializeField]
	private float recoilMultiplier = 1f;

	[SerializeField]
	internal float enemyReactionTimeFactor = 1f;

	[SerializeField]
	internal float enemyAttackTimeSpaceFactor = 1f;

	[SerializeField]
	internal float enemyAttackTimeFactor = 1f;

	[LocalizationKey("UIText")]
	internal string descriptionKey
	{
		get
		{
			return displayNameKey + "_Desc";
		}
		set
		{
		}
	}

	public string DisplayName => displayNameKey.ToPlainText();

	public string Description => descriptionKey.ToPlainText();

	public bool SpawnDeadBody => spawnDeadBody;

	public int SaveDeadbodyCount => saveDeadbodyCount;

	public bool FogOfWar => fogOfWar;

	public bool AdvancedDebuffMode => advancedDebuffMode;

	public float RecoilMultiplier => recoilMultiplier;

	public float DamageFactor_ToPlayer => damageFactor_ToPlayer;

	public float EnemyHealthFactor => enemyHealthFactor;

	public float EnemyReactionTimeFactor => enemyReactionTimeFactor;

	public float EnemyAttackTimeSpaceFactor => enemyAttackTimeSpaceFactor;

	public float EnemyAttackTimeFactor => enemyAttackTimeFactor;
}
