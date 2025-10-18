using System;
using System.Collections.Generic;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

[CreateAssetMenu(menuName = "Duckov/Stat Info Database")]
public class StatInfoDatabase : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		public string statName;

		public Polarity polarity;

		public string displayFormat;

		public string DisplayFormat
		{
			get
			{
				if (string.IsNullOrEmpty(displayFormat))
				{
					return "0.##";
				}
				return displayFormat;
			}
		}
	}

	[SerializeField]
	private Entry[] entries = new Entry[0];

	private Dictionary<string, Entry> _dic;

	public static StatInfoDatabase Instance => GameplayDataSettings.StatInfo;

	private static Dictionary<string, Entry> Dic => Instance._dic;

	public static Entry Get(string statName)
	{
		if (!(Instance == null))
		{
			if (Dic == null)
			{
				RebuildDic();
			}
			if (Dic.TryGetValue(statName, out var value))
			{
				return value;
			}
		}
		return new Entry
		{
			statName = statName,
			polarity = Polarity.Neutral,
			displayFormat = "0.##"
		};
	}

	public static Polarity GetPolarity(string statName)
	{
		return Get(statName).polarity;
	}

	[ContextMenu("Rebuild Dic")]
	private static void RebuildDic()
	{
		if (Instance == null)
		{
			return;
		}
		Instance._dic = new Dictionary<string, Entry>();
		Entry[] array = Instance.entries;
		for (int i = 0; i < array.Length; i++)
		{
			Entry value = array[i];
			if (Instance._dic.ContainsKey(value.statName))
			{
				Debug.LogError("Stat Info 中有重复的 key: " + value.statName);
			}
			else
			{
				Instance._dic[value.statName] = value;
			}
		}
	}
}
