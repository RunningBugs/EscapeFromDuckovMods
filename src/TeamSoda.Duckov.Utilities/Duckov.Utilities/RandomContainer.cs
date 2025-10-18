using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Duckov.Utilities;

[Serializable]
public class RandomContainer<T> : IPercentRefreshable
{
	[Serializable]
	public struct Entry
	{
		public T value;

		public float weight;

		public string percent;
	}

	public List<Entry> entries = new List<Entry>();

	public int Count => entries.Count;

	public void AddEntry(T _value, float _weight)
	{
		entries.Add(new Entry
		{
			value = _value,
			weight = _weight
		});
	}

	public T GetRandom(float lowPercent = 0f)
	{
		if (entries.Count < 1)
		{
			return default(T);
		}
		float num = 0f;
		for (int i = 0; i < entries.Count; i++)
		{
			num += entries[i].weight;
		}
		float num2 = UnityEngine.Random.Range(num * lowPercent, num);
		float num3 = 0f;
		for (int j = 0; j < entries.Count; j++)
		{
			Entry entry = entries[j];
			num3 += entry.weight;
			if (num3 >= num2)
			{
				return entry.value;
			}
		}
		List<Entry> list = entries;
		return list[list.Count - 1].value;
	}

	public T GetRandom(System.Random overrideRandom, float lowPercent = 0f)
	{
		if (entries.Count < 1)
		{
			return default(T);
		}
		float num = 0f;
		for (int i = 0; i < entries.Count; i++)
		{
			num += entries[i].weight;
		}
		float a = num * lowPercent;
		float b = num;
		float num2 = Mathf.Lerp(a, b, (float)overrideRandom.NextDouble());
		float num3 = 0f;
		for (int j = 0; j < entries.Count; j++)
		{
			Entry entry = entries[j];
			num3 += entry.weight;
			if (num3 >= num2)
			{
				return entry.value;
			}
		}
		List<Entry> list = entries;
		return list[list.Count - 1].value;
	}

	public T GetRandom(System.Random overrideRandom, Func<T, bool> predicator, float lowPercent = 0f)
	{
		if (entries.Count < 1)
		{
			return default(T);
		}
		List<Entry> list = entries.Where((Entry e) => predicator(e.value)).ToList();
		if (list.Count < 1)
		{
			return default(T);
		}
		float num = 0f;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			num += list[num2].weight;
		}
		float a = num * lowPercent;
		float b = num;
		float num3 = Mathf.Lerp(a, b, (float)overrideRandom.NextDouble());
		float num4 = 0f;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			Entry entry = list[num5];
			num4 += entry.weight;
			if (num4 >= num3)
			{
				return entry.value;
			}
		}
		return list[list.Count - 1].value;
	}

	public List<T> GetRandomMultiple(int count, bool repeatable = true)
	{
		List<T> list = new List<T>();
		if (count < 1)
		{
			return list;
		}
		if (entries.Count < 1)
		{
			return list;
		}
		List<Entry> candidates = new List<Entry>(entries);
		float totalWeight = default(float);
		RecalculateTotalWeight();
		for (int i = 0; i < count; i++)
		{
			if (candidates.Count < 1)
			{
				return list;
			}
			int candidateIndex;
			T item = GetOnceInCandidates(out candidateIndex);
			list.Add(item);
			if (!repeatable)
			{
				candidates.RemoveAt(candidateIndex);
				RecalculateTotalWeight();
			}
		}
		return list;
		T GetOnceInCandidates(out int reference)
		{
			float num = UnityEngine.Random.Range(0f, totalWeight);
			float num2 = 0f;
			for (int j = 0; j < candidates.Count; j++)
			{
				Entry entry = candidates[j];
				num2 += entry.weight;
				if (!(num2 < num))
				{
					reference = j;
					return entry.value;
				}
			}
			reference = candidates.Count - 1;
			return candidates[reference].value;
		}
		void RecalculateTotalWeight()
		{
			totalWeight = 0f;
			foreach (Entry item2 in candidates)
			{
				totalWeight += item2.weight;
			}
		}
	}

	public void RefreshPercent()
	{
		float num = 0f;
		foreach (Entry item in new List<Entry>(entries))
		{
			num += item.weight;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			Entry value = entries[i];
			float num2 = value.weight * 100f / num;
			if (num2 >= 0.01f)
			{
				value.percent = num2.ToString("0.00") + "%";
			}
			else
			{
				value.percent = num2.ToString("0.000") + "%";
			}
			entries[i] = value;
		}
	}

	public static RandomContainer<string> FromString(string str)
	{
		string[] array = str.Split(",");
		RandomContainer<string> randomContainer = new RandomContainer<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(":");
			if (array3.Length > 2 || array3.Length < 1)
			{
				Debug.LogError("Invalid entry format:\n " + text);
				continue;
			}
			string value = array3[0].Trim();
			if (string.IsNullOrEmpty(value))
			{
				Debug.LogError("Empty value, skip");
				continue;
			}
			float result = 1f;
			if (array3.Length == 2 && !float.TryParse(array3[1], out result))
			{
				Debug.LogError("Cannot resolve random container entry:\n " + text + " \n cannot resolve weight");
			}
			else
			{
				randomContainer.AddEntry(value, result);
			}
		}
		return randomContainer;
	}
}
