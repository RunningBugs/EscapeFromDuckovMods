using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

[RequireComponent(typeof(Effect))]
public class EffectFilter : EffectComponent, ISelfValidator
{
	[SerializeField]
	private bool ignoreNegativeTrigger = true;

	protected override Color ActiveLabelColor => DuckovUtilitiesSettings.Colors.EffectFilter;

	public override string DisplayName => "未命名过滤器(" + GetType().Name + ")";

	protected bool IgnoreNegativeTrigger
	{
		get
		{
			return ignoreNegativeTrigger;
		}
		set
		{
			ignoreNegativeTrigger = value;
		}
	}

	public bool Evaluate(EffectTriggerEventContext context)
	{
		if (!base.enabled)
		{
			return true;
		}
		if (!context.positive && IgnoreNegativeTrigger)
		{
			return true;
		}
		return OnEvaluate(context);
	}

	protected virtual bool OnEvaluate(EffectTriggerEventContext context)
	{
		return true;
	}

	public override void Validate(SelfValidationResult result)
	{
		base.Validate(result);
		if (base.Master != null && !base.Master.Filters.Contains(this))
		{
			result.AddError("Master 中不包含本 Filter。").WithFix("将此 Filter 添加到 Master 中。", delegate
			{
				base.Master.AddEffectComponent(this);
			});
		}
	}
}
