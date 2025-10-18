using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.Utilities;
using Eflatun.SceneReference;
using UnityEngine;

public class SceneLoaderProxy : MonoBehaviour
{
	[SceneID]
	[SerializeField]
	private string sceneID;

	[SerializeField]
	private bool useLocation;

	[SerializeField]
	private MultiSceneLocation location;

	[SerializeField]
	private bool showClosure = true;

	[SerializeField]
	private bool notifyEvacuation = true;

	[SerializeField]
	private SceneReference overrideCurtainScene;

	[SerializeField]
	private bool hideTips;

	[SerializeField]
	private bool circleFade = true;

	private bool saveToFile;

	public void LoadScene()
	{
		if (SceneLoader.Instance == null)
		{
			Debug.LogWarning("没找到SceneLoader实例，已取消加载场景");
		}
		else
		{
			Task().Forget();
		}
	}

	private async UniTask Task()
	{
		if ("Base" == sceneID)
		{
			saveToFile = true;
		}
		if (showClosure)
		{
			if (notifyEvacuation)
			{
				LevelManager.Instance?.NotifyEvacuated(new EvacuationInfo(MultiSceneCore.ActiveSubSceneID, base.transform.position));
			}
			await ClosureView.ShowAndReturnTask(1f);
		}
		if (notifyEvacuation)
		{
			overrideCurtainScene = GameplayDataSettings.SceneManagement.EvacuateScreenScene;
		}
		if (useLocation)
		{
			SceneLoader instance = SceneLoader.Instance;
			string text = sceneID;
			MultiSceneLocation multiSceneLocation = location;
			bool flag = notifyEvacuation;
			instance.LoadScene(overrideCurtainScene: overrideCurtainScene, clickToConinue: false, notifyEvacuation: flag, saveToFile: saveToFile, hideTips: hideTips, sceneID: text, location: multiSceneLocation, doCircleFade: circleFade).Forget();
		}
		else
		{
			SceneLoader instance2 = SceneLoader.Instance;
			string text2 = sceneID;
			bool flag2 = notifyEvacuation;
			instance2.LoadScene(overrideCurtainScene: overrideCurtainScene, clickToConinue: false, notifyEvacuation: flag2, saveToFile: saveToFile, hideTips: hideTips, sceneID: text2, doCircleFade: circleFade).Forget();
		}
	}

	public void LoadMainMenu()
	{
		SceneLoader.LoadMainMenu(circleFade);
	}
}
