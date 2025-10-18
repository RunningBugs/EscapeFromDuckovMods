using System;
using System.Collections.Generic;
using System.Text;
using Duckov.Utilities;
using ItemStatsSystem.Stats;
using SodaCraft.Localizations;
using UnityEngine;

namespace ItemStatsSystem;

[Serializable]
public class Stat
{
	[NonSerialized]
	private StatCollection collection;

	[Tooltip("Stat Key")]
	[SerializeField]
	private string key;

	[SerializeField]
	private bool display;

	[Tooltip("Base Value")]
	[SerializeField]
	private float baseValue;

	private List<Modifier> modifiers = new List<Modifier>();

	private bool _dirty;

	private float cachedBaseValue = float.NaN;

	private float cachedValue;

	public Item Master => collection.Master;

	private StringList referenceKeys => StringLists.StatKeys;

	public string Key => key;

	public string DisplayNameKey => "Stat_" + key;

	public string DisplayName => DisplayNameKey.ToPlainText();

	public float BaseValue
	{
		get
		{
			return baseValue;
		}
		set
		{
			baseValue = value;
		}
	}

	public List<Modifier> Modifiers => modifiers;

	private bool Dirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
			if (value)
			{
				this.OnSetDirty?.Invoke(this);
			}
		}
	}

	public float Value
	{
		get
		{
			if (Dirty || cachedBaseValue != BaseValue)
			{
				Recalculate();
			}
			return cachedValue;
		}
	}

	private string ValueToolTip
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"基础:{BaseValue}");
			foreach (Modifier modifier in modifiers)
			{
				stringBuilder.AppendLine(modifier.ToString());
			}
			return stringBuilder.ToString();
		}
	}

	public bool Display => display;

	public event Action<Stat> OnSetDirty;

	private void Recalculate()
	{
		cachedBaseValue = baseValue;
		modifiers.RemoveAll((Modifier e) => e == null);
		modifiers.Sort(Modifier.OrderComparison);
		float result = baseValue;
		float percentageAddValue = 0f;
		bool percentageAdding = false;
		int num = int.MinValue;
		for (int num2 = 0; num2 < modifiers.Count; num2++)
		{
			Modifier modifier = modifiers[num2];
			int order = modifier.Order;
			if (percentageAdding && (order != num || modifier.Type != ModifierType.PercentageAdd))
			{
				ApplyPercentageAdd();
			}
			num = modifier.Order;
			switch (modifier.Type)
			{
			case ModifierType.Add:
				result += modifier.Value;
				break;
			case ModifierType.PercentageAdd:
				percentageAdding = true;
				percentageAddValue += modifier.Value;
				break;
			case ModifierType.PercentageMultiply:
				result *= Mathf.Max(0f, 1f + modifier.Value);
				break;
			}
		}
		if (percentageAdding)
		{
			ApplyPercentageAdd();
		}
		cachedValue = result;
		_dirty = false;
		void ApplyPercentageAdd()
		{
			Mathf.Max(0f, 1f + percentageAddValue);
			result *= Mathf.Max(0f, 1f + percentageAddValue);
			percentageAddValue = 0f;
			percentageAdding = false;
		}
	}

	public void AddModifier(Modifier modifier)
	{
		modifiers.Add(modifier);
		modifier.NotifyAddedToStat(this);
		Dirty = true;
	}

	public bool RemoveModifier(Modifier modifier)
	{
		bool result = modifiers.Remove(modifier);
		Dirty = true;
		return result;
	}

	public int RemoveAllModifiersFromSource(object source)
	{
		int result = modifiers.RemoveAll((Modifier e) => e.Source == source);
		Dirty = true;
		return result;
	}

	internal void Initialize(StatCollection collection)
	{
		this.collection = collection;
		Recalculate();
	}

	internal void SetDirty()
	{
		Dirty = true;
	}

	public Stat()
	{
	}

	public Stat(string key, float value, bool display = false)
	{
		this.key = key;
		baseValue = value;
		this.display = display;
	}
}
