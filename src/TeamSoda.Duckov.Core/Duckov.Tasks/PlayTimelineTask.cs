using UnityEngine;
using UnityEngine.Playables;

namespace Duckov.Tasks;

public class PlayTimelineTask : MonoBehaviour, ITaskBehaviour
{
	[SerializeField]
	private PlayableDirector timeline;

	private bool running;

	private void Awake()
	{
		timeline.stopped += OnTimelineStopped;
	}

	private void OnDestroy()
	{
		if (timeline != null)
		{
			timeline.stopped -= OnTimelineStopped;
		}
	}

	private void OnTimelineStopped(PlayableDirector director)
	{
		running = false;
	}

	public void Begin()
	{
		running = true;
		timeline.Play();
	}

	public bool IsComplete()
	{
		if (timeline.time > timeline.duration - 0.009999999776482582)
		{
			return true;
		}
		return timeline.state != PlayState.Playing;
	}

	public bool IsPending()
	{
		if (timeline.time > timeline.duration - 0.009999999776482582)
		{
			return false;
		}
		return timeline.state == PlayState.Playing;
	}

	public void Skip()
	{
		timeline.Stop();
	}
}
