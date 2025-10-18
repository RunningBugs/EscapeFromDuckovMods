using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Options;
using Duckov.Scenes;
using Duckov.UI;
using FMOD.Studio;
using FMODUnity;
using ItemStatsSystem;
using SodaCraft.StringUtilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Duckov;

public class AudioManager : MonoBehaviour
{
	[Serializable]
	public class Bus
	{
		[SerializeField]
		private string volumeRTPC = "Master";

		[HideInInspector]
		[SerializeField]
		private float volume = 1f;

		[HideInInspector]
		[SerializeField]
		private bool mute;

		private float appliedVolume = float.MinValue;

		public string Name => volumeRTPC;

		public float Volume
		{
			get
			{
				return volume;
			}
			set
			{
				volume = value;
				Apply();
			}
		}

		public bool Mute
		{
			get
			{
				return mute;
			}
			set
			{
				mute = value;
				Apply();
			}
		}

		public bool Dirty => appliedVolume != Volume;

		private string SaveKey => "Audio/" + volumeRTPC;

		public void Apply()
		{
			try
			{
				FMOD.Studio.Bus bus = RuntimeManager.GetBus("bus:/" + volumeRTPC);
				bus.setVolume(Volume);
				bus.setMute(Mute);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			appliedVolume = Volume;
			OptionsManager.Save(SaveKey, volume);
		}

		public Bus(string rtpc)
		{
			volumeRTPC = rtpc;
		}

		internal void LoadOptions()
		{
			volume = OptionsManager.Load(SaveKey, 1f);
		}

		internal void NotifyOptionsChanged(string key)
		{
			if (key == SaveKey)
			{
				LoadOptions();
			}
		}
	}

	public enum FootStepMaterialType
	{
		organic,
		mech,
		danger
	}

	public enum VoiceType
	{
		Duck,
		Robot,
		Wolf,
		Chicken,
		Crow,
		Eagle
	}

	private bool useArchivedSound;

	[SerializeField]
	private AudioObject ambientSource;

	[SerializeField]
	private AudioObject bgmSource;

	[SerializeField]
	private AudioObject stingerSource;

	[SerializeField]
	private Bus masterBus = new Bus("Master");

	[SerializeField]
	private Bus sfxBus = new Bus("Master/SFX");

	[SerializeField]
	private Bus musicBus = new Bus("Master/Music");

	private static Transform _soundSourceParent;

	private static ObjectPool<GameObject> _soundSourcePool;

	private const string path_hitmarker_norm = "SFX/Combat/Marker/hitmarker";

	private const string path_hitmarker_crit = "SFX/Combat/Marker/hitmarker_head";

	private const string path_killmarker_norm = "SFX/Combat/Marker/killmarker";

	private const string path_killmarker_crit = "SFX/Combat/Marker/killmarker_head";

	private const string path_music_death = "Music/Stinger/stg_death";

	private const string path_bullet_flyby = "SFX/Combat/Bullet/flyby";

	private const string path_pickup_item_fmt_soundkey = "SFX/Item/pickup_{soundkey}";

	private const string path_put_item_fmt_soundkey = "SFX/Item/put_{soundkey}";

	private const string path_ambient_fmt_soundkey = "Amb/amb_{soundkey}";

	private const string path_music_loop_fmt_soundkey = "Music/Loop/{soundkey}";

	private const string path_footstep_fmt_soundkey = "Char/Footstep/footstep_{charaType}_{strengthType}";

	public const string path_reload_fmt_soundkey = "SFX/Combat/Gun/Reload/{soundkey}";

	public const string path_shoot_fmt_gunkey = "SFX/Combat/Gun/Shoot/{soundkey}";

	public const string path_task_finished = "UI/mission_small";

	public const string path_building_built = "UI/building_up";

	public const string path_gun_unload = "SFX/Combat/Gun/unload";

	public const string path_stinger_fmt_key = "Music/Stinger/{key}";

	private static bool playingBGM;

	private static EventInstance bgmEvent;

	private static string currentBGMName;

	private static Dictionary<string, string> globalStates = new Dictionary<string, string>();

	private static Dictionary<int, VoiceType> gameObjectVoiceTypes = new Dictionary<int, VoiceType>();

