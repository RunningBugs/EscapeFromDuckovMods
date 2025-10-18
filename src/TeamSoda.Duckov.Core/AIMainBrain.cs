using System;
using System.Collections.Generic;
using Duckov.Utilities;
using ParadoxNotion;
using UnityEngine;

public class AIMainBrain : MonoBehaviour
{
	public struct SearchTaskContext
	{
		public Vector3 searchCenter;

		public Vector3 searchDirection;

		public float searchAngle;

		public float searchDistance;

		public Teams selfTeam;

		public bool checkObsticle;

		public bool thermalOn;

		public bool ignoreFowBlockLayer;

		public int searchPickupID;

		public Action<DamageReceiver, InteractablePickup> onSearchFinishedCallback;

		public SearchTaskContext(Vector3 center, Vector3 dir, float searchAngle, float searchDistance, Teams selfTeam, bool checkObsticle, bool thermalOn, bool ignoreFowBlockLayer, int searchPickupID, Action<DamageReceiver, InteractablePickup> callback)
		{
			searchCenter = center;
			searchDirection = dir;
			this.searchAngle = searchAngle;
			this.searchDistance = searchDistance;
			this.selfTeam = selfTeam;
			this.thermalOn = thermalOn;
			this.checkObsticle = checkObsticle;
			this.searchPickupID = searchPickupID;
			onSearchFinishedCallback = callback;
			this.ignoreFowBlockLayer = ignoreFowBlockLayer;
		}
	}

	public struct CheckObsticleTaskContext
	{
		public Vector3 start;

		public Vector3 end;

		public bool thermalOn;

		public bool ignoreFowBlockLayer;

		public Action<bool> onCheckFinishCallback;

		public CheckObsticleTaskContext(Vector3 start, Vector3 end, bool thermalOn, bool ignoreFowBlockLayer, Action<bool> onCheckFinishCallback)
		{
			this.start = start;
			this.end = end;
			this.thermalOn = thermalOn;
			this.onCheckFinishCallback = onCheckFinishCallback;
			this.ignoreFowBlockLayer = ignoreFowBlockLayer;
		}
	}

	private Queue<SearchTaskContext> searchTasks;

	private Queue<CheckObsticleTaskContext> checkObsticleTasks;

	private LayerMask dmgReceiverLayers;

	private LayerMask interactLayers;

	private LayerMask obsticleLayers;

	private LayerMask obsticleLayersWithThermal;

	private Collider[] cols;

	private RaycastHit[] ObsHits;

	public int maxSeachCount;

	public int maxCheckObsticleCount;

	private static CharacterMainControl _mc;

	private int fowBlockLayer;

	private DamageReceiver dmgReceiverTemp;

	private static CharacterMainControl mainCharacter
	{
		get
		{
			if (_mc == null)
			{
				_mc = CharacterMainControl.Main;
			}
			return _mc;
		}
	}

	public static event Action<AISound> OnSoundSpawned;

	public static event Action<AISound> OnPlayerHearSound;

	public static void MakeSound(AISound sound)
	{
		AIMainBrain.OnSoundSpawned?.Invoke(sound);
		FilterPlayerHearSound(sound);
	}

	private static void FilterPlayerHearSound(AISound sound)
	{
		if (!mainCharacter || !Team.IsEnemy(Teams.player, sound.fromTeam) || ((bool)sound.fromCharacter && (bool)sound.fromCharacter.characterModel && !sound.fromCharacter.characterModel.Hidden && !GameCamera.Instance.IsOffScreen(sound.pos)))
		{
			return;
		}
		float num = Vector3.Distance(sound.pos, mainCharacter.transform.position);
		if (!(mainCharacter.SoundVisable < 0.2f))
		{
			float hearingAbility = mainCharacter.HearingAbility;
			if (!(num > sound.radius * hearingAbility))
			{
				AIMainBrain.OnPlayerHearSound?.Invoke(sound);
			}
		}
	}

	public void Awake()
	{
		searchTasks = new Queue<SearchTaskContext>();
		checkObsticleTasks = new Queue<CheckObsticleTaskContext>();
		fowBlockLayer = LayerMask.NameToLayer("FowBlock");
	}

	private void Start()
	{
		dmgReceiverLayers = GameplayDataSettings.Layers.damageReceiverLayerMask;
		interactLayers = 1 << LayerMask.NameToLayer("Interactable");
		obsticleLayers = GameplayDataSettings.Layers.fowBlockLayers;
		obsticleLayersWithThermal = GameplayDataSettings.Layers.fowBlockLayersWithThermal;
		cols = new Collider[15];
		ObsHits = new RaycastHit[15];
	}

