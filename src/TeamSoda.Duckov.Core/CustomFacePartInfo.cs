using System;
using UnityEngine;

[Serializable]
public struct CustomFacePartInfo
{
	public float radius;

	public Color color;

	public float height;

	public float heightOffset;

	public float scale;

	public float twist;

	[Range(0f, 90f)]
	public float distanceAngle;

	[Range(-90f, 90f)]
	public float leftRightAngle;
}
