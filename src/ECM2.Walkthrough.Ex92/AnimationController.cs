using UnityEngine;

namespace ECM2.Walkthrough.Ex92;

public class AnimationController : MonoBehaviour
{
	private static readonly int Forward = Animator.StringToHash("Forward");

	private static readonly int Turn = Animator.StringToHash("Turn");

	private static readonly int Ground = Animator.StringToHash("OnGround");

	private static readonly int Crouch = Animator.StringToHash("Crouch");

	private static readonly int Jump = Animator.StringToHash("Jump");

	private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");

	private Character _character;

	private void Awake()
	{
		_character = GetComponentInParent<Character>();
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Animator animator = _character.GetAnimator();
		Vector3 vector = base.transform.InverseTransformDirection(_character.GetMovementDirection());
		float num = ((_character.useRootMotion && (bool)_character.GetRootMotionController()) ? vector.z : Mathf.InverseLerp(0f, _character.GetMaxSpeed(), _character.GetSpeed()));
		animator.SetFloat(Forward, num, 0.1f, deltaTime);
		animator.SetFloat(Turn, Mathf.Atan2(vector.x, vector.z), 0.1f, deltaTime);
		animator.SetBool(Ground, _character.IsGrounded());
		animator.SetBool(Crouch, _character.IsCrouched());
		if (_character.IsFalling())
		{
			animator.SetFloat(Jump, _character.GetVelocity().y, 0.1f, deltaTime);
		}
		float value = ((Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1f) < 0.5f) ? 1f : (-1f)) * num;
		if (_character.IsGrounded())
		{
			animator.SetFloat(JumpLeg, value);
		}
	}
}
