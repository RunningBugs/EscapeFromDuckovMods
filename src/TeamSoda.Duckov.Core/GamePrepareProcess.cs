using Cysharp.Threading.Tasks;
using Duckov.Rules.UI;
using Duckov.Scenes;
using Eflatun.SceneReference;
using UnityEngine;

public class GamePrepareProcess : MonoBehaviour
{
	[SerializeField]
	private DifficultySelection difficultySelection;

	[SerializeField]
	[SceneID]
	private string introScene;

	[SerializeField]
	[SceneID]
	private string guideScene;

	public bool goToBaseSceneIfVisted;

	[SerializeField]
	[SceneID]
	private string baseScene;

	public SceneReference overrideCurtainScene;

	private async UniTask Execute()
	{
		difficultySelection.SkipHide();
		await difficultySelection.Execute();
		if (goToBaseSceneIfVisted && !string.IsNullOrEmpty(baseScene) && MultiSceneCore.GetVisited(baseScene))
		{
			SceneLoader.Instance.LoadScene(baseScene, overrideCurtainScene).Forget();
		}
		else if (goToBaseSceneIfVisted && !string.IsNullOrEmpty(guideScene) && MultiSceneCore.GetVisited(guideScene))
		{
			SceneLoader.Instance.LoadScene(guideScene, overrideCurtainScene).Forget();
		}
		else
		{
			SceneLoader.Instance.LoadScene(introScene, overrideCurtainScene).Forget();
		}
	}

	private void Start()
	{
		Execute().Forget();
	}
}
