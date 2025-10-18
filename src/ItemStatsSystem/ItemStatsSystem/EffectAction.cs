using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

[RequireComponent(typeof(Effect))]
public class EffectAction : EffectComponent, ISelfValidator
{
	protected override Color ActiveLabelColor => DuckovUtilitiesSettings.Colors.EffectAction;

	public override string DisplayName => "未命名动作(" + GetType().Name + ")";

	internal void NotifyTriggered(EffectTriggerEventContext context)
	{
		if (base.enabled)
		{
			OnTriggered(context.positive);
			if (context.positive)
			{
				OnTriggeredPositive();
			}
			else
			{
				OnTriggeredNegative();
			}
		}
	}

	protected virtual void OnTriggered(bool positive)
	{
	}

	protected virtual void OnTriggeredPositive()
	{
	}

	protected virtual void OnTriggeredNegative()
	{
	}

	private void OnDisable()
	{
		OnTriggeredNegative();
	}

	public override void Validate(SelfValidationResult result)
	{
		base.Validate(result);
		if (base.Master != null && !base.Master.Actions.Contains(this))
		{
			result.AddError("Master 中不包含本 Filter。").WithFix("将此 Filter 添加到 Master 中。", delegate
			{
				base.Master.AddEffectComponent(this);
			});
		}
	}
}
