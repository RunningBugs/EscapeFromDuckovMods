using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Duckov.Utilities;
using Saves;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.BubblePoppers;

public class BubblePopper : MiniGameBehaviour
{
	public enum Status
	{
		Idle,
		Loaded,
		Launched,
		Settled,
		GameOver
	}

	public struct CastResult
	{
		public Vector2 origin;

		public Vector2 castDirection;

		public float castDistance;

		public bool clipWall;

		public bool touchWall;

		public int touchWallDirection;

		public bool collide;

		public Bubble touchingBubble;

		public Vector2Int touchBubbleCoord;

		public bool touchCeiling;

		public Vector2 endPosition;

		public Vector2Int endCoord;

		public bool Collide
		{
			get
			{
				if (!collide && !clipWall && !touchWall)
				{
					return touchingBubble;
				}
				return true;
			}
		}
	}

	[SerializeField]
	private Bubble bubbleTemplate;

	[SerializeField]
	private BubblePopperLayout layout;

	[SerializeField]
	private Image waitingColorIndicator;

	[SerializeField]
	private Image loadedColorIndicator;

	[SerializeField]
	private Transform cannon;

	[SerializeField]
	private LineRenderer aimingLine;

	[SerializeField]
	private Transform cameraParent;

	[SerializeField]
	private Animator duckAnimator;

	[SerializeField]
	private Transform gear;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI highScoreText;

	[SerializeField]
	private GameObject startScreen;

	[SerializeField]
	private GameObject endScreen;

	[SerializeField]
	private GameObject failIndicator;

	[SerializeField]
	private GameObject clearIndicator;

	[SerializeField]
	private GameObject newRecordIndicator;

	[SerializeField]
	private GameObject allLevelsClearIndicator;

	[SerializeField]
	private TextMeshProUGUI endScreenLevelText;

	[SerializeField]
	private TextMeshProUGUI endScreenScoreText;

	[SerializeField]
	private BubblePopperLevelDataProvider levelDataProvider;

	[SerializeField]
	private Color[] colorPallette;

	[SerializeField]
	private float aimingDistance = 100f;

	[SerializeField]
	private Vector2 cannonAngleRange = new Vector2(-45f, 45f);

	[SerializeField]
	private float cannonRotateSpeed = 20f;

	[SerializeField]
	private int ceilingYCoord;

	[SerializeField]
	private int initialFloorYCoord = -18;

	[SerializeField]
	private int floorStepAfterShots = 4;

	[SerializeField]
	private float bubbleMoveSpeed = 100f;

	private float shockwaveStrength = 2f;

	[SerializeField]
	private float moveCeilingTime = 1f;

	[SerializeField]
	private AnimationCurve moveCeilingCurve;

	private PrefabPool<Bubble> _bubblePool;

	private Dictionary<Vector2Int, Bubble> attachedBubbles = new Dictionary<Vector2Int, Bubble>();

	private float cannonAngle;

	private int waitingColor;

	private int loadedColor;

	private Bubble activeBubble;

	private bool clear;

	private bool fail;

	private bool allLevelsClear;

	private bool playing;

	[SerializeField]
	private int floorYCoord;

	private int levelIndex;

	private int _score;

	private bool isHighScore;

	private const string HighScoreSaveKey = "MiniGame/BubblePopper/HighScore";

	private const string HighLevelSaveKey = "MiniGame/BubblePopper/HighLevel";

	private const int CriticalCount = 3;

	private bool movingCeiling;

	private float moveCeilingT;

	private Vector2 originalCeilingPos;

	private Vector3[] aimlinePoints = new Vector3[3];

	[SerializeField]
	private bool drawGizmos = true;

	[SerializeField]
	private float distance;

	public int AvaliableColorCount => colorPallette.Length;

	public BubblePopperLayout Layout => layout;

	public float BubbleRadius
	{
		get
		{
			if (bubbleTemplate == null)
			{
				return 8f;
			}
			return bubbleTemplate.Radius;
		}
	}

	public Bubble BubbleTemplate => bubbleTemplate;

	private PrefabPool<Bubble> BubblePool
	{
		get
		{
			if (_bubblePool == null)
			{
				_bubblePool = new PrefabPool<Bubble>(bubbleTemplate, null, OnGetBubble);
			}
			return _bubblePool;
		}
	}

