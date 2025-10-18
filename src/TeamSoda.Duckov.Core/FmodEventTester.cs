using Duckov;
using UnityEngine;

public class FmodEventTester : MonoBehaviour
{
	[SerializeField]
	private string e;

	public void PlayEvent()
	{
		AudioManager.Post(e, base.gameObject);
	}
}
