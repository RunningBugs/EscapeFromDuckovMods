using UnityEngine;

namespace Duckov.UI.Animations;

public abstract class LooperElement : MonoBehaviour
{
	[SerializeField]
	private LooperClock clock;

	protected virtual void OnEnable()
	{
		clock.onTick += OnTick;
		OnTick(clock, clock.t);
	}

	protected virtual void OnDisable()
	{
		if (clock != null)
		{
			clock.onTick -= OnTick;
		}
	}

	protected abstract void OnTick(LooperClock clock, float t);
}
