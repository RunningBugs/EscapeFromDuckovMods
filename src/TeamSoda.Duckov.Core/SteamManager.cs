using System;
using System.Text;
using AOT;
using Duckov;
using Duckov.Achievements;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	public const bool SteamEnabled = true;

	public const int AppID_Int = 3167020;

	protected static bool s_EverInitialized;

	protected static SteamManager s_instance;

	protected bool m_bInitialized;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	protected static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				Debug.Log("Creating steam manager");
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitOnPlayMode()
	{
		s_EverInitialized = false;
		s_instance = null;
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)3167020u))
			{
				Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		s_EverInitialized = true;
		AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
		AchievementManager.OnAchievementDataLoaded += OnAchievementDataLoaded;
		RichPresenceManager.OnInstanceChanged = (Action<RichPresenceManager>)Delegate.Combine(RichPresenceManager.OnInstanceChanged, new Action<RichPresenceManager>(OnRichPresenceChanged));
		PlatformInfo.GetIDFunc = GetID;
	}

	private static string GetID()
	{
		if (s_instance == null)
		{
			return null;
		}
		if (!Initialized)
		{
			return null;
		}
		return SteamUser.GetSteamID().ToString();
	}

	protected virtual void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
				AchievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
				AchievementManager.OnAchievementDataLoaded -= OnAchievementDataLoaded;
				RichPresenceManager.OnInstanceChanged = (Action<RichPresenceManager>)Delegate.Remove(RichPresenceManager.OnInstanceChanged, new Action<RichPresenceManager>(OnRichPresenceChanged));
			}
		}
	}

	private void OnRichPresenceChanged(RichPresenceManager manager)
	{
		if (Initialized && !(manager == null))
		{
			string steamDisplay = manager.GetSteamDisplay();
			if (!SteamFriends.SetRichPresence("steam_display", steamDisplay))
			{
				Debug.LogError("Failed setting rich presence: level = " + steamDisplay);
			}
			if (!SteamFriends.SetRichPresence("level", manager.levelDisplayNameRaw))
			{
				Debug.LogError("Failed setting rich presence: level = " + manager.levelDisplayNameRaw);
			}
		}
	}

	private void OnAchievementDataLoaded(AchievementManager manager)
	{
		if (!Initialized || manager == null)
		{
			return;
		}
		bool flag = false;
		foreach (string unlockedAchievement in manager.UnlockedAchievements)
		{
			if (SteamUserStats.GetAchievement(unlockedAchievement, out var pbAchieved) && !pbAchieved)
			{
				SteamUserStats.SetAchievement(unlockedAchievement);
				flag = true;
			}
		}
		if (flag)
		{
			SteamUserStats.StoreStats();
		}
	}

	private void OnAchievementUnlocked(string achievementKey)
	{
		if (Initialized)
		{
			SteamUserStats.SetAchievement(achievementKey);
			SteamUserStats.StoreStats();
		}
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
