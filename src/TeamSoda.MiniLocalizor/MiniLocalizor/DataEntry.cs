using System;

namespace MiniLocalizor;

[Serializable]
public class DataEntry
{
	public string key { get; set; }

	public string value { get; set; }

	public string version { get; set; }

	public string sheet { get; set; }

	public bool IsNewerThan(string version)
	{
		if (string.IsNullOrEmpty(this.version))
		{
			return false;
		}
		bool flag = this.version.StartsWith('#');
		bool flag2 = version.StartsWith('#');
		if (flag && !flag2)
		{
			return true;
		}
		string text = this.version;
		if (flag)
		{
			text = text.Substring(1);
		}
		string text2 = version;
		if (flag2)
		{
			text2 = text2.Substring(1);
		}
		if (!long.TryParse(text, out var result))
		{
			return false;
		}
		if (!long.TryParse(text2, out var result2))
		{
			return true;
		}
		return result > result2;
	}
}
