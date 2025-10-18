using System;
using Saves;

public class SavesCounter
{
	public static Action<string, int> OnKillCountChanged;

	public static int AddCount(string countKey)
	{
		int num = SavesSystem.Load<int>("Count/" + countKey);
		num++;
		SavesSystem.Save("Count/" + countKey, num);
		return num;
	}

	public static int GetCount(string countKey)
	{
		return SavesSystem.Load<int>("Count/" + countKey);
	}

	public static int AddKillCount(string key)
	{
		int num = AddCount("Kills/" + key);
		OnKillCountChanged?.Invoke(key, num);
		return num;
	}

	public static int GetKillCount(string key)
	{
		return GetCount("Kills/" + key);
	}
}
