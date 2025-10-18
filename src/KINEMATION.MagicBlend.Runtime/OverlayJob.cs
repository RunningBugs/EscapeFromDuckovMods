using KINEMATION.KAnimationCore.Runtime.Core;
using Unity.Collections;
using UnityEngine.Animations;

namespace KINEMATION.MagicBlend.Runtime;

public struct OverlayJob : IAnimationJob
{
	[ReadOnly]
	public bool alwaysAnimate;

	[ReadOnly]
	public bool cachePose;

	public NativeArray<BlendStreamAtom> atoms;

	public void ProcessAnimation(AnimationStream stream)
	{
		if (alwaysAnimate || cachePose)
		{
			BlendStreamAtom value = atoms[0];
			atoms[0] = value;
			KTransform kTransform = new KTransform
			{
				rotation = value.handle.GetRotation(stream),
				position = value.handle.GetPosition(stream)
			};
			int length = atoms.Length;
			for (int i = 1; i < length; i++)
			{
				BlendStreamAtom value2 = atoms[i];
				KTransform worldTransform = new KTransform
				{
					rotation = value2.handle.GetRotation(stream),
					position = value2.handle.GetPosition(stream)
				};
				worldTransform = kTransform.GetRelativeTransform(worldTransform, useScale: false);
				value2.activePose.overlayPose = worldTransform;
				value2.activePose.overlayPose.position = value2.handle.GetLocalPosition(stream);
				value2.activePose.localOverlayRotation = value2.handle.GetLocalRotation(stream);
				atoms[i] = value2;
			}
		}
	}

	public void ProcessRootMotion(AnimationStream stream)
	{
	}
}
