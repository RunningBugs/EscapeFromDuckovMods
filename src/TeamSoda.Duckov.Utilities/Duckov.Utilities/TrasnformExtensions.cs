using UnityEngine;

namespace Duckov.Utilities;

public static class TrasnformExtensions
{
	public static void DestroyAllChildren(this Transform transform)
	{
		while (transform.childCount > 0)
		{
			Transform child = transform.GetChild(0);
			child.SetParent(null);
			if (Application.isPlaying)
			{
				Object.Destroy(child.gameObject);
			}
			else
			{
				Object.DestroyImmediate(child.gameObject);
			}
		}
	}
}
