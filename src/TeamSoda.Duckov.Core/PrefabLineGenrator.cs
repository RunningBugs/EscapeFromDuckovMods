using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Points))]
[ExecuteInEditMode]
public class PrefabLineGenrator : MonoBehaviour, IOnPointsChanged
{
	[Serializable]
	public struct SapwnInfo
	{
		public List<PrefabPair> prefabs;

		public float rotateOffset;

		[Range(0f, 1f)]
		public float flatten;

		public Vector3 posOffset;

		public GameObject GetRandomPrefab()
		{
			if (prefabs.Count < 1)
			{
				return null;
			}
			float num = 0f;
			for (int i = 0; i < prefabs.Count; i++)
			{
				num += prefabs[i].weight;
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			for (int j = 0; j < prefabs.Count; j++)
			{
				if (num2 <= prefabs[j].weight)
				{
					return prefabs[j].prefab;
				}
				num2 -= prefabs[j].weight;
			}
			return prefabs[prefabs.Count - 1].prefab;
		}
	}

	[Serializable]
	public struct PrefabPair
	{
		public GameObject prefab;

		public float weight;
	}

	[SerializeField]
	private float prefabLength = 2f;

	public SapwnInfo spawnPrefab;

	public SapwnInfo startPrefab;

	public SapwnInfo endPrefab;

	[SerializeField]
	private Points points;

	[SerializeField]
	[HideInInspector]
	private List<BoxCollider> colliderObjects;

	[SerializeField]
	private float updateTick = 0.5f;

	private float lastModifyTime;

	private bool dirty;

	public List<Vector3> searchedPointsLocalSpace;

	private List<Vector3> originPoints => points.points;

	public void OnPointsChanged()
	{
	}
}
