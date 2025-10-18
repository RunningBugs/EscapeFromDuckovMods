using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Saves;
using UnityEngine;

public class PetProxy : MonoBehaviour
{
	[SerializeField]
	private Inventory inventory;

	private float checkTimer = 0.02f;

	public static PetProxy Instance
	{
		get
		{
			if (LevelManager.Instance == null)
			{
				return null;
			}
			return LevelManager.Instance.PetProxy;
		}
	}

	public static Inventory PetInventory
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.Inventory;
		}
	}

	public Inventory Inventory => inventory;

	private void Start()
	{
		SavesSystem.OnCollectSaveData += OnCollectSaveData;
		ItemSavesUtilities.LoadInventory("Inventory_Safe", inventory).Forget();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= OnCollectSaveData;
	}

	private void OnCollectSaveData()
	{
		inventory.Save("Inventory_Safe");
	}

	public void DestroyItemInBase()
	{
		if (!Inventory)
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (Item item in Inventory)
		{
			list.Add(item);
		}
		foreach (Item item2 in list)
		{
			if (item2.Tags.Contains("DestroyInBase"))
			{
				item2.DestroyTree();
			}
		}
	}

	private void Update()
	{
		if (!LevelManager.LevelInited || LevelManager.Instance.PetCharacter == null)
		{
			return;
		}
		base.transform.position = LevelManager.Instance.PetCharacter.transform.position;
		if (checkTimer > 0f)
		{
			checkTimer -= Time.unscaledDeltaTime;
			return;
		}
		if (CharacterMainControl.Main.PetCapcity != inventory.Capacity)
		{
			inventory.SetCapacity(CharacterMainControl.Main.PetCapcity);
		}
		checkTimer = 1f;
	}
}
