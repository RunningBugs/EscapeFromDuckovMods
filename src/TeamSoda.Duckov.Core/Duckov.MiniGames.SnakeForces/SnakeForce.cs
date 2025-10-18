using System;
using System.Collections.Generic;
using DG.Tweening;
using Duckov.Utilities;
using Saves;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.SnakeForces;

public class SnakeForce : MiniGameBehaviour
{
	public class Part
	{
		public Vector2Int coord;

		public Vector2Int direction;

		public readonly SnakeForce Master;

		public bool IsHead => this == Master.Head;

		public bool IsTail => this == Master.Tail;

		public event Action<Part> OnMove;

		public Part(SnakeForce master, Vector2Int coord, Vector2Int direction)
		{
			Master = master;
			this.coord = coord;
			this.direction = direction;
		}

		internal void MoveTo(Vector2Int coord)
		{
			this.coord = coord;
			this.OnMove?.Invoke(this);
		}
	}

	[SerializeField]
	private GameObject gameOverScreen;

	[SerializeField]
	private GameObject titleScreen;

	[SerializeField]
	private GameObject winIndicator;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private TextMeshProUGUI highScoreText;

	[SerializeField]
	private GameObject highScoreIndicator;

	[SerializeField]
	private TextMeshProUGUI scoreTextGameOver;

	[SerializeField]
	private Transform cameraParent;

	[SerializeField]
	private float tickIntervalFrom = 0.5f;

	[SerializeField]
	private float tickIntervalTo = 0.01f;

	[SerializeField]
	private int maxSpeedTick = 4096;

	[SerializeField]
	private AnimationCurve speedCurve;

	[SerializeField]
	private int borderXMin = -10;

	[SerializeField]
	private int borderXMax = 10;

	[SerializeField]
	private int borderYMin = -10;

	[SerializeField]
	private int borderYMax = 10;

	private bool playing;

	private bool dead;

	private bool won;

	private List<Part> snake = new List<Part>();

	private List<Vector2Int> foods = new List<Vector2Int>();

	private int _score;

	public const string HighScoreKey = "MiniGame/Snake/HighScore";

	private float tickETA;

	private List<Vector2Int> allCoords = new List<Vector2Int>();

	private ulong playTick;

	private Vector2Int lastFrameAxis;

	private double freezeCountDown;

	private bool axisInput;

	public List<Part> Snake => snake;

	public List<Vector2Int> Foods => foods;

	public int Score
	{
		get
		{
			return _score;
		}
		private set
		{
			_score = value;
			this.OnScoreChanged?.Invoke(this);
		}
	}

	public static int HighScore
	{
		get
		{
			return SavesSystem.Load<int>("MiniGame/Snake/HighScore");
		}
		private set
		{
			SavesSystem.Save("MiniGame/Snake/HighScore", value);
		}
	}

	public Part Head
	{
		get
		{
			if (snake.Count <= 0)
			{
				return null;
			}
			return snake[0];
		}
	}

	public Part Tail
	{
		get
		{
			if (snake.Count <= 0)
			{
				return null;
			}
			List<Part> list = snake;
			return list[list.Count - 1];
		}
	}

	public event Action<Part> OnAddPart;

	public event Action<Part> OnRemovePart;

	public event Action<SnakeForce> OnAfterTick;

	public event Action<SnakeForce> OnScoreChanged;

	public event Action<SnakeForce> OnGameStart;

	public event Action<SnakeForce> OnGameOver;

	public event Action<SnakeForce, Vector2Int> OnFoodEaten;

	protected override void Start()
	{
		base.Start();
		titleScreen.SetActive(value: true);
	}

	private void Restart()
	{
		Clear();
		gameOverScreen.SetActive(value: false);
		for (int i = borderXMin; i <= borderXMax; i++)
		{
			for (int j = borderYMin; j <= borderYMax; j++)
			{
				allCoords.Add(new Vector2Int(i, j));
			}
		}
		AddPart(new Vector2Int((borderXMax + borderXMin) / 2, (borderYMax + borderYMin) / 2), Vector2Int.up);
		Grow();
		Grow();
		AddFood();
		PunchCamera();
		playing = true;
		RefreshScoreText();
		highScoreText.text = $"{HighScore}";
		this.OnGameStart?.Invoke(this);
	}

	private void AddFood(int count = 3)
	{
		List<Vector2Int> list = new List<Vector2Int>(allCoords);
		foreach (Part item2 in snake)
		{
			list.Remove(item2.coord);
		}
		if (list.Count <= 0)
		{
			Win();
			return;
		}
		Vector2Int[] randomSubSet = list.GetRandomSubSet(count);
		foreach (Vector2Int item in randomSubSet)
		{
			foods.Add(item);
		}
	}

	private void GameOver()
	{
		this.OnGameOver?.Invoke(this);
		bool active = Score > HighScore;
		if (Score > HighScore)
		{
			HighScore = Score;
		}
		highScoreIndicator.SetActive(active);
		winIndicator.SetActive(won);
		scoreTextGameOver.text = $"{Score}";
		gameOverScreen.SetActive(value: true);
		PunchCamera();
	}

