using ItemStatsSystem;
using UnityEngine;

[MenuPath("Health/一段时间没受伤")]
public class NotHurtForSeconds : EffectFilter
{
	public float time;

	private float lastHurtTime = -9999f;

	private bool binded;

	private CharacterMainControl _mainControl;

	public override string DisplayName => time + "秒内没受伤";

	private CharacterMainControl MainControl
	{
		get
		{
			if (_mainControl == null)
			{
				_mainControl = base.Master?.Item?.GetCharacterMainControl();
			}
			return _mainControl;
		}
	}

	protected override bool OnEvaluate(EffectTriggerEventContext context)
	{
		if (!binded && (bool)MainControl)
		{
			MainControl.Health.OnHurtEvent.AddListener(OnHurt);
			binded = true;
		}
		return Time.time - lastHurtTime > time;
	}

	private void OnDestroy()
	{
		if ((bool)MainControl)
		{
			MainControl.Health.OnHurtEvent.RemoveListener(OnHurt);
		}
	}

	private void OnHurt(DamageInfo dmgInfo)
	{
		lastHurtTime = Time.time;
	}
}
