using System;
using UnityEngine;

namespace Duckov.MiniGames.BubblePoppers;

public class BubblePopperLevelDataProvider : MonoBehaviour
{
	[SerializeField]
	private BubblePopper master;

	[SerializeField]
	private int totalLevels = 10;

	[SerializeField]
	public int seed;

	public int TotalLevels => totalLevels;

	internal int[] GetData(int levelIndex)
	{
		int num = seed + levelIndex;
		int[] array = new int[60 + 10 * (levelIndex / 2)];
		System.Random random = new System.Random(num);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = random.Next(0, master.AvaliableColorCount);
		}
		return array;
	}
}
