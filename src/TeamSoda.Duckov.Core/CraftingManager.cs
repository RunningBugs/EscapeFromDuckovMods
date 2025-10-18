using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using ItemStatsSystem;
using Saves;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
	public static Action<CraftingFormula, Item> OnItemCrafted;

	public static Action<string> OnFormulaUnlocked;

	private const string SaveKey = "Crafting/UnlockedFormulaIDs";

	private List<string> unlockedFormulaIDs = new List<string>();

	private static CraftingFormulaCollection FormulaCollection => CraftingFormulaCollection.Instance;

	public static CraftingManager Instance { get; private set; }

	public static IEnumerable<string> UnlockedFormulaIDs
	{
		get
		{
			if (Instance == null)
			{
				yield break;
			}
			foreach (CraftingFormula entry in CraftingFormulaCollection.Instance.Entries)
			{
				if (IsFormulaUnlocked(entry.id))
				{
					yield return entry.id;
				}
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		Load();
		SavesSystem.OnCollectSaveData += Save;
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		SavesSystem.Save("Crafting/UnlockedFormulaIDs", unlockedFormulaIDs);
	}

	private void Load()
	{
		unlockedFormulaIDs = SavesSystem.Load<List<string>>("Crafting/UnlockedFormulaIDs");
		if (unlockedFormulaIDs == null)
		{
			unlockedFormulaIDs = new List<string>();
		}
		foreach (CraftingFormula entry in FormulaCollection.Entries)
		{
			if (entry.unlockByDefault && !unlockedFormulaIDs.Contains(entry.id))
			{
				unlockedFormulaIDs.Add(entry.id);
			}
		}
		unlockedFormulaIDs.Sort();
	}

	public static void UnlockFormula(string formulaID)
	{
		if (Instance == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(formulaID))
		{
			Debug.LogError("Invalid formula ID");
			return;
		}
		CraftingFormula craftingFormula = FormulaCollection.Entries.FirstOrDefault((CraftingFormula e) => e.id == formulaID);
		if (!craftingFormula.IDValid)
		{
			Debug.LogError("Invalid formula ID: " + formulaID);
		}
		else if (craftingFormula.unlockByDefault)
		{
			Debug.LogError("Formula is unlocked by default: " + formulaID);
		}
		else if (!Instance.unlockedFormulaIDs.Contains(formulaID))
		{
			Instance.unlockedFormulaIDs.Add(formulaID);
			OnFormulaUnlocked?.Invoke(formulaID);
		}
	}

	private async UniTask<List<Item>> Craft(CraftingFormula formula)
	{
		if (!formula.cost.Enough)
		{
			return null;
		}
		Cost cost = new Cost((formula.result.id, formula.result.amount));
		if (!formula.cost.Pay())
		{
			return null;
		}
		List<Item> generatedBuffer = new List<Item>();
		await cost.Return(directToBuffer: false, toPlayerInventory: true, 1, generatedBuffer);
		foreach (Item item in generatedBuffer)
		{
			if (!(item == null))
			{
				OnItemCrafted?.Invoke(formula, item);
			}
		}
		return generatedBuffer;
	}

	public async UniTask<List<Item>> Craft(string id)
	{
		if (!CraftingFormulaCollection.TryGetFormula(id, out var formula))
		{
			return null;
		}
		return await Craft(formula);
	}

	internal static bool IsFormulaUnlocked(string value)
	{
		if (Instance == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		return Instance.unlockedFormulaIDs.Contains(value);
	}

	internal static CraftingFormula GetFormula(string id)
	{
		if (CraftingFormulaCollection.TryGetFormula(id, out var formula))
		{
			return formula;
		}
		return default(CraftingFormula);
	}
}
