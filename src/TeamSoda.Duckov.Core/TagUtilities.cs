using System.Linq;
using Duckov.Utilities;
using UnityEngine;

public class TagUtilities
{
	public static Tag TagFromString(string name)
	{
		name = name.Trim();
		Tag tag = GameplayDataSettings.Tags.AllTags.FirstOrDefault((Tag e) => e != null && e.name == name);
		if (tag == null)
		{
			Debug.LogError("未找到Tag: " + name);
		}
		return tag;
	}
}
