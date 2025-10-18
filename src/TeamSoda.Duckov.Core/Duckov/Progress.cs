using UnityEngine;

namespace Duckov;

public struct Progress
{
	public bool inProgress;

	public float total;

	public float current;

	public string progressName;

	public float progress
	{
		get
		{
			if (!(total <= 0f))
			{
				return Mathf.Clamp01(current / total);
			}
			return 1f;
		}
	}
}
