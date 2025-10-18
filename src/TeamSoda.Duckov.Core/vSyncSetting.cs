using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine;

public class vSyncSetting : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string onKey = "gSync_On";

	[LocalizationKey("Default")]
	public string offKey = "gSync_Off";

	public GameObject setActiveIfOn;

	public override string Key => "GSyncSetting";

	public override string[] GetOptions()
	{
		return new string[2]
		{
			onKey.ToPlainText(),
			offKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		switch (OptionsManager.Load(Key, 1))
		{
		case 0:
			SyncObjectActive(active: true);
			return onKey.ToPlainText();
		case 1:
			SyncObjectActive(active: false);
			return offKey.ToPlainText();
		default:
			return offKey.ToPlainText();
		}
	}

	public override void Set(int index)
	{
		switch (index)
		{
		case 0:
			QualitySettings.vSyncCount = 1;
			SyncObjectActive(active: true);
			break;
		case 1:
			QualitySettings.vSyncCount = 0;
			SyncObjectActive(active: false);
			break;
		}
		OptionsManager.Save(Key, index);
	}

	private void SyncObjectActive(bool active)
	{
		if ((bool)setActiveIfOn)
		{
			setActiveIfOn.SetActive(active);
		}
	}

	private void Awake()
	{
		LevelManager.OnLevelInitialized += RefreshOnLevelInited;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= RefreshOnLevelInited;
	}

	private void RefreshOnLevelInited()
	{
		int index = OptionsManager.Load(Key, 1);
		Set(index);
	}
}
