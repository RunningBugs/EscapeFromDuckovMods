using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class CheckObsticle : ActionTask<AICharacterController>
{
	public bool useTransform;

	[ShowIf("useTransform", 1)]
	public BBParameter<Transform> targetTransform;

	[ShowIf("useTransform", 0)]
	public BBParameter<Vector3> targetPoint;

	public bool alwaysSuccess;

	private bool waitingResult;

	private bool isHurtSearch;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		isHurtSearch = false;
		DamageInfo dmgInfo = default(DamageInfo);
		if (base.agent.IsHurt(1.5f, 1, ref dmgInfo) && (bool)dmgInfo.fromCharacter && (bool)dmgInfo.fromCharacter.mainDamageReceiver)
		{
			isHurtSearch = true;
		}
	}

	private void Check()
	{
		waitingResult = true;
		Vector3 end = (useTransform ? targetTransform.value.position : targetPoint.value);
		end += Vector3.up * 0.4f;
		Vector3 start = base.agent.transform.position + Vector3.up * 0.4f;
		ItemAgent_Gun gun = base.agent.CharacterMainControl.GetGun();
		if ((bool)gun && (bool)gun.muzzle)
		{
			start = gun.muzzle.position - gun.muzzle.forward * 0.1f;
		}
		LevelManager.Instance.AIMainBrain.AddCheckObsticleTask(start, end, base.agent.CharacterMainControl.ThermalOn, isHurtSearch, OnCheckFinished);
	}

	private void OnCheckFinished(bool result)
	{
		if (!(base.agent.gameObject == null))
		{
			base.agent.hasObsticleToTarget = result;
			waitingResult = false;
			if (base.isRunning)
			{
				EndAction(alwaysSuccess || result);
			}
		}
	}

	protected override void OnUpdate()
	{
		if (!waitingResult)
		{
			Check();
		}
	}

	protected override void OnStop()
	{
		waitingResult = false;
	}

	protected override void OnPause()
	{
	}
}
