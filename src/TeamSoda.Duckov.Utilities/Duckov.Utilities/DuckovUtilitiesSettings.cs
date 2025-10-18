using System;
using UnityEngine;

namespace Duckov.Utilities;

public class DuckovUtilitiesSettings : ScriptableObject
{
	[Serializable]
	public class ColorsData
	{
		[SerializeField]
		private Color effectTrigger = Color.cyan;

		[SerializeField]
		private Color effectFilter = Color.yellow;

		[SerializeField]
		private Color effectAction = Color.green;

		public Color EffectTrigger => effectTrigger;

		public Color EffectFilter => effectFilter;

		public Color EffectAction => effectAction;
	}

	private static DuckovUtilitiesSettings _instance;

	private const string fileName = "DuckovUtilitiesSettings";

	[SerializeField]
	private ColorsData colors;

	private static DuckovUtilitiesSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Load();
			}
			return _instance;
		}
	}

	public static ColorsData Colors => Instance.colors;

	private static DuckovUtilitiesSettings Load()
	{
		DuckovUtilitiesSettings duckovUtilitiesSettings = Resources.Load("DuckovUtilitiesSettings") as DuckovUtilitiesSettings;
		if (duckovUtilitiesSettings != null)
		{
			return duckovUtilitiesSettings;
		}
		return null;
	}

	private static void CreateAsset()
	{
	}

	private static void LoadOrCreate()
	{
		_instance = Load();
	}
}
