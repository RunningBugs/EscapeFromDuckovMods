using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Duckov.Economy;
using UnityEngine;
using UnityEngine.Events;

public class CostTaker : InteractableBase
{
	[SerializeField]
	private Cost cost;

	public UnityEvent<CostTaker> onPayedUnityEvent;

	private static List<CostTaker> activeCostTakers = new List<CostTaker>();

	private static ReadOnlyCollection<CostTaker> _activeCostTakers_ReadOnly;

	public Cost Cost => cost;

	public static ReadOnlyCollection<CostTaker> ActiveCostTakers
	{
		get
		{
			if (_activeCostTakers_ReadOnly == null)
			{
				_activeCostTakers_ReadOnly = new ReadOnlyCollection<CostTaker>(activeCostTakers);
			}
			return _activeCostTakers_ReadOnly;
		}
	}

	public event Action<CostTaker> onPayed;

	public static event Action<CostTaker> OnCostTakerRegistered;

	public static event Action<CostTaker> OnCostTakerUnregistered;

	protected override bool IsInteractable()
	{
		return cost.Enough;
	}

	protected override void OnInteractFinished()
	{
		if (cost.Enough && cost.Pay())
		{
			this.onPayed?.Invoke(this);
			onPayedUnityEvent?.Invoke(this);
		}
	}

	private void OnEnable()
	{
		Register(this);
	}

	private void OnDisable()
	{
		Unregister(this);
	}

	public static void Register(CostTaker costTaker)
	{
		activeCostTakers.Add(costTaker);
		CostTaker.OnCostTakerRegistered?.Invoke(costTaker);
	}

	public static void Unregister(CostTaker costTaker)
	{
		if (activeCostTakers.Remove(costTaker))
		{
			CostTaker.OnCostTakerUnregistered?.Invoke(costTaker);
		}
	}

	public void SetCost(Cost cost)
	{
		Unregister(this);
		this.cost = cost;
		if (base.isActiveAndEnabled)
		{
			Register(this);
		}
	}
}
