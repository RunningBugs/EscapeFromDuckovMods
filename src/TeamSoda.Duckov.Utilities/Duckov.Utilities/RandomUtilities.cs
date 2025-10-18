using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Duckov.Utilities;

public static class RandomUtilities
{
	public static void RandomizeOrder<T>(this List<T> list)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			int index = UnityEngine.Random.Range(i, count - 1);
			T item = list[index];
			list.RemoveAt(index);
			list.Insert(i, item);
		}
	}

	public static T GetRandom<T>(this IList<T> list)
	{
		if (list.Count < 1)
		{
			return default(T);
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	public static T GetRandom<T>(this IList<T> list, System.Random rng)
	{
		if (list.Count < 1)
		{
			return default(T);
		}
		int index = rng.Next(0, list.Count);
		return list[index];
	}

	public static T[] GetRandomSubSet<T>(this IList<T> list, int amount)
	{
		if (list.Count < 1)
		{
			return null;
		}
		if (list.Count <= amount)
		{
			return list.ToArray();
		}
		T[] array = new T[amount];
		HashSet<int> hashSet = new HashSet<int>();
		int num = amount * 100;
		int num2 = 0;
		for (int i = 0; i < amount; i++)
		{
			num2++;
			if (num2 >= num)
			{
				Debug.LogError("在选取子集的时候尝试了过多次数，选取失败");
				return null;
			}
			int num3 = UnityEngine.Random.Range(0, list.Count);
			if (hashSet.Contains(num3))
			{
				i--;
			}
			else
			{
				array[i] = list[num3];
			}
		}
		return array;
	}

	public static T GetRandom<T>(this T[] array)
	{
		if (array.Length < 1)
		{
			return default(T);
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		return array[num];
	}

	public static T GetRandomWeighted<T>(this IList<T> list, Func<T, float> weightFunction, float lowPercent = 0f)
	{
		if (list.Count < 1)
		{
			return default(T);
		}
		if (weightFunction == null)
		{
			return list.GetRandom();
		}
		float num = 0f;
		float[] array = new float[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = weightFunction(list[i]);
			num += array[i];
		}
		if (num <= 0f)
		{
			return list.GetRandom();
		}
		float num2 = 0f;
		float num3 = UnityEngine.Random.Range(num * lowPercent, num);
		for (int j = 0; j < list.Count; j++)
		{
			num2 += array[j];
			if (num2 >= num3)
			{
				return list[j];
			}
		}
		return list[list.Count - 1];
	}
}
