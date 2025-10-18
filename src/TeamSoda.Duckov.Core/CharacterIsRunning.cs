using ItemStatsSystem;

[MenuPath("角色/角色正在奔跑")]
public class CharacterIsRunning : EffectFilter
{
	private CharacterMainControl _mainControl;

	public override string DisplayName => "角色正在奔跑";

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
		return MainControl.Running;
	}

	private void OnDestroy()
	{
	}
}
