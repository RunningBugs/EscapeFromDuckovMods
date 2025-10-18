using KINEMATION.KAnimationCore.Runtime.Core;
using Unity.Collections;
using UnityEngine.Animations;

namespace KINEMATION.MagicBlend.Runtime;

public struct PoseJob : IAnimationJob
{
	[ReadOnly]
	public bool alwaysAnimate;

	[ReadOnly]
	public bool readPose;

	public NativeArray<BlendStreamAtom> atoms;

	public void ProcessAnimation(AnimationStream stream)
	{
		if (alwaysAnimate || readPose)
		{
			BlendStreamAtom blendStreamAtom = atoms[0];
			KTransform kTransform = new KTransform
			{
				rotation = blendStreamAtom.handle.GetRotation(stream),
				position = blendStreamAtom.handle.GetPosition(stream)
			};
			int length = atoms.Length;
			for (int i = 1; i < length; i++)
			{
				BlendStreamAtom value = atoms[i];
				KTransform worldTransform = new KTransform
				{
					position = value.handle.GetPosition(stream),
					rotation = value.handle.GetRotation(stream)
				};
				worldTransform = kTransform.GetRelativeTransform(worldTransform, useScale: false);
				value.activePose.basePose = worldTransform;
				value.activePose.basePose.position = value.handle.GetLocalPosition(stream);
				atoms[i] = value;
			}
		}
	}

	public void ProcessRootMotion(AnimationStream stream)
	{
	}
}
