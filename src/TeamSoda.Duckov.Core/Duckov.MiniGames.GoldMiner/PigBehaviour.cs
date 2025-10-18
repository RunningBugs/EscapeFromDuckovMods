using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class PigBehaviour : MiniGameBehaviour
{
	[SerializeField]
	private GoldMinerEntity entity;

	[SerializeField]
	private float moveSpeed = 50f;

	private bool attached;

	private bool movingRight;

	private void Awake()
	{
		if (entity == null)
		{
			entity = GetComponent<GoldMinerEntity>();
		}
		GoldMinerEntity goldMinerEntity = entity;
		goldMinerEntity.OnAttached = (Action<GoldMinerEntity, Hook>)Delegate.Combine(goldMinerEntity.OnAttached, new Action<GoldMinerEntity, Hook>(OnAttached));
	}

	protected override void OnUpdate(float deltaTime)
	{
		Quaternion localRotation = Quaternion.AngleAxis((!movingRight) ? 180 : 0, Vector3.up);
		base.transform.localRotation = localRotation;
		base.transform.localPosition += (movingRight ? Vector3.right : Vector3.left) * moveSpeed * entity.master.run.GameSpeedFactor * deltaTime;
		if (base.transform.localPosition.x > entity.master.Bounds.max.x)
		{
			movingRight = false;
		}
		else if (base.transform.localPosition.x < entity.master.Bounds.min.x)
		{
			movingRight = true;
		}
	}

	private void OnAttached(GoldMinerEntity entity, Hook hook)
	{
	}
}