	public Status status { get; private set; }

	public int FloorStepETA { get; private set; }

	public int Score
	{
		get
		{
			return _score;
		}
		private set
		{
			_score = value;
			RefreshScoreText();
		}
	}

	public static int HighScore
	{
		get
		{
			return SavesSystem.Load<int>("MiniGame/BubblePopper/HighScore");
		}
		set
		{
			SavesSystem.Save("MiniGame/BubblePopper/HighScore", value);
		}
	}

	public static int HighLevel
	{
		get
		{
			return SavesSystem.Load<int>("MiniGame/BubblePopper/HighLevel");
		}
		set
		{
			SavesSystem.Save("MiniGame/BubblePopper/HighLevel", value);
		}
	}

	public bool Busy { get; private set; }

	public static event Action<int> OnLevelClear;

	private void OnGetBubble(Bubble bubble)
	{
		bubble.Rest();
	}

	protected override void Start()
	{
		base.Start();
		RefreshScoreText();
		RefreshLevelText();
		HideEndScreen();
		ShowStartScreen();
	}

	private void RefreshScoreText()
	{
		scoreText.text = $"{Score}";
		highScoreText.text = $"{HighScore}";
	}

	private void RefreshLevelText()
	{
		levelText.text = $"{levelIndex}";
	}

	protected override void OnUpdate(float deltaTime)
	{
		UpdateStatus(deltaTime);
		HandleInput(deltaTime);
		UpdateAimingLine();
	}

	private void ShowStartScreen()
	{
		startScreen.SetActive(value: true);
	}

	private void HideStartScreen()
	{
		startScreen.SetActive(value: false);
	}

	private void ShowEndScreen()
	{
		endScreen.SetActive(value: true);
		endScreenLevelText.text = $"LEVEL {levelIndex}";
		endScreenScoreText.text = $"{Score}";
		failIndicator.SetActive(fail);
		clearIndicator.SetActive(clear);
		newRecordIndicator.SetActive(isHighScore);
		allLevelsClearIndicator.SetActive(allLevelsClear);
	}

	private void HideEndScreen()
	{
		endScreen.SetActive(value: false);
	}

	private void NewGame()
	{
		playing = true;
		levelIndex = 0;
		Score = 0;
		isHighScore = false;
		HideStartScreen();
		HideEndScreen();
		int[] levelData = LoadLevelData(levelIndex);
		StartNewLevel(levelData);
		RefreshLevelText();
	}

	private void NextLevel()
	{
		levelIndex++;
		HideStartScreen();
		HideEndScreen();
		int[] levelData = LoadLevelData(levelIndex);
		StartNewLevel(levelData);
		RefreshLevelText();
	}

	private int[] LoadLevelData(int levelIndex)
	{
		return levelDataProvider.GetData(levelIndex);
	}

	private Vector2Int LevelDataIndexToCoord(int index)
	{
		int num = layout.XCoordBorder.y - layout.XCoordBorder.x + 1;
		int num2 = index / num;
		return new Vector2Int(index % num, -num2);
	}

	private void StartNewLevel(int[] levelData)
	{
		clear = false;
		fail = false;
		FloorStepETA = floorStepAfterShots;
		BubblePool.ReleaseAll();
		attachedBubbles.Clear();
		ResetFloor();
		for (int i = 0; i < levelData.Length; i++)
		{
			int num = levelData[i];
			if (num >= 0)
			{
				Vector2Int coord = LevelDataIndexToCoord(i);
				Bubble bubble = BubblePool.Get();
				bubble.Setup(this, num);
				Set(bubble, coord);
			}
		}
		PushRandomColor();
		PushRandomColor();
		SetStatus(Status.Loaded);
	}

	private void ResetFloor()
	{
		floorYCoord = initialFloorYCoord;
		RefreshLayoutPosition();
	}

	private void StepFloor()
	{
		floorYCoord++;
		BeginMovingCeiling();
	}

