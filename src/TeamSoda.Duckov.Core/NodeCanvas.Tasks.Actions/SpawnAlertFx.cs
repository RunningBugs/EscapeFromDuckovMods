using Duckov.Utilities;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class SpawnAlertFx : ActionTask<AICharacterController>
{
	public BBParameter<float> alertTime = 0.2f;

	protected override string info => $"AlertFx";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (!base.agent || !base.agent.CharacterMainControl)
		{
			EndAction(success: true);
		}
		Transform rightHandSocket = base.agent.CharacterMainControl.RightHandSocket;
		if (!rightHandSocket)
		{
			EndAction(success: true);
		}
		Object.Instantiate(GameplayDataSettings.Prefabs.AlertFxPrefab, rightHandSocket).transform.localPosition = Vector3.zero;
		if (alertTime.value <= 0f)
		{
			EndAction(success: true);
		}
	}

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= alertTime.value)
		{
			EndAction(success: true);
		}
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
