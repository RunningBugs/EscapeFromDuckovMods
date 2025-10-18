using Duckov.Utilities;
using Saves;
using UnityEngine;

public class CustomFaceManager : MonoBehaviour
{
	public void SaveSettingToMainCharacter(CustomFaceSettingData setting)
	{
		SaveSetting("CustomFace_MainCharacter", setting);
	}

	public CustomFaceSettingData LoadMainCharacterSetting()
	{
		return LoadSetting("CustomFace_MainCharacter");
	}

	private void SaveSetting(string key, CustomFaceSettingData setting)
	{
		setting.savedSetting = true;
		SavesSystem.Save(key, setting);
	}

	private CustomFaceSettingData LoadSetting(string key)
	{
		CustomFaceSettingData result = SavesSystem.Load<CustomFaceSettingData>(key);
		if (!result.savedSetting)
		{
			result = GameplayDataSettings.CustomFaceData.DefaultPreset.settings;
		}
		return result;
	}
}
