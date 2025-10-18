using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Points))]
public class RandomCharacterSpawner : CharacterSpawnerComponentBase
{
	public Points spawnPoints;

	public CharacterSpawnerRoot spawnerRoot;

	public CharacterSpawnerGroup masterGroup;

	public List<CharacterRandomPresetInfo> randomPresetInfos;

	private float delayTime = 1f;

	public Vector2Int spawnCountRange;

	private float totalWeight = -1f;

	public bool isStaticTarget;

	public static string currentGizmosTag;

	public bool firstIsLeader;

	private bool firstCreateStarted;

	public UnityEvent OnStartCreateEvent;

	private int targetSpawnCount;

	private int currentSpawnedCount;

	private bool destroied;

	public string gizmosTag;

	private float minDistanceToMainCharacter => spawnerRoot.minDistanceToPlayer;

	private int scene => spawnerRoot.RelatedScene;

	private void ShowGizmo()
	{
		currentGizmosTag = gizmosTag;
	}

	public override void Init(CharacterSpawnerRoot root)
	{
		spawnerRoot = root;
		if (spawnPoints == null)
		{
			spawnPoints = GetComponent<Points>();
		}
	}

	private void OnDestroy()
	{
		destroied = true;
	}

	private CharacterRandomPresetInfo GetAPresetByWeight()
	{
		if (totalWeight < 0f)
		{
			totalWeight = 0f;
			for (int i = 0; i < randomPresetInfos.Count; i++)
			{
				if (randomPresetInfos[i].randomPreset == null)
				{
					randomPresetInfos.RemoveAt(i);
					i--;
					Debug.Log("Null preset");
				}
				else
				{
					totalWeight += randomPresetInfos[i].weight;
				}
			}
		}
		float num = Random.Range(0f, totalWeight);
		float num2 = 0f;
		for (int j = 0; j < randomPresetInfos.Count; j++)
		{
			num2 += randomPresetInfos[j].weight;
			if (num < num2)
			{
				return randomPresetInfos[j];
			}
		}
		Debug.LogError("权重计算错误", base.gameObject);
		return randomPresetInfos[randomPresetInfos.Count - 1];
	}

	public override void StartSpawn()
	{
		CreateAsync().Forget();
	}

	private async UniTaskVoid CreateAsync()
	{
		if ((bool)LevelManager.Instance && LevelManager.Instance.IsBaseLevel)
		{
			delayTime = 0.5f;
		}
		if (LevelManager.Instance == null || spawnPoints == null)
		{
			return;
		}
		OnStartCreateEvent?.Invoke();
		int count = (targetSpawnCount = Random.Range(spawnCountRange.x, spawnCountRange.y + 1));
		List<Vector3> randomPoints = spawnPoints.GetRandomPoints(count);
		foreach (Vector3 item in randomPoints)
		{
			bool flag = false;
			if (!firstCreateStarted)
			{
				flag = true;
				firstCreateStarted = true;
			}
			CreateAt(item, scene, masterGroup, flag && firstIsLeader).Forget();
			currentSpawnedCount++;
			await UniTask.WaitForSeconds(0.1f, ignoreTimeScale: true);
		}
	}

	private async UniTask<CharacterMainControl> CreateAt(Vector3 point, int scene, CharacterSpawnerGroup group, bool isLeader)
	{
		if (randomPresetInfos.Count <= 0)
		{
			return null;
		}
		Vector3 direction = Random.insideUnitCircle.normalized;
		direction.z = direction.y;
		direction.y = 0f;
		while ((bool)CharacterMainControl.Main && Vector3.Distance(point, CharacterMainControl.Main.transform.position) < minDistanceToMainCharacter)
		{
			await UniTask.Yield();
		}
		if (destroied || base.gameObject == null || !LevelManager.Instance || CharacterMainControl.Main == null)
		{
			return null;
		}
		if (isStaticTarget)
		{
			direction = base.transform.forward;
		}
		CharacterMainControl characterMainControl = await GetAPresetByWeight().randomPreset.CreateCharacterAsync(point, direction, scene, group, isLeader);
		if (isStaticTarget)
		{
			Rigidbody component = characterMainControl.GetComponent<Rigidbody>();
			component.collisionDetectionMode = CollisionDetectionMode.Discrete;
			component.isKinematic = true;
		}
		spawnerRoot.AddCreatedCharacter(characterMainControl);
		return characterMainControl;
	}

	private void OnDrawGizmos()
	{
		if (!(currentGizmosTag != gizmosTag))
		{
			Gizmos.color = Color.yellow;
			if ((bool)spawnPoints && spawnPoints.points.Count > 0)
			{
				Vector3 point = spawnPoints.GetPoint(0);
				Vector3 vector = point + Vector3.up * 20f;
				Gizmos.DrawWireSphere(point, 10f);
				Gizmos.DrawLine(point, vector);
				Gizmos.DrawSphere(vector, 3f);
			}
		}
	}
}
