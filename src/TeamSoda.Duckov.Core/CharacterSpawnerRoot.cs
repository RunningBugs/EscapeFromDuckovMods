using System.Collections.Generic;
using Duckov.Scenes;
using Duckov.Weathers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CharacterSpawnerRoot : MonoBehaviour
{
	public bool needTrigger;

	public OnTriggerEnterEvent trigger;

	private bool playerInTrigger;

	private bool created;

	private bool inited;

	[Range(0f, 1f)]
	public float spawnChance = 1f;

	public float minDistanceToPlayer = 25f;

	public bool useTimeOfDay;

	public float whenToSpawn;

	[Range(0f, 24f)]
	public float spawnTimeRangeFrom;

	[Range(0f, 24f)]
	public float spawnTimeRangeTo;

	[FormerlySerializedAs("despawnIfOutOfTime")]
	public bool despawnIfTimingWrong;

	public bool checkWeather;

	public List<Weather> targetWeathers;

	private int relatedScene = -1;

	[SerializeField]
	private CharacterSpawnerComponentBase spawnerComponent;

	public bool autoRefreshGuid = true;

	public int SpawnerGuid;

	private List<CharacterMainControl> createdCharacters = new List<CharacterMainControl>();

	private List<CharacterMainControl> despawningCharacters = new List<CharacterMainControl>();

	private float despawnTickTimer = 1f;

	public UnityEvent OnStartEvent;

	public UnityEvent OnAllDeadEvent;

	private bool allDeadEventInvoked;

	private bool stillhasAliveCharacters;

	private bool allDead;

	public int RelatedScene => relatedScene;

	private void Awake()
	{
		if (createdCharacters == null)
		{
			createdCharacters = new List<CharacterMainControl>();
		}
		if (despawningCharacters == null)
		{
			despawningCharacters = new List<CharacterMainControl>();
		}
		if (!useTimeOfDay && !checkWeather)
		{
			despawnIfTimingWrong = false;
		}
		if (needTrigger && (bool)trigger)
		{
			trigger.triggerOnce = false;
			trigger.onlyMainCharacter = true;
			trigger.DoOnTriggerEnter.AddListener(DoOnTriggerEnter);
			trigger.DoOnTriggerExit.AddListener(DoOnTriggerLeave);
		}
	}

	private void OnDestroy()
	{
		if (needTrigger && (bool)trigger)
		{
			trigger.DoOnTriggerEnter.RemoveListener(DoOnTriggerEnter);
			trigger.DoOnTriggerExit.RemoveListener(DoOnTriggerLeave);
		}
	}

	private void Start()
	{
		if ((bool)LevelManager.Instance && LevelManager.Instance.IsBaseLevel)
		{
			minDistanceToPlayer = 0f;
		}
	}

	private void Update()
	{
		if (!inited && LevelManager.LevelInited)
		{
			Init();
		}
		bool flag = CheckTiming();
		if (inited && !created && flag)
		{
			StartSpawn();
		}
		if (created && !flag && despawnIfTimingWrong)
		{
			despawningCharacters.AddRange(createdCharacters);
			createdCharacters.Clear();
			created = false;
		}
		despawnTickTimer -= Time.deltaTime;
		if (despawnTickTimer < 0f && despawnIfTimingWrong && despawningCharacters.Count > 0)
		{
			CheckDespawn();
		}
		if (!(despawnTickTimer < 0f) || allDead || !stillhasAliveCharacters || allDeadEventInvoked)
		{
			return;
		}
		if (createdCharacters.Count <= 0)
		{
			allDead = true;
		}
		else
		{
			allDead = true;
			foreach (CharacterMainControl createdCharacter in createdCharacters)
			{
				if (createdCharacter != null && (bool)createdCharacter.Health && !createdCharacter.Health.IsDead)
				{
					allDead = false;
					break;
				}
			}
		}
		if (allDead)
		{
			stillhasAliveCharacters = false;
			OnAllDeadEvent?.Invoke();
			allDeadEventInvoked = true;
		}
	}

	private void CheckDespawn()
	{
		for (int i = 0; i < despawningCharacters.Count; i++)
		{
			CharacterMainControl characterMainControl = despawningCharacters[i];
			if (!characterMainControl)
			{
				despawningCharacters.RemoveAt(i);
				i--;
			}
			else if (!characterMainControl.gameObject.activeInHierarchy)
			{
				Object.Destroy(characterMainControl.gameObject);
				despawningCharacters.RemoveAt(i);
				i--;
			}
		}
	}

	private bool CheckTiming()
	{
		if (LevelManager.Instance == null)
		{
			return false;
		}
		if (needTrigger && !playerInTrigger)
		{
			return false;
		}
		bool flag = false;
		if (useTimeOfDay)
		{
			float num = (float)GameClock.TimeOfDay.TotalHours % 24f;
			flag = (num >= spawnTimeRangeFrom && num <= spawnTimeRangeTo) || (spawnTimeRangeTo < spawnTimeRangeFrom && (num >= spawnTimeRangeFrom || num <= spawnTimeRangeTo));
		}
		else
		{
			flag = LevelManager.Instance.LevelTime >= whenToSpawn;
		}
		bool flag2 = true;
		if (checkWeather && !targetWeathers.Contains(TimeOfDayController.Instance.CurrentWeather))
		{
			flag2 = false;
		}
		return flag && flag2;
	}

	private void Init()
	{
		inited = true;
		spawnerComponent.Init(this);
		_ = SceneManager.GetActiveScene().buildIndex;
		bool flag = true;
		if (MultiSceneCore.Instance != null)
		{
			flag = MultiSceneCore.Instance.usedCreatorIds.Contains(SpawnerGuid);
		}
		if (flag)
		{
			Debug.Log("Contain this spawner");
			Object.Destroy(base.gameObject);
			return;
		}
		relatedScene = SceneManager.GetActiveScene().buildIndex;
		flag = true;
		base.transform.SetParent(null);
		MultiSceneCore.MoveToMainScene(base.gameObject);
		MultiSceneCore.Instance.usedCreatorIds.Add(SpawnerGuid);
	}

	private void StartSpawn()
	{
		if (created)
		{
			return;
		}
		created = true;
		if (!(Random.Range(0f, 1f) > spawnChance))
		{
			OnStartEvent?.Invoke();
			if ((bool)spawnerComponent)
			{
				spawnerComponent.StartSpawn();
			}
		}
	}

	private void DoOnTriggerEnter()
	{
		playerInTrigger = true;
	}

	private void DoOnTriggerLeave()
	{
		playerInTrigger = false;
	}

	public void AddCreatedCharacter(CharacterMainControl c)
	{
		createdCharacters.Add(c);
		stillhasAliveCharacters = true;
	}
}
