using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duckov.MiniGames.BubblePoppers;

public class BubblePopperLayout : MiniGameBehaviour
{
	[SerializeField]
	private Vector2Int xBorder;

	public Vector2Int XCoordBorder;

	public float BubbleRadius = 8f;

	public static readonly float YOffsetFactor = Mathf.Tan(MathF.PI / 3f);

	[SerializeField]
	private Transform tester;

	[SerializeField]
	private float distance = 10f;

	[SerializeField]
	private Vector2Int min;

	[SerializeField]
	private Vector2Int max;

	public Vector2 XPositionBorder => new Vector2((float)xBorder.x * BubbleRadius * 2f - BubbleRadius, (float)xBorder.y * BubbleRadius * 2f);

	public Vector2 CoordToLocalPosition(Vector2Int coord)
	{
		float bubbleRadius = BubbleRadius;
		return new Vector2(((coord.y % 2 != 0) ? bubbleRadius : 0f) + (float)coord.x * bubbleRadius * 2f, (float)coord.y * bubbleRadius * YOffsetFactor);
	}

	public Vector2Int LocalPositionToCoord(Vector2 localPosition)
	{
		float bubbleRadius = BubbleRadius;
		int num = Mathf.RoundToInt(localPosition.y / bubbleRadius / YOffsetFactor);
		float num2 = ((num % 2 != 0) ? bubbleRadius : 0f);
		return new Vector2Int(Mathf.RoundToInt((localPosition.x - num2) / bubbleRadius / 2f), num);
	}

	public Vector2Int WorldPositionToCoord(Vector2 position)
	{
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint(position);
		return LocalPositionToCoord(vector);
	}

	public Vector2Int[] GetAllNeighbourCoords(Vector2Int center, bool includeCenter)
	{
		int num = ((center.y % 2 == 0) ? (-1) : 0);
		if (includeCenter)
		{
			return new Vector2Int[7]
			{
				new Vector2Int(center.x + num, center.y + 1),
				new Vector2Int(center.x + num + 1, center.y + 1),
				new Vector2Int(center.x - 1, center.y),
				center,
				new Vector2Int(center.x + 1, center.y),
				new Vector2Int(center.x + num, center.y - 1),
				new Vector2Int(center.x + num + 1, center.y - 1)
			};
		}
		return new Vector2Int[6]
		{
			new Vector2Int(center.x + num, center.y + 1),
			new Vector2Int(center.x + num + 1, center.y + 1),
			new Vector2Int(center.x - 1, center.y),
			new Vector2Int(center.x + 1, center.y),
			new Vector2Int(center.x + num, center.y - 1),
			new Vector2Int(center.x + num + 1, center.y - 1)
		};
	}

	public List<Vector2Int> GetAllPassingCoords(Vector2 localOrigin, Vector2 direction, float length)
	{
		float num = BubbleRadius * 2f;
		List<Vector2Int> list = new List<Vector2Int> { LocalPositionToCoord(localOrigin) };
		if (num > 0f)
		{
			float num2 = 0f - num;
			while (num2 < length)
			{
				num2 += num;
				Vector2 localPosition = localOrigin + num2 * direction;
				Vector2Int center = LocalPositionToCoord(localPosition);
				list.AddRange(GetAllNeighbourCoords(center, includeCenter: true));
			}
		}
		return list;
	}

	private void OnDrawGizmos()
	{
		_ = BubbleRadius;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(new Vector3(XPositionBorder.x, 0f), new Vector3(XPositionBorder.x, -100f));
		Gizmos.DrawLine(new Vector3(XPositionBorder.y, 0f), new Vector3(XPositionBorder.y, -100f));
	}

	public void GizmosDrawCoord(Vector2Int coord, float ratio)
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawSphere(CoordToLocalPosition(coord), BubbleRadius * ratio);
		Gizmos.matrix = matrix;
	}
}
