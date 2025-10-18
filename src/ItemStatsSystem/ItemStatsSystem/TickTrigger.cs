using Duckov.Utilities;
using Duckov.Utilities.Updatables;
using UnityEngine;

namespace ItemStatsSystem;

public class TickTrigger : EffectTrigger, IUpdatable
{
	[SerializeField]
	private float period = 1f;

	[SerializeField]
	private bool allowMultipleTrigger = true;

	private float buffer;

	private float _currentPeriod;

	private float _factor = 1f;

	public override string DisplayName => $"每{period}秒";

	private float Factor
	{
		get
		{
			if (period <= 0f)
			{
				return 0f;
			}
			if (_currentPeriod != period)
			{
				_factor = 1f / period;
				_currentPeriod = period;
			}
			return _factor;
		}
	}

	private void OnEnable()
	{
		UpdatableInvoker.Register(this);
	}

	private new void OnDisable()
	{
		UpdatableInvoker.Unregister(this);
	}

	private void UpdateBuffer()
	{
		buffer += Time.deltaTime * Factor;
		while (buffer > 1f)
		{
			buffer -= 1f;
			Trigger();
			if (!allowMultipleTrigger)
			{
				break;
			}
		}
	}

	public void OnUpdate()
	{
		UpdateBuffer();
	}
}
