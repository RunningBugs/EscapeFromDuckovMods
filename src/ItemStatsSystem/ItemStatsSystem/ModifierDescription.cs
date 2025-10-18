using System;
using System.Text.RegularExpressions;
using Duckov.Utilities;
using ItemStatsSystem.Stats;
using SodaCraft.Localizations;
using UnityEngine;

namespace ItemStatsSystem;

[Serializable]
public class ModifierDescription
{
	[NonSerialized]
	private ModifierDescriptionCollection collection;

	[SerializeField]
	private ModifierTarget target = ModifierTarget.Parent;

	[Tooltip("在背包中是否生效")]
	[SerializeField]
	private bool enableInInventory;

	[Tooltip("Target Stat Key")]
	[SerializeField]
	private string key;

	[Tooltip("Target Stat Key")]
	[SerializeField]
	private bool display;

	[SerializeField]
	private ModifierType type;

	[SerializeField]
	private float value;

	[Tooltip("Order Override")]
	[SerializeField]
	private bool overrideOrder;

	[SerializeField]
	private int order;

	private Modifier activeModifier;

	private static Regex reg = new Regex("(?'instructions'(\\[\\w+\\])*)(?'Target'[a-zA-Z]+)/(?'Key'[a-zA-Z_]+)\\s*(?'Operation'[*+-]+)\\s*(?'Value'[-\\d\\.]+)");

	private Item Master => collection?.Master;

	private StringList referenceStatKeys => StringLists.StatKeys;

	private string displayNamekey => "Stat_" + key;

	private int ResultOrder
	{
		get
		{
			if (overrideOrder)
			{
				return order;
			}
			return (int)type;
		}
	}

	public string Key => key;

	public ModifierType Type => type;

	public float Value => value;

	public bool IsOverrideOrder => overrideOrder;

	public int Order => order;

	public bool Display => display;

	public string DisplayName => displayNamekey.ToPlainText();

	public Modifier CreateModifier(object source)
	{
		return new Modifier(type, value, overrideOrder, order, source);
	}

	public void ReapplyModifier(ModifierDescriptionCollection collection)
	{
		if (this.collection != null && collection != this.collection)
		{
			Debug.LogWarning("One Modifier Description seem to be used in different collections! This could cause errors in the future.");
		}
		this.collection = collection;
		if (activeModifier != null)
		{
			activeModifier.RemoveFromTarget();
		}
		Item targetItem = GetTargetItem();
		if (!(targetItem == null) && !(targetItem.Stats == null))
		{
			Stat stat = targetItem.Stats.GetStat(key);
			if (stat == null)
			{
				Stat stat2 = new Stat(key, 0f);
				targetItem.Stats.Add(stat2);
				stat = stat2;
			}
			Modifier modifier = CreateModifier(Master);
			stat.AddModifier(modifier);
			activeModifier = modifier;
		}
	}

	public Item GetTargetItem()
	{
		if (Master == null)
		{
			return null;
		}
		if (target == ModifierTarget.Self)
		{
			return Master;
		}
		if (!enableInInventory)
		{
			if (target == ModifierTarget.Character && !Master.IsInCharacterSlot())
			{
				return null;
			}
			if (target == ModifierTarget.Parent && Master.PluggedIntoSlot == null)
			{
				return null;
			}
		}
		switch (target)
		{
		case ModifierTarget.Parent:
			return Master.ParentItem;
		case ModifierTarget.Character:
			return Master.GetCharacterItem();
		default:
			Debug.LogWarning("Invalid Modifier Target Type!");
			return null;
		}
	}

	public ModifierDescription(ModifierTarget target, string key, ModifierType type, float value, bool overrideOrder = false, int overrideOrderValue = 0)
	{
		this.target = target;
		this.key = key;
		this.type = type;
		this.value = value;
		this.overrideOrder = overrideOrder;
		order = overrideOrderValue;
	}

	public ModifierDescription()
	{
	}

	public static ModifierDescription FromString(string str)
	{
		ModifierDescription modifierDescription = new ModifierDescription();
		string text = str;
		str = str.Trim();
		Match match = reg.Match(str);
		if (!match.Success)
		{
			Debug.LogError("无法解析Modifier: " + text);
			return null;
		}
		GroupCollection groups = match.Groups;
		string text2 = groups["instructions"].Value;
		string text3 = groups["Target"].Value;
		string text4 = groups["Key"].Value;
		string text5 = groups["Operation"].Value;
		string text6 = groups["Value"].Value;
		modifierDescription.display = true;
		string[] array = text2.Split(']');
		foreach (string text7 in array)
		{
			if (!string.IsNullOrWhiteSpace(text7) && text7.Trim('[', ']') == "hide")
			{
				modifierDescription.display = false;
			}
		}
		switch (text3)
		{
		case "Self":
			modifierDescription.target = ModifierTarget.Self;
			break;
		case "Parent":
			modifierDescription.target = ModifierTarget.Parent;
			break;
		case "Character":
			modifierDescription.target = ModifierTarget.Character;
			break;
		default:
			Debug.LogError("无法解析Modifier目标 " + text3 + "\n" + text);
			return null;
		}
		modifierDescription.key = text4;
		bool flag = false;
		switch (text5)
		{
		case "+":
			modifierDescription.type = ModifierType.Add;
			break;
		case "-":
			modifierDescription.type = ModifierType.Add;
			flag = true;
			break;
		case "*+":
			modifierDescription.type = ModifierType.PercentageAdd;
			break;
		case "*-":
			modifierDescription.type = ModifierType.PercentageAdd;
			flag = true;
			break;
		case "*":
			modifierDescription.type = ModifierType.PercentageMultiply;
			break;
		default:
			Debug.LogError("无法解析Modifier的operation: " + text5 + " \n" + text);
			return null;
		}
		if (!float.TryParse(text6, out var result))
		{
			Debug.LogError("无法解析Modifier的Value: " + text6 + " \n" + text);
		}
		if (flag)
		{
			result = 0f - result;
		}
		modifierDescription.value = result;
		return modifierDescription;
	}

	internal void Release()
	{
		if (activeModifier != null)
		{
			activeModifier.RemoveFromTarget();
		}
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3} {4}", target, key, type, value, overrideOrder ? "" : $" override order:{order}");
	}

	public string GetDisplayValueString(string format = "0.##")
	{
		return type switch
		{
			ModifierType.Add => ((Value > 0f) ? "+" : "") + Value.ToString(format), 
			ModifierType.PercentageAdd => string.Format("{0}{1:0.##}%", (Value > 0f) ? "+" : "", Value * 100f), 
			ModifierType.PercentageMultiply => $"x{100f + Value * 100f:0.##}%", 
			_ => $"?{Value}", 
		};
	}
}
