using System;
using UnityEngine;

namespace VLB;

[Serializable]
public class RaymarchingQuality
{
	public string name;

	public int stepCount;

	[SerializeField]
	private int _UniqueID;

	private static RaymarchingQuality ms_DefaultInstance = new RaymarchingQuality(-1);

	private const int kRandomUniqueIdMinRange = 4;

	public int uniqueID => _UniqueID;

	public bool hasValidUniqueID => _UniqueID >= 0;

	public static RaymarchingQuality defaultInstance => ms_DefaultInstance;

	private RaymarchingQuality(int uniqueID)
	{
		_UniqueID = uniqueID;
		name = "New quality";
		stepCount = 10;
	}

	public static RaymarchingQuality New()
	{
		return new RaymarchingQuality(UnityEngine.Random.Range(4, int.MaxValue));
	}

	public static RaymarchingQuality New(string name, int forcedUniqueID, int stepCount)
	{
		return new RaymarchingQuality(forcedUniqueID)
		{
			name = name,
			stepCount = stepCount
		};
	}

	private static bool HasRaymarchingQualityWithSameUniqueID(RaymarchingQuality[] values, int id)
	{
		foreach (RaymarchingQuality raymarchingQuality in values)
		{
			if (raymarchingQuality != null && raymarchingQuality.uniqueID == id)
			{
				return true;
			}
		}
		return false;
	}
}
