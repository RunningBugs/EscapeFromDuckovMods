using Duckov.Buffs;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using UnityEngine;

public class ModifierAction : EffectAction
{
	[SerializeField]
	private Buff buff;

	public string targetStatKey;

	private int targetStatHash;

	public ModifierType ModifierType;

	public float modifierValue;

	public bool overrideOrder;

	public int overrideOrderValue;

	private Modifier modifier;

	private Stat targetStat;

	protected override void Awake()
	{
		base.Awake();
		modifier = new Modifier(ModifierType, modifierValue, overrideOrder, overrideOrderValue, base.Master);
		targetStatHash = targetStatKey.GetHashCode();
		if ((bool)buff)
		{
			buff.OnLayerChangedEvent += OnBuffLayerChanged;
		}
		OnBuffLayerChanged();
	}

	private void OnBuffLayerChanged()
	{
		if ((bool)buff && modifier != null)
		{
			modifier.Value = modifierValue * (float)buff.CurrentLayers;
		}
	}

	protected override void OnTriggered(bool positive)
	{
		if (base.Master.Item == null)
		{
			return;
		}
		Item characterItem = base.Master.Item.GetCharacterItem();
		if (characterItem == null)
		{
			return;
		}
		if (positive)
		{
			if (targetStat != null)
			{
				targetStat.RemoveModifier(modifier);
				targetStat = null;
			}
			targetStat = characterItem.GetStat(targetStatHash);
			targetStat.AddModifier(modifier);
		}
		else if (targetStat != null)
		{
			targetStat.RemoveModifier(modifier);
			targetStat = null;
		}
	}

	private void OnDestroy()
	{
		if (targetStat != null)
		{
			targetStat.RemoveModifier(modifier);
			targetStat = null;
		}
		if ((bool)buff)
		{
			buff.OnLayerChangedEvent -= OnBuffLayerChanged;
		}
	}
}
