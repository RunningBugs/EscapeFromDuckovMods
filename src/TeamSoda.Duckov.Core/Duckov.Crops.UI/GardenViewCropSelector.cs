using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Crops.UI;

public class GardenViewCropSelector : MonoBehaviour
{
	[SerializeField]
	private GardenView master;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button btnConfirm;

	[SerializeField]
	private InventoryDisplay playerInventoryDisplay;

	[SerializeField]
	private InventoryDisplay storageInventoryDisplay;

	private void Awake()
	{
		btnConfirm.onClick.AddListener(OnConfirm);
	}

	private void OnConfirm()
	{
		Item selectedItem = ItemUIUtilities.SelectedItem;
		if (selectedItem != null)
		{
			master.SelectSeed(selectedItem.TypeID);
		}
		Hide();
	}

	public void Show()
	{
		fadeGroup.Show();
		if (!(LevelManager.Instance == null))
		{
			ItemUIUtilities.Select(null);
			playerInventoryDisplay.Setup(CharacterMainControl.Main.CharacterItem.Inventory, null, null, movable: false, (Item e) => e != null && CropDatabase.IsSeed(e.TypeID));
			storageInventoryDisplay.Setup(PlayerStorage.Inventory, null, null, movable: false, (Item e) => e != null && CropDatabase.IsSeed(e.TypeID));
		}
	}

	private void OnEnable()
	{
		ItemUIUtilities.OnSelectionChanged += OnSelectionChanged;
	}

	private void OnDisable()
	{
		ItemUIUtilities.OnSelectionChanged -= OnSelectionChanged;
	}

	private void OnSelectionChanged()
	{
	}

	public void Hide()
	{
		fadeGroup.Hide();
	}
}
