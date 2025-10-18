using System;
using System.Text;
using Duckov.Utilities;

namespace ItemStatsSystem;

[Serializable]
public struct ItemFilter
{
	public Tag[] requireTags;

	public Tag[] excludeTags;

	public int minQuality;

	public int maxQuality;

	public string caliber;

	private static StringBuilder sb = new StringBuilder();

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public override string ToString()
	{
		sb.Clear();
		sb.AppendLine("R");
		if (requireTags != null)
		{
			Tag[] array = requireTags;
			foreach (Tag tag in array)
			{
				if (!(tag == null))
				{
					sb.AppendLine(tag.name);
				}
			}
		}
		sb.AppendLine("E");
		if (excludeTags != null)
		{
			Tag[] array = excludeTags;
			foreach (Tag tag2 in array)
			{
				if (!(tag2 == null))
				{
					sb.AppendLine(tag2.name);
				}
			}
		}
		sb.AppendLine("MinQ");
		sb.AppendLine(minQuality.ToString());
		sb.AppendLine("MaxQ");
		sb.AppendLine(maxQuality.ToString());
		sb.AppendLine("CALIBER");
		sb.AppendLine(caliber);
		return sb.ToString();
	}
}
