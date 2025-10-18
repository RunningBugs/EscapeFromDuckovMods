using Duckov.Achievements;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Duckov;

public class SocialManager : MonoBehaviour
{
	private bool initialized;

	private IAchievement[] _achievement_cache;

	private void Awake()
	{
		AchievementManager.OnAchievementUnlocked += UnlockAchievement;
	}

	private void Start()
	{
		Social.localUser.Authenticate(ProcessAuthentication);
	}

	private void ProcessAuthentication(bool success)
	{
		if (success)
		{
			initialized = true;
			Social.LoadAchievements(ProcessLoadedAchievements);
		}
	}

	private void ProcessLoadedAchievements(IAchievement[] loadedAchievements)
	{
		_achievement_cache = loadedAchievements;
	}

	private void UnlockAchievement(string id)
	{
		if (!initialized)
		{
			Social.ReportProgress(id, 100.0, OnReportProgressResult);
		}
	}

	private void OnReportProgressResult(bool success)
	{
		Social.LoadAchievements(ProcessLoadedAchievements);
	}
}
