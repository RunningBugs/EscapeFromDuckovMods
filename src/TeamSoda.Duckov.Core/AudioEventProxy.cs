using Duckov;
using UnityEngine;

public class AudioEventProxy : MonoBehaviour
{
	public bool playOnAwake;

	[SerializeField]
	private string eventName;

	private void Awake()
	{
		if (playOnAwake)
		{
			Post();
		}
	}

	public void Post()
	{
		if (base.gameObject.activeInHierarchy)
		{
			AudioManager.Post(eventName, base.gameObject);
		}
	}
}