	private void RefreshLayoutPosition()
	{
		Vector3 localPosition = layout.transform.localPosition;
		localPosition.y = (float)(-(floorYCoord - initialFloorYCoord)) * BubbleRadius * BubblePopperLayout.YOffsetFactor;
		layout.transform.localPosition = localPosition;
	}

	private void UpdateStatus(float deltaTime)
	{
		switch (status)
		{
		case Status.Idle:
		case Status.GameOver:
			if (base.Game.GetButtonDown(MiniGame.Button.Start))
			{
				if (!playing || fail || allLevelsClear)
				{
					NewGame();
				}
				else
				{
					NextLevel();
				}
			}
			break;
		case Status.Launched:
			UpdateLaunched(deltaTime);
			break;
		case Status.Settled:
			UpdateSettled(deltaTime);
			break;
		case Status.Loaded:
			break;
		}
	}

	private void BeginMovingCeiling()
	{
		movingCeiling = true;
		moveCeilingT = 0f;
		originalCeilingPos = layout.transform.localPosition;
	}

	private void UpdateMoveCeiling(float deltaTime)
	{
		moveCeilingT += deltaTime;
		if (moveCeilingT >= moveCeilingTime)
		{
			movingCeiling = false;
			RefreshLayoutPosition();
			return;
		}
		Vector2 b = new Vector2(layout.transform.localPosition.x, (float)(-(floorYCoord - initialFloorYCoord)) * BubbleRadius * BubblePopperLayout.YOffsetFactor);
		float t = moveCeilingCurve.Evaluate(moveCeilingT / moveCeilingTime);
		Vector3 localPosition = Vector2.LerpUnclamped(originalCeilingPos, b, t);
		layout.transform.localPosition = localPosition;
	}

	private void UpdateSettled(float deltaTime)
	{
		if (movingCeiling)
		{
			UpdateMoveCeiling(deltaTime);
		}
		else if (CheckGameOver())
		{
			SetStatus(Status.GameOver);
		}
		else
		{
			SetStatus(Status.Loaded);
		}
	}

	private void HandleFloorStep()
	{
		FloorStepETA--;
		if (FloorStepETA <= 0)
		{
			StepFloor();
			FloorStepETA = floorStepAfterShots;
		}
	}

	private bool CheckGameOver()
	{
		if (attachedBubbles.Count == 0)
		{
			clear = true;
			allLevelsClear = levelIndex >= levelDataProvider.TotalLevels;
			if (clear)
			{
				if (levelIndex > HighLevel)
				{
					HighLevel = levelIndex;
				}
				BubblePopper.OnLevelClear?.Invoke(levelIndex);
			}
			return true;
		}
		if (attachedBubbles.Keys.Any((Vector2Int e) => e.y <= floorYCoord))
		{
			fail = true;
			return true;
		}
		return false;
	}

	private void SetStatus(Status newStatus)
	{
		OnExitStatus(status);
		status = newStatus;
		switch (status)
		{
		case Status.Settled:
			PushRandomColor();
			HandleFloorStep();
			break;
		case Status.GameOver:
			if (Score > HighScore)
			{
				HighScore = Score;
				isHighScore = true;
			}
			ShowGameOverScreen();
			break;
		case Status.Idle:
		case Status.Loaded:
		case Status.Launched:
			break;
		}
	}

	private void ShowGameOverScreen()
	{
		ShowEndScreen();
	}

	private void OnExitStatus(Status status)
	{
		switch (status)
		{
		}
	}

	private void Set(Bubble bubble, Vector2Int coord)
	{
		attachedBubbles[coord] = bubble;
		bubble.NotifyAttached(coord);
	}

	private void Attach(Bubble bubble, Vector2Int coord)
	{
		if (attachedBubbles.TryGetValue(coord, out var _))
		{
			Debug.LogError("Target coord is occupied!");
			return;
		}
		Set(bubble, coord);
		List<Vector2Int> continousCoords = GetContinousCoords(coord);
		if (continousCoords.Count >= 3)
		{
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			int num = 0;
			foreach (Vector2Int item in continousCoords)
			{
				hashSet.AddRange(layout.GetAllNeighbourCoords(item, includeCenter: false));
				Explode(item, coord);
				num++;
			}
			PunchCamera();
			HashSet<Vector2Int> looseCoords = GetLooseCoords(hashSet);
			foreach (Vector2Int item2 in looseCoords)
			{
				Detach(item2);
			}
			CalculateAndAddScore(looseCoords, continousCoords);
		}
		Shockwave(coord, shockwaveStrength).Forget();
	}

