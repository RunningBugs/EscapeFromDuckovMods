using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Saves;
using UnityEngine;

namespace Debugging;

public class InventorySaveLoad : MonoBehaviour
{
	public Inventory inventory;

	public string key = "helloInventory";

	private bool loading;

	public void Save()
	{
		inventory.Save(key);
	}

	public async UniTask Load()
	{
		loading = true;
		await ItemSavesUtilities.LoadInventory(key, inventory);
		loading = false;
		OnLoadFinished();
	}

	private void OnLoadFinished()
	{
	}

	public void BeginLoad()
	{
		Load().Forget();
	}
}
