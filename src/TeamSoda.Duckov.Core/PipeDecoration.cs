using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PipeRenderer))]
public class PipeDecoration : MonoBehaviour
{
	[Serializable]
	public class GameObjectOffset
	{
		public GameObject gameObject;

		public float offset;
	}

	public PipeRenderer pipeRenderer;

	[HideInInspector]
	public List<GameObjectOffset> decorations = new List<GameObjectOffset>();

	public Vector3 rotate;

	public Vector3 scale = Vector3.one;

	public float uniformScale = 1f;

	private void OnDrawGizmosSelected()
	{
		if (pipeRenderer == null)
		{
			pipeRenderer = GetComponent<PipeRenderer>();
		}
	}

	private void Refresh()
	{
		if (pipeRenderer.splineInUse == null || pipeRenderer.splineInUse.Length < 1)
		{
			return;
		}
		for (int i = 0; i < decorations.Count; i++)
		{
			GameObjectOffset gameObjectOffset = decorations[i];
			Quaternion rotation;
			Vector3 positionByOffset = pipeRenderer.GetPositionByOffset(gameObjectOffset.offset, out rotation);
			Vector3 position = pipeRenderer.transform.localToWorldMatrix.MultiplyPoint(positionByOffset);
			if (!(gameObjectOffset.gameObject == null))
			{
				gameObjectOffset.gameObject.transform.position = position;
				gameObjectOffset.gameObject.transform.localRotation = rotation;
				gameObjectOffset.gameObject.transform.Rotate(rotate);
				gameObjectOffset.gameObject.transform.localScale = scale * uniformScale;
			}
		}
	}

	public void OnValidate()
	{
		if (pipeRenderer == null)
		{
			pipeRenderer = GetComponent<PipeRenderer>();
		}
		Refresh();
	}
}
