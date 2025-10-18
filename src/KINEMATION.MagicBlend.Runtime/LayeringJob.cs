using KINEMATION.KAnimationCore.Runtime.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace KINEMATION.MagicBlend.Runtime;

public struct LayeringJob : IAnimationJob
{
	[ReadOnly]
	public float blendWeight;

	[ReadOnly]
	public bool cachePose;

	public NativeArray<BlendStreamAtom> atoms;

	public void ProcessAnimation(AnimationStream stream)
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
				rotation = value.handle.GetRotation(stream),
				position = value.handle.GetPosition(stream),
				scale = Vector3.one
			};
			value.meshStreamPose = kTransform.GetRelativeTransform(worldTransform, useScale: false);
			value.meshStreamPose.position = value.handle.GetLocalPosition(stream);
			value.activePose.additiveWeight = value.additiveWeight;
			value.activePose.baseWeight = value.baseWeight;
			value.activePose.localWeight = value.localWeight;
			atoms[i] = value;
		}
		for (int j = 1; j < length; j++)
		{
			BlendStreamAtom value2 = atoms[j];
			AtomPose blendedAtomPose = value2.GetBlendedAtomPose(blendWeight);
			if (cachePose)
			{
				value2.cachedPose = blendedAtomPose;
				atoms[j] = value2;
			}
			KTransform basePose = blendedAtomPose.basePose;
			KTransform overlayPose = blendedAtomPose.overlayPose;
			Quaternion localOverlayRotation = blendedAtomPose.localOverlayRotation;
			float additiveWeight = blendedAtomPose.additiveWeight;
			float baseWeight = blendedAtomPose.baseWeight;
			float localWeight = blendedAtomPose.localWeight;
			KTransform kTransform2 = new KTransform
			{
				rotation = value2.meshStreamPose.rotation * Quaternion.Inverse(basePose.rotation),
				position = value2.meshStreamPose.position - basePose.position
			};
			Quaternion b = kTransform2.rotation * overlayPose.rotation;
			b = Quaternion.Slerp(overlayPose.rotation, b, additiveWeight);
			b = Quaternion.Slerp(value2.meshStreamPose.rotation, b, baseWeight);
			b = kTransform.rotation * b;
			Vector3 b2 = overlayPose.position + kTransform2.position * additiveWeight;
			b2 = Vector3.Lerp(value2.meshStreamPose.position, b2, baseWeight);
			value2.handle.SetRotation(stream, b);
			b = Quaternion.Slerp(value2.handle.GetLocalRotation(stream), localOverlayRotation, localWeight);
			value2.handle.SetLocalRotation(stream, b);
			b2 = Vector3.Lerp(b2, overlayPose.position, localWeight);
			value2.handle.SetLocalPosition(stream, b2);
		}
	}

	public void ProcessRootMotion(AnimationStream stream)
	{
	}
}
