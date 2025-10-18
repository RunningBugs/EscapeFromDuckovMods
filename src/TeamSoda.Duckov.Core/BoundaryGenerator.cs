using System.Collections.Generic;
using UnityEngine;

public class BoundaryGenerator : MonoBehaviour
{
	public List<Vector3> points;

	[HideInInspector]
	public int lastSelectedPointIndex = -1;

	public float colliderHeight = 1f;

	public float yOffset;

	public float colliderThickness = 0.1f;

	public bool inverseFaceDirection;

	public bool provideContects;

	[SerializeField]
	[HideInInspector]
	private List<BoxCollider> colliderObjects;

	public void UpdateColliderParameters()
	{
		if (colliderObjects == null || colliderObjects.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < colliderObjects.Count; i++)
		{
			if (i >= points.Count - 1)
			{
				continue;
			}
			BoxCollider boxCollider = colliderObjects[i];
			if (!(boxCollider == null))
			{
				boxCollider.gameObject.layer = base.gameObject.layer;
				Vector3 vector = base.transform.TransformPoint(points[i]);
				Vector3 vector2 = base.transform.TransformPoint(points[i + 1]);
				vector2.y = (vector.y = Mathf.Min(vector.y, vector2.y));
				Vector3 normalized = (vector2 - vector).normalized;
				float z = Vector3.Distance(vector, vector2);
				boxCollider.size = new Vector3(colliderThickness, colliderHeight, z);
				boxCollider.transform.forward = normalized;
				boxCollider.transform.position = (vector + vector2) / 2f + Vector3.up * 0.5f * colliderHeight + Vector3.up * yOffset + boxCollider.transform.right * 0.5f * colliderThickness * (inverseFaceDirection ? (-1f) : 1f);
				if (provideContects)
				{
					boxCollider.providesContacts = true;
				}
			}
		}
	}

	private void DestroyAllChildren()
	{
		while (base.transform.childCount > 0)
		{
			Transform child = base.transform.GetChild(0);
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

	public void UpdateColliders()
	{
		DestroyAllChildren();
		if (colliderObjects == null)
		{
			colliderObjects = new List<BoxCollider>();
		}
		colliderObjects.Clear();
		for (int i = 0; i < points.Count - 1; i++)
		{
			GameObject obj = new GameObject($"Collider_{i}");
			obj.transform.parent = base.transform;
			BoxCollider item = obj.AddComponent<BoxCollider>();
			colliderObjects.Add(item);
		}
	}

	public void SetYtoZero()
	{
		for (int i = 0; i < points.Count; i++)
		{
			points[i] = new Vector3(points[i].x, 0f, points[i].z);
		}
	}

	public void OnPointsUpdated(bool OnValidate = false)
	{
		if (points == null)
		{
			points = new List<Vector3>();
		}
		if (base.transform.childCount != points.Count - 1 && !OnValidate)
		{
			UpdateColliders();
		}
		UpdateColliderParameters();
	}

	public void RemoveAllPoints()
	{
		points.Clear();
		OnPointsUpdated();
	}

	public void RespawnColliders()
	{
		DestroyAllChildren();
		colliderObjects.Clear();
		OnPointsUpdated();
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			OnPointsUpdated(OnValidate: true);
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		if (colliderObjects == null || colliderObjects.Count <= 0)
		{
			return;
		}
		foreach (Vector3 point in points)
		{
			Gizmos.DrawCube(base.transform.TransformPoint(point), Vector3.one * 0.15f);
		}
	}
}
