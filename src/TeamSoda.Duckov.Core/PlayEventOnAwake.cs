using Duckov;
using UnityEngine;

public class PlayEventOnAwake : MonoBehaviour
{
	[SerializeField]
	private string sfx;

	private void Awake()
	{
		AudioManager.Post(sfx, base.gameObject);
	}
}
