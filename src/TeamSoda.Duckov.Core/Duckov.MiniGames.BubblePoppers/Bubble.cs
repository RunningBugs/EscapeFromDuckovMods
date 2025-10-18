using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.BubblePoppers;

public class Bubble : MiniGameBehaviour
{
	public enum Status
	{
		Idle,
		Moving,
		Attached,
		Detached,
		Explode
	}

	[SerializeField]
	private float radius;

	[SerializeField]
	private int colorIndex;

	[SerializeField]
	private float gravity;

	[SerializeField]
	private float explodeAfterDetachedFor = 1f;

	[SerializeField]
	private ParticleSystem explodeFXrefab;

	[SerializeField]
	private Image image;

	[SerializeField]
	private RectTransform graphicsRoot;

	[SerializeField]
	private float gSpring = 1f;

	[SerializeField]
	private float gDamping = 10f;

	[SerializeField]
	private float vibrationFrequency = 10f;

	[SerializeField]
	private float vibrationAmp = 4f;

	private float explodeETA;

	private float explodeCountDown;

	private Vector2 gVelocity;

	public BubblePopper Master { get; private set; }

	public float Radius => radius;

	public int ColorIndex => colorIndex;

	public Color DisplayColor
	{
		get
		{
			if (Master == null)
			{
				return Color.white;
			}
			return Master.GetDisplayColor(ColorIndex);
		}
	}

	public Vector2Int Coord { get; internal set; }

	public Vector2 MoveDirection { get; internal set; }

	public Vector2 Velocity { get; internal set; }

	public Status status { get; private set; }

	private Vector2 gPos
	{
		get
		{
			return graphicsRoot.localPosition;
		}
		set
		{
			graphicsRoot.localPosition = value;
		}
	}

	private Vector2 gForce => (new Vector2(Mathf.PerlinNoise(7.3f, Time.time * vibrationFrequency) * 2f - 1f, Mathf.PerlinNoise(0.3f, Time.time * vibrationFrequency) * 2f - 1f) * vibrationAmp - gPos) * gSpring;

	internal void Setup(BubblePopper master, int colorIndex)
	{
		Master = master;
		this.colorIndex = colorIndex;
		image.color = DisplayColor;
	}

	internal void Launch(Vector2 direction)
	{
		MoveDirection = direction;
		status = Status.Moving;
	}

	internal void NotifyExplode(Vector2Int origin)
	{
		status = Status.Explode;
		Vector2Int vector2Int = Coord - origin;
		float magnitude = vector2Int.magnitude;
		explodeETA = magnitude * 0.025f;
		Impact(((Vector2)vector2Int).normalized * 5f);
	}

	internal void NotifyAttached(Vector2Int coord)
	{
		Vector2 vector = Master.Layout.CoordToLocalPosition(coord);
		base.transform.position = Master.Layout.transform.localToWorldMatrix.MultiplyPoint(vector);
		status = Status.Attached;
		Coord = coord;
	}

	public void NotifyDetached()
	{
		status = Status.Detached;
		Velocity = Vector2.zero;
		explodeCountDown = explodeAfterDetachedFor;
	}

	protected override void OnUpdate(float deltaTime)
	{
		UpdateLogic(deltaTime);
		UpdateGraphics(deltaTime);
	}

	private void UpdateLogic(float deltaTime)
	{
		if (!(Master == null) && !Master.Busy && status == Status.Moving)
		{
			Master.MoveBubble(this, deltaTime);
		}
	}

	private void UpdateGraphics(float deltaTime)
	{
		if (status == Status.Explode)
		{
			explodeETA -= deltaTime;
			if (explodeETA <= 0f)
			{
				FXPool.Play(explodeFXrefab, base.transform.position, base.transform.rotation, DisplayColor);
				Master.Release(this);
			}
		}
		if (status == Status.Detached)
		{
			base.transform.localPosition += (Vector3)Velocity * deltaTime;
			Velocity += -Vector2.up * gravity;
			explodeCountDown -= deltaTime;
			if (explodeCountDown <= 0f)
			{
				NotifyExplode(Coord);
			}
		}
		UpdateElasticMovement(deltaTime);
	}

	private void UpdateElasticMovement(float deltaTime)
	{
		float num = ((Vector2.Dot(gVelocity, gForce.normalized) < 0f) ? gDamping : 1f);
		gVelocity += gForce * deltaTime;
		gVelocity = Vector2.MoveTowards(gVelocity, Vector2.zero, num * gVelocity.magnitude * deltaTime);
		gPos += gVelocity;
	}

	public void Impact(Vector2 velocity)
	{
		gVelocity = velocity;
	}

	internal void Rest()
	{
		gPos = Vector2.zero;
		gVelocity = Vector2.zero;
	}
}
