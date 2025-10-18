using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ItemStatsSystem;

public class Effect : MonoBehaviour, ISelfValidator
{
	[SerializeField]
	private Item item;

	[SerializeField]
	private bool display;

	[SerializeField]
	private string description = "未定义描述";

	[SerializeField]
	internal List<EffectTrigger> triggers = new List<EffectTrigger>();

	[SerializeField]
	internal List<EffectFilter> filters = new List<EffectFilter>();

	[SerializeField]
	internal List<EffectAction> actions = new List<EffectAction>();

	private ReadOnlyCollection<EffectTrigger> _Triggers;

	private ReadOnlyCollection<EffectFilter> _Filters;

	private ReadOnlyCollection<EffectAction> _Actions;

	public Item Item => item;

	public bool Display => display;

	public string Description => description;

	public ReadOnlyCollection<EffectTrigger> Triggers
	{
		get
		{
			if (_Triggers == null)
			{
				_Triggers = new ReadOnlyCollection<EffectTrigger>(triggers);
			}
			return _Triggers;
		}
	}

	public ReadOnlyCollection<EffectFilter> Filters
	{
		get
		{
			if (_Filters == null)
			{
				_Filters = new ReadOnlyCollection<EffectFilter>(filters);
			}
			return _Filters;
		}
	}

	public ReadOnlyCollection<EffectAction> Actions
	{
		get
		{
			if (_Actions == null)
			{
				_Actions = new ReadOnlyCollection<EffectAction>(actions);
			}
			return _Actions;
		}
	}

	private static Color TriggerColor => DuckovUtilitiesSettings.Colors.EffectTrigger;

	private static Color FilterColor => DuckovUtilitiesSettings.Colors.EffectFilter;

	private static Color ActionColor => DuckovUtilitiesSettings.Colors.EffectAction;

	public event Action<Effect, Item> onSetTargetItem;

	public event Action<Effect, Item> onItemTreeChanged;

	public string GetDisplayString()
	{
		return Description;
	}

	private bool EvaluateFilters(EffectTriggerEventContext context)
	{
		foreach (EffectFilter filter in filters)
		{
			if (filter.enabled && !filter.Evaluate(context))
			{
				return false;
			}
		}
		return true;
	}

