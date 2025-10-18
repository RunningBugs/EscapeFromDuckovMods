using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class SetAim : ActionTask<AICharacterController>
{
	public bool useTransfom = true;

	[ShowIf("useTransfom", 1)]
	public BBParameter<Transform> aimTarget;

	[ShowIf("useTransfom", 0)]
	public BBParameter<Vector3> aimPos;

	private bool waitingSearchResult;

	protected override string info
	{
		get
		{
			if (useTransfom && string.IsNullOrEmpty(aimTarget.name))
			{
				return "Set aim to null";
			}
			if (!useTransfom)
			{
				return "Set aim to " + aimPos.name;
			}
			return "Set aim to " + aimTarget.name;
		}
	}

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		base.agent.SetTarget(aimTarget.value);
		if (!useTransfom || !(aimTarget.value != null))
		{
			if (!useTransfom)
			{
				base.agent.SetAimInput((aimPos.value - base.agent.transform.position).normalized, AimTypes.normalAim);
			}
			else
			{
				base.agent.SetAimInput(Vector3.zero, AimTypes.normalAim);
			}
		}
		EndAction(success: true);
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