	private void CalculateAndAddScore(HashSet<Vector2Int> detached, List<Vector2Int> exploded)
	{
		int count = exploded.Count;
		int count2 = detached.Count;
		int num = Mathf.FloorToInt(Mathf.Pow(count, 2f)) * (1 + count2);
		Score += num;
	}

	private void Explode(Vector2Int coord, Vector2Int origin)
	{
		if (attachedBubbles.TryGetValue(coord, out var value))
		{
			attachedBubbles.Remove(coord);
			if (!(value == null))
			{
				value.NotifyExplode(origin);
			}
		}
	}

	private List<Vector2Int> GetContinousCoords(Vector2Int root)
	{
		List<Vector2Int> list = new List<Vector2Int>();
		if (!attachedBubbles.TryGetValue(root, out var value))
		{
			return list;
		}
		if (value == null)
		{
			return list;
		}
		int colorIndex = value.ColorIndex;
		HashSet<Vector2Int> visitedCoords = new HashSet<Vector2Int>();
		Stack<Vector2Int> coords = new Stack<Vector2Int>();
		Push(root);
		while (coords.Count > 0)
		{
			Vector2Int vector2Int = coords.Pop();
			if (!attachedBubbles.TryGetValue(vector2Int, out var value2) || value2 == null || value2.ColorIndex != colorIndex)
			{
				continue;
			}
			list.Add(vector2Int);
			Vector2Int[] allNeighbourCoords = layout.GetAllNeighbourCoords(vector2Int, includeCenter: false);
			foreach (Vector2Int vector2Int2 in allNeighbourCoords)
			{
				if (!visitedCoords.Contains(vector2Int2))
				{
					Push(vector2Int2);
				}
			}
		}
		return list;
		void Push(Vector2Int coord)
		{
			coords.Push(coord);
			visitedCoords.Add(coord);
		}
	}

	private HashSet<Vector2Int> GetLooseCoords(HashSet<Vector2Int> roots)
	{
		List<Vector2Int> pendingRoots = roots.ToList();
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		while (pendingRoots.Count > 0)
		{
			Vector2Int root = PopRoot();
			if (CheckConnectedLoose(root, out var connected))
			{
				hashSet.AddRange(connected);
			}
		}
		return hashSet;
		bool CheckConnectedLoose(Vector2Int item, out List<Vector2Int> reference)
		{
			reference = new List<Vector2Int>();
			bool result = true;
			Stack<Vector2Int> stack = new Stack<Vector2Int>();
			HashSet<Vector2Int> hashSet2 = new HashSet<Vector2Int>();
			stack.Push(item);
			hashSet2.Add(item);
			while (stack.Count > 0)
			{
				Vector2Int vector2Int = stack.Pop();
				pendingRoots.Remove(vector2Int);
				if (attachedBubbles.ContainsKey(vector2Int))
				{
					if (vector2Int.y >= ceilingYCoord)
					{
						result = false;
					}
					reference.Add(vector2Int);
					Vector2Int[] allNeighbourCoords = layout.GetAllNeighbourCoords(vector2Int, includeCenter: false);
					foreach (Vector2Int item2 in allNeighbourCoords)
					{
						if (!hashSet2.Contains(item2))
						{
							stack.Push(item2);
							hashSet2.Add(item2);
						}
					}
				}
			}
			return result;
		}
		Vector2Int PopRoot()
		{
			Vector2Int result = pendingRoots[0];
			pendingRoots.RemoveAt(0);
			return result;
		}
	}

	private void Detach(Vector2Int coord)
	{
		if (attachedBubbles.TryGetValue(coord, out var value))
		{
			attachedBubbles.Remove(coord);
			value.NotifyDetached();
		}
	}

