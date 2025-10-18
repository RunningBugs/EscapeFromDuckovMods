using NodeCanvas.Framework;
using UnityEngine;

public class GetCurrentPosition : ActionTask<Transform>
{
	public BBParameter<Vector3> positionResult;

	protected override void OnExecute()
	{
		if (base.agent != null)
		{
			positionResult.value = base.agent.position;
		}
		EndAction(success: true);
	}
}
