using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class SearchEnemyAround : ActionTask<AICharacterController>
{
	public bool useSight;

	public bool affactByNightVisionAbility;

	[ShowIf("useSight", 0)]
	public BBParameter<float> searchAngle = 180f;

	[ShowIf("useSight", 0)]
	public BBParameter<float> searchDistance;

	[ShowIf("useSight", 1)]
	public BBParameter<float> sightDistanceMultiplier = 1f;

	[ShowIf("useSight", 0)]
	public bool checkObsticle = true;

	public BBParameter<DamageReceiver> result;

	public BBParameter<InteractablePickup> pickupResult;

	public bool searchPickup;

	public bool alwaysSuccess;

	public bool setNullIfNotFound;

	private bool waitingSearchResult;

	private float searchStartTimeMarker;

	private bool isHurtSearch;

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		DamageInfo dmgInfo = default(DamageInfo);
		isHurtSearch = false;
		if (base.agent.IsHurt(1.5f, 1, ref dmgInfo) && (bool)dmgInfo.fromCharacter && (bool)dmgInfo.fromCharacter.mainDamageReceiver)
		{
			isHurtSearch = true;
		}
	}

	private void Search()
	{
		waitingSearchResult = true;
		float num = (useSight ? base.agent.sightAngle : searchAngle.value);
		float num2 = (useSight ? (base.agent.sightDistance * sightDistanceMultiplier.value) : searchDistance.value);
		if (isHurtSearch)
		{
			num2 *= 2f;
		}
		if (affactByNightVisionAbility && (bool)base.agent.CharacterMainControl)
		{
			float nightVisionAbility = base.agent.CharacterMainControl.NightVisionAbility;
			num *= Mathf.Lerp(TimeOfDayController.NightViewAngleFactor, 1f, nightVisionAbility);
		}
		bool flag = useSight || checkObsticle;
		searchStartTimeMarker = Time.time;
		bool thermalOn = base.agent.CharacterMainControl.ThermalOn;
		LevelManager.Instance.AIMainBrain.AddSearchTask(base.agent.transform.position + Vector3.up * 1.5f, base.agent.CharacterMainControl.CurrentAimDirection, num, num2, base.agent.CharacterMainControl.Team, flag, thermalOn, isHurtSearch, searchPickup ? base.agent.wantItem : (-1), OnSearchFinished);
	}

	private void OnSearchFinished(DamageReceiver dmgReceiver, InteractablePickup pickup)
	{
		if (!(base.agent.gameObject == null))
		{
			_ = Time.time;
			_ = searchStartTimeMarker;
			if (dmgReceiver != null)
			{
				result.value = dmgReceiver;
			}
			else if (setNullIfNotFound)
			{
				result.value = null;
			}
			if (pickup != null)
			{
				pickupResult.value = pickup;
			}
			waitingSearchResult = false;
			if (base.isRunning)
			{
				EndAction(alwaysSuccess || result.value != null || pickupResult != null);
			}
		}
	}

	protected override void OnUpdate()
	{
		if (!waitingSearchResult)
		{
			Search();
		}
	}

	protected override void OnStop()
	{
		waitingSearchResult = false;
	}

	protected override void OnPause()
	{
	}
}
