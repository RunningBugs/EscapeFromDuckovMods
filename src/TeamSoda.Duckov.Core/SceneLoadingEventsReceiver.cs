using UnityEngine;
using UnityEngine.Events;

public class SceneLoadingEventsReceiver : MonoBehaviour
{
	public UnityEvent onStartLoadingScene;

	public UnityEvent onFinishedLoadingScene;

	private void OnEnable()
	{
		SceneLoader.onStartedLoadingScene += OnStartedLoadingScene;
		SceneLoader.onFinishedLoadingScene += OnFinishedLoadingScene;
	}

	private void OnDisable()
	{
		SceneLoader.onStartedLoadingScene -= OnStartedLoadingScene;
		SceneLoader.onFinishedLoadingScene -= OnFinishedLoadingScene;
	}

	private void OnStartedLoadingScene(SceneLoadingContext context)
	{
		onStartLoadingScene?.Invoke();
	}

	private void OnFinishedLoadingScene(SceneLoadingContext context)
	{
		onFinishedLoadingScene?.Invoke();
	}
}
