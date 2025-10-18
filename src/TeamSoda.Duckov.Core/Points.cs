using System.Collections.Generic;
using UnityEngine;

public class Points : MonoBehaviour
{
	public List<Vector3> points;

	[HideInInspector]
	public int lastSelectedPointIndex = -1;

	public bool worldSpace = true;

	public bool syncToSelectedPoint;

	public void SetYtoZero()
	{
		for (int i = 0; i < points.Count; i++)
		{
			points[i] = new Vector3(points[i].x, 0f, points[i].z);
		}
	}

	public void RemoveAllPoints()
	{
		points = new List<Vector3>();
	}

	public List<Vector3> GetRandomPoints(int count)
	{
		List<Vector3> list = new List<Vector3>();
		list.AddRange(points);
		List<Vector3> list2 = new List<Vector3>();
		while (list2.Count < count && list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			Vector3 item = PointToWorld(list[index]);
			list2.Add(item);
			list.RemoveAt(index);
		}
		return list2;
	}

	public Vector3 GetRandomPoint()
	{
		int index = Random.Range(0, points.Count);
		return GetPoint(index);
	}

	public Vector3 GetPoint(int index)
	{
		if (index >= points.Count)
		{
			return Vector3.zero;
		}
		Vector3 point = points[index];
		return PointToWorld(point);
	}

	private Vector3 PointToWorld(Vector3 point)
	{
		if (!worldSpace)
		{
			point = base.transform.TransformPoint(point);
		}
		return point;
	}

	public void SendPointsChangedMessage()
	{
		GetComponent<IOnPointsChanged>()?.OnPointsChanged();
	}
}
