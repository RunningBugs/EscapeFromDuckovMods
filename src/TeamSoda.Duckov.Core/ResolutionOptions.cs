using Duckov.Options;
using UnityEngine;

public class ResolutionOptions : OptionsProviderBase
{
	private DuckovResolution[] avaliableResolutions;

	public override string Key => ResolutionSetter.Key_Resolution;

	public override string GetCurrentOption()
	{
		return OptionsManager.Load(Key, new DuckovResolution(Screen.resolutions[Screen.resolutions.Length - 1])).ToString();
	}

	public override string[] GetOptions()
	{
		avaliableResolutions = ResolutionSetter.GetResolutions();
		string[] array = new string[avaliableResolutions.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = avaliableResolutions[i].ToString();
		}
		return array;
	}

	public override void Set(int index)
	{
		if (avaliableResolutions == null || index >= avaliableResolutions.Length)
		{
			Debug.Log("设置分辨率流程错误");
			index = 0;
		}
		DuckovResolution obj = avaliableResolutions[index];
		OptionsManager.Save(Key, obj);
	}
}
