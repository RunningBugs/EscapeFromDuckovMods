using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Buffs;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;

public class ItemSetting_Gun : ItemSettingBase
{
	public enum TriggerModes
	{
		auto,
		semi,
		bolt
	}

	public enum ReloadModes
	{
		fullMag,
		singleBullet
	}

	private int targetBulletID = -1;

	public ADSAimMarker adsAimMarker;

	public GameObject muzzleFxPfb;

	public Projectile bulletPfb;

	public string shootKey = "Default";

	public string reloadKey = "Default";

	private int bulletCountHash = "BulletCount".GetHashCode();

	private int _bulletCountCache = -1;

	private static int CapacityHash = "Capacity".GetHashCode();

	private bool loadingBullets;

	private bool loadBulletsSuccess;

	private int caliberHash = "Caliber".GetHashCode();

	public TriggerModes triggerMode;

	public ReloadModes reloadMode;

	public bool autoReload;

	public ElementTypes element;

	public Buff buff;

	private Item preferedBulletsToLoad;

	public int TargetBulletID => targetBulletID;

	public string CurrentBulletName
	{
		get
		{
			if (TargetBulletID < 0)
			{
				return "UI_Bullet_NotAssigned".ToPlainText();
			}
			return ItemAssetsCollection.GetMetaData(TargetBulletID).DisplayName;
		}
	}

	public int BulletCount
	{
		get
		{
			if (loadingBullets)
			{
				return -1;
			}
			if (bulletCount < 0)
			{
				bulletCount = GetBulletCount();
			}
			return bulletCount;
		}
	}

	private int bulletCount
	{
		get
		{
			return _bulletCountCache;
		}
		set
		{
			_bulletCountCache = value;
			base.Item.Variables.SetInt(bulletCountHash, _bulletCountCache);
		}
	}

	public int Capacity => Mathf.RoundToInt(base.Item.GetStatValue(CapacityHash));

	public bool LoadingBullets => loadingBullets;

	public bool LoadBulletsSuccess => loadBulletsSuccess;

	public Item PreferdBulletsToLoad
	{
		get
		{
			return preferedBulletsToLoad;
		}
		set
		{
			preferedBulletsToLoad = value;
		}
	}

	public void SetTargetBulletType(Item bulletItem)
	{
		if (bulletItem != null)
		{
			SetTargetBulletType(bulletItem.TypeID);
		}
		else
		{
			SetTargetBulletType(-1);
		}
	}

	public void SetTargetBulletType(int typeID)
	{
		bool flag = false;
		if (TargetBulletID != typeID && TargetBulletID != -1)
		{
			flag = true;
		}
		targetBulletID = typeID;
		if (flag)
		{
			TakeOutAllBullets();
		}
	}

	public override void Start()
	{
		base.Start();
		AutoSetTypeInInventory(null);
	}

	public void UseABullet()
	{
		if (LevelManager.Instance.IsBaseLevel)
		{
			return;
		}
		foreach (Item item in base.Item.Inventory)
		{
			if (!(item == null) && item.StackCount >= 1)
			{
				item.StackCount--;
				break;
			}
		}
		bulletCount--;
	}

	public bool IsFull()
	{
		return bulletCount >= Capacity;
	}

	public bool IsValidBullet(Item newBulletItem)
	{
		if (newBulletItem == null)
		{
			return false;
		}
		if (!newBulletItem.Tags.Contains(GameplayDataSettings.Tags.Bullet))
		{
			return false;
		}
		Item currentLoadedBullet = GetCurrentLoadedBullet();
		if (currentLoadedBullet != null && currentLoadedBullet.TypeID == newBulletItem.TypeID && bulletCount >= Capacity)
		{
			return false;
		}
		string text = newBulletItem.Constants.GetString(caliberHash);
		string text2 = base.Item.Constants.GetString(caliberHash);
		if (text != text2)
		{
			return false;
		}
		return true;
	}

	public bool LoadSpecificBullet(Item newBulletItem)
	{
		Debug.Log("尝试安装指定弹药");
		if (!IsValidBullet(newBulletItem))
		{
			return false;
		}
		Debug.Log("指定弹药判定通过");
		ItemAgent_Gun itemAgent_Gun = base.Item.ActiveAgent as ItemAgent_Gun;
		if (itemAgent_Gun != null)
		{
			if (itemAgent_Gun.Holder != null)
			{
				bool flag = itemAgent_Gun.CharacterReload(newBulletItem);
				Debug.Log($"角色reload:{flag}");
				return true;
			}
			return false;
		}
		Inventory inventory = base.Item.InInventory;
		if (inventory != null && inventory != CharacterMainControl.Main.CharacterItem.Inventory)
		{
			inventory = null;
		}
		preferedBulletsToLoad = newBulletItem;
		LoadBulletsFromInventory(inventory).Forget();
		return true;
	}

