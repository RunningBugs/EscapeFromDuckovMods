using System;
using UnityEngine;

namespace Duckov.UI.Animations;

public class LooperClock : MonoBehaviour
{
	[SerializeField]
	private float duration = 1f;

	private float time;

	public float t
	{
		get
		{
			if (duration > 0f)
			{
				return time / duration;
			}
			return 1f;
		}
	}

	public event Action<LooperClock, float> onTick;

	private void Update()
	{
		if (duration > 0f)
		{
			time += Time.unscaledDeltaTime;
			time %= duration;
			Tick();
		}
	}

	private void Tick()
	{
		this.onTick?.Invoke(this, t);
	}
}
