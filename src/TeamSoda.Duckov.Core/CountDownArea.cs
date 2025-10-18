using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using UnityEngine;
using UnityEngine.Events;

public class CountDownArea : MonoBehaviour
{
	[SerializeField]
	private float requiredExtrationTime = 5f;

	[SerializeField]
	private bool disableWhenSucceed = true;

	public UnityEvent onCountDownSucceed;

	public UnityEvent onTickSecond;

	public UnityEvent<CountDownArea> onCountDownStarted;

	public UnityEvent<CountDownArea> onCountDownStopped;

	private bool countingDown;

	private float timeWhenCountDownBegan = float.MaxValue;

	private HashSet<CharacterMainControl> hoveringMainCharacters = new HashSet<CharacterMainControl>();

	public float RequiredExtrationTime => requiredExtrationTime;

	private float TimeSinceCountDownBegan => Time.time - timeWhenCountDownBegan;

	public float RemainingTime => Mathf.Clamp(RequiredExtrationTime - TimeSinceCountDownBegan, 0f, RequiredExtrationTime);

	public float Progress
	{
		get
		{
			if (requiredExtrationTime <= 0f)
			{
				return 1f;
			}
			return TimeSinceCountDownBegan / RequiredExtrationTime;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (base.enabled)
		{
			CharacterMainControl component = other.GetComponent<CharacterMainControl>();
			if (!(component == null) && component.IsMainCharacter())
			{
				hoveringMainCharacters.Add(component);
				OnHoveringMainCharactersChanged();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (base.enabled)
		{
			CharacterMainControl component = other.GetComponent<CharacterMainControl>();
			if (!(component == null) && component.IsMainCharacter())
			{
				hoveringMainCharacters.Remove(component);
				OnHoveringMainCharactersChanged();
			}
		}
	}

	private void OnHoveringMainCharactersChanged()
	{
		if (!countingDown && hoveringMainCharacters.Count > 0)
		{
			BeginCountDown();
		}
		else if (countingDown && hoveringMainCharacters.Count < 1)
		{
			AbortCountDown();
		}
	}

	private void BeginCountDown()
	{
		countingDown = true;
		timeWhenCountDownBegan = Time.time;
		onCountDownStarted?.Invoke(this);
	}

	private void AbortCountDown()
	{
		countingDown = false;
		timeWhenCountDownBegan = float.MaxValue;
		onCountDownStopped?.Invoke(this);
	}

	private void UpdateCountDown()
	{
		if (hoveringMainCharacters.All((CharacterMainControl e) => e.Health.IsDead))
		{
			AbortCountDown();
		}
		if (TimeSinceCountDownBegan >= RequiredExtrationTime)
		{
			OnCountdownSucceed();
		}
		int num = (int)(RemainingTime + Time.deltaTime);
		if ((int)RemainingTime != num)
		{
			onTickSecond?.Invoke();
		}
	}

	private void OnCountdownSucceed()
	{
		onCountDownStopped?.Invoke(this);
		onCountDownSucceed?.Invoke();
		countingDown = false;
		if (disableWhenSucceed)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (base.enabled && countingDown && View.ActiveView == null)
		{
			UpdateCountDown();
		}
	}
}
