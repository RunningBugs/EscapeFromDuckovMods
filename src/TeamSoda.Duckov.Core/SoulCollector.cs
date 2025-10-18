using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

public class SoulCollector : MonoBehaviour
{
	public DuckovItemAgent selfAgent;

	private CharacterMainControl selfCharacter;

	[ItemTypeID]
	public int soulCubeID = 1165;

	private Slot cubeSlot;

	public GameObject addFx;

	public SoulCube cubePfb;

	private void Awake()
	{
		Health.OnDead += OnCharacterDie;
	}

	private void OnDestroy()
	{
		Health.OnDead -= OnCharacterDie;
	}

	private void Update()
	{
	}

	private void OnCharacterDie(Health health, DamageInfo dmgInfo)
	{
		if (!health || !health.hasSoul)
		{
			return;
		}
		if (!selfCharacter && (bool)selfAgent.Item)
		{
			selfCharacter = selfAgent.Item.GetCharacterMainControl();
		}
		if ((bool)selfCharacter && !(Vector3.Distance(health.transform.position, selfCharacter.transform.position) > 40f))
		{
			int num = Mathf.RoundToInt(health.MaxHealth / 15f);
			if (num < 1)
			{
				num = 1;
			}
			if (LevelManager.Rule.AdvancedDebuffMode)
			{
				num *= 3;
			}
			SpawnCubes(health.transform.position + Vector3.up * 0.75f, num).Forget();
		}
	}

	private async UniTaskVoid SpawnCubes(Vector3 startPoint, int times)
	{
		if (this == null)
		{
			return;
		}
		for (int i = 0; i < times; i++)
		{
			if (this == null)
			{
				break;
			}
			Object.Instantiate(cubePfb, startPoint, Quaternion.identity).Init(this);
			await UniTask.WaitForSeconds(0.05f);
		}
	}

	public void AddCube()
	{
		AddCubeAsync().Forget();
	}

	private async UniTaskVoid AddCubeAsync()
	{
		if (cubeSlot == null)
		{
			cubeSlot = selfAgent.Item.Slots["SoulCube"];
		}
		if (cubeSlot == null)
		{
			return;
		}
		if (cubeSlot.Content != null)
		{
			if (cubeSlot.Content.StackCount >= cubeSlot.Content.MaxStackCount)
			{
				return;
			}
			cubeSlot.Content.StackCount++;
		}
		else
		{
			Item otherItem = await ItemAssetsCollection.InstantiateAsync(soulCubeID);
			cubeSlot.Plug(otherItem, out var _);
		}
		Object.Instantiate(addFx, base.transform, worldPositionStays: false);
	}
}
