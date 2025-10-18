using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class ExplodeOnAttach : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner goldMiner;

	[SerializeField]
	private GoldMinerEntity master;

	[SerializeField]
	private float explodeRange;

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponent<GoldMinerEntity>();
		}
		if (goldMiner == null)
		{
			goldMiner = GetComponentInParent<GoldMiner>();
		}
		GoldMinerEntity goldMinerEntity = master;
		goldMinerEntity.OnAttached = (Action<GoldMinerEntity, Hook>)Delegate.Combine(goldMinerEntity.OnAttached, new Action<GoldMinerEntity, Hook>(OnAttached));
	}

	private void OnAttached(GoldMinerEntity target, Hook hook)
	{
		if (goldMiner == null || goldMiner.run == null || goldMiner.run.defuse.Value > 0.1f)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, explodeRange);
		for (int i = 0; i < array.Length; i++)
		{
			GoldMinerEntity component = array[i].GetComponent<GoldMinerEntity>();
			if (!(component == null))
			{
				component.Explode(base.transform.position);
			}
		}
		master.Explode(base.transform.position);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, explodeRange);
	}
}
