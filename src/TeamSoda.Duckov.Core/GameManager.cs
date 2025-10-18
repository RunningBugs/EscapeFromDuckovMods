using DG.Tweening;
using Duckov;
using Duckov.Achievements;
using Duckov.Modding;
using Duckov.NoteIndexs;
using Duckov.Rules;
using Duckov.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;

	[SerializeField]
	private AudioManager audioManager;

	[SerializeField]
	private UIInputManager uiInputManager;

	[SerializeField]
	private GameRulesManager difficultyManager;

	[SerializeField]
	private PauseMenu pauseMenu;

	[SerializeField]
	private SceneLoader sceneLoader;

	[SerializeField]
	private BlackScreen blackScreen;

	[SerializeField]
	private EventSystem eventSystem;

	[SerializeField]
	private PlayerInput mainPlayerInput;

	[SerializeField]
	private NightVisionVisual nightVision;

	[SerializeField]
	private ModManager modManager;

	[SerializeField]
	private NoteIndex noteIndex;

	[SerializeField]
	private AchievementManager achievementManager;

	public static bool newBoot;

	public static GameManager Instance
	{
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<GameManager>();
				if ((bool)_instance)
				{
					Object.DontDestroyOnLoad(_instance.gameObject);
				}
			}
			if (_instance == null)
			{
				GameObject obj = Resources.Load<GameObject>("GameManager");
				if (obj == null)
				{
					Debug.LogError("Resources中找不到GameManager的Prefab");
				}
				GameManager component = Object.Instantiate(obj).GetComponent<GameManager>();
				if (component == null)
				{
					Debug.LogError("GameManager的prefab上没有GameManager组件");
					return null;
				}
				_instance = component;
				if ((bool)_instance)
				{
					Object.DontDestroyOnLoad(_instance.gameObject);
				}
			}
			return _instance;
		}
	}

	public static bool Paused
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			if (Instance.pauseMenu.Shown)
			{
				return true;
			}
			return false;
		}
	}

	public static AudioManager AudioManager => Instance.audioManager;

	public static UIInputManager UiInputManager => Instance.uiInputManager;

	public static PauseMenu PauseMenu => Instance.pauseMenu;

	public static GameRulesManager DifficultyManager => Instance.difficultyManager;

	public static SceneLoader SceneLoader => Instance.sceneLoader;

	public static BlackScreen BlackScreen => Instance.blackScreen;

	public static EventSystem EventSystem => Instance.eventSystem;

	public static NightVisionVisual NightVision => Instance.nightVision;

	public static bool BloodFxOn => GameMetaData.BloodFxOn;

	public static PlayerInput MainPlayerInput => Instance.mainPlayerInput;

	public static ModManager ModManager => Instance.modManager;

	public static NoteIndex NoteIndex => Instance.noteIndex;

	public static AchievementManager AchievementManager => Instance.achievementManager;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (_instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		DOTween.defaultTimeScaleIndependent = true;
		DebugManager.instance.enableRuntimeUI = false;
		DebugManager.instance.displayRuntimeUI = false;
	}

	private void Update()
	{
		_ = Application.isEditor;
	}

	public static void TimeTravelDetected()
	{
		Debug.Log("检测到穿越者");
	}
}
