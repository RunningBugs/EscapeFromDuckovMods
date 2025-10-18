using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Duckov.Utilities;
using UnityEngine;

[CreateAssetMenu]
public class CraftingFormulaCollection : ScriptableObject
{
	[SerializeField]
	private List<CraftingFormula> list;

	private ReadOnlyCollection<CraftingFormula> _entries_ReadOnly;

	public static CraftingFormulaCollection Instance => GameplayDataSettings.CraftingFormulas;

	public ReadOnlyCollection<CraftingFormula> Entries
	{
		get
		{
			if (_entries_ReadOnly == null)
			{
				_entries_ReadOnly = new ReadOnlyCollection<CraftingFormula>(list);
			}
			return _entries_ReadOnly;
		}
	}

	public static bool TryGetFormula(string id, out CraftingFormula formula)
	{
		if (!(Instance == null))
		{
			CraftingFormula craftingFormula = Instance.list.FirstOrDefault((CraftingFormula e) => e.id == id);
			if (!string.IsNullOrEmpty(craftingFormula.id))
			{
				formula = craftingFormula;
				return true;
			}
		}
		formula = default(CraftingFormula);
		return false;
	}
}
