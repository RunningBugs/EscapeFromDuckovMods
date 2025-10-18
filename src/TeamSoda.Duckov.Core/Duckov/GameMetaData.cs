using UnityEngine;

namespace Duckov;

[CreateAssetMenu(menuName = "Settings/MetaData")]
public class GameMetaData : ScriptableObject
{
	[SerializeField]
	private GameVersionData versionData;

	[SerializeField]
	private bool isTestVersion;

	[SerializeField]
	private bool isDemo;

	[SerializeField]
	private Platform platform;

	[SerializeField]
	private bool bloodFxOn = true;

	private static GameMetaData _instance;

	public VersionData Version
	{
		get
		{
			if (Instance == null)
			{
				return default(VersionData);
			}
			return Instance.versionData.versionData;
		}
	}

	public bool IsDemo => isDemo;

	public bool IsTestVersion
	{
		get
		{
			if (isTestVersion)
			{
				return true;
			}
			return false;
		}
	}

	public static GameMetaData Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<GameMetaData>("MetaData");
			}
			return _instance;
		}
	}

	public static bool BloodFxOn => Instance.bloodFxOn;

	public Platform Platform
	{
		get
		{
			return platform;
		}
		set
		{
			platform = value;
		}
	}
}
