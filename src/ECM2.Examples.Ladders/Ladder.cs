using UnityEngine;

namespace ECM2.Examples.Ladders;

public sealed class Ladder : MonoBehaviour
{
	[Header("Ladder Path")]
	public float PathLength = 10f;

	public Vector3 PathOffset = new Vector3(0f, 0f, -0.5f);

	[Header("Anchor Points")]
	public Transform TopPoint;

	public Transform BottomPoint;

	public Vector3 bottomAnchorPoint => base.transform.position + base.transform.TransformVector(PathOffset);

	public Vector3 topAnchorPoint => bottomAnchorPoint + base.transform.up * PathLength;

	public Vector3 ClosestPointOnPath(Vector3 position, out float pathPosition)
	{
		Vector3 vector = topAnchorPoint - bottomAnchorPoint;
		float num = Vector3.Dot(position - bottomAnchorPoint, vector.normalized);
		if (num > 0f)
		{
			if (num <= vector.magnitude)
			{
				pathPosition = 0f;
				return bottomAnchorPoint + vector.normalized * num;
			}
			pathPosition = num - vector.magnitude;
			return topAnchorPoint;
		}
		pathPosition = num;
		return bottomAnchorPoint;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(bottomAnchorPoint, topAnchorPoint);
		if (!(BottomPoint == null) && !(TopPoint == null))
		{
			Gizmos.DrawWireCube(BottomPoint.position, Vector3.one * 0.25f);
			Gizmos.DrawWireCube(TopPoint.position, Vector3.one * 0.25f);
		}
	}
}
