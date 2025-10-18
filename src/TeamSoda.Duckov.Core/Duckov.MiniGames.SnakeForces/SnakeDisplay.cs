using System.Linq;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.MiniGames.SnakeForces;

public class SnakeDisplay : MiniGameBehaviour
{
	[SerializeField]
	private SnakeForce master;

	[SerializeField]
	private SnakePartDisplay partDisplayTemplate;

	[SerializeField]
	private Transform foodDisplayTemplate;

	[SerializeField]
	private Transform exitDisplayTemplte;

	[SerializeField]
	private ParticleSystem eatFXPrefab;

	[SerializeField]
	private int gridSize = 8;

	private PrefabPool<SnakePartDisplay> _partPool;

	private PrefabPool<Transform> _foodPool;

	private bool punchingColor;

	private int punchColorIndex;

	private PrefabPool<SnakePartDisplay> PartPool
	{
		get
		{
			if (_partPool == null)
			{
				_partPool = new PrefabPool<SnakePartDisplay>(partDisplayTemplate);
			}
			return _partPool;
		}
	}

	private PrefabPool<Transform> FoodPool
	{
		get
		{
			if (_foodPool == null)
			{
				_foodPool = new PrefabPool<Transform>(foodDisplayTemplate);
			}
			return _foodPool;
		}
	}

	private void Awake()
	{
		master.OnAddPart += OnAddPart;
		master.OnGameStart += OnGameStart;
		master.OnRemovePart += OnRemovePart;
		master.OnAfterTick += OnAfterTick;
		master.OnFoodEaten += OnFoodEaten;
		partDisplayTemplate.gameObject.SetActive(value: false);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		HandlePunchColor();
	}

	private void HandlePunchColor()
	{
		if (!punchingColor)
		{
			return;
		}
		if (punchColorIndex >= master.Snake.Count)
		{
			punchingColor = false;
			return;
		}
		SnakePartDisplay snakePartDisplay = PartPool.ActiveEntries.First((SnakePartDisplay e) => e.Target == master.Snake[punchColorIndex]);
		if ((bool)snakePartDisplay)
		{
			snakePartDisplay.PunchColor(Color.HSVToRGB((float)punchColorIndex % 12f / 12f, 1f, 1f));
		}
		punchColorIndex++;
	}

	private void OnGameStart(SnakeForce force)
	{
		RefreshFood();
	}

	private void OnFoodEaten(SnakeForce force, Vector2Int coord)
	{
		FXPool.Play(eatFXPrefab, GetWorldPosition(coord), Quaternion.LookRotation((Vector3Int)master.Head.direction, Vector3.forward));
		foreach (SnakePartDisplay activeEntry in PartPool.ActiveEntries)
		{
			activeEntry.Punch();
		}
		StartPunchingColor();
	}

	private void StartPunchingColor()
	{
		punchingColor = true;
		punchColorIndex = 0;
	}

	private void OnAfterTick(SnakeForce force)
	{
		RefreshFood();
	}

	private void RefreshFood()
	{
		FoodPool.ReleaseAll();
		foreach (Vector2Int food in master.Foods)
		{
			FoodPool.Get().localPosition = GetPosition(food);
		}
	}

	private void OnRemovePart(SnakeForce.Part part)
	{
		PartPool.ReleaseAll((SnakePartDisplay e) => e.Target == part);
	}

	private void OnAddPart(SnakeForce.Part part)
	{
		PartPool.Get().Setup(this, part);
	}

	internal Vector3 GetPosition(Vector2Int coord)
	{
		return (Vector2)(coord * gridSize);
	}

	internal Vector3 GetWorldPosition(Vector2Int coord)
	{
		Vector3 position = GetPosition(coord);
		return base.transform.TransformPoint(position);
	}
}
