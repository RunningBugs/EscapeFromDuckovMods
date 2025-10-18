using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
	private void Calculate(float fps = 100f)
	{
		float num = 1f / fps;
		int num2 = 7;
		float num3 = 3f;
		float num4 = 3f;
		int num5 = 2;
		int num6 = 15;
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		for (float num7 = 0f; num7 <= 100f; num7 += num)
		{
			while (num6 >= num2)
			{
				num6 -= num2;
				list.Add(0f);
			}
			for (int i = 0; i < list.Count; i++)
			{
				float num8 = list[i];
				num8 += num;
				if (num8 >= num3)
				{
					list.RemoveAt(i);
					i--;
					list2.Add(0f);
				}
				else
				{
					list[i] = num8;
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				float num9 = list2[j];
				num9 += num;
				while (num9 > num4)
				{
					num9 -= num4;
					num6 += num5;
				}
				list2[j] = num9;
			}
		}
		Debug.Log($"{list2.Count} {num6}");
	}
}
