using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class CheckHurt : ConditionTask<AICharacterController>
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
		if (base.agent == null || cacheFromCharacterDmgReceiver == null)
		{
			return false;
		}
		bool result = false;
		DamageInfo dmgInfo = default(DamageInfo);
		if (base.agent.IsHurt(hurtTimeThreshold, damageThreshold, ref dmgInfo))
		{
			cacheFromCharacterDmgReceiver.value = dmgInfo.fromCharacter.mainDamageReceiver;
			result = true;
		}
		return result;
	}
}
