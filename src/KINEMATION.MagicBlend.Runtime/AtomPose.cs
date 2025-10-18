using KINEMATION.KAnimationCore.Runtime.Core;
using UnityEngine;

namespace KINEMATION.MagicBlend.Runtime;

public struct AtomPose
{
	public KTransform basePose;

	public KTransform overlayPose;

	public Quaternion localOverlayRotation;

	public float baseWeight;

	public float additiveWeight;

	public float localWeight;

	public static AtomPose Lerp(AtomPose a, AtomPose b, float alpha)
	{
		return new AtomPose
		{
			basePose = KTransform.Lerp(a.basePose, b.basePose, alpha),
			overlayPose = KTransform.Lerp(a.overlayPose, b.overlayPose, alpha),
			localOverlayRotation = Quaternion.Slerp(a.localOverlayRotation, b.localOverlayRotation, alpha),
			additiveWeight = Mathf.Lerp(a.additiveWeight, b.additiveWeight, alpha),
			baseWeight = Mathf.Lerp(a.baseWeight, b.baseWeight, alpha),
			localWeight = Mathf.Lerp(a.localWeight, b.localWeight, alpha)
		};
	}
}
