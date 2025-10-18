using System;
using System.Collections.Generic;
using Duckov.Buffs;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

[Serializable]
public struct DamageInfo
{
	public DamageTypes damageType;

	public bool isFromBuffOrEffect;

	public float damageValue;

	public bool ignoreArmor;

	public bool ignoreDifficulty;

	public float critDamageFactor;

	public float critRate;

	public float armorPiercing;

	[SerializeField]
	public List<ElementFactor> elementFactors;

	public bool isExplosion;

	public float armorBreak;

	public float finalDamage;

	public CharacterMainControl fromCharacter;

	public DamageReceiver toDamageReceiver;

	[HideInInspector]
	public Vector3 damagePoint;

	[HideInInspector]
	public Vector3 damageNormal;

	public int crit;

	[ItemTypeID]
	public int fromWeaponItemID;

	public float buffChance;

	public Buff buff;

	public float bleedChance;

	public string GenerateDescription()
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		if (fromCharacter != null)
		{
			if (fromCharacter.IsMainCharacter)
			{
				text = "DeathReason_Self".ToPlainText();
			}
			else if (fromCharacter.characterPreset != null)
			{
				text = fromCharacter.characterPreset.DisplayName;
			}
		}
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(fromWeaponItemID);
		if (metaData.id > 0)
		{
			text2 = metaData.DisplayName;
		}
		if (isExplosion)
		{
			text2 = "DeathReason_Explosion".ToPlainText();
		}
		if (crit > 0)
		{
			text3 = "DeathReason_Critical".ToPlainText();
		}
		bool flag = string.IsNullOrEmpty(text);
		bool flag2 = string.IsNullOrEmpty(text2);
		if (flag && flag2)
		{
			return "?";
		}
		if (flag)
		{
			return text2;
		}
		if (flag2)
		{
			return text;
		}
		return text + " (" + text2 + ") " + text3;
	}

	public DamageInfo(CharacterMainControl fromCharacter = null)
	{
		damageValue = 0f;
		critDamageFactor = 1f;
		ignoreArmor = false;
		critRate = 0f;
		armorBreak = 0f;
		armorPiercing = 0f;
		this.fromCharacter = fromCharacter;
		toDamageReceiver = null;
		damagePoint = Vector3.zero;
		damageNormal = Vector3.up;
		elementFactors = new List<ElementFactor>();
		crit = -1;
		damageType = DamageTypes.normal;
		buffChance = 0f;
		buff = null;
		finalDamage = 0f;
		isFromBuffOrEffect = false;
		fromWeaponItemID = 0;
		isExplosion = false;
		bleedChance = 0f;
		ignoreDifficulty = false;
	}
}
