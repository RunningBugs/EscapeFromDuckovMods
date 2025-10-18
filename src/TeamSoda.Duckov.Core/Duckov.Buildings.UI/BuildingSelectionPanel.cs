using System;
using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.Buildings.UI;

public class BuildingSelectionPanel : MonoBehaviour
{
	[SerializeField]
	private BuildingBtnEntry buildingBtnTemplate;

	private PrefabPool<BuildingBtnEntry> _pool;

	private BuildingArea targetArea;

	private PrefabPool<BuildingBtnEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<BuildingBtnEntry>(buildingBtnTemplate, null, OnGetButtonEntry, OnReleaseButtonEntry);
			}
			return _pool;
		}
	}

	public event Action<BuildingBtnEntry> onButtonSelected;

	public event Action<BuildingBtnEntry> onRecycleRequested;

	private void OnGetButtonEntry(BuildingBtnEntry entry)
	{
		entry.onButtonClicked += OnButtonSelected;
		entry.onRecycleRequested += OnRecycleRequested;
	}

	private void OnReleaseButtonEntry(BuildingBtnEntry entry)
	{
		entry.onButtonClicked -= OnButtonSelected;
		entry.onRecycleRequested -= OnRecycleRequested;
	}

	private void OnRecycleRequested(BuildingBtnEntry entry)
	{
		this.onRecycleRequested?.Invoke(entry);
	}

	private void OnButtonSelected(BuildingBtnEntry entry)
	{
		this.onButtonSelected?.Invoke(entry);
	}

	public void Show()
	{
	}

	internal void Setup(BuildingArea targetArea)
	{
		this.targetArea = targetArea;
		Refresh();
	}

	public void Refresh()
	{
		Pool.ReleaseAll();
		BuildingInfo[] buildingsToDisplay = GetBuildingsToDisplay();
		foreach (BuildingInfo buildingInfo in buildingsToDisplay)
		{
			BuildingBtnEntry buildingBtnEntry = Pool.Get();
			buildingBtnEntry.Setup(buildingInfo);
			buildingBtnEntry.transform.SetAsLastSibling();
		}
		foreach (BuildingBtnEntry activeEntry in Pool.ActiveEntries)
		{
			if (!activeEntry.CostEnough)
			{
				activeEntry.transform.SetAsLastSibling();
			}
		}
	}

	public static BuildingInfo[] GetBuildingsToDisplay()
	{
		BuildingDataCollection instance = BuildingDataCollection.Instance;
		if (instance == null)
		{
			return new BuildingInfo[0];
		}
		List<BuildingInfo> list = new List<BuildingInfo>();
		foreach (BuildingInfo info in instance.Infos)
		{
			if (info.CurrentAmount > 0 || info.RequirementsSatisfied())
			{
				list.Add(info);
			}
		}
		return list.ToArray();
	}
}
