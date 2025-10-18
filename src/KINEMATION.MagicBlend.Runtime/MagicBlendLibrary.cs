using KINEMATION.KAnimationCore.Runtime.Rig;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KINEMATION.MagicBlend.Runtime;

public class MagicBlendLibrary
{
	public static NativeArray<BlendStreamAtom> SetupBlendAtoms(Animator animator, KRigComponent rigComponent)
	{
		Transform[] rigTransforms = rigComponent.GetRigTransforms();
		int num = rigTransforms.Length + 1;
		NativeArray<BlendStreamAtom> result = new NativeArray<BlendStreamAtom>(num, Allocator.Persistent);
		for (int i = 0; i < num; i++)
		{
			Transform transform = animator.transform;
			if (i > 0)
			{
				transform = rigTransforms[i - 1];
			}
			result[i] = new BlendStreamAtom
			{
				handle = animator.BindStreamTransform(transform)
			};
		}
		return result;
	}

	public static void ConnectPose(AnimationScriptPlayable playable, PlayableGraph graph, AnimationClip pose)
	{
		if (playable.GetInput(0).IsValid())
		{
			playable.DisconnectInput(0);
		}
		AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(graph, pose);
		animationClipPlayable.SetSpeed(0.0);
		animationClipPlayable.SetApplyFootIK(value: false);
		playable.ConnectInput(0, animationClipPlayable, 0, 1f);
	}
}
