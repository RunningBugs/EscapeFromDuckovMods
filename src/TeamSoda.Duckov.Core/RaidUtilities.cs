using System;
using Duckov;
using Saves;
using UnityEngine;

public static class RaidUtilities
{
	[Serializable]
	public struct RaidInfo
	{
		public bool valid;

		public uint ID;

		public bool dead;

		public bool ended;

		public float raidBeginTime;

		public float raidEndTime;

		public float totalTime;

		public long expOnBegan;

		public long expOnEnd;

		public long expGained;
	}

	public static Action<RaidInfo> OnNewRaid;

	public static Action<RaidInfo> OnRaidDead;

	public static Action<RaidInfo> OnRaidEnd;

	private const string SaveID = "RaidInfo";

	public static RaidInfo CurrentRaid
	{
		get
		{
			RaidInfo result = SavesSystem.Load<RaidInfo>("RaidInfo");
			result.totalTime = Time.unscaledTime - result.raidBeginTime;
			result.expOnEnd = EXPManager.EXP;
			result.expGained = result.expOnEnd - result.expOnBegan;
			return result;
		}
		private set
		{
			SavesSystem.Save("RaidInfo", value);
		}
	}

	public static void NewRaid()
	{
		RaidInfo currentRaid = CurrentRaid;
		RaidInfo obj = (CurrentRaid = new RaidInfo
		{
			valid = true,
			ID = currentRaid.ID + 1,
			dead = false,
			ended = false,
			raidBeginTime = Time.unscaledTime,
			raidEndTime = 0f,
			expOnBegan = EXPManager.EXP
		});
		OnNewRaid?.Invoke(obj);
	}

	public static void NotifyDead()
	{
		RaidInfo currentRaid = CurrentRaid;
		currentRaid.dead = true;
		currentRaid.ended = true;
		currentRaid.raidEndTime = Time.unscaledTime;
		currentRaid.totalTime = currentRaid.raidEndTime - currentRaid.raidBeginTime;
		currentRaid.expOnEnd = EXPManager.EXP;
		currentRaid.expGained = currentRaid.expOnEnd - currentRaid.expOnBegan;
		CurrentRaid = currentRaid;
		OnRaidEnd?.Invoke(currentRaid);
		OnRaidDead?.Invoke(currentRaid);
	}

	public static void NotifyEnd()
	{
		RaidInfo currentRaid = CurrentRaid;
		currentRaid.ended = true;
		currentRaid.raidEndTime = Time.unscaledTime;
		currentRaid.totalTime = currentRaid.raidEndTime - currentRaid.raidBeginTime;
		currentRaid.expOnEnd = EXPManager.EXP;
		currentRaid.expGained = currentRaid.expOnEnd - currentRaid.expOnBegan;
		CurrentRaid = currentRaid;
		OnRaidEnd?.Invoke(currentRaid);
	}
}
