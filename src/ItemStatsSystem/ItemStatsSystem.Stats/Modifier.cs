using System;
using UnityEngine;

namespace ItemStatsSystem.Stats;

public class Modifier
{
	private Stat target;

	[SerializeField]
	private ModifierType type;

	[SerializeField]
	private float value;

	[SerializeField]
	private bool overrideOrder;

	[SerializeField]
	private int overrideOrderValue;

	[SerializeField]
	private object source;

	public static readonly Comparison<Modifier> OrderComparison = (Modifier a, Modifier b) => a.Order - b.Order;

	public int Order
	{
		get
		{
			if (overrideOrder)
			{
				return overrideOrderValue;
			}
			return (int)type;
		}
	}

	public ModifierType Type => type;

	public float Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value != this.value)
			{
				this.value = value;
				if (target != null)
				{
					target.SetDirty();
				}
			}
		}
	}

	public object Source => source;

	public Modifier(ModifierType type, float value, object source)
		: this(type, value, overrideOrder: false, 0, source)
	{
	}

	public Modifier(ModifierType type, float value, bool overrideOrder, int overrideOrderValue, object source)
	{
		this.type = type;
		this.value = value;
		this.overrideOrder = overrideOrder;
		this.overrideOrderValue = overrideOrderValue;
		this.source = source;
	}

	public void NotifyAddedToStat(Stat stat)
	{
		if (target != null && target != stat)
		{
			Debug.LogError("Modifier被赋予给了多了个不同的Stat");
			target.RemoveModifier(this);
		}
		target = stat;
	}

	public void RemoveFromTarget()
	{
		if (target != null)
		{
			target.RemoveModifier(this);
			target = null;
		}
	}

	public override string ToString()
	{
		string text = "";
		bool flag = value > 0f;
		switch (type)
		{
		case ModifierType.Add:
			text = string.Format("{0}{1}", flag ? "+" : "", value);
			break;
		case ModifierType.PercentageAdd:
			text = string.Format("x(..{0}{1})", flag ? "+" : "", value);
			break;
		case ModifierType.PercentageMultiply:
			text = string.Format("x(1{0}{1})", flag ? "+" : "", value);
			break;
		}
		string text2 = (flag ? "#55FF55" : "#FF5555");
		text = "<color=" + text2 + ">" + text + "</color>";
		return text + " 来自 " + source.ToString();
	}
}
