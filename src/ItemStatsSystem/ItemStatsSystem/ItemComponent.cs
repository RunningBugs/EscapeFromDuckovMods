using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

public class ItemComponent : MonoBehaviour, ISelfValidator
{
	[SerializeField]
	private Item master;

	private bool initialized;

	public Item Master
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

	private void Awake()
	{
		if (!initialized)
		{
			Initialize();
		}
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	internal void Initialize()
	{
		initialized = true;
		if (master == null)
		{
			master = GetComponent<Item>();
		}
		OnInitialize();
	}

	internal virtual void OnInitialize()
	{
	}

	public void Validate(SelfValidationResult result)
	{
		if (Master == null)
		{
			result.AddError("这个组件依赖Item，Item不可以留空。").WithFix("设置为本Game Object上的Item", delegate
			{
				master = GetComponent<Item>();
			});
		}
		else if (Master.gameObject != base.gameObject)
		{
			result.AddError("Master需要和本组件处于同一个Game Object上。").WithFix("设置为本Game Object上的Item", delegate
			{
				master = GetComponent<Item>();
			});
		}
	}
}
