using System;
using System.Collections.Generic;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using UnityEngine;

namespace Duckov.Buildings;

public class BuildingEffect : MonoBehaviour
{
	[Serializable]
	public struct ModifierDescription
	{
		public string stat;

		public ModifierType type;

		public float value;
	}

	[SerializeField]
	private string buildingID;

	[SerializeField]
	private List<ModifierDescription> modifierDescriptions = new List<ModifierDescription>();

	private List<Modifier> modifiers = new List<Modifier>();

	private void Awake()
	{
		BuildingManager.OnBuildingListChanged += OnBuildingStatusChanged;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
	}

	private void OnDestroy()
	{
		DisableEffects();
		BuildingManager.OnBuildingListChanged -= OnBuildingStatusChanged;
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		Refresh();
	}

	private void Start()
	{
		Refresh();
	}

	private void OnBuildingStatusChanged()
	{
		Refresh();
	}

	private void Refresh()
	{
		DisableEffects();
		if (IsBuildingConstructed())
		{
			EnableEffects();
		}
	}

	private bool IsBuildingConstructed()
	{
		return BuildingManager.Any(buildingID);
	}

	private void DisableEffects()
	{
		foreach (Modifier modifier in modifiers)
		{
			modifier?.RemoveFromTarget();
		}
		modifiers.Clear();
	}

	private void EnableEffects()
	{
		DisableEffects();
		if (CharacterMainControl.Main == null)
		{
			return;
		}
		foreach (ModifierDescription modifierDescription in modifierDescriptions)
		{
			Apply(modifierDescription);
		}
	}

	private void Apply(ModifierDescription description)
	{
		Stat stat = CharacterMainControl.Main?.CharacterItem?.GetStat(description.stat);
		if (stat != null)
		{
			Modifier modifier = new Modifier(description.type, description.value, this);
			stat.AddModifier(modifier);
			modifiers.Add(modifier);
		}
	}
}
