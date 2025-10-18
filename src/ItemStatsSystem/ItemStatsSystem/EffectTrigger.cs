using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

[RequireComponent(typeof(Effect))]
public class EffectTrigger : EffectComponent, ISelfValidator
{
	protected override Color ActiveLabelColor => DuckovUtilitiesSettings.Colors.EffectTrigger;

	public override string DisplayName => "未命名触发器(" + GetType().Name + ")";

	protected void Trigger(bool positive = true)
	{
		base.Master.Trigger(new EffectTriggerEventContext(this, positive));
	}

	protected void TriggerPositive()
	{
		Trigger();
	}

	protected void TriggerNegative()
	{
		Trigger(positive: false);
	}

	public override void Validate(SelfValidationResult result)
	{
		base.Validate(result);
		if (base.Master != null && !base.Master.Triggers.Contains(this))
		{
			result.AddError("Master 中不包含本 Filter。").WithFix("将此 Filter 添加到 Master 中。", delegate
			{
				base.Master.AddEffectComponent(this);
			});
		}
	}

	protected virtual void OnDisable()
	{
		Trigger(positive: false);
	}

	protected virtual void OnMasterSetTargetItem(Effect effect, Item item)
	{
	}

	internal void NotifySetItem(Effect effect, Item targetItem)
	{
		OnMasterSetTargetItem(effect, targetItem);
	}
}
