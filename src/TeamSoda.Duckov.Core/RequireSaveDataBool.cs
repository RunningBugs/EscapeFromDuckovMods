using Duckov.Quests;
using Saves;
using UnityEngine;

public class RequireSaveDataBool : Condition
{
	[SerializeField]
	private string key;

	[SerializeField]
	private bool requireValue;

	public override bool Evaluate()
	{
		bool flag = SavesSystem.Load<bool>(key);
		Debug.Log($"Load bool:{key}  value:{flag}");
		return flag == requireValue;
	}
}
