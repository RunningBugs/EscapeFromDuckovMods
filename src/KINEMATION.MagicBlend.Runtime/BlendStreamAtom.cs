using KINEMATION.KAnimationCore.Runtime.Core;
using Unity.Collections;
using UnityEngine.Animations;

namespace KINEMATION.MagicBlend.Runtime;

public struct BlendStreamAtom
{
	[ReadOnly]
	public TransformStreamHandle handle;

	[ReadOnly]
	public float baseWeight;

	[ReadOnly]
	public float additiveWeight;

	[ReadOnly]
	public float localWeight;

	public KTransform meshStreamPose;

	public AtomPose activePose;

	public AtomPose cachedPose;

	public AtomPose GetBlendedAtomPose(float blendWeight)
	{
		return AtomPose.Lerp(cachedPose, activePose, blendWeight);
	}
}