	private void UpdateAimingLine()
	{
		aimingLine.gameObject.SetActive(status == Status.Loaded);
		Matrix4x4 worldToLocalMatrix = layout.transform.worldToLocalMatrix;
		Vector3 vector = worldToLocalMatrix.MultiplyPoint(cannon.position);
		Vector3 vector2 = worldToLocalMatrix.MultiplyVector(cannon.up);
		Vector3 vector3 = vector2 * aimingDistance;
		CastResult castResult = SlideCast(vector, vector3);
		vector.z = 0f;
		aimlinePoints[0] = vector;
		aimlinePoints[1] = castResult.endPosition;
		if (castResult.touchWall)
		{
			float num = Mathf.Max(aimingDistance - (castResult.endPosition - (Vector2)vector).magnitude, 0f);
			Vector2 vector4 = vector2;
			vector4.x *= -1f;
			aimlinePoints[2] = castResult.endPosition + vector4 * num;
		}
		else
		{
			aimlinePoints[2] = castResult.endPosition;
		}
		aimingLine.SetPositions(aimlinePoints);
	}

	private void UpdateLaunched(float deltaTime)
	{
		if (activeBubble == null || activeBubble.status != Bubble.Status.Moving)
		{
			activeBubble = null;
			SetStatus(Status.Settled);
		}
	}

	private void HandleInput(float deltaTime)
	{
		float x = base.Game.GetAxis().x;
		cannonAngle = Mathf.Clamp(cannonAngle - x * cannonRotateSpeed * deltaTime, cannonAngleRange.x, cannonAngleRange.y);
		cannon.rotation = Quaternion.Euler(0f, 0f, cannonAngle);
		duckAnimator.SetInteger("MovementDirection", (x > 0.01f) ? 1 : ((x < -0.01f) ? (-1) : 0));
		gear.Rotate(0f, 0f, x * cannonRotateSpeed * deltaTime);
		if (base.Game.GetButtonDown(MiniGame.Button.A))
		{
			LaunchBubble();
		}
	}

	public void MoveBubble(Bubble bubble, float deltaTime)
	{
		if (bubble == null)
		{
			return;
		}
		Vector2 moveDirection = bubble.MoveDirection;
		float num = deltaTime * bubbleMoveSpeed;
		Matrix4x4 worldToLocalMatrix = layout.transform.worldToLocalMatrix;
		Matrix4x4 localToWorldMatrix = layout.transform.localToWorldMatrix;
		Vector2 normalized = moveDirection.normalized;
		Vector2 origin = worldToLocalMatrix.MultiplyPoint(bubble.transform.position);
		Vector2 delta = (Vector2)worldToLocalMatrix.MultiplyVector(moveDirection.normalized) * num;
		CastResult castResult = SlideCast(origin, delta);
		bubble.transform.position = localToWorldMatrix.MultiplyPoint(castResult.endPosition);
		if (castResult.Collide)
		{
			if (castResult.touchWall && (float)castResult.touchWallDirection * normalized.x > 0f)
			{
				moveDirection.x *= -1f;
				bubble.MoveDirection = moveDirection;
			}
			if ((bool)castResult.touchingBubble || castResult.touchCeiling)
			{
				Attach(bubble, castResult.endCoord);
			}
		}
	}

	private Bubble LaunchBubble(Vector2 origin, Vector2 direction, int colorIndex)
	{
		Bubble bubble = BubblePool.Get();
		bubble.transform.position = layout.transform.localToWorldMatrix.MultiplyPoint(origin);
		bubble.MoveDirection = direction;
		bubble.Setup(this, colorIndex);
		bubble.Launch(direction);
		return bubble;
	}

	private void LaunchBubble()
	{
		if (status == Status.Loaded)
		{
			activeBubble = LaunchBubble(layout.transform.worldToLocalMatrix.MultiplyPoint(cannon.transform.position), layout.transform.worldToLocalMatrix.MultiplyVector(cannon.transform.up), loadedColor);
			loadedColor = -1;
			RefreshColorIndicators();
			SetStatus(Status.Launched);
		}
	}

	private void PunchLoadedIndicator()
	{
		loadedColorIndicator.transform.DOKill(complete: true);
		loadedColorIndicator.transform.localPosition = Vector2.left * 15f;
		loadedColorIndicator.transform.DOLocalMove(Vector3.zero, 0.1f, snapping: true);
	}

