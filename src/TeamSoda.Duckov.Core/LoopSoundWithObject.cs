using Duckov;
using FMOD.Studio;
using UnityEngine;

public class LoopSoundWithObject : MonoBehaviour
{
	public string sfx;

	private EventInstance? eventInstance;

	private bool stoped = true;

	private void Start()
	{
		eventInstance = AudioManager.Post(sfx, base.gameObject);
		stoped = false;
	}

	public void Stop()
	{
		if (!stoped)
		{
			stoped = true;
			if (eventInstance.HasValue)
			{
				eventInstance.Value.stop(STOP_MODE.ALLOWFADEOUT);
			}
		}
	}

	private void OnDestroy()
	{
		Stop();
	}

	private void OnDisable()
	{
		Stop();
	}
}
