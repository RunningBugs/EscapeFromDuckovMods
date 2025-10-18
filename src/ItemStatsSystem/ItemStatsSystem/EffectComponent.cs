using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

public class EffectComponent : MonoBehaviour, ISelfValidator
{
	[SerializeField]
	private Effect master;

	private Empty label;

	public Effect Master
	{
		get
		{
			return master;
		}
		internal set
		{
			master = value;
		}
	}

	public virtual string DisplayName => GetType().Name;

	internal Color LabelColor
	{
		get
		{
			if (!base.enabled)
			{
				return Color.gray;
			}
			return ActiveLabelColor;
		}
	}

	protected virtual Color ActiveLabelColor => Color.white;

	public virtual void Validate(SelfValidationResult result)
	{
		if (master == null)
		{
			result.AddError("需要一个Master。").WithFix("将Master设为本物体上的Effect。", delegate
			{
				master = GetComponent<Effect>();
			});
		}
		else if (master.gameObject != base.gameObject)
		{
			result.AddError("Master必须处于同一Game Object上。").WithFix("将Master设为本物体上的Effect。", delegate
			{
				master = GetComponent<Effect>();
			});
		}
	}

	protected virtual void Awake()
	{
		if (Master == null)
		{
			master = GetComponent<Effect>();
		}
		if (Master == null)
		{
			Debug.LogWarning("No Effect component on current game object.");
		}
	}

	private void Start()
	{
	}

	private void RemoveThisComponent()
	{
	}
}