	private void PunchWaitingIndicator()
	{
		waitingColorIndicator.transform.localPosition = Vector2.zero;
		waitingColorIndicator.transform.DOKill(complete: true);
		waitingColorIndicator.transform.DOPunchPosition(Vector3.down * 5f, 0.5f, 10, 1f, snapping: true);
	}

	private void PushRandomColor()
	{
		loadedColor = waitingColor;
		waitingColor = UnityEngine.Random.Range(0, AvaliableColorCount);
		if (attachedBubbles.Count <= 0)
		{
			waitingColor = UnityEngine.Random.Range(0, AvaliableColorCount);
		}
		List<int> list = (from e in attachedBubbles.Values
			group e by e.ColorIndex into g
			select g.Key).ToList();
		waitingColor = list.GetRandom();
		RefreshColorIndicators();
		PunchLoadedIndicator();
		PunchWaitingIndicator();
	}

	private void RefreshColorIndicators()
	{
		loadedColorIndicator.color = GetDisplayColor(loadedColor);
		waitingColorIndicator.color = GetDisplayColor(waitingColor);
	}

	private bool IsCoordOccupied(Vector2Int coord, out Bubble touchingBubble, out bool ceiling)
	{
		ceiling = false;
		if (attachedBubbles.TryGetValue(coord, out touchingBubble))
		{
			return true;
		}
		if (coord.y > ceilingYCoord)
		{
			ceiling = true;
			return true;
		}
		return false;
	}

	public CastResult SlideCast(Vector2 origin, Vector2 delta)
	{
		float magnitude = delta.magnitude;
		Vector2 normalized = delta.normalized;
		float bubbleRadius = BubbleRadius;
		CastResult result = new CastResult
		{
			origin = origin,
			castDirection = normalized,
			castDistance = magnitude
		};
		Vector2 vector = origin + delta;
		float num = 1f;
		float num2 = layout.XPositionBorder.x + bubbleRadius;
		float num3 = layout.XPositionBorder.y - bubbleRadius;
		if (origin.x < num2 || origin.x > num3)
		{
			Vector2 endPosition = origin;
			endPosition.x = Mathf.Clamp(endPosition.x, num2 + 0.001f, num3 - 0.001f);
			result.endPosition = endPosition;
			result.clipWall = true;
			result.collide = true;
		}
		else
		{
			if (vector.x < num2)
			{
				result.touchWall = true;
				num = Mathf.Abs(origin.x - num2) / Mathf.Abs(delta.x);
				result.touchWallDirection = -1;
			}
			else if (vector.x > num3)
			{
				result.touchWall = true;
				num = Mathf.Abs(num3 - origin.x) / Mathf.Abs(delta.x);
				result.touchWallDirection = 1;
			}
			delta *= num;
			magnitude = delta.magnitude;
			result.endPosition = origin + delta;
			List<Vector2Int> allPassingCoords = layout.GetAllPassingCoords(origin, normalized, delta.magnitude);
			float num4 = magnitude;
			foreach (Vector2Int item in allPassingCoords)
			{
				if (IsCoordOccupied(item, out var touchingBubble, out var ceiling) && BubbleCast(layout.CoordToLocalPosition(item), origin, normalized, magnitude, out var hitCircleCenter))
				{
					float magnitude2 = (hitCircleCenter - origin).magnitude;
					if (magnitude2 < num4)
					{
						result.collide = true;
						result.touchingBubble = touchingBubble;
						result.touchBubbleCoord = item;
						result.endPosition = hitCircleCenter;
						result.touchCeiling = ceiling;
						num4 = magnitude2;
						result.touchWall = false;
					}
				}
			}
		}
		result.endCoord = layout.LocalPositionToCoord(result.endPosition);
		return result;
	}