	private void Win()
	{
		won = true;
		GameOver();
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector2 axis = base.Game.GetAxis();
		if (axis.sqrMagnitude > 0.1f)
		{
			Vector2Int vector2Int = default(Vector2Int);
			if (axis.x > 0f)
			{
				vector2Int = Vector2Int.right;
			}
			else if (axis.x < 0f)
			{
				vector2Int = Vector2Int.left;
			}
			else if (axis.y > 0f)
			{
				vector2Int = Vector2Int.up;
			}
			else if (axis.y < 0f)
			{
				vector2Int = Vector2Int.down;
			}
			if (lastFrameAxis != vector2Int)
			{
				axisInput = true;
			}
			lastFrameAxis = vector2Int;
		}
		else
		{
			lastFrameAxis = Vector2Int.zero;
		}
		if (freezeCountDown > 0.0)
		{
			freezeCountDown -= Time.unscaledDeltaTime;
			return;
		}
		if (dead || won || !playing)
		{
			if (base.Game.GetButtonDown(MiniGame.Button.Start))
			{
				Restart();
			}
			return;
		}
		RefreshScoreText();
		bool flag = base.Game.GetButton(MiniGame.Button.B) || base.Game.GetButton(MiniGame.Button.A);
		tickETA -= deltaTime * (flag ? 10f : 1f);
		float time = ((playTick < (ulong)maxSpeedTick) ? ((float)playTick / (float)maxSpeedTick) : 1f);
		float num = Mathf.Lerp(tickIntervalFrom, tickIntervalTo, speedCurve.Evaluate(time));
		if (tickETA <= 0f || axisInput)
		{
			Tick();
			tickETA = num;
			axisInput = false;
		}
	}

	private void RefreshScoreText()
	{
		scoreText.text = $"{Score}";
	}

	private void Tick()
	{
		playTick++;
		if (Head != null)
		{
			HandleMovement();
			DetectDeath();
			HandleEatAndGrow();
			this.OnAfterTick?.Invoke(this);
		}
	}

	private void HandleMovement()
	{
		Vector2Int vector2Int = lastFrameAxis;
		if ((!(vector2Int == -Head.direction) || snake.Count <= 1) && vector2Int != Vector2Int.zero)
		{
			Head.direction = vector2Int;
		}
		for (int num = snake.Count - 1; num >= 0; num--)
		{
			Part part = snake[num];
			Vector2Int coord = ((num > 0) ? snake[num - 1].coord : (part.coord + part.direction));
			if (num > 0)
			{
				part.direction = snake[num - 1].direction;
			}
			if (coord.x > borderXMax)
			{
				coord.x = borderXMin;
			}
			if (coord.y > borderYMax)
			{
				coord.y = borderYMin;
			}
			if (coord.x < borderXMin)
			{
				coord.x = borderXMax;
			}
			if (coord.y < borderYMin)
			{
				coord.y = borderYMax;
			}
			part.MoveTo(coord);
		}
	}

	private void HandleEatAndGrow()
	{
		Vector2Int coord = Head.coord;
		if (foods.Remove(coord))
		{
			Grow();
			Score++;
			int num = 3 + Mathf.FloorToInt(Mathf.Log(Score, 2f));
			int count = Mathf.Max(0, num - foods.Count);
			AddFood(count);
			this.OnFoodEaten?.Invoke(this, coord);
			PunchCamera();
		}
	}

	private void DetectDeath()
	{
		Vector2Int coord = Head.coord;
		int num = 1;
		while (true)
		{
			if (num < snake.Count)
			{
				if (snake[num].coord == coord)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		dead = true;
		GameOver();
	}

	private Part Grow()
	{
		if (snake.Count == 0)
		{
			Debug.LogError("Cannot grow the snake! It haven't been created yet.");
			return null;
		}
		Part tail = Tail;
		Vector2Int coord = tail.coord - tail.direction;
		return AddPart(coord, tail.direction);
	}

	private Part AddPart(Vector2Int coord, Vector2Int direction)
	{
		Part part = new Part(this, coord, direction);
		snake.Add(part);
		this.OnAddPart?.Invoke(part);
		return part;
	}

	private bool RemovePart(Part part)
	{
		if (!snake.Remove(part))
		{
			return false;
		}
		this.OnRemovePart?.Invoke(part);
		return true;
	}

	private void Clear()
	{
		titleScreen.SetActive(value: false);
		won = false;
		dead = false;
		Score = 0;
		playTick = 0uL;
		allCoords.Clear();
		foods.Clear();
		for (int num = snake.Count - 1; num >= 0; num--)
		{
			Part part = snake[num];
			if (part == null)
			{
				snake.RemoveAt(num);
			}
			else
			{
				RemovePart(part);
			}
		}
	}

	private void PunchCamera()
	{
		freezeCountDown = 0.10000000149011612;
		cameraParent.DOKill(complete: true);
		cameraParent.DOShakePosition(0.4f);
		cameraParent.DOShakeRotation(0.4f, Vector3.forward);
	}
}
