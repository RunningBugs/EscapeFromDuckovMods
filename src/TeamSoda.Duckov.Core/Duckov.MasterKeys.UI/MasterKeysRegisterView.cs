using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.MasterKeys.UI;

public class MasterKeysRegisterView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private InventoryDisplay inventoryDisplay;

	[SerializeField]
	private InventoryDisplay playerStorageInventoryDisplay;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private Button submitButton;

	[SerializeField]
	private Item keySlotItem;

	[SerializeField]
	private string keySlotKey = "Key";

	[SerializeField]
	private SlotDisplay registerSlotDisplay;

	[SerializeField]
	private GameObject recordExistsIndicator;

	[SerializeField]
	private FadeGroup succeedIndicator;

	[SerializeField]
	private float successIndicationTime = 1.5f;

	private string sfx_Register = "UI/register";

	public static MasterKeysRegisterView Instance => View.GetViewInstance<MasterKeysRegisterView>();

	private Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	private Slot KeySlot
	{
		get
		{
			if (keySlotItem == null)
			{
				return null;
			}
			if (keySlotItem.Slots == null)
			{
				return null;
			}
			return keySlotItem.Slots[keySlotKey];
		}
	}

	protected override void Awake()
	{
		base.Awake();
		submitButton.onClick.AddListener(OnSubmitButtonClicked);
		succeedIndicator.SkipHide();
		detailsFadeGroup.SkipHide();
		registerSlotDisplay.onSlotDisplayDoubleClicked += OnSlotDoubleClicked;
		inventoryDisplay.onDisplayDoubleClicked += OnInventoryItemDoubleClicked;
		playerStorageInventoryDisplay.onDisplayDoubleClicked += OnInventoryItemDoubleClicked;
	}

	private void OnInventoryItemDoubleClicked(InventoryDisplay display, InventoryEntry entry, PointerEventData data)
	{
		if (!entry.Editable)
		{
			return;
		}
		Item item = entry.Item;
		if (!(item == null) && KeySlot.CanPlug(item))
		{
			item.Detach();
			KeySlot.Plug(item, out var unpluggedItem);
			if (unpluggedItem != null)
			{
				ItemUtilities.SendToPlayer(unpluggedItem);
			}
		}
	}

	private void OnSlotDoubleClicked(SlotDisplay display)
	{
		Item item = display.GetItem();
		if (!(item == null))
		{
			item.Detach();
			ItemUtilities.SendToPlayer(item);
		}
	}

	private void OnSubmitButtonClicked()
	{
		if (KeySlot != null && KeySlot.Content != null && MasterKeysManager.SubmitAndActivate(KeySlot.Content))
		{
			IndicateSuccess();
		}
	}

	private void IndicateSuccess()
	{
		SuccessIndicationTask().Forget();
	}

	private async UniTask SuccessIndicationTask()
	{
		succeedIndicator.Show();
		AudioManager.Post(sfx_Register);
		await UniTask.WaitForSeconds(successIndicationTime, ignoreTimeScale: true);
		succeedIndicator.Hide();
	}

	private void HideSuccessIndication()
	{
		succeedIndicator.Hide();
	}

	private bool EntryFunc_ShouldHighligh(Item e)
	{
		if (e == null)
		{
			return false;
		}
		if (!KeySlot.CanPlug(e))
		{
			return false;
		}
		if (MasterKeysManager.IsActive(e.TypeID))
		{
			return false;
		}
		return true;
	}

	private bool EntryFunc_CanOperate(Item e)
	{
		if (e == null)
		{
			return true;
		}
		return KeySlot.CanPlug(e);
	}

	protected override void OnOpen()
	{
		UnregisterEvents();
		base.OnOpen();
		Item characterItem = CharacterItem;
		if (characterItem == null)
		{
			Debug.LogError("物品栏开启失败，角色物体不存在");
			Close();
			return;
		}
		base.gameObject.SetActive(value: true);
		inventoryDisplay.ShowOperationButtons = false;
		inventoryDisplay.Setup(characterItem.Inventory, EntryFunc_ShouldHighligh, EntryFunc_CanOperate);
		if (PlayerStorage.Inventory != null)
		{
			playerStorageInventoryDisplay.ShowOperationButtons = false;
			playerStorageInventoryDisplay.gameObject.SetActive(value: true);
			playerStorageInventoryDisplay.Setup(PlayerStorage.Inventory, EntryFunc_ShouldHighligh, EntryFunc_CanOperate);
		}
		else
		{
			playerStorageInventoryDisplay.gameObject.SetActive(value: false);
		}
		registerSlotDisplay.Setup(KeySlot);
		RefreshRecordExistsIndicator();
		RegisterEvents();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		UnregisterEvents();
		base.OnClose();
		fadeGroup.Hide();
		detailsFadeGroup.Hide();
		if (KeySlot != null && KeySlot.Content != null)
		{
			Item content = KeySlot.Content;
			content.Detach();
			ItemUtilities.SendToPlayerCharacterInventory(content);
		}
	}

	private void RegisterEvents()
	{
		KeySlot.onSlotContentChanged += OnSlotContentChanged;
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
	}

	private void UnregisterEvents()
	{
		KeySlot.onSlotContentChanged -= OnSlotContentChanged;
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
	}

	private void OnSlotContentChanged(Slot slot)
	{
		RefreshRecordExistsIndicator();
		HideSuccessIndication();
		if (slot?.Content != null)
		{
			AudioManager.PlayPutItemSFX(slot.Content);
		}
	}

	private void RefreshRecordExistsIndicator()
	{
		Item content = KeySlot.Content;
		if (content == null)
		{
			recordExistsIndicator.SetActive(value: false);
			return;
		}
		bool active = MasterKeysManager.IsActive(content.TypeID);
		recordExistsIndicator.SetActive(active);
	}

	private void OnItemSelectionChanged()
	{
		if (ItemUIUtilities.SelectedItem != null)
		{
			detailsDisplay.Setup(ItemUIUtilities.SelectedItem);
			detailsFadeGroup.Show();
		}
		else
		{
			detailsFadeGroup.Hide();
		}
	}

	public static void Show()
	{
		if (!(Instance == null))
		{
			Instance.Open();
		}
	}
}
