using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Duckov.MiniMaps;

public static class PointsOfInterests
{
	private static List<MonoBehaviour> points = new List<MonoBehaviour>();

	private static ReadOnlyCollection<MonoBehaviour> points_ReadOnly;

	public static ReadOnlyCollection<MonoBehaviour> Points
	{
		get
		{
			if (points_ReadOnly == null)
			{
				points_ReadOnly = new ReadOnlyCollection<MonoBehaviour>(points);
			}
			return points_ReadOnly;
		}
	}

	public static event Action<MonoBehaviour> OnPointRegistered;

	public static event Action<MonoBehaviour> OnPointUnregistered;

	public static void Register(MonoBehaviour point)
	{
		points.Add(point);
		PointsOfInterests.OnPointRegistered?.Invoke(point);
		CleanUp();
	}

	public static void Unregister(MonoBehaviour point)
	{
		if (points.Remove(point))
		{
			PointsOfInterests.OnPointUnregistered?.Invoke(point);
		}
		CleanUp();
	}

	private static void CleanUp()
	{
		points.RemoveAll((MonoBehaviour e) => e == null);
	}
}
