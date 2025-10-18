using System.Collections.Generic;
using System.Linq;
using Duckov.UI.Animations;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemOperationMenu : ManagedUIElement
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI weightText;

	[SerializeField]
	private string weightTextFormat = "{0:0.#}kg";

	[SerializeField]
	private Button btn_Use;

	[SerializeField]
	private Button btn_Split;

	[SerializeField]
	private Button btn_Dump;

	[SerializeField]
	private Button btn_Equip;

	[SerializeField]
	private Button btn_Modify;

	[SerializeField]
	private Button btn_Unload;

	[SerializeField]
	private Button btn_Wishlist;

	[SerializeField]
	private bool alwaysModifyable;

	private View targetView;

	private ItemDisplay TargetDisplay;

	private Item displayingItem;

	public static ItemOperationMenu Instance { get; private set; }

	private Item TargetItem => TargetDisplay?.Target;

	private bool Usable => TargetItem.UsageUtilities != null;

	private bool UseButtonInteractable
	{
		get
		{
			if ((bool)TargetItem)
			{
				return TargetItem.IsUsable(LevelManager.Instance?.MainCharacter);
			}
			return false;
		}
	}

	private bool Splittable
	{
		get
		{
			CharacterMainControl main = CharacterMainControl.Main;
			if ((object)main != null && main.CharacterItem.Inventory.GetFirstEmptyPosition() < 0)
			{
				return false;
			}
			if ((bool)TargetItem && TargetItem.Stackable)
			{
				return TargetItem.StackCount > 1;
			}
			return false;
		}
	}

	private bool Dumpable
	{
		get
		{
			if (!TargetItem.CanDrop)
			{
				return false;
			}
			Item item = LevelManager.Instance?.MainCharacter?.CharacterItem;
			if (TargetItem.GetRoot() == item)
			{
				return true;
			}
			return false;
		}
	}

	private bool Equipable
	{
		get
		{
			if (TargetItem == null)
			{
				return false;
			}
			if (TargetItem.PluggedIntoSlot != null)
			{
				return false;
			}
			bool? flag = LevelManager.Instance?.MainCharacter?.CharacterItem?.Slots.Any((Slot e) => e.CanPlug(TargetItem));
			if (!flag.HasValue)
			{
				return false;
			}
			return flag.Value;
		}
	}

	private bool Modifyable
	{
		get
		{
			if (alwaysModifyable)
			{
				return true;
			}
			return false;
		}
	}

	private bool Unloadable
	{
		get
		{
			if (TargetItem == null)
			{
				return false;
			}
			if (!TargetItem.GetComponent<ItemSetting_Gun>())
			{
				return false;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		Initialize();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void Update()
	{
		if (!fadeGroup.IsHidingInProgress && fadeGroup.IsShown && (Mouse.current.leftButton.wasReleasedThisFrame || targetView == null || !targetView.open || (!fadeGroup.IsShowingInProgress && Mouse.current.rightButton.wasReleasedThisFrame)))
		{
			Close();
		}
	}

	private void Initialize()
	{
		btn_Use.onClick.AddListener(Use);
		btn_Split.onClick.AddListener(Split);
		btn_Dump.onClick.AddListener(Dump);
		btn_Equip.onClick.AddListener(Equip);
		btn_Modify.onClick.AddListener(Modify);
		btn_Unload.onClick.AddListener(Unload);
		btn_Wishlist.onClick.AddListener(Wishlist);
	}

	private void Wishlist()
	{
		if (!(TargetItem == null))
		{
			int typeID = TargetItem.TypeID;
			if (ItemWishlist.GetWishlistInfo(typeID).isManuallyWishlisted)
			{
				ItemWishlist.RemoveFromWishlist(typeID);
			}
			else
			{
				ItemWishlist.AddToWishList(TargetItem.TypeID);
			}
		}
	}

	private void Use()
	{
		LevelManager.Instance?.MainCharacter?.UseItem(TargetItem);
		InventoryView.Hide();
		Close();
	}

	private void Split()
	{
		SplitDialogue.SetupAndShow(TargetItem);
		Close();
	}

	private void Dump()
	{
		if ((bool)LevelManager.Instance?.MainCharacter)
		{
			TargetItem.Drop(LevelManager.Instance.MainCharacter, createRigidbody: true);
		}
		Close();
	}

	private void Modify()
	{
		if (TargetItem == null)
		{
			return;
		}
		ItemCustomizeView instance = ItemCustomizeView.Instance;
		if (!(instance == null))
		{
			List<Inventory> list = new List<Inventory>();
			Inventory inventory = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
			if ((bool)inventory)
			{
				list.Add(inventory);
			}
			instance.Setup(TargetItem, list);
			instance.Open();
			Close();
		}
	}

	private void Equip()
	{
		LevelManager.Instance?.MainCharacter?.CharacterItem?.TryPlug(TargetItem);
		Close();
	}

	private void Unload()
	{
		ItemSetting_Gun itemSetting_Gun = TargetItem?.GetComponent<ItemSetting_Gun>();
		if (!(itemSetting_Gun == null))
		{
			AudioManager.Post("SFX/Combat/Gun/unload");
			itemSetting_Gun.TakeOutAllBullets();
		}
	}

	protected override void OnOpen()
	{
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		fadeGroup.Hide();
		displayingItem = null;
	}

	public static void Show(ItemDisplay id)
	{
		if (!(Instance == null))
		{
			Instance.MShow(id);
		}
	}

	private void MShow(ItemDisplay targetDisplay)
	{
		if (!(targetDisplay == null))
		{
			TargetDisplay = targetDisplay;
			targetView = targetDisplay.GetComponentInParent<View>();
			Setup();
			Open();
		}
	}

	private void Setup()
	{
		if (!(TargetItem == null))
		{
			displayingItem = TargetItem;
			icon.sprite = TargetItem.Icon;
			nameText.text = TargetItem.DisplayName;
			btn_Use.gameObject.SetActive(Usable);
			btn_Use.interactable = UseButtonInteractable;
			btn_Split.gameObject.SetActive(Splittable);
			btn_Dump.gameObject.SetActive(Dumpable);
			btn_Equip.gameObject.SetActive(Equipable);
			btn_Modify.gameObject.SetActive(Modifyable);
			btn_Unload.gameObject.SetActive(Unloadable);
			RefreshWeightText();
			RefreshPosition();
		}
	}

	private void RefreshPosition()
	{
		RectTransform obj = TargetDisplay.transform as RectTransform;
		Rect rect = obj.rect;
		Vector2 min = rect.min;
		Vector2 max = rect.max;
		Vector3 point = obj.localToWorldMatrix.MultiplyPoint(min);
		Vector3 point2 = obj.localToWorldMatrix.MultiplyPoint(max);
		Vector3 vector = rectTransform.worldToLocalMatrix.MultiplyPoint(point);
		Vector3 vector2 = rectTransform.worldToLocalMatrix.MultiplyPoint(point2);
		Vector2[] array = new Vector2[4]
		{
			new Vector2(vector.x, vector.y),
			new Vector2(vector.x, vector2.y),
			new Vector2(vector2.x, vector.y),
			new Vector2(vector2.x, vector2.y)
		};
		int num = 0;
		float num2 = float.MaxValue;
		Vector2 center = rectTransform.rect.center;
		for (int i = 0; i < array.Length; i++)
		{
			float sqrMagnitude = (array[i] - center).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num = i;
				num2 = sqrMagnitude;
			}
		}
		bool flag = (num & 2) > 0;
		bool flag2 = (num & 1) > 0;
		float x = (flag ? vector2.x : vector.x);
		float y = (flag2 ? vector.y : vector2.y);
		contentRectTransform.pivot = new Vector2((!flag) ? 1 : 0, (!flag2) ? 1 : 0);
		contentRectTransform.localPosition = new Vector2(x, y);
	}

	private void RefreshWeightText()
	{
		if (!(displayingItem == null))
		{
			weightText.text = string.Format(weightTextFormat, displayingItem.TotalWeight);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Close();
	}
}
