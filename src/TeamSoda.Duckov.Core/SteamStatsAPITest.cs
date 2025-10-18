using Steamworks;
using UnityEngine;

public class SteamStatsAPITest : MonoBehaviour
{
	private Callback<UserStatsReceived_t> onStatsReceivedCallback;

	private Callback<UserStatsStored_t> onStatsStoredCallback;

	private void Awake()
	{
		onStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatReceived);
		onStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnUserStatStored);
	}

	private void OnUserStatStored(UserStatsStored_t param)
	{
		Debug.Log("Stat Stored!");
	}

	private void OnUserStatReceived(UserStatsReceived_t param)
	{
		CSteamID steamIDUser = param.m_steamIDUser;
		Debug.Log("Stat Fetched:" + steamIDUser.ToString() + " " + param.m_nGameID);
	}

	private void Start()
	{
		SteamUserStats.RequestGlobalStats(60);
	}

	private void Test()
	{
		Debug.Log(SteamUserStats.GetStat("game_finished", out int pData) + " " + pData);
		bool flag = SteamUserStats.SetStat("game_finished", pData + 1);
		Debug.Log($"Set: {flag}");
		SteamUserStats.StoreStats();
	}

	private void GetGlobalStat()
	{
		if (SteamUserStats.GetGlobalStat("game_finished", out long pData))
		{
			Debug.Log($"game finished: {pData}");
		}
		else
		{
			Debug.Log("Failed");
		}
	}
}