	private bool BubbleCast(Vector2 pos, Vector2 origin, Vector2 direction, float distance, out Vector2 hitCircleCenter)
	{
		float bubbleRadius = BubbleRadius;
		hitCircleCenter = origin;
		Vector2 vector = pos - origin;
		float sqrMagnitude = vector.sqrMagnitude;
		float magnitude = vector.magnitude;
		if (magnitude > distance + 2f * bubbleRadius)
		{
			return false;
		}
		if (magnitude <= bubbleRadius * 2f)
		{
			hitCircleCenter = pos - 2f * vector.normalized * bubbleRadius;
			return true;
		}
		if (Vector2.Dot(vector, direction) < 0f)
		{
			return false;
		}
		float f = MathF.PI / 180f * Vector2.Angle(vector, direction);
		float num = vector.magnitude * Mathf.Sin(f);
		if (num > 2f * bubbleRadius)
		{
			return false;
		}
		float num2 = num * num;
		float num3 = bubbleRadius * bubbleRadius * 2f * 2f;
		float num4 = Mathf.Sqrt(sqrMagnitude - num2) - Mathf.Sqrt(num3 - num2);
		if (num4 > distance)
		{
			return false;
		}
		hitCircleCenter = origin + direction * num4;
		return true;
	}

	private void OnDrawGizmos()
	{
		if (!drawGizmos)
		{
			return;
		}
		float bubbleRadius = BubbleRadius;
		Matrix4x4 worldToLocalMatrix = layout.transform.worldToLocalMatrix;
		Vector3 vector = worldToLocalMatrix.MultiplyPoint(cannon.position);
		Vector3 vector2 = worldToLocalMatrix.MultiplyVector(cannon.up);
		CastResult castResult = SlideCast(vector, vector2 * distance);
		Gizmos.matrix = layout.transform.localToWorldMatrix;
		Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
		for (int i = layout.XCoordBorder.x; i <= layout.XCoordBorder.y; i++)
		{
			for (int j = floorYCoord; j <= ceilingYCoord; j++)
			{
				new Vector2Int(i, j);
				layout.GizmosDrawCoord(new Vector2Int(i, j), 0.25f);
			}
		}
		Gizmos.color = (castResult.Collide ? Color.red : Color.green);
		Gizmos.DrawWireSphere(vector, bubbleRadius);
		Gizmos.DrawWireSphere(castResult.endPosition, bubbleRadius);
		Gizmos.DrawLine(vector, castResult.endPosition);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(layout.CoordToLocalPosition(castResult.endCoord), bubbleRadius * 0.8f);
		if (castResult.collide)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(layout.CoordToLocalPosition(castResult.touchBubbleCoord), bubbleRadius * 0.5f);
		}
	}

	internal void Release(Bubble bubble)
	{
		BubblePool.Release(bubble);
	}

	internal Color GetDisplayColor(int colorIndex)
	{
		if (colorIndex < 0)
		{
			return Color.clear;
		}
		if (colorIndex >= colorPallette.Length)
		{
			return Color.white;
		}
		return colorPallette[colorIndex];
	}

	private async UniTask Shockwave(Vector2Int origin, float amplitude)
	{
		HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
		List<Vector2Int> next = new List<Vector2Int>();
		visited.Add(origin);
		next.Add(origin);
		List<Vector2Int> buffer = new List<Vector2Int>();
		while (next.Count > 0)
		{
			buffer.Clear();
			foreach (Vector2Int item2 in next)
			{
				attachedBubbles.TryGetValue(item2, out var value);
				if (value != null)
				{
					value.Impact(((Vector2)(item2 - origin)).normalized * amplitude);
				}
				Vector2Int[] allNeighbourCoords = layout.GetAllNeighbourCoords(item2, includeCenter: false);
				for (int i = 0; i < allNeighbourCoords.Length; i++)
				{
					Vector2Int item = allNeighbourCoords[i];
					if (!visited.Contains(item) && item.x >= layout.XCoordBorder.x && item.x <= layout.XCoordBorder.y && item.y <= ceilingYCoord && item.y >= floorYCoord)
					{
						buffer.Add(item);
					}
				}
			}
			next.Clear();
			visited.AddRange(buffer);
			next.AddRange(buffer);
			await UniTask.WaitForSeconds(0.025f);
			amplitude *= 0.5f;
			if (base.gameObject == null)
			{
				break;
			}
		}
	}

	private void PunchCamera()
	{
		cameraParent.DOKill(complete: true);
		cameraParent.DOShakePosition(0.4f);
		cameraParent.DOShakeRotation(0.4f, Vector3.forward);
	}
}
