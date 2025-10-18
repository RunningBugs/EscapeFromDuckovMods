using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace Fishing;

[CreateAssetMenu(menuName = "Fishing/Fishing Pool")]
public class FishingPool : ScriptableObject
{
	[SerializeField]
	private List<FishingPoolEntry> entries;

	public int GetRandom(WeightModifications[] modifications)
	{
		if (entries == null || entries.Count <= 0)
		{
			Debug.LogError("Fishing Pool " + base.name + " 里面没有配置任何项目，返回-1");
			return -1;
		}
		if (modifications != null && modifications.Length != 0)
		{
			return entries.GetRandomWeighted(delegate(FishingPoolEntry e)
			{
				WeightModifications[] array = modifications;
				for (int i = 0; i < array.Length; i++)
				{
					WeightModifications weightModifications = array[i];
					if (weightModifications.id == e.ID)
					{
						return e.Weight + weightModifications.addAmount;
					}
				}
				return e.Weight;
			}).ID;
		}
		return entries.GetRandomWeighted((FishingPoolEntry e) => e.Weight).ID;
	}
}
