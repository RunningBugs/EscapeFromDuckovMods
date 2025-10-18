using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.SnakeForces;

public class SnakePartDisplay : MiniGameBehaviour
{
	[SerializeField]
	private Image image;

	private Vector2Int cachedCoord;

	public SnakeDisplay Master { get; private set; }

	public SnakeForce.Part Target { get; private set; }

	internal void Setup(SnakeDisplay master, SnakeForce.Part part)
	{
		if (Target != null)
		{
			Target.OnMove -= OnTargetMove;
		}
		Master = master;
		Target = part;
		cachedCoord = Target.coord;
		base.transform.localPosition = Master.GetPosition(cachedCoord);
		Target.OnMove += OnTargetMove;
	}

	private void OnTargetMove(SnakeForce.Part part)
	{
		if (base.enabled)
		{
			_ = (Target.coord - cachedCoord).sqrMagnitude;
			cachedCoord = Target.coord;
			Vector3 position = Master.GetPosition(cachedCoord);
			DoMove(position);
		}
	}

	private void DoMove(Vector3 vector3)
	{
		base.transform.localPosition = vector3;
	}

	internal void Punch()
	{
		base.transform.DOKill(complete: true);
		base.transform.localScale = Vector3.one;
		base.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f, 4);
	}

	internal void PunchColor(Color color)
	{
		image.DOKill();
		image.color = color;
		image.DOColor(Color.white, 0.4f);
	}
}
