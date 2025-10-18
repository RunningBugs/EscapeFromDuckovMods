using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using Unity.VisualScripting;
using UnityEngine;

public class ItemPickerDebug : MonoBehaviour
{
	public void PickPlayerInventoryAndLog()
	{
		Pick().Forget();
	}

	private async UniTask Pick()
	{
		Item item = await ItemPicker.Pick((LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory).AsReadOnlyList());
		if (item == null)
		{
			Debug.Log("Nothing is selected");
		}
		else
		{
			Debug.Log(item.DisplayName);
		}
	}
}
