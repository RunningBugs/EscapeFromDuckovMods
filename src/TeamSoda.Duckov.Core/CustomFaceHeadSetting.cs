using System;
using UnityEngine;

[Serializable]
public struct CustomFaceHeadSetting
{
	public Color mainColor;

	[Range(-0.4f, 0.4f)]
	public float headScaleOffset;

	[Range(0f, 4f)]
	public float foreheadHeight;

	[Range(0.4f, 4f)]
	public float foreheadRound;
}
