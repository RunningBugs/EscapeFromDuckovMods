using Duckov;
using UnityEngine;

public class PostAudioEventOnEnter : StateMachineBehaviour
{
	[SerializeField]
	private string eventName;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		AudioManager.Post(eventName, animator.gameObject);
	}
}
