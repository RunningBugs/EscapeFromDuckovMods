using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class Title : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private PlayableDirector timelineToTitle;

	[SerializeField]
	private PlayableDirector timelineToMainMenu;

	private string sfx_PressStart = "UI/game_start";

	private void Start()
	{
		StartTask().Forget();
	}

	private async UniTask StartTask()
	{
		timelineToTitle.Play();
		await WaitForTimeline(timelineToTitle);
		fadeGroup.Show();
		timelineToTitle.gameObject.SetActive(value: false);
	}

	private async UniTask ContinueTask()
	{
		fadeGroup.Hide();
		AudioManager.Post(sfx_PressStart);
		timelineToTitle.gameObject.SetActive(value: false);
		timelineToMainMenu.gameObject.SetActive(value: true);
		timelineToMainMenu.Play();
		AudioManager.PlayBGM("mus_title");
		await WaitForTimeline(timelineToMainMenu);
		if (timelineToMainMenu != null)
		{
			timelineToMainMenu.gameObject.SetActive(value: false);
		}
	}

	private async UniTask WaitForTimeline(PlayableDirector timeline)
	{
		while (timeline != null && timeline.state == PlayState.Playing)
		{
			await UniTask.NextFrame();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (fadeGroup.IsShown)
		{
			ContinueTask().Forget();
		}
	}
}
