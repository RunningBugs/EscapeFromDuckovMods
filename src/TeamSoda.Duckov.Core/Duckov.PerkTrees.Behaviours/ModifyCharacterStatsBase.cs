using System;
using System.Collections.Generic;
using System.Text;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.PerkTrees.Behaviours;

public class ModifyCharacterStatsBase : PerkBehaviour
{
	[Serializable]
	public class Entry
	{
		public string key;

		public float value;

		public bool percentage;

		private StringList AvaliableKeys => StringLists.StatKeys;
	}

	private struct Record
	{
		public Stat stat;

		public float value;
	}

	[SerializeField]
	private List<Entry> entries = new List<Entry>();

	private Item targetItem;

	private List<Record> records = new List<Record>();

	private string DescriptionFormat => "PerkBehaviour_ModifyCharacterStatsBase".ToPlainText();

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Entry entry in entries)
			{
				if (entry != null && !string.IsNullOrEmpty(entry.key))
				{
					string statDisplayName = ("Stat_" + entry.key.Trim()).ToPlainText();
					bool num = entry.value > 0f;
					float value = entry.value;
					string value2 = string.Concat(str1: entry.percentage ? $"{value * 100f}%" : value.ToString(), str0: num ? "+" : "");
					string value3 = DescriptionFormat.Format(new
					{
						statDisplayName = statDisplayName,
						value = value2
					});
					stringBuilder.AppendLine(value3);
				}
			}
			return stringBuilder.ToString().Trim();
		}
	}

	protected override void OnUnlocked()
	{
		targetItem = LevelManager.Instance?.MainCharacter?.CharacterItem;
		if (targetItem == null)
		{
			return;
		}
		StatCollection stats = targetItem.Stats;
		if (stats == null)
		{
			return;
		}
		foreach (Entry entry in entries)
		{
			Stat stat = stats.GetStat(entry.key);
			if (stat == null)
			{
				break;
			}
			stat.BaseValue += entry.value;
			records.Add(new Record
			{
				stat = stat,
				value = entry.value
			});
		}
	}

	protected override void OnLocked()
	{
		if (targetItem == null || targetItem.Stats == null)
		{
			return;
		}
		foreach (Record record in records)
		{
			if (record.stat == null)
			{
				break;
			}
			record.stat.BaseValue -= record.value;
		}
	}
}
