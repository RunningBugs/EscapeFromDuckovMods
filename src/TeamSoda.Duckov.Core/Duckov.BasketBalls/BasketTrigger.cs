using UnityEngine;
using UnityEngine.Events;

namespace Duckov.BasketBalls;

public class BasketTrigger : MonoBehaviour
{
	public UnityEvent<BasketBall> onGoal;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("ONTRIGGERENTER:" + other.name);
		BasketBall component = other.GetComponent<BasketBall>();
		if (!(component == null))
		{
			onGoal.Invoke(component);
		}
	}
}
