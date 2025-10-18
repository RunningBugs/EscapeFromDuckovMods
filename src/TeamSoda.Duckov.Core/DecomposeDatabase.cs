using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

[CreateAssetMenu]
public class DecomposeDatabase : ScriptableObject
{
	[SerializeField]
	private DecomposeFormula[] entries;

	private Dictionary<int, DecomposeFormula> _dic;

	public static DecomposeDatabase Instance => GameplayDataSettings.DecomposeDatabase;

	private Dictionary<int, DecomposeFormula> Dic
	{
		get
		{
			if (_dic == null)
			{
				RebuildDictionary();
			}
			return _dic;
		}
	}

	public void RebuildDictionary()
	{
		_dic = new Dictionary<int, DecomposeFormula>();
		DecomposeFormula[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			DecomposeFormula value = array[i];
			_dic[value.item] = value;
		}
	}

	public DecomposeFormula GetFormula(int itemTypeID)
	{
		if (!Dic.TryGetValue(itemTypeID, out var value))
		{
			return default(DecomposeFormula);
		}
		return value;
	}

	public static async UniTask<bool> Decompose(Item item, int count)
	{
		if (Instance == null)
		{
			return false;
		}
		DecomposeFormula formula = Instance.GetFormula(item.TypeID);
		if (!formula.valid)
		{
			return false;
		}
		Item splitedItem = item;
		if (item.Stackable)
		{
			int stackCount = item.StackCount;
			if (stackCount <= count)
			{
				count = stackCount;
			}
			else
			{
				splitedItem = await item.Split(count);
			}
		}
		Cost result = formula.result;
		await result.Return(directToBuffer: false, toPlayerInventory: true, (!splitedItem.Stackable) ? 1 : splitedItem.StackCount);
		splitedItem.Detach();
		splitedItem.DestroyTree();
		return true;
	}

	public static bool CanDecompose(int itemTypeID)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.GetFormula(itemTypeID).valid;
	}

	public static bool CanDecompose(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return CanDecompose(item.TypeID);
	}

	public static DecomposeFormula GetDecomposeFormula(int itemTypeID)
	{
		if (Instance == null)
		{
			return default(DecomposeFormula);
		}
		return Instance.GetFormula(itemTypeID);
	}

	public void SetData(List<DecomposeFormula> formulas)
	{
		entries = formulas.ToArray();
	}
}
