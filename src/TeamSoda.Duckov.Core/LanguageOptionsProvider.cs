using SodaCraft.Localizations;
using UnityEngine;

public class LanguageOptionsProvider : OptionsProviderBase
{
	private string[] cache;

	public override string Key => "Language";

	public override string GetCurrentOption()
	{
		return LocalizationManager.CurrentLanguageDisplayName;
	}

	public override string[] GetOptions()
	{
		LocalizationDatabase instance = LocalizationDatabase.Instance;
		if (instance == null)
		{
			return new string[1] { "?" };
		}
		return cache = instance.GetLanguageDisplayNameList();
	}

	public override void Set(int index)
	{
		if (cache == null)
		{
			GetOptions();
		}
		if (index < 0 || index >= cache.Length)
		{
			Debug.LogError("语言越界");
		}
		else
		{
			LocalizationManager.SetLanguage(index);
		}
	}
}
