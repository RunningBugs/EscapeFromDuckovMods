using System;
using System.Collections.Generic;
using System.IO;
using Duckov.Utilities;
using MiniExcelLibs;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Achievements;

[CreateAssetMenu]
public class AchievementDatabase : ScriptableObject
{
	[Serializable]
	public class Achievement
	{
		public string id { get; set; }

		public string overrideDisplayNameKey { get; set; }

		public string overrideDescriptionKey { get; set; }

		[LocalizationKey("Default")]
		private string DisplayNameKey
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(overrideDisplayNameKey))
				{
					return overrideDisplayNameKey;
				}
				return "Achievement_" + id;
			}
			set
			{
			}
		}

		[LocalizationKey("Default")]
		public string DescriptionKey
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(overrideDescriptionKey))
				{
					return overrideDescriptionKey;
				}
				return "Achievement_" + id + "_Desc";
			}
			set
			{
			}
		}

		public string DisplayName => DisplayNameKey.ToPlainText();

		public string Description => DescriptionKey.ToPlainText();
	}

	[SerializeField]
	private XlsxObject achievementChart;

	private Dictionary<string, Achievement> _dic;

	public static AchievementDatabase Instance => GameplayDataSettings.AchievementDatabase;

	private Dictionary<string, Achievement> dic
	{
		get
		{
			if (_dic == null)
			{
				RebuildDictionary();
			}
			return _dic;
		}
	}

	private void RebuildDictionary()
	{
		if (_dic == null)
		{
			_dic = new Dictionary<string, Achievement>();
		}
		_dic.Clear();
		if (achievementChart == null)
		{
			Debug.LogError("Achievement Chart is not assinged", this);
			return;
		}
		using MemoryStream stream = new MemoryStream(achievementChart.bytes);
		foreach (Achievement item in stream.Query<Achievement>())
		{
			_dic[item.id.Trim()] = item;
		}
	}

	public static bool TryGetAchievementData(string id, out Achievement achievement)
	{
		achievement = null;
		if (Instance == null)
		{
			return false;
		}
		return Instance.dic.TryGetValue(id, out achievement);
	}

	internal bool IsIDValid(string id)
	{
		return dic.ContainsKey(id);
	}
}
