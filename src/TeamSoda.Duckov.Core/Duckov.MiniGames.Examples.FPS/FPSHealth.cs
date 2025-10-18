using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duckov.MiniGames.Examples.FPS;

public class FPSHealth : MiniGameBehaviour
{
	[SerializeField]
	private int maxHp;

	[SerializeField]
	private List<FPSDamageReceiver> damageReceivers;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private float hurtValueDropRate = 1f;

	private int hp;

	private bool dead;

	private float hurtValue;

	private MaterialPropertyBlock materialPropertyBlock;

	public int HP => hp;

	public bool Dead => dead;

	public event Action<FPSHealth> onDead;

	protected override void Start()
	{
		base.Start();
		hp = maxHp;
		materialPropertyBlock = new MaterialPropertyBlock();
		foreach (FPSDamageReceiver damageReceiver in damageReceivers)
		{
			damageReceiver.onReceiveDamage += OnReceiverReceiveDamage;
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (hurtValue > 0f)
		{
			hurtValue = Mathf.MoveTowards(hurtValue, 0f, deltaTime * hurtValueDropRate);
		}
		materialPropertyBlock.SetFloat("_HurtValue", hurtValue);
		meshRenderer.SetPropertyBlock(materialPropertyBlock, 0);
	}

	private void OnReceiverReceiveDamage(FPSDamageReceiver receiver, FPSDamageInfo info)
	{
		ReceiveDamage(info);
	}

	private void ReceiveDamage(FPSDamageInfo info)
	{
		if (!dead)
		{
			hurtValue = 1f;
			hp -= Mathf.FloorToInt(info.amount);
			if (hp <= 0)
			{
				hp = 0;
				Die();
			}
		}
	}

	private void Die()
	{
		dead = true;
		this.onDead?.Invoke(this);
	}
}