	public static AudioManager Instance => GameManager.AudioManager;

	public static bool IsStingerPlaying
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			if (Instance.stingerSource == null)
			{
				return false;
			}
			return Instance.stingerSource.events.Any((EventInstance e) => e.isValid());
		}
	}

	private Transform listener => base.transform;

	private static Transform SoundSourceParent
	{
		get
		{
			if (_soundSourceParent == null)
			{
				GameObject obj = new GameObject("Sound Sources");
				_soundSourceParent = obj.transform;
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _soundSourceParent;
		}
	}

	private static ObjectPool<GameObject> SoundSourcePool
	{
		get
		{
			if (_soundSourcePool == null)
			{
				_soundSourcePool = new ObjectPool<GameObject>(delegate
				{
					GameObject obj = new GameObject("SoundSource");
					obj.transform.SetParent(SoundSourceParent);
					return obj;
				}, delegate(GameObject e)
				{
					e.SetActive(value: true);
				}, delegate(GameObject e)
				{
					e.SetActive(value: false);
				});
			}
			return _soundSourcePool;
		}
	}

	public static bool PlayingBGM => playingBGM;

	private static bool LogEvent => false;

	public static bool Initialized => RuntimeManager.IsInitialized;

	private IEnumerable<Bus> AllBueses()
	{
		yield return masterBus;
		yield return sfxBus;
		yield return musicBus;
	}

	public static EventInstance? Post(string eventName, GameObject gameObject)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return null;
		}
		if (gameObject == null)
		{
			Debug.LogError($"Posting event but gameObject is null: {gameObject}");
		}
		if (!gameObject.activeSelf)
		{
			Debug.LogError($"Posting event but gameObject is not active: {gameObject}");
		}
		return Instance.MPost(eventName, gameObject);
	}

	public static EventInstance? Post(string eventName)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return null;
		}
		return Instance.MPost(eventName);
	}

	public static EventInstance? Post(string eventName, Vector3 position)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return null;
		}
		return Instance.MPost(eventName, position);
	}

	internal static EventInstance? PostQuak(string soundKey, VoiceType voiceType, GameObject gameObject)
	{
		AudioObject orCreate = AudioObject.GetOrCreate(gameObject);
		orCreate.VoiceType = voiceType;
		return orCreate.PostQuak(soundKey);
	}

	public static void PostHitMarker(bool crit)
	{
		Post(crit ? "SFX/Combat/Marker/hitmarker_head" : "SFX/Combat/Marker/hitmarker");
	}

	public static void PostKillMarker(bool crit = false)
	{
		Post(crit ? "SFX/Combat/Marker/killmarker_head" : "SFX/Combat/Marker/killmarker");
	}

	private void Awake()
	{
		CharacterSoundMaker.OnFootStepSound = (Action<Vector3, CharacterSoundMaker.FootStepTypes, CharacterMainControl>)Delegate.Combine(CharacterSoundMaker.OnFootStepSound, new Action<Vector3, CharacterSoundMaker.FootStepTypes, CharacterMainControl>(OnFootStepSound));
		Projectile.OnBulletFlyByCharacter = (Action<Vector3>)Delegate.Combine(Projectile.OnBulletFlyByCharacter, new Action<Vector3>(OnBulletFlyby));
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
		ItemUIUtilities.OnPutItem += OnPutItem;
		Health.OnDead += OnHealthDead;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		SceneLoader.onStartedLoadingScene += OnStartedLoadingScene;
		OptionsManager.OnOptionsChanged += OnOptionsChanged;
		foreach (Bus item in AllBueses())
		{
			item.LoadOptions();
		}
	}

	private void OnDestroy()
	{
		CharacterSoundMaker.OnFootStepSound = (Action<Vector3, CharacterSoundMaker.FootStepTypes, CharacterMainControl>)Delegate.Remove(CharacterSoundMaker.OnFootStepSound, new Action<Vector3, CharacterSoundMaker.FootStepTypes, CharacterMainControl>(OnFootStepSound));
		Projectile.OnBulletFlyByCharacter = (Action<Vector3>)Delegate.Remove(Projectile.OnBulletFlyByCharacter, new Action<Vector3>(OnBulletFlyby));
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
		ItemUIUtilities.OnPutItem -= OnPutItem;
		Health.OnDead -= OnHealthDead;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
		SceneLoader.onStartedLoadingScene -= OnStartedLoadingScene;
		OptionsManager.OnOptionsChanged -= OnOptionsChanged;
	}

	private void OnOptionsChanged(string key)
	{
		foreach (Bus item in AllBueses())
		{
			item.NotifyOptionsChanged(key);
		}
	}

	public static Bus GetBus(string name)
	{
		if (Instance == null)
		{
			return null;
		}
		foreach (Bus item in Instance.AllBueses())
		{
			if (item.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	private void OnStartedLoadingScene(SceneLoadingContext context)
	{
		if ((bool)ambientSource)
		{
			ambientSource.StopAll();
		}
	}

	private void OnLevelInitialized()
	{
	}

	private void Start()
	{
		UpdateBuses();
	}

	private void OnHealthDead(Health health, DamageInfo info)
	{
		if (health.TryGetCharacter() == CharacterMainControl.Main)
		{
			StopBGM();
			Post("Music/Stinger/stg_death");
		}
	}

	private void OnPutItem(Item item, bool pickup = false)
	{
		PlayPutItemSFX(item, pickup);
	}

	public static void PlayPutItemSFX(Item item, bool pickup = false)
	{
		if (!(item == null) && LevelManager.LevelInited)
		{
			Post((pickup ? "SFX/Item/pickup_{soundkey}" : "SFX/Item/put_{soundkey}").Format(new
			{
				soundkey = item.SoundKey.ToLower()
			}));
		}
	}

	private void OnSubSceneLoaded(MultiSceneCore core, Scene scene)
	{
		LevelManager.LevelInitializingComment = "Opening ears";
		SubSceneEntry subSceneInfo = core.GetSubSceneInfo(scene);
		if (subSceneInfo != null)
		{
			if ((bool)ambientSource)
			{
				LevelManager.LevelInitializingComment = "Hearing Ambient";
				ambientSource.StopAll();
				ambientSource.Post("Amb/amb_{soundkey}".Format(new
				{
					soundkey = subSceneInfo.AmbientSound.ToLower()
				}));
			}
			LevelManager.LevelInitializingComment = "Hearing Buses";
			ApplyBuses();
		}
	}

	public static bool TryCreateEventInstance(string eventPath, out EventInstance eventInstance)
	{
		eventInstance = default(EventInstance);
		if (Instance.useArchivedSound)
		{
			eventPath = "Archived/" + eventPath;
		}
		string text = "event:/" + eventPath;
		try
		{
			eventInstance = RuntimeManager.CreateInstance(text);
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (LogEvent)
			{
				Debug.LogError("[AudioEvent][Failed] " + text);
			}
		}
		return false;
	}

	public static void PlayBGM(string name)
	{
		StopBGM();
		if (Instance == null)
		{
			return;
		}
		playingBGM = true;
		if (!string.IsNullOrWhiteSpace(name))
		{
			string eventName = "Music/Loop/{soundkey}".Format(new
			{
				soundkey = name
			});
			if (!Instance.bgmSource.Post(eventName).HasValue)
			{
				currentBGMName = null;
			}
			else
			{
				currentBGMName = name;
			}
		}
	}

	public static void StopBGM()
	{
		if (!(Instance == null))
		{
			Instance.bgmSource.StopAll(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			currentBGMName = null;
		}
	}

	public static void PlayStringer(string key)
	{
		string eventName = "Music/Stinger/{key}".Format(new { key });
		Instance.stingerSource.Post(eventName);
	}

	private void OnBulletFlyby(Vector3 vector)
	{
		Post("SFX/Combat/Bullet/flyby", vector);
	}

	public static void SetState(string stateGroup, string state)
	{
		globalStates[stateGroup] = state;
	}

	public static string GetState(string stateGroup)
	{
		if (globalStates.TryGetValue(stateGroup, out var value))
		{
			return value;
		}
		return null;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Backslash))
		{
			useArchivedSound = !useArchivedSound;
			Debug.Log($"USE ARCHIVED SOUND:{useArchivedSound}");
		}
		UpdateListener();
		UpdateBuses();
	}

	private void UpdateListener()
	{
		if (LevelManager.Instance == null)
		{
			Camera main = Camera.main;
			if (main != null)
			{
				listener.transform.position = main.transform.position;
				listener.transform.rotation = main.transform.rotation;
			}
			return;
		}
		GameCamera gameCamera = LevelManager.Instance.GameCamera;
		if (gameCamera != null)
		{
			if (CharacterMainControl.Main != null)
			{
				listener.transform.position = CharacterMainControl.Main.transform.position + Vector3.up * 2f;
			}
			else
			{
				listener.transform.position = gameCamera.renderCamera.transform.position;
			}
			listener.transform.rotation = gameCamera.renderCamera.transform.rotation;
		}
	}

	private void UpdateBuses()
	{
		foreach (Bus item in AllBueses())
		{
			if (item.Dirty)
			{
				item.Apply();
			}
		}
	}

	private void ApplyBuses()
	{
		foreach (Bus item in AllBueses())
		{
			item.Apply();
		}
	}

	private void OnFootStepSound(Vector3 position, CharacterSoundMaker.FootStepTypes type, CharacterMainControl character)
	{
		if (!(character == null))
		{
			GameObject gameObject = character.gameObject;
			string value = "floor";
			MSetParameter(gameObject, "terrain", value);
			string charaType = character.FootStepMaterialType.ToString();
			string strengthType = "light";
			switch (type)
			{
			case CharacterSoundMaker.FootStepTypes.walkLight:
			case CharacterSoundMaker.FootStepTypes.runLight:
				strengthType = "light";
				break;
			case CharacterSoundMaker.FootStepTypes.walkHeavy:
			case CharacterSoundMaker.FootStepTypes.runHeavy:
				strengthType = "heavy";
				break;
			}
			Post("Char/Footstep/footstep_{charaType}_{strengthType}".Format(new { charaType, strengthType }), character.gameObject);
		}
	}

	private void MSetParameter(GameObject gameObject, string parameterName, string value)
	{
		if (gameObject == null)
		{
			Debug.LogError("Game Object must exist");
		}
		else
		{
			AudioObject.GetOrCreate(gameObject).SetParameterByNameWithLabel(parameterName, value);
		}
	}

	private EventInstance? MPost(string eventName, GameObject gameObject = null)
	{
		if (!Initialized)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(eventName))
		{
			return null;
		}
		if (gameObject == null)
		{
			gameObject = Instance.gameObject;
		}
		else if (!gameObject.activeInHierarchy)
		{
			Debug.LogWarning("Posting event on inactive object, canceled");
			return null;
		}
		return AudioObject.GetOrCreate(gameObject).Post(eventName ?? "");
	}

	private EventInstance? MPost(string eventName, Vector3 position)
	{
		SoundSourcePool.Get().transform.position = position;
		if (!TryCreateEventInstance(eventName ?? "", out var eventInstance))
		{
			return null;
		}
		eventInstance.set3DAttributes(position.To3DAttributes());
		eventInstance.start();
		eventInstance.release();
		return eventInstance;
	}

	public static void StopAll(GameObject gameObject, FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
	{
		AudioObject.GetOrCreate(gameObject).StopAll(mode);
	}

	internal void MSetRTPC(string key, float value, GameObject gameObject = null)
	{
		if (gameObject == null)
		{
			RuntimeManager.StudioSystem.setParameterByName("parameter:/" + key, value);
			if (LogEvent)
			{
				Debug.Log($"[AudioEvent][Parameter][Global] {key} = {value}");
			}
		}
		else
		{
			AudioObject.GetOrCreate(gameObject).SetParameterByName("parameter:/" + key, value);
			if (LogEvent)
			{
				Debug.Log($"[AudioEvent][Parameter][GameObject] {key} = {value}", gameObject);
			}
		}
	}

	internal static void SetRTPC(string key, float value, GameObject gameObject = null)
	{
		if (!(Instance == null))
		{
			Instance.MSetRTPC(key, value, gameObject);
		}
	}

	public static void SetVoiceType(GameObject gameObject, VoiceType voiceType)
	{
		if (!(gameObject == null))
		{
			AudioObject.GetOrCreate(gameObject).VoiceType = voiceType;
		}
	}
}
