using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class FormulasRegisterView : View
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
	private Tag formulaTag;

	[SerializeField]
	private Item keySlotItem;

	[SerializeField]
	private string slotKey = "SubmitItem";

	[SerializeField]
	private SlotDisplay registerSlotDisplay;

	[SerializeField]
	private GameObject recordExistsIndicator;

	[SerializeField]
	private FadeGroup succeedIndicator;

	[SerializeField]
	private float successIndicationTime = 1.5f;

	private string sfx_Register = "UI/register";

	[LocalizationKey("Default")]
	[SerializeField]
	private string formulaUnlockedFormatKey = "UI_Formulas_RegisterSucceedFormat";

	public static FormulasRegisterView Instance => View.GetViewInstance<FormulasRegisterView>();

	private string FormulaUnlockedNotificationFormat => formulaUnlockedFormatKey.ToPlainText();

	private Item CharacterItem => LevelManager.Instance?.MainCharacter?.CharacterItem;

	private Slot SubmitItemSlot
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
			return keySlotItem.Slots[slotKey];
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
		if (!(item == null) && SubmitItemSlot.CanPlug(item))
		{
			item.Detach();
			SubmitItemSlot.Plug(item, out var unpluggedItem);
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
		if (SubmitItemSlot == null || !(SubmitItemSlot.Content != null))
		{
			return;
		}
		Item content = SubmitItemSlot.Content;
		string formulaID = GetFormulaID(content);
		if (!string.IsNullOrEmpty(formulaID) && !CraftingManager.IsFormulaUnlocked(formulaID))
		{
			CraftingManager.UnlockFormula(formulaID);
			CraftingFormula formula = CraftingManager.GetFormula(formulaID);
			if (formula.IDValid)
			{
				ItemMetaData metaData = ItemAssetsCollection.GetMetaData(formula.result.id);
				string mainText = FormulaUnlockedNotificationFormat.Format(new
				{
					itemDisplayName = metaData.DisplayName
				});
				Sprite icon = metaData.icon;
				StrongNotification.Push(new StrongNotificationContent(mainText, "", icon));
			}
			content.Detach();
			content.DestroyTreeImmediate();
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
		if (!SubmitItemSlot.CanPlug(e))
		{
			return false;
		}
		if (CraftingManager.IsFormulaUnlocked(GetFormulaID(e)))
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
		return SubmitItemSlot.CanPlug(e);
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
		inventoryDisplay.Setup(characterItem.Inventory, EntryFunc_ShouldHighligh, EntryFunc_CanOperate, movable: true);
		if (PlayerStorage.Inventory != null)
		{
			playerStorageInventoryDisplay.ShowOperationButtons = false;
			playerStorageInventoryDisplay.gameObject.SetActive(value: true);
			playerStorageInventoryDisplay.Setup(PlayerStorage.Inventory, EntryFunc_ShouldHighligh, EntryFunc_CanOperate, movable: true);
		}
		else
		{
			playerStorageInventoryDisplay.gameObject.SetActive(value: false);
		}
		registerSlotDisplay.Setup(SubmitItemSlot);
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
		if (SubmitItemSlot != null && SubmitItemSlot.Content != null)
		{
			Item content = SubmitItemSlot.Content;
			content.Detach();
			ItemUtilities.SendToPlayerCharacterInventory(content);
		}
	}

	private void RegisterEvents()
	{
		SubmitItemSlot.onSlotContentChanged += OnSlotContentChanged;
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
	}

	private void UnregisterEvents()
	{
		SubmitItemSlot.onSlotContentChanged -= OnSlotContentChanged;
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
		Item content = SubmitItemSlot.Content;
		if (content == null)
		{
			recordExistsIndicator.SetActive(value: false);
			return;
		}
		bool active = CraftingManager.IsFormulaUnlocked(GetFormulaID(content));
		recordExistsIndicator.SetActive(active);
	}

	private bool IsFormulaItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.GetComponent<ItemSetting_Formula>() != null;
	}

	public static string GetFormulaID(Item item)
	{
		if (item == null)
		{
			return null;
		}
		ItemSetting_Formula component = item.GetComponent<ItemSetting_Formula>();
		if (component == null)
		{
			return null;
		}
		return component.formulaID;
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

	public static void Show(ICollection<Tag> requireTags = null)
	{
		if (!(Instance == null))
		{
			SetupTags(requireTags);
			Instance.Open();
		}
	}

	private static void SetupTags(ICollection<Tag> requireTags = null)
	{
		if (Instance == null)
		{
			return;
		}
		Slot submitItemSlot = Instance.SubmitItemSlot;
		if (submitItemSlot != null)
		{
			submitItemSlot.requireTags.Clear();
			submitItemSlot.requireTags.Add(Instance.formulaTag);
			if (requireTags != null)
			{
				submitItemSlot.requireTags.AddRange(requireTags);
			}
		}
	}
}
