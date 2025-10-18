using UnityEngine;

namespace ItemStatsSystem;

[MenuPath("Debug/Log Item Name")]
public class LogItemNameAction : EffectAction
{
	public override string DisplayName => "Log 物品名称";

	protected override void OnTriggeredPositive()
	{
		if (base.Master.Item == null)
		{
			Debug.Log("物品不存在");
		}
		else
		{
			Debug.Log(base.Master.Item.DisplayName);
		}
	}
}
