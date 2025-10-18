using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class HasObsticleToTarget : ConditionTask<AICharacterController>
{
	public float hurtTimeThreshold = 0.2f;

	public int damageThreshold = 3;

	public BBParameter<DamageReceiver> cacheFromCharacterDmgReceiver;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnEnable()
	{
	}

	protected override void OnDisable()
	{
	}

	protected override bool OnCheck()
	{
		return base.agent.hasObsticleToTarget;
	}
}
