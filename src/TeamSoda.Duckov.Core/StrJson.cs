using System.Collections.Generic;
using System.Text;

public class StrJson
{
	public struct Entry
	{
		public string key;

		public string value;

		public Entry(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}

	public List<Entry> entries;

	private StrJson(params string[] contentPairs)
	{
		entries = new List<Entry>();
		for (int i = 0; i < contentPairs.Length - 1; i += 2)
		{
			entries.Add(new Entry(contentPairs[i], contentPairs[i + 1]));
		}
	}

	public StrJson Add(string key, string value)
	{
		entries.Add(new Entry(key, value));
		return this;
	}

	public static StrJson Create(params string[] contentPairs)
	{
		return new StrJson(contentPairs);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		for (int i = 0; i < entries.Count; i++)
		{
			Entry entry = entries[i];
			if (i > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append("\"" + entry.key + "\":\"" + entry.value + "\"");
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
