using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

public class CheckHeathLessThan : ConditionTask<AICharacterController>
{
	public float percent;

	private float checkTimeMarker = -1f;

	public float checkTimeSpace = 1.5f;

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
		if (Time.time - checkTimeMarker < checkTimeSpace)
		{
			return false;
		}
		checkTimeMarker = Time.time;
		if (!base.agent || !base.agent.CharacterMainControl)
		{
			return false;
		}
		Health health = base.agent.CharacterMainControl.Health;
		if (!health)
		{
			return false;
		}
		return health.CurrentHealth / health.MaxHealth <= percent;
	}
}
