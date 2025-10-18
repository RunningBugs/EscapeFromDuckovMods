using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatingManager : MonoBehaviour
{
	private static CheatingManager _instance;

	private bool isInvincible;

	private bool typing;

	private int typingID;

	private int lockedItem;

	public static CheatingManager Instance => _instance;

	private void Awake()
	{
		_instance = this;
		CheatMode.Activate();
	}

	private void Update()
	{
		if (!CheatMode.Active || !CharacterMainControl.Main)
		{
			return;
		}
		if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.equalsKey.wasPressedThisFrame)
		{
			ToggleInvincible();
		}
		if (Keyboard.current != null && Keyboard.current.numpadMultiplyKey.wasPressedThisFrame)
		{
			typing = !typing;
			if (typing)
			{
				typingID = 0;
				LogCurrentTypingID();
			}
			else
			{
				LockItem();
			}
		}
		UpdateTyping();
		if (Keyboard.current != null && typing && Keyboard.current.backspaceKey.wasPressedThisFrame && typingID > 0)
		{
			typingID /= 10;
			LogCurrentTypingID();
		}
		if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed && Mouse.current.backButton.wasPressedThisFrame)
		{
			CheatMove();
		}
		if (Keyboard.current != null && Keyboard.current.leftAltKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame)
		{
			SleepView.Instance.Open();
		}
		if (Keyboard.current != null && Keyboard.current.numpadPlusKey.wasPressedThisFrame)
		{
			if (typing)
			{
				LockItem();
				typing = false;
			}
			CreateItem(lockedItem);
		}
		if (Keyboard.current != null && Keyboard.current.numpadMinusKey.wasPressedThisFrame)
		{
			int displayingItemID = ItemHoveringUI.DisplayingItemID;
			if (displayingItemID > 0)
			{
				SetTypedItem(displayingItemID);
				CreateItem(lockedItem);
			}
		}
	}

	private void UpdateTyping()
	{
		if (Keyboard.current != null && Keyboard.current.numpad0Key.wasPressedThisFrame)
		{
			TypeOne(0);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad1Key.wasPressedThisFrame)
		{
			TypeOne(1);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad2Key.wasPressedThisFrame)
		{
			TypeOne(2);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad3Key.wasPressedThisFrame)
		{
			TypeOne(3);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad4Key.wasPressedThisFrame)
		{
			TypeOne(4);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad5Key.wasPressedThisFrame)
		{
			TypeOne(5);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad6Key.wasPressedThisFrame)
		{
			TypeOne(6);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad7Key.wasPressedThisFrame)
		{
			TypeOne(7);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad8Key.wasPressedThisFrame)
		{
			TypeOne(8);
		}
		else if (Keyboard.current != null && Keyboard.current.numpad9Key.wasPressedThisFrame)
		{
			TypeOne(9);
		}
	}

	private void LogCurrentTypingID()
	{
		if (typingID <= 0)
		{
			CharacterMainControl.Main.PopText("_", 999f);
			return;
		}
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(typingID);
		if (metaData.id > 0)
		{
			CharacterMainControl.Main.PopText($" {typingID}_  ({metaData.DisplayName})", 999f);
		}
		else
		{
			CharacterMainControl.Main.PopText($"{typingID}_", 999f);
		}
	}

	private void TypeOne(int i)
	{
		typingID = typingID * 10 + i;
		LogCurrentTypingID();
	}

	private void SetTypedItem(int id)
	{
		typingID = id;
		LockItem();
	}

	private void LockItem()
	{
		typing = false;
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(typingID);
		if (metaData.id <= 0)
		{
			CharacterMainControl.Main.PopText("没有这个物品。", 999f);
			return;
		}
		lockedItem = typingID;
		CharacterMainControl.Main.PopText(metaData.DisplayName + " 已选定", 999f);
	}

	public void CreateItem(int id, int quantity = 1)
	{
		if (ItemAssetsCollection.GetMetaData(id).id <= 0)
		{
			CharacterMainControl.Main.PopText("没有这个物品。", 999f);
		}
		else
		{
			CreateItemAsync(id, quantity).Forget();
		}
	}

	private async UniTaskVoid CreateItemAsync(int id, int quantity = 1)
	{
		while (quantity > 0)
		{
			Item item = await ItemAssetsCollection.InstantiateAsync(id);
			int maxStackCount = item.MaxStackCount;
			if (quantity > maxStackCount)
			{
				item.StackCount = maxStackCount;
				quantity -= maxStackCount;
			}
			else
			{
				item.StackCount = quantity;
				quantity = 0;
			}
			ItemUtilities.SendToPlayer(item);
		}
	}

	private void ToggleTypeing()
	{
	}

	public void ToggleInvincible()
	{
		isInvincible = !isInvincible;
		CharacterMainControl.Main.Health.SetInvincible(isInvincible);
		CharacterMainControl.Main.PopText(isInvincible ? "我无敌了" : "我不无敌了");
	}

	public void CheatMove()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		Ray ray = LevelManager.Instance.GameCamera.renderCamera.ScreenPointToRay(vector);
		LayerMask layerMask = (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask;
		if (Physics.Raycast(ray, out var hitInfo, 100f, layerMask, QueryTriggerInteraction.Ignore))
		{
			CharacterMainControl.Main.SetPosition(hitInfo.point);
		}
	}
}