	public async UniTaskVoid LoadBulletsFromInventory(Inventory inventory)
	{
		if (loadingBullets)
		{
			return;
		}
		loadingBullets = true;
		loadBulletsSuccess = false;
		Item item = preferedBulletsToLoad;
		preferedBulletsToLoad = null;
		if (item != null)
		{
			SetTargetBulletType(item);
		}
		bulletCount = GetBulletCount();
		if (bulletCount > 0)
		{
			Item currentLoadedBullet = GetCurrentLoadedBullet();
			if ((bool)currentLoadedBullet && currentLoadedBullet.TypeID != TargetBulletID)
			{
				TakeOutAllBullets();
			}
		}
		int capacity = Capacity;
		int needCount = capacity - bulletCount;
		if (needCount < 0)
		{
			loadBulletsSuccess = false;
			loadingBullets = false;
			return;
		}
		if (reloadMode == ReloadModes.singleBullet)
		{
			needCount = 1;
		}
		List<Item> bullets = new List<Item>();
		if (item != null)
		{
			if (item.StackCount > needCount)
			{
				bullets.Add(await item.Split(needCount));
				needCount = 0;
			}
			else
			{
				item.Detach();
				bullets.Add(item);
				needCount -= item.StackCount;
			}
		}
		if (needCount > 0 && inventory != null)
		{
			CharacterMainControl characterMainControl = base.Item.GetCharacterMainControl();
			if (characterMainControl == LevelManager.Instance.MainCharacter)
			{
				bullets.AddRange(await inventory.GetItemsOfAmount(targetBulletID, needCount));
			}
			else if (characterMainControl != null)
			{
				Item item2 = await ItemAssetsCollection.InstantiateAsync(targetBulletID);
				item2.StackCount = needCount;
				bullets.Add(item2);
			}
		}
		if (bullets.Count <= 0)
		{
			loadBulletsSuccess = false;
			loadingBullets = false;
			return;
		}
		foreach (Item item3 in bullets)
		{
			if (item3 == null)
			{
				loadBulletsSuccess = false;
				continue;
			}
			item3.Inspected = true;
			base.Item.Inventory.AddAndMerge(item3);
		}
		bulletCount = GetBulletCount();
		loadBulletsSuccess = true;
		loadingBullets = false;
	}

	public bool AutoSetTypeInInventory(Inventory inventory)
	{
		string text = base.Item.Constants.GetString(caliberHash);
		Item currentLoadedBullet = GetCurrentLoadedBullet();
		if (currentLoadedBullet != null)
		{
			SetTargetBulletType(currentLoadedBullet);
			return false;
		}
		if (inventory == null)
		{
			return false;
		}
		foreach (Item item in inventory)
		{
			if (item.GetBool("IsBullet") && !(item.Constants.GetString(caliberHash) != text))
			{
				SetTargetBulletType(item);
				break;
			}
		}
		if (targetBulletID == -1)
		{
			return false;
		}
		return true;
	}

	public int GetBulletCount()
	{
		int num = 0;
		if (base.Item == null)
		{
			return 0;
		}
		foreach (Item item in base.Item.Inventory)
		{
			if (!(item == null))
			{
				num += item.StackCount;
			}
		}
		return num;
	}

	public Item GetCurrentLoadedBullet()
	{
		foreach (Item item in base.Item.Inventory)
		{
			if (!(item == null))
			{
				return item;
			}
		}
		return null;
	}

	public int GetBulletCountofTypeInInventory(int bulletItemTypeID, Inventory inventory)
	{
		if (targetBulletID == -1)
		{
			return 0;
		}
		int num = 0;
		foreach (Item item in inventory)
		{
			if (!(item == null) && item.TypeID == bulletItemTypeID)
			{
				num += item.StackCount;
			}
		}
		return num;
	}

	public void TakeOutAllBullets()
	{
		if (base.Item == null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (Item item2 in base.Item.Inventory)
		{
			if (!(item2 == null))
			{
				list.Add(item2);
			}
		}
		CharacterMainControl characterMainControl = base.Item.GetCharacterMainControl();
		if ((bool)base.Item.InInventory && (bool)LevelManager.Instance && base.Item.InInventory == LevelManager.Instance.PetProxy.Inventory)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Item item = list[i];
			if (item == null)
			{
				continue;
			}
			if ((bool)characterMainControl)
			{
				item.Drop(characterMainControl, createRigidbody: true);
				characterMainControl.PickupItem(item);
				continue;
			}
			bool flag = false;
			Inventory inInventory = base.Item.InInventory;
			if ((bool)inInventory)
			{
				flag = inInventory.AddAndMerge(item);
			}
			if (!flag)
			{
				item.Detach();
				item.DestroyTree();
			}
		}
		bulletCount = 0;
	}

	public Dictionary<int, BulletTypeInfo> GetBulletTypesInInventory(Inventory inventory)
	{
		Dictionary<int, BulletTypeInfo> dictionary = new Dictionary<int, BulletTypeInfo>();
		string text = base.Item.Constants.GetString(caliberHash);
		foreach (Item item in inventory)
		{
			if (!(item == null) && item.GetBool("IsBullet") && !(item.Constants.GetString(caliberHash) != text))
			{
				if (!dictionary.ContainsKey(item.TypeID))
				{
					BulletTypeInfo bulletTypeInfo = new BulletTypeInfo();
					bulletTypeInfo.bulletTypeID = item.TypeID;
					bulletTypeInfo.count = item.StackCount;
					dictionary.Add(bulletTypeInfo.bulletTypeID, bulletTypeInfo);
				}
				else
				{
					dictionary[item.TypeID].count += item.StackCount;
				}
			}
		}
		return dictionary;
	}

	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsGun", value: true);
	}
}
