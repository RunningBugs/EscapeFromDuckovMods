using System.Collections.Generic;
using Duckov.Quests;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.BasketBalls;

public class Basket : MonoBehaviour
{
	[SerializeField]
	private Animator netAnimator;

	[SerializeField]
	private List<Condition> conditions = new List<Condition>();

	[SerializeField]
	private BasketTrigger trigger;

	public UnityEvent<BasketBall> onGoal;

	private void Awake()
	{
		trigger.onGoal.AddListener(OnGoal);
	}

	private void OnGoal(BasketBall ball)
	{
		if (conditions.Satisfied())
		{
			onGoal.Invoke(ball);
			netAnimator.SetTrigger("Goal");
		}
	}
}
