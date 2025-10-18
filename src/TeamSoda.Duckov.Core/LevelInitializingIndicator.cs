using Duckov.UI.Animations;
using TMPro;
using UnityEngine;

public class LevelInitializingIndicator : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI levelInitializationCommentText;

	private void Awake()
	{
		SceneLoader.onBeforeSetSceneActive += SceneLoader_onBeforeSetSceneActive;
		SceneLoader.onAfterSceneInitialize += SceneLoader_onAfterSceneInitialize;
		LevelManager.OnLevelInitializingCommentChanged += OnCommentChanged;
		SceneLoader.OnSetLoadingComment += OnSetLoadingComment;
		fadeGroup.SkipHide();
	}

	private void OnSetLoadingComment(string comment)
	{
		levelInitializationCommentText.text = SceneLoader.LoadingComment;
	}

	private void OnCommentChanged(string comment)
	{
		levelInitializationCommentText.text = SceneLoader.LoadingComment;
	}

	private void OnDestroy()
	{
		SceneLoader.onBeforeSetSceneActive -= SceneLoader_onBeforeSetSceneActive;
		SceneLoader.onAfterSceneInitialize -= SceneLoader_onAfterSceneInitialize;
		LevelManager.OnLevelInitializingCommentChanged -= OnCommentChanged;
		SceneLoader.OnSetLoadingComment -= OnSetLoadingComment;
	}

	private void SceneLoader_onBeforeSetSceneActive(SceneLoadingContext obj)
	{
		fadeGroup.Show();
		levelInitializationCommentText.text = LevelManager.LevelInitializingComment;
	}

	private void SceneLoader_onAfterSceneInitialize(SceneLoadingContext obj)
	{
		fadeGroup.Hide();
	}
}
