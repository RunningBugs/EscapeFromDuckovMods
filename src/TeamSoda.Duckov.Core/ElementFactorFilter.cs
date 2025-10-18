using ItemStatsSystem;

[MenuPath("弱属性")]
public class ElementFactorFilter : EffectFilter
{
	public enum ElementFactorFilterTypes
	{
		GreaterThan,
		LessThan
	}

	public ElementFactorFilterTypes type;

	public float compareTo = 1f;

	public ElementTypes element;

	private CharacterMainControl _mainControl;

	public override string DisplayName => string.Format("如果{0}系数{1}{2}", element, (type == ElementFactorFilterTypes.GreaterThan) ? "大于" : "小于", compareTo);

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
		if (!MainControl)
		{
			return false;
		}
		if (!MainControl.Health)
		{
			return false;
		}
		float num = MainControl.Health.ElementFactor(element);
		if (type != ElementFactorFilterTypes.GreaterThan)
		{
			return num < compareTo;
		}
		return num > compareTo;
	}

	private void OnDestroy()
	{
	}
}
