using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class PlayableDirectorEvents : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector playableDirector;

	[SerializeField]
	private UnityEvent onPlayed;

	[SerializeField]
	private UnityEvent onPaused;

	[SerializeField]
	private UnityEvent onStopped;

	private void OnEnable()
	{
		playableDirector.played += OnPlayed;
		playableDirector.paused += OnPaused;
		playableDirector.stopped += OnStopped;
	}

	private void OnDisable()
	{
		playableDirector.played -= OnPlayed;
		playableDirector.paused -= OnPaused;
		playableDirector.stopped -= OnStopped;
	}

	private void OnStopped(PlayableDirector director)
	{
		onStopped?.Invoke();
	}

	private void OnPaused(PlayableDirector director)
	{
		onPaused?.Invoke();
	}

	private void OnPlayed(PlayableDirector director)
	{
		onPlayed?.Invoke();
	}
}