	private void Update()
	{
		for (int i = 0; i < maxSeachCount; i++)
		{
			if (searchTasks.Count <= 0)
			{
				break;
			}
			DoSearch(searchTasks.Dequeue());
		}
		for (int j = 0; j < maxCheckObsticleCount; j++)
		{
			if (checkObsticleTasks.Count <= 0)
			{
				break;
			}
			DoCheckObsticle(checkObsticleTasks.Dequeue());
		}
	}

	private void DoSearch(SearchTaskContext context)
	{
		int num = Physics.OverlapSphereNonAlloc(context.searchCenter, context.searchDistance, cols, (context.searchPickupID > 0) ? ((int)dmgReceiverLayers | (int)interactLayers) : ((int)dmgReceiverLayers), QueryTriggerInteraction.Collide);
		if (num <= 0)
		{
			context.onSearchFinishedCallback(null, null);
			return;
		}
		float num2 = 9999f;
		DamageReceiver arg = null;
		float num3 = 9999f;
		InteractablePickup arg2 = null;
		float num4 = 1.5f;
		for (int i = 0; i < num; i++)
		{
			Collider collider = cols[i];
			if (Mathf.Abs(context.searchCenter.y - collider.transform.position.y) > 4f)
			{
				continue;
			}
			float num5 = Vector3.Distance(context.searchCenter, collider.transform.position);
			if (Vector3.Angle(context.searchDirection.normalized, (collider.transform.position - context.searchCenter).normalized) > context.searchAngle * 0.5f && num5 > num4)
			{
				continue;
			}
			dmgReceiverTemp = null;
			float num6 = 1f;
			if (collider.gameObject.IsInLayerMask(dmgReceiverLayers))
			{
				dmgReceiverTemp = collider.GetComponent<DamageReceiver>();
				if (dmgReceiverTemp != null && (bool)dmgReceiverTemp.health)
				{
					CharacterMainControl characterMainControl = dmgReceiverTemp.health.TryGetCharacter();
					if ((bool)characterMainControl)
					{
						num6 = characterMainControl.VisableDistanceFactor;
					}
				}
			}
			if (num5 > context.searchDistance * num6 || (num5 >= num2 && num5 >= num3) || (context.checkObsticle && num5 > num4 && CheckObsticle(context.searchCenter, collider.transform.position + Vector3.up * 1.5f, context.thermalOn, context.ignoreFowBlockLayer)))
			{
				continue;
			}
			if ((bool)dmgReceiverTemp)
			{
				if (!(dmgReceiverTemp.health == null) && Team.IsEnemy(context.selfTeam, dmgReceiverTemp.Team))
				{
					num2 = num5;
					arg = dmgReceiverTemp;
				}
			}
			else if (context.searchPickupID > 0)
			{
				InteractablePickup component = collider.GetComponent<InteractablePickup>();
				if ((bool)component && (bool)component.ItemAgent && (bool)component.ItemAgent.Item && component.ItemAgent.Item.TypeID == context.searchPickupID)
				{
					num3 = num5;
					arg2 = component;
				}
			}
		}
		context.onSearchFinishedCallback(arg, arg2);
	}

	public void AddSearchTask(Vector3 center, Vector3 dir, float searchAngle, float searchDistance, Teams selfTeam, bool checkObsticle, bool thermalOn, bool ignoreFowBlockLayer, int searchPickupID, Action<DamageReceiver, InteractablePickup> callback)
	{
		SearchTaskContext item = new SearchTaskContext(center, dir, searchAngle, searchDistance, selfTeam, checkObsticle, thermalOn, ignoreFowBlockLayer, searchPickupID, callback);
		searchTasks.Enqueue(item);
	}

	private void DoCheckObsticle(CheckObsticleTaskContext context)
	{
		bool obj = CheckObsticle(context.start, context.end, context.thermalOn, context.ignoreFowBlockLayer);
		context.onCheckFinishCallback(obj);
	}

	public void AddCheckObsticleTask(Vector3 start, Vector3 end, bool thermalOn, bool ignoreFowBlockLayer, Action<bool> callback)
	{
		CheckObsticleTaskContext item = new CheckObsticleTaskContext(start, end, thermalOn, ignoreFowBlockLayer, callback);
		checkObsticleTasks.Enqueue(item);
	}

	private bool CheckObsticle(Vector3 startPoint, Vector3 endPoint, bool thermalOn, bool ignoreFowBlockLayer)
	{
		Ray ray = new Ray(startPoint, (endPoint - startPoint).normalized);
		LayerMask layerMask = (thermalOn ? obsticleLayersWithThermal : obsticleLayers);
		if (ignoreFowBlockLayer)
		{
			layerMask = (int)layerMask & ~(1 << fowBlockLayer);
		}
		return Physics.RaycastNonAlloc(ray, ObsHits, (endPoint - startPoint).magnitude, layerMask) > 0;
	}
}
