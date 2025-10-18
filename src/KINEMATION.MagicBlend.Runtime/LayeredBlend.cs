using System;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

namespace KINEMATION.MagicBlend.Runtime;

[Serializable]
public struct LayeredBlend
{
	public KRigElementChain layer;

	[Range(0f, 1f)]
	public float baseWeight;

	[Range(0f, 1f)]
	public float additiveWeight;

	[Range(0f, 1f)]
	public float localWeight;
}
