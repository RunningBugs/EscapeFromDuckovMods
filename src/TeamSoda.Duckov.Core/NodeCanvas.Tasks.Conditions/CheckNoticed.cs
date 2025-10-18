using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions;

public class CheckNoticed : ConditionTask<AICharacterController>
{
	public float noticedTimeThreshold = 0.2f;

	public bool resetNotice;

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
		bool result = base.agent.isNoticing(noticedTimeThreshold);
		if (resetNotice)
		{
			base.agent.noticed = false;
		}
		return result;
	}
}
