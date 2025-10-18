using System.Collections.Generic;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class Bomb : MiniGameBehaviour
{
	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float maxLifeTime = 10f;

	[SerializeField]
	private ParticleSystem explodeFX;

	private float lifeTime;

	private List<GoldMinerEntity> hoveringTargets = new List<GoldMinerEntity>();

	protected override void OnUpdate(float deltaTime)
	{
		base.transform.position += base.transform.up * moveSpeed * deltaTime;
		hoveringTargets.RemoveAll((GoldMinerEntity e) => e == null);
		if (hoveringTargets.Count > 0)
		{
			Explode(hoveringTargets[0]);
		}
		lifeTime += deltaTime;
		if (lifeTime > maxLifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Explode(GoldMinerEntity goldMinerTarget)
	{
		goldMinerTarget.Explode(base.transform.position);
		FXPool.Play(explodeFX, base.transform.position, base.transform.rotation);
		Object.Destroy(base.gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		GoldMinerEntity component = collision.gameObject.GetComponent<GoldMinerEntity>();
		if (component != null)
		{
			hoveringTargets.Add(component);
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		GoldMinerEntity component = collision.gameObject.GetComponent<GoldMinerEntity>();
		if (component != null)
		{
			hoveringTargets.Remove(component);
		}
	}
}
