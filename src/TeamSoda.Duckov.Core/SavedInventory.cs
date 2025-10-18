using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Saves;
using UnityEngine;

public class SavedInventory : MonoBehaviour
{
	[SerializeField]
	private Inventory inventory;

	[SerializeField]
	private string key = "DefaultSavedInventory";

	private static Dictionary<string, SavedInventory> activeInventories = new Dictionary<string, SavedInventory>();

	private bool registered;

	private void Awake()
	{
		if (inventory == null)
		{
			inventory = GetComponent<Inventory>();
		}
		Register();
	}

	private void Start()
	{
		if (registered)
		{
			Load();
		}
	}

	private void OnDestroy()
	{
		Unregsister();
	}

	private void Register()
	{
		if (activeInventories.TryGetValue(key, out var _))
		{
			Debug.LogError("存在多个带有相同Key的Saved Inventory: " + key, base.gameObject);
			return;
		}
		SavesSystem.OnCollectSaveData += Save;
		registered = true;
	}

	private void Unregsister()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		inventory.Save(key);
	}

	private void Load()
	{
		if (inventory.Loading)
		{
			Debug.LogError("Inventory is already loading.", base.gameObject);
		}
		else
		{
			ItemSavesUtilities.LoadInventory(key, inventory).Forget();
		}
	}
}