	internal void Trigger(EffectTriggerEventContext context)
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy || !EvaluateFilters(context))
		{
			return;
		}
		foreach (EffectAction action in actions)
		{
			action.NotifyTriggered(context);
		}
	}

	private void OnValidate()
	{
		if (item == null)
		{
			item = base.transform.parent?.GetComponent<Item>();
		}
		base.transform.hideFlags = HideFlags.HideInInspector;
	}

	public void SetItem(Item targetItem)
	{
		UnregisterItemEvents();
		if (targetItem == null)
		{
			item = null;
			base.transform.SetParent(null);
		}
		item = targetItem;
		foreach (EffectTrigger trigger in triggers)
		{
			trigger.NotifySetItem(this, targetItem);
		}
		this.onSetTargetItem?.Invoke(this, targetItem);
		RegisterItemEvents();
	}

	private void RegisterItemEvents()
	{
		if (!(item == null))
		{
			item.onItemTreeChanged += OnItemTreeChanged;
		}
	}

	private void UnregisterItemEvents()
	{
		if (!(item == null))
		{
			item.onItemTreeChanged -= OnItemTreeChanged;
		}
	}

	private void OnItemTreeChanged(Item item)
	{
		this.onItemTreeChanged?.Invoke(this, this.item);
	}

	public void Validate(SelfValidationResult result)
	{
		Item item = base.transform.parent?.GetComponent<Item>();
		if (this.item != item)
		{
			result.AddError("Item 应为直接父物体").WithFix("将 Item 设为正确的值", delegate
			{
				this.item = base.transform.parent?.GetComponent<Item>();
			});
		}
		else if (this.item != null && !this.item.Effects.Contains(this))
		{
			result.AddError("Item中未引用本Effect").WithFix("在Item中加入本Effect。", delegate
			{
				this.item.Effects.Add(this);
			});
		}
		bool flag = false;
		foreach (EffectTrigger trigger in triggers)
		{
			if (!ValidateComponent(trigger))
			{
				flag = true;
			}
		}
		if (flag)
		{
			result.AddError("Trigger 列表中包含来自其他 Game Object 的 Trigger。").WithFix("移除来自其他 Game Object 的 Trigger。", delegate
			{
				triggers.RemoveAll((EffectTrigger e) => e.gameObject != base.gameObject);
			});
		}
		flag = false;
		foreach (EffectFilter filter in filters)
		{
			if (!ValidateComponent(filter))
			{
				flag = true;
			}
		}
		if (flag)
		{
			result.AddError("Filter 列表中包含来自其他 Game Object 的 Filter。").WithFix("移除来自其他 Game Object 的 Filter。", delegate
			{
				filters.RemoveAll((EffectFilter e) => e.gameObject != base.gameObject);
			});
		}
		flag = false;
		foreach (EffectAction action in actions)
		{
			if (!ValidateComponent(action))
			{
				flag = true;
			}
		}
		if (flag)
		{
			result.AddError("Trigger 列表中包含来自其他 Game Object 的 Trigger。").WithFix("移除来自其他 Game Object 的 Trigger。", delegate
			{
				actions.RemoveAll((EffectAction e) => e.gameObject != base.gameObject);
			});
		}
		if (AnyDuplicate<EffectTrigger>(triggers))
		{
			result.AddError("Trigger 列表中有重复的元素。").WithFix("移除重复元素。", delegate
			{
				triggers = new List<EffectTrigger>(triggers.Distinct());
			});
		}
		if (AnyDuplicate<EffectFilter>(filters))
		{
			result.AddError("Filter 列表中有重复的元素。").WithFix("移除重复元素。", delegate
			{
				filters = new List<EffectFilter>(filters.Distinct());
			});
		}
		if (AnyDuplicate<EffectAction>(actions))
		{
			result.AddError("Trigger 列表中有重复的元素。").WithFix("移除重复元素。", delegate
			{
				actions = new List<EffectAction>(actions.Distinct());
			});
		}
		if (AnyNull<EffectTrigger>(triggers))
		{
			result.AddError("Trigger 列表中有空元素。").WithFix("移除空元素。", delegate
			{
				triggers.RemoveAll((EffectTrigger e) => e == null);
			});
		}
		if (AnyNull<EffectFilter>(filters))
		{
			result.AddError("Filter 列表中有空元素。").WithFix("移除空元素。", delegate
			{
				filters.RemoveAll((EffectFilter e) => e == null);
			});
		}
		if (AnyNull<EffectAction>(actions))
		{
			result.AddError("Trigger 列表中有空元素。").WithFix("移除空元素。", delegate
			{
				actions.RemoveAll((EffectAction e) => e == null);
			});
		}
		if (triggers.Count < 1)
		{
			result.AddWarning("没有配置任何触发器(Trigger)，将无法触发效果。");
		}
		if (actions.Count < 1)
		{
			result.AddWarning("没有配置任何动作(Action),该效果没有任何实际作用。");
		}
		static bool AnyDuplicate<T>(List<T> list)
		{
			return (from e in list
				group e by e).Any((IGrouping<T, T> g) => g.Count() > 1);
		}
		static bool AnyNull<T>(List<T> list)
		{
			return list.Any((T e) => e == null);
		}
		bool ValidateComponent(EffectComponent component)
		{
			if (component == null)
			{
				return true;
			}
			if (component.gameObject != base.gameObject)
			{
				return false;
			}
			return true;
		}
	}

	internal void AddEffectComponent(EffectComponent effectComponent)
	{
		if (effectComponent is EffectTrigger)
		{
			triggers.Add(effectComponent as EffectTrigger);
			effectComponent.Master = this;
		}
		else if (effectComponent is EffectFilter)
		{
			filters.Add(effectComponent as EffectFilter);
			effectComponent.Master = this;
		}
		else if (effectComponent is EffectAction)
		{
			actions.Add(effectComponent as EffectAction);
			effectComponent.Master = this;
		}
	}

	private void Awake()
	{
		RegisterItemEvents();
	}
}
