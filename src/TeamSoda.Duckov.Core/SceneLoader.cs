using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Duckov;
using Duckov.Economy;
using Duckov.Scenes;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public SceneReference defaultCurtainScene;

	[SerializeField]
	private OnPointerClick pointerClickEventRecevier;

	[SerializeField]
	private float minimumLoadingTime = 1f;

	[SerializeField]
	private float waitAfterSceneLoaded = 1f;

	[SerializeField]
	private FadeGroup content;

	[SerializeField]
	private FadeGroup loadingIndicator;

	[SerializeField]
	private FadeGroup clickIndicator;

	[SerializeField]
	private AnimationCurve fadeCurve1;

	[SerializeField]
	private AnimationCurve fadeCurve2;

	[SerializeField]
	private AnimationCurve fadeCurve3;

	[SerializeField]
	private AnimationCurve fadeCurve4;

	private string _loadingComment;

	[SerializeField]
	private SceneReference target;

	private bool clicked;

	public static SceneLoader Instance => GameManager.SceneLoader;

	public static bool IsSceneLoading { get; private set; }

	public static string LoadingComment
	{
		get
		{
			if (LevelManager.LevelInitializing)
			{
				return LevelManager.LevelInitializingComment;
			}
			if (Instance != null)
			{
				return Instance._loadingComment;
			}
			return null;
		}
		set
		{
			if (!(Instance == null))
			{
				Instance._loadingComment = value;
				SceneLoader.OnSetLoadingComment?.Invoke(value);
			}
		}
	}

	public static bool HideTips { get; private set; }

	public static event Action<SceneLoadingContext> onStartedLoadingScene;

	public static event Action<SceneLoadingContext> onFinishedLoadingScene;

	public static event Action<SceneLoadingContext> onBeforeSetSceneActive;

	public static event Action<SceneLoadingContext> onAfterSceneInitialize;

	public static event Action<string> OnSetLoadingComment;

	private void Awake()
	{
		if (Instance != this)
		{
			Debug.LogError(base.gameObject.scene.name + " 场景中出现了应当删除的Scene Loader");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			pointerClickEventRecevier.onPointerClick.AddListener(NotifyPointerClick);
			pointerClickEventRecevier.gameObject.SetActive(value: false);
			content.Hide();
		}
	}

	public async UniTask LoadScene(string sceneID, MultiSceneLocation location, SceneReference overrideCurtainScene = null, bool clickToConinue = false, bool notifyEvacuation = false, bool doCircleFade = true, bool saveToFile = true, bool hideTips = false)
	{
		await LoadScene(sceneID, overrideCurtainScene, clickToConinue, notifyEvacuation, doCircleFade, useLocation: true, location, saveToFile, hideTips);
	}

	public async UniTask LoadScene(string sceneID, SceneReference overrideCurtainScene = null, bool clickToConinue = false, bool notifyEvacuation = false, bool doCircleFade = true, bool useLocation = false, MultiSceneLocation location = default(MultiSceneLocation), bool saveToFile = true, bool hideTips = false)
	{
		SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(sceneID);
		if (sceneInfo != null && sceneInfo.SceneReference.UnsafeReason == SceneReferenceUnsafeReason.None)
		{
			await LoadScene(sceneInfo.SceneReference, overrideCurtainScene, clickToConinue, notifyEvacuation, doCircleFade, useLocation, location, saveToFile, hideTips);
		}
	}

	public async UniTask LoadScene(SceneReference sceneReference, SceneReference overrideCurtainScene = null, bool clickToConinue = false, bool notifyEvacuation = false, bool doCircleFade = true, bool useLocation = false, MultiSceneLocation location = default(MultiSceneLocation), bool saveToFile = true, bool hideTips = false)
	{
		SceneLoadingContext context = new SceneLoadingContext
		{
			sceneName = sceneReference.Name,
			useLocation = useLocation,
			location = location
		};
		if (IsSceneLoading)
		{
			Debug.LogError("已经在加载场景了");
			return;
		}
		HideTips = hideTips;
		LoadingComment = "Handling pre-loading work...";
		AudioManager.StopBGM();
		LoadingComment = "Wrapping up level...";
		if ((bool)LevelManager.Instance)
		{
			LoadingComment = "Handling evacuation...";
			if (notifyEvacuation)
			{
				LevelManager.Instance.NotifyEvacuated(new EvacuationInfo(MultiSceneCore.ActiveSubSceneID, default(Vector3)));
			}
			LoadingComment = "Handling target scene...";
			if (SceneInfoCollection.GetSceneID(sceneReference) != null)
			{
				LoadingComment = "Notifying saves system...";
				LevelManager.Instance.NotifySaveBeforeLoadScene(saveToFile);
			}
		}
		LoadingComment = "Begin loading...";
		IsSceneLoading = true;
		LoadingComment = "Notifying start loading events...";
		SceneLoader.onStartedLoadingScene?.Invoke(context);
		LoadingComment = "Referencing curtain scene...";
		SceneReference curtainScene = ((overrideCurtainScene != null && overrideCurtainScene.UnsafeReason != SceneReferenceUnsafeReason.Empty) ? overrideCurtainScene : defaultCurtainScene);
		LoadingComment = "Showing black screen...";
		await BlackScreen.ShowAndReturnTask(fadeCurve1, doCircleFade ? 1 : 0);
		LoadingComment = "Wait for object returning...";
		if (Cost.TaskPending)
		{
			Debug.LogError("SceneLoader: 检测到正在返还物品");
		}
		LoadingComment = "Showing loading indicator...";
		loadingIndicator.Show();
		LoadingComment = "Stopping tweens...";
		DOTween.KillAll();
		LoadingComment = "Loading curtain...";
		await SceneManager.LoadSceneAsync(curtainScene.Name, LoadSceneMode.Single);
		LoadingComment = "Showing curtain...";
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(curtainScene.Name));
		LoadingComment = "Showing HUD...";
		content.Show();
		GameManager.EventSystem.gameObject.SetActive(value: false);
		await UniTask.WaitForEndOfFrame(this);
		GameManager.EventSystem.gameObject.SetActive(value: true);
		float timeWhenLoadingStarted = Time.unscaledTime;
		LoadingComment = "Waiting...";
		await UniTask.WaitForSeconds(0.1f, ignoreTimeScale: true);
		LoadingComment = "Handling audio...";
		AudioManager.SetState("GameStatus", "LoadingLevel");
		LoadingComment = "Hiding black screen...";
		await BlackScreen.HideAndReturnTask(fadeCurve2);
		LoadingComment = "Loading target scene...";
		AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneReference.Name, LoadSceneMode.Additive);
		loadSceneOperation.allowSceneActivation = false;
		LoadingComment = "Waiting for scene loading operation...";
		while (loadSceneOperation.progress < 0.9f)
		{
			await UniTask.NextFrame();
		}
		LoadingComment = "Wait for an eternity...";
		while (TimeSinceLoadingStarted() < minimumLoadingTime)
		{
			await UniTask.NextFrame();
		}
		LoadingComment = "Hiding UI...";
		content.Hide();
		LoadingComment = "Handling click to continue...";
		if (clickToConinue)
		{
			LoadingComment = "Hiding loading indicator...";
			await loadingIndicator.HideAndReturnTask();
			LoadingComment = "Showing click indicator...";
			await clickIndicator.ShowAndReturnTask();
			LoadingComment = "Setting up click handler...";
			pointerClickEventRecevier.gameObject.SetActive(value: true);
			clicked = false;
			LoadingComment = "Wait for click...";
			while (!clicked)
			{
				await UniTask.NextFrame();
			}
			LoadingComment = "Handle click...";
			pointerClickEventRecevier.gameObject.SetActive(value: false);
			clickIndicator.Hide();
		}
		else
		{
			LoadingComment = "Hiding UI...";
			loadingIndicator.Hide();
		}
		AudioManager.StopBGM();
		LoadingComment = "Summoning black...";
		await BlackScreen.ShowAndReturnTask(fadeCurve3);
		LoadingComment = "Invoking before scene active...";
		SceneLoader.onBeforeSetSceneActive?.Invoke(context);
		LoadingComment = "Allowing activation...";
		loadSceneOperation.allowSceneActivation = true;
		LoadingComment = "Waiting for scene loading finish-up...";
		await loadSceneOperation.ToUniTask();
		LoadingComment = "Setting active scene...";
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneReference.Name));
		LoadingComment = "Pulling away curtain...";
		await SceneManager.UnloadSceneAsync(curtainScene.Name);
		IsSceneLoading = false;
		LoadingComment = "Calling stuff service...";
		SceneLoader.onFinishedLoadingScene?.Invoke(context);
		AudioManager.SetState("GameStatus", "Playing");
		LoadingComment = "Waiting for level initialization...";
		while (LevelManager.Instance != null && !LevelManager.LevelInited)
		{
			await UniTask.NextFrame();
		}
		SceneLoader.onAfterSceneInitialize?.Invoke(context);
		LoadingComment = "Getting ready...";
		await UniTask.WaitForSeconds(waitAfterSceneLoaded, ignoreTimeScale: true);
		pointerClickEventRecevier.gameObject.SetActive(value: false);
		LoadingComment = "Expelling the darkness...";
		await BlackScreen.HideAndReturnTask(fadeCurve4, doCircleFade ? 1 : 0);
		float TimeSinceLoadingStarted()
		{
			return Time.unscaledTime - timeWhenLoadingStarted;
		}
	}

	public void LoadTarget()
	{
		LoadScene(target).Forget();
	}

	public async UniTask LoadBaseScene(SceneReference overrideCurtainScene = null, bool doCircleFade = true)
	{
		SceneReference sceneReference = GameplayDataSettings.SceneManagement?.BaseScene;
		if (sceneReference == null)
		{
			Debug.LogError("未配置基地场景(GameplayDataSettings/SceneManagement/BaseScene)");
		}
		await LoadScene(sceneReference, overrideCurtainScene, clickToConinue: true, notifyEvacuation: false, doCircleFade);
	}

	public void NotifyPointerClick(PointerEventData eventData)
	{
		clicked = true;
		AudioManager.Post("UI/sceneloader_click");
	}

	internal static void StaticLoadSingle(SceneReference sceneReference)
	{
		SceneManager.LoadScene(sceneReference.Name, LoadSceneMode.Single);
	}

	internal static void StaticLoadSingle(string sceneID)
	{
		SceneManager.LoadScene(SceneInfoCollection.GetBuildIndex(sceneID), LoadSceneMode.Single);
	}

	public static void LoadMainMenu(bool circleFade = true)
	{
		if ((bool)Instance)
		{
			Instance.LoadScene(GameplayDataSettings.SceneManagement.MainMenuScene, null, clickToConinue: false, notifyEvacuation: false, circleFade).Forget();
		}
	}
}
