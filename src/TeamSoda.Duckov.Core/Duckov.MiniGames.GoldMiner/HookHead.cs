using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class HookHead : MonoBehaviour
{
	public Action<HookHead, Collision2D> onCollisionEnter;

	public Action<HookHead, Collision2D> onCollisionExit;

	public Action<HookHead, Collision2D> onCollisionStay;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		onCollisionEnter?.Invoke(this, collision);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		onCollisionExit?.Invoke(this, collision);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		onCollisionStay?.Invoke(this, collision);
	}
}
