using UnityEngine;

namespace KINEMATION.MagicBlend.Runtime;

public class MagicBlendState : StateMachineBehaviour
{
	[SerializeField]
	private MagicBlendAsset magicBlendAsset;

	private bool _isInitialized;

	private MagicBlending _magicBlending;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!_isInitialized)
		{
			_magicBlending = animator.gameObject.GetComponent<MagicBlending>();
			if (_magicBlending == null)
			{
				return;
			}
			_isInitialized = true;
		}
		float duration = animator.GetAnimatorTransitionInfo(layerIndex).duration;
		_magicBlending.UpdateMagicBlendAsset(magicBlendAsset, useBlending: true, duration);
	}
}
