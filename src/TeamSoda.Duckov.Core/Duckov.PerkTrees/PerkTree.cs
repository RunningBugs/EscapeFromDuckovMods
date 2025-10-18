using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.PerkTrees;

public class PerkTree : MonoBehaviour, ISaveDataProvider
{
	[Serializable]
	private class SaveData
	{
		[Serializable]
		public class Entry
		{
			public string perkName;

			public bool unlocking;

			public long unlockingBeginTime;

			public bool unlocked;

			public Entry(Perk perk)
			{
				perkName = perk.name;
				unlocked = perk.Unlocked;
				unlocking = perk.Unlocking;
				unlockingBeginTime = perk.unlockingBeginTimeRaw;
			}
		}

		public List<Entry> entries;

		public SaveData(PerkTree perkTree)
		{
			entries = new List<Entry>();
			for (int i = 0; i < perkTree.perks.Count; i++)
			{
				Perk perk = perkTree.perks[i];
				if (!(perk == null))
				{
					entries.Add(new Entry(perk));
				}
			}
		}
	}

	[SerializeField]
	private string perkTreeID = "DefaultPerkTree";

	[SerializeField]
	private bool horizontal;

	[SerializeField]
	private PerkTreeRelationGraphOwner relationGraphOwner;

	[SerializeField]
	internal List<Perk> perks = new List<Perk>();

	private ReadOnlyCollection<Perk> perks_ReadOnly;

	private bool loaded;

	[LocalizationKey("Perks")]
	private string perkTreeName
	{
		get
		{
			return displayNameKey;
		}
		set
		{
		}
	}

	public string ID => perkTreeID;

	private string displayNameKey => "PerkTree_" + ID;

	public string DisplayName => displayNameKey.ToPlainText();

	public bool Horizontal => horizontal;

	public ReadOnlyCollection<Perk> Perks
	{
		get
		{
			if (perks_ReadOnly == null)
			{
				perks_ReadOnly = perks.AsReadOnly();
			}
			return perks_ReadOnly;
		}
	}

	public PerkTreeRelationGraphOwner RelationGraphOwner => relationGraphOwner;

	private string SaveKey => "PerkTree_" + perkTreeID;

	public event Action<PerkTree> onPerkTreeStatusChanged;

	private void Awake()
	{
		Load();
		SavesSystem.OnCollectSaveData += Save;
		SavesSystem.OnSetFile += Load;
	}

	private void Start()
	{
		foreach (Perk perk in perks)
		{
			if (!(perk == null) && perk.DefaultUnlocked)
			{
				perk.ForceUnlock();
			}
		}
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
		SavesSystem.OnSetFile -= Load;
	}

	public object GenerateSaveData()
	{
		return new SaveData(this);
	}

	public void SetupSaveData(object data)
	{
		foreach (Perk perk in perks)
		{
			perk.Unlocked = false;
		}
		if (!(data is SaveData saveData))
		{
			return;
		}
		foreach (Perk cur in perks)
		{
			if (!(cur == null))
			{
				SaveData.Entry entry = saveData.entries.Find((SaveData.Entry e) => e != null && e.perkName == cur.name);
				if (entry != null)
				{
					cur.Unlocked = entry.unlocked;
					cur.unlocking = entry.unlocking;
					cur.unlockingBeginTimeRaw = entry.unlockingBeginTime;
				}
			}
		}
	}

	public void Save()
	{
		SavesSystem.Save(SaveKey, GenerateSaveData() as SaveData);
	}

	public void Load()
	{
		if (SavesSystem.KeyExisits(SaveKey))
		{
			SaveData data = SavesSystem.Load<SaveData>(SaveKey);
			SetupSaveData(data);
			loaded = true;
		}
	}

	public void ReapplyPerks()
	{
		foreach (Perk perk in perks)
		{
			perk.Unlocked = false;
		}
		foreach (Perk perk2 in perks)
		{
			perk2.Unlocked = perk2.Unlocked;
		}
	}

	internal bool AreAllParentsUnlocked(Perk perk)
	{
		PerkRelationNode relatedNode = RelationGraphOwner.GetRelatedNode(perk);
		if (relatedNode == null)
		{
			return false;
		}
		foreach (PerkRelationNode incomingNode in relationGraphOwner.RelationGraph.GetIncomingNodes(relatedNode))
		{
			Perk relatedNode2 = incomingNode.relatedNode;
			if (!(relatedNode2 == null) && !relatedNode2.Unlocked)
			{
				return false;
			}
		}
		return true;
	}

	internal void NotifyChildStateChanged(Perk perk)
	{
		PerkRelationNode relatedNode = RelationGraphOwner.GetRelatedNode(perk);
		if (relatedNode == null)
		{
			return;
		}
		foreach (PerkRelationNode outgoingNode in relationGraphOwner.RelationGraph.GetOutgoingNodes(relatedNode))
		{
			outgoingNode.NotifyIncomingStateChanged();
		}
		this.onPerkTreeStatusChanged?.Invoke(this);
	}

	private void Collect()
	{
		perks.Clear();
		Perk[] componentsInChildren = base.transform.GetComponentsInChildren<Perk>();
		Perk[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Master = this;
		}
		perks.AddRange(componentsInChildren);
	}
}
