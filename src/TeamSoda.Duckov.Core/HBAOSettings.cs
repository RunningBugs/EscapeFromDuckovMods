using Duckov.Options;
using HorizonBasedAmbientOcclusion.Universal;
using SodaCraft.Localizations;
using UnityEngine.Rendering;

public class HBAOSettings : OptionsProviderBase
{
	[LocalizationKey("Default")]
	public string offKey = "HBAOSettings_Off";

	[LocalizationKey("Default")]
	public string lowKey = "HBAOSettings_Low";

	[LocalizationKey("Default")]
	public string normalKey = "HBAOSettings_Normal";

	[LocalizationKey("Default")]
	public string highKey = "HBAOSettings_High";

	public VolumeProfile GlobalVolumePorfile;

	public override string Key => "HBAOSettings";

	public override string[] GetOptions()
	{
		return new string[4]
		{
			offKey.ToPlainText(),
			lowKey.ToPlainText(),
			normalKey.ToPlainText(),
			highKey.ToPlainText()
		};
	}

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, 2) switch
		{
			0 => offKey.ToPlainText(), 
			1 => lowKey.ToPlainText(), 
			2 => normalKey.ToPlainText(), 
			3 => highKey.ToPlainText(), 
			_ => offKey.ToPlainText(), 
		};
	}

	public override void Set(int index)
	{
		if (GlobalVolumePorfile.TryGet<HBAO>(out var component))
		{
			switch (index)
			{
			case 0:
				component.EnableHBAO(enable: false);
				break;
			case 1:
				component.EnableHBAO(enable: true);
				component.resolution = new HBAO.ResolutionParameter(HBAO.Resolution.Half);
				component.bias.value = 64f;
				break;
			case 2:
				component.EnableHBAO(enable: true);
				component.resolution = new HBAO.ResolutionParameter(HBAO.Resolution.Half);
				component.bias.value = 128f;
				break;
			case 3:
				component.EnableHBAO(enable: true);
				component.resolution = new HBAO.ResolutionParameter(HBAO.Resolution.Full);
				component.bias.value = 128f;
				break;
			}
		}
		OptionsManager.Save(Key, index);
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
