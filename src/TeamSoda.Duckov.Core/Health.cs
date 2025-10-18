using System;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Buffs;
using Duckov.Scenes;
using Duckov.Utilities;
using Duckov.Weathers;
using FX;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
	public Teams team;

	public bool hasSoul = true;

	private Item item;

	private int maxHealthHash = "MaxHealth".GetHashCode();

	private float lastMaxHealth;

	private bool _showHealthBar;

	[SerializeField]
	private int defaultMaxHealth;

	private float _currentHealth;

	public UnityEvent<Health> OnHealthChange;

	public UnityEvent<Health> OnMaxHealthChange;

	public UnityEvent<DamageInfo> OnDeadEvent;

	public UnityEvent<DamageInfo> OnHurtEvent;

	public float healthBarHeight = 2f;

	private bool isDead;

	public bool autoInit = true;

	[SerializeField]
	private bool DestroyOnDead = true;

	[SerializeField]
	private float DeadDestroyDelay = 0.5f;

	private bool inited;

	private bool invincible;

	private bool hasCharacter = true;

	private CharacterMainControl characterCached;

	private int bodyArmorHash = "BodyArmor".GetHashCode();

	private int headArmorHash = "HeadArmor".GetHashCode();

	private int Hash_ElementFactor_Physics = "ElementFactor_Physics".GetHashCode();

	private int Hash_ElementFactor_Fire = "ElementFactor_Fire".GetHashCode();

	private int Hash_ElementFactor_Poison = "ElementFactor_Poison".GetHashCode();

	private int Hash_ElementFactor_Electricity = "ElementFactor_Electricity".GetHashCode();

	private int Hash_ElementFactor_Space = "ElementFactor_Space".GetHashCode();

	public bool showHealthBar
	{
		get
		{
			return _showHealthBar;
		}
		set
		{
			_showHealthBar = value;
		}
	}

	public bool Hidden
	{
		get
		{
			if ((bool)TryGetCharacter())
			{
				return characterCached.Hidden;
			}
			return false;
		}
	}

	public float MaxHealth
	{
		get
		{
			float num = 0f;
			num = ((!item) ? ((float)defaultMaxHealth) : item.GetStatValue(maxHealthHash));
			if (!Mathf.Approximately(lastMaxHealth, num))
			{
				lastMaxHealth = num;
				OnMaxHealthChange?.Invoke(this);
			}
			return num;
		}
	}

	public bool IsMainCharacterHealth
	{
		get
		{
			if (LevelManager.Instance == null)
			{
				return false;
			}
			if (LevelManager.Instance.MainCharacter == null)
			{
				return false;
			}
			if (LevelManager.Instance.MainCharacter != TryGetCharacter())
			{
				return false;
			}
			return true;
		}
	}

	public float CurrentHealth
	{
		get
		{
			return _currentHealth;
		}
		set
		{
			float currentHealth = _currentHealth;
			_currentHealth = value;
			if (_currentHealth != currentHealth)
			{
				OnHealthChange?.Invoke(this);
			}
		}
	}

	public bool IsDead => isDead;

	public bool Invincible => invincible;

	public float BodyArmor
	{
		get
		{
			if ((bool)item)
			{
				return item.GetStatValue(bodyArmorHash);
			}
			return 0f;
		}
	}

	public float HeadArmor
	{
		get
		{
			if ((bool)item)
			{
				return item.GetStatValue(headArmorHash);
			}
			return 0f;
		}
	}

	public static event Action<Health, DamageInfo> OnHurt;

	public static event Action<Health, DamageInfo> OnDead;

	public static event Action<Health> OnRequestHealthBar;

	public CharacterMainControl TryGetCharacter()
	{
		if (characterCached != null)
		{
			return characterCached;
		}
		if (!hasCharacter)
		{
			return null;
		}
		if (!item)
		{
			hasCharacter = false;
			return null;
		}
		characterCached = item.GetCharacterMainControl();
		if (!characterCached)
		{
			hasCharacter = true;
		}
		return characterCached;
	}

	public float ElementFactor(ElementTypes type)
	{
		float num = 1f;
		if (!item)
		{
			return num;
		}
		Weather currentWeather = TimeOfDayController.Instance.CurrentWeather;
		bool isBaseLevel = LevelManager.Instance.IsBaseLevel;
		switch (type)
		{
		case ElementTypes.physics:
			num = item.GetStat(Hash_ElementFactor_Physics).Value;
			break;
		case ElementTypes.fire:
			num = item.GetStat(Hash_ElementFactor_Fire).Value;
			if (!isBaseLevel && currentWeather == Weather.Rainy)
			{
				num -= 0.15f;
			}
			break;
		case ElementTypes.poison:
			num = item.GetStat(Hash_ElementFactor_Poison).Value;
			break;
		case ElementTypes.electricity:
			num = item.GetStat(Hash_ElementFactor_Electricity).Value;
			if (!isBaseLevel && currentWeather == Weather.Rainy)
			{
				num += 0.2f;
			}
			break;
		case ElementTypes.space:
			num = item.GetStat(Hash_ElementFactor_Space).Value;
			break;
		}
		return num;
	}

	private void Start()
	{
		if (autoInit)
		{
			Init();
		}
	}

	public void SetItemAndCharacter(Item _item, CharacterMainControl _character)
	{
		item = _item;
		if ((bool)_character)
		{
			hasCharacter = true;
			characterCached = _character;
		}
	}

	public void Init()
	{
		if (CurrentHealth <= 0f)
		{
			CurrentHealth = MaxHealth;
		}
	}

	public void AddBuff(Buff buffPfb, CharacterMainControl fromWho, int overrideFromWeaponID = 0)
	{
		TryGetCharacter()?.AddBuff(buffPfb, fromWho, overrideFromWeaponID);
	}

	private void Update()
	{
	}

	public bool Hurt(DamageInfo damageInfo)
	{
		if (MultiSceneCore.Instance != null && MultiSceneCore.Instance.IsLoading)
		{
			return false;
		}
		if (invincible)
		{
			return false;
		}
		if (isDead)
		{
			return false;
		}
		if (damageInfo.buff != null && UnityEngine.Random.Range(0f, 1f) < damageInfo.buffChance)
		{
			AddBuff(damageInfo.buff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);
		}
		bool flag = LevelManager.Rule.AdvancedDebuffMode;
		if (LevelManager.Instance.IsBaseLevel)
		{
			flag = false;
		}
		float num = 0.2f;
		float num2 = 0.12f;
		CharacterMainControl characterMainControl = TryGetCharacter();
		if (!IsMainCharacterHealth)
		{
			num = 0.1f;
			num2 = 0.1f;
		}
		if (flag && UnityEngine.Random.Range(0f, 1f) < damageInfo.bleedChance * num)
		{
			AddBuff(GameplayDataSettings.Buffs.BoneCrackBuff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);
		}
		else if (flag && UnityEngine.Random.Range(0f, 1f) < damageInfo.bleedChance * num2)
		{
			AddBuff(GameplayDataSettings.Buffs.WoundBuff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);
		}
		else if (UnityEngine.Random.Range(0f, 1f) < damageInfo.bleedChance)
		{
			if (flag)
			{
				AddBuff(GameplayDataSettings.Buffs.UnlimitBleedBuff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);
			}
			else
			{
				AddBuff(GameplayDataSettings.Buffs.BleedSBuff, damageInfo.fromCharacter, damageInfo.fromWeaponItemID);
			}
		}
		bool flag2 = UnityEngine.Random.Range(0f, 1f) < damageInfo.critRate;
		damageInfo.crit = (flag2 ? 1 : 0);
		if (!damageInfo.ignoreDifficulty && team == Teams.player)
		{
			damageInfo.damageValue *= LevelManager.Rule.DamageFactor_ToPlayer;
		}
		float num3 = damageInfo.damageValue * (flag2 ? damageInfo.critDamageFactor : 1f);
		if (damageInfo.damageType != DamageTypes.realDamage && !damageInfo.ignoreArmor)
		{
			float num4 = (flag2 ? HeadArmor : BodyArmor);
			if ((bool)characterMainControl && LevelManager.Instance.IsRaidMap)
			{
				Item item = (flag2 ? characterMainControl.GetHelmatItem() : characterMainControl.GetArmorItem());
				if ((bool)item)
				{
					item.Durability = Mathf.Max(0f, item.Durability - damageInfo.armorBreak);
				}
			}
			float num5 = 1f;
			if (num4 > 0f)
			{
				num5 = 2f / (Mathf.Clamp(num4 - damageInfo.armorPiercing, 0f, 999f) + 2f);
			}
			if ((bool)characterMainControl && !characterMainControl.IsMainCharacter && (bool)damageInfo.fromCharacter && !damageInfo.fromCharacter.IsMainCharacter)
			{
				CharacterRandomPreset characterPreset = damageInfo.fromCharacter.characterPreset;
				CharacterRandomPreset characterPreset2 = characterMainControl.characterPreset;
				if ((bool)characterPreset && (bool)characterPreset2)
				{
					num5 *= characterPreset.aiCombatFactor / characterPreset2.aiCombatFactor;
				}
			}
			num3 *= num5;
		}
		if (damageInfo.elementFactors.Count <= 0)
		{
			damageInfo.elementFactors.Add(new ElementFactor(ElementTypes.physics, 1f));
		}
		float num6 = 0f;
		foreach (ElementFactor elementFactor in damageInfo.elementFactors)
		{
			float factor = elementFactor.factor;
			float num7 = ElementFactor(elementFactor.elementType);
			float num8 = num3 * factor * num7;
			if (num8 < 1f && num8 > 0f && num7 > 0f && factor > 0f)
			{
				num8 = 1f;
			}
			if (num8 > 0f && !Hidden && (bool)PopText.instance)
			{
				GameplayDataSettings.UIStyleData.DisplayElementDamagePopTextLook elementDamagePopTextLook = GameplayDataSettings.UIStyle.GetElementDamagePopTextLook(elementFactor.elementType);
				float size = (flag2 ? elementDamagePopTextLook.critSize : elementDamagePopTextLook.normalSize);
				Color color = elementDamagePopTextLook.color;
				PopText.Pop(num8.ToString("F1"), damageInfo.damagePoint + Vector3.up * 2f, color, size, flag2 ? GameplayDataSettings.UIStyle.CritPopSprite : null);
			}
			num6 += num8;
		}
		damageInfo.finalDamage = num6;
		if (CurrentHealth < damageInfo.finalDamage)
		{
			damageInfo.finalDamage = CurrentHealth + 1f;
		}
		CurrentHealth -= damageInfo.finalDamage;
		OnHurtEvent?.Invoke(damageInfo);
		Health.OnHurt?.Invoke(this, damageInfo);
		if (isDead)
		{
			return true;
		}
		if (CurrentHealth <= 0f)
		{
			bool flag3 = true;
			if (!LevelManager.Instance.IsRaidMap)
			{
				flag3 = false;
			}
			if (!flag3)
			{
				SetHealth(1f);
			}
		}
		if (CurrentHealth <= 0f)
		{
			CurrentHealth = 0f;
			isDead = true;
			if (LevelManager.Instance.MainCharacter != TryGetCharacter())
			{
				DestroyOnDelay().Forget();
			}
			if (this.item != null && team != Teams.player && (bool)damageInfo.fromCharacter && damageInfo.fromCharacter.IsMainCharacter)
			{
				EXPManager.AddExp(this.item.GetInt("Exp"));
			}
			OnDeadEvent?.Invoke(damageInfo);
			Health.OnDead?.Invoke(this, damageInfo);
			base.gameObject.SetActive(value: false);
			if ((bool)damageInfo.fromCharacter && damageInfo.fromCharacter.IsMainCharacter)
			{
				Debug.Log("Killed by maincharacter");
			}
		}
		return true;
	}

	public void RequestHealthBar()
	{
		if (showHealthBar && LevelManager.LevelInited)
		{
			Health.OnRequestHealthBar?.Invoke(this);
		}
	}

	public async UniTask DestroyOnDelay()
	{
		await UniTask.WaitForSeconds(DeadDestroyDelay);
		if (base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void AddHealth(float healthValue)
	{
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healthValue);
	}

	public void SetHealth(float healthValue)
	{
		CurrentHealth = Mathf.Min(MaxHealth, healthValue);
	}

	public void SetInvincible(bool value)
	{
		invincible = value;
	}
}
