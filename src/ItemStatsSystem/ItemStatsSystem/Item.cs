using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using ItemStatsSystem.Items;
using ItemStatsSystem.Stats;
using Sirenix.OdinInspector;
using SodaCraft.Localizations;
using UnityEngine;

namespace ItemStatsSystem;

public class Item : MonoBehaviour, ISelfValidator
{
	[SerializeField]
	private int typeID;

	[SerializeField]
	private int order;

	[LocalizationKey("Items")]
	[SerializeField]
	private string displayName;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private int maxStackCount = 1;

	[SerializeField]
	private int value;

	[SerializeField]
	private int quality;

	[SerializeField]
	private DisplayQuality displayQuality;

	[SerializeField]
	private float weight;

	private float _cachedWeight;

	private float? _cachedTotalWeight;

	private int handheldHash = "Handheld".GetHashCode();

	[SerializeField]
	private TagCollection tags = new TagCollection();

	[SerializeField]
	private ItemAgentUtilities agentUtilities = new ItemAgentUtilities();

	[SerializeField]
	private ItemGraphicInfo itemGraphic;

	[SerializeField]
	private StatCollection stats;

	[SerializeField]
	private SlotCollection slots;

	[SerializeField]
	private ModifierDescriptionCollection modifiers;

	[SerializeField]
	private CustomDataCollection variables = new CustomDataCollection();

	[SerializeField]
	private CustomDataCollection constants = new CustomDataCollection();

	[SerializeField]
	private Inventory inventory;

	[SerializeField]
	private List<Effect> effects = new List<Effect>();

	[SerializeField]
	private UsageUtilities usageUtilities;

	private Slot pluggedIntoSlot;

	private Inventory inInventory;

	private bool initialized;

	private const string StackCountVariableKey = "Count";

	private static readonly int StackCountVariableHash = "Count".GetHashCode();

	private const string InspectedVariableKey = "Inspected";

	private static readonly int InspectedVariableHash = "Inspected".GetHashCode();

	private const string MaxDurabilityConstantKey = "MaxDurability";

	private const string DurabilityVariableKey = "Durability";

	private static readonly int MaxDurabilityConstantHash = "MaxDurability".GetHashCode();

	private static readonly int DurabilityVariableHash = "Durability".GetHashCode();

	private bool _inspecting;

	public string soundKey;

	private bool isBeingDestroyed;

	public int TypeID
	{
		get
		{
			return typeID;
		}
		internal set
		{
			typeID = value;
		}
	}

	public int Order
	{
		get
		{
			return order;
		}
		set
		{
			order = value;
		}
	}

	public string DisplayName => displayName.ToPlainText();

	public string DisplayNameRaw
	{
		get
		{
			return displayName;
		}
		set
		{
			displayName = value;
		}
	}

	[LocalizationKey("Items")]
	private string description
	{
		get
		{
			return displayName + "_Desc";
		}
		set
		{
		}
	}

	public string DescriptionRaw => description;

	public string Description => description.ToPlainText();

	public Sprite Icon
	{
		get
		{
			return icon;
		}
		set
		{
			icon = value;
		}
	}

	private string MaxStackCountSuffixLabel
	{
		get
		{
			if (MaxStackCount <= 1)
			{
				return "不可堆叠";
			}
			return "可堆叠";
		}
	}

	public int MaxStackCount
	{
		get
		{
			return maxStackCount;
		}
		set
		{
			maxStackCount = value;
		}
	}

	public bool Stackable => MaxStackCount > 1;

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public int Quality
	{
		get
		{
			return quality;
		}
		set
		{
			quality = value;
		}
	}

	public DisplayQuality DisplayQuality
	{
		get
		{
			return displayQuality;
		}
		set
		{
			displayQuality = value;
		}
	}

	public float UnitSelfWeight => weight;

	public float SelfWeight => weight * (float)StackCount;

	public bool Sticky => Tags.Contains("Sticky");

	public bool CanBeSold
	{
		get
		{
			if (Sticky)
			{
				return false;
			}
			return !Tags.Contains("NotSellable");
		}
	}

	public bool CanDrop => !Sticky;

	public float TotalWeight
	{
		get
		{
			if (!_cachedTotalWeight.HasValue || _cachedWeight != SelfWeight)
			{
				_cachedWeight = SelfWeight;
				_cachedTotalWeight = RecalculateTotalWeight();
			}
			return _cachedTotalWeight.Value;
		}
	}

	public bool HasHandHeldAgent => AgentUtilities.GetPrefab(handheldHash) != null;

	private string TagsLabelText
	{
		get
		{
			string text = "Tags: ";
			bool flag = true;
			foreach (Tag tag in tags)
			{
				text = text + (flag ? "" : ", ") + tag.DisplayName;
				flag = false;
			}
			return text;
		}
	}

	public ItemAgentUtilities AgentUtilities => agentUtilities;

	public ItemAgent ActiveAgent => agentUtilities.ActiveAgent;

	public ItemGraphicInfo ItemGraphic => itemGraphic;

	private string StatsTabLabelText
	{
		get
		{
			if (!stats)
			{
				return "No Stats";
			}
			return $"Stats({stats.Count})";
		}
	}

	private string SlotsTabLabelText
	{
		get
		{
			if (!slots)
			{
				return "No Slots";
			}
			return $"Slots({slots.Count})";
		}
	}

	private string ModifiersTabLabelText
	{
		get
		{
			if (!modifiers)
			{
				return "No Modifiers";
			}
			return $"Modifiers({modifiers.Count})";
		}
	}

	public UsageUtilities UsageUtilities => usageUtilities;

	public float UseTime
	{
		get
		{
			if (usageUtilities != null)
			{
				return usageUtilities.UseTime;
			}
			return 0f;
		}
	}

	public StatCollection Stats => stats;

	public ModifierDescriptionCollection Modifiers => modifiers;

	public SlotCollection Slots => slots;

	public Inventory Inventory
	{
		get
		{
			return inventory;
		}
		internal set
		{
			inventory = value;
		}
	}

	public List<Effect> Effects => effects;

	public Slot PluggedIntoSlot
	{
		get
		{
			if (pluggedIntoSlot == null)
			{
				return null;
			}
			if (pluggedIntoSlot.Master == null)
			{
				return null;
			}
			return pluggedIntoSlot;
		}
	}

	public Inventory InInventory => inInventory;

	public Item ParentItem
	{
		get
		{
			UnityEngine.Object parentObject = ParentObject;
			if (parentObject == null)
			{
				return null;
			}
			Item item = ParentObject as Item;
			if (item != null)
			{
				return item;
			}
			Inventory inventory = parentObject as Inventory;
			if (inventory == null)
			{
				Debug.LogError("侦测到不合法的Parent Object。需要检查ParentObject代码。");
				return null;
			}
			Item attachedToItem = inventory.AttachedToItem;
			if (attachedToItem != null)
			{
				return attachedToItem;
			}
			return null;
		}
	}

	public UnityEngine.Object ParentObject
	{
		get
		{
			if (PluggedIntoSlot != null && InInventory != null)
			{
				Debug.LogError($"物品 {DisplayName} ({GetInstanceID()})同时存在于Slot和Inventory中。");
			}
			if (PluggedIntoSlot != null)
			{
				return PluggedIntoSlot?.Master;
			}
			if (InInventory != null)
			{
				return InInventory;
			}
			return null;
		}
	}

	public TagCollection Tags => tags;

	public CustomDataCollection Variables => variables;

	public CustomDataCollection Constants => constants;

	public bool IsCharacter => tags.Any((Tag e) => e != null && e.name == "Character");

	public int StackCount
	{
		get
		{
			if (Stackable)
			{
				return GetInt(StackCountVariableHash, 1);
			}
			return 1;
		}
		set
		{
			if (!Stackable)
			{
				if (value != 1)
				{
					Debug.LogError("该物品 " + DisplayName + " 不可堆叠。无法设置数量。");
				}
				return;
			}
			int num = value;
			if (value >= 1 && value > MaxStackCount)
			{
				Debug.LogWarning($"尝试将数量设为{value},但该物品 {DisplayName} 的数量最多为{MaxStackCount}。将改为设为{MaxStackCount}。");
				num = MaxStackCount;
			}
			SetInt("Count", num);
			this.onSetStackCount?.Invoke(this);
			NotifyChildChanged();
			if ((bool)InInventory)
			{
				InInventory.NotifyContentChanged(this);
			}
			if (StackCount < 1)
			{
				this.DestroyTree();
			}
		}
	}

	public bool UseDurability => MaxDurability > 0f;

	public float MaxDurability
	{
		get
		{
			return Constants.GetFloat(MaxDurabilityConstantHash);
		}
		set
		{
			Constants.SetFloat("MaxDurability", value);
			this.onDurabilityChanged?.Invoke(this);
		}
	}

	public float MaxDurabilityWithLoss => MaxDurability * (1f - DurabilityLoss);

	public float DurabilityLoss
	{
		get
		{
			return Mathf.Clamp01(Variables.GetFloat("DurabilityLoss"));
		}
		set
		{
			Variables.SetFloat("DurabilityLoss", value);
		}
	}

	public float Durability
	{
		get
		{
			return GetFloat(DurabilityVariableHash);
		}
		set
		{
			float num = Mathf.Min(MaxDurability, value);
			if (num < 0f)
			{
				num = 0f;
			}
			SetFloat("Durability", num);
			this.onDurabilityChanged?.Invoke(this);
			HandleEffectsActive();
		}
	}

	public bool Inspected
	{
		get
		{
			return Variables.GetBool(InspectedVariableHash);
		}
		set
		{
			Variables.SetBool("Inspected", value);
			if (slots != null)
			{
				foreach (Slot slot in slots)
				{
					if (slot != null)
					{
						Item content = slot.Content;
						if (!(content == null))
						{
							content.Inspected = value;
						}
					}
				}
			}
			this.onInspectionStateChanged?.Invoke(this);
		}
	}

	public bool Inspecting
	{
		get
		{
			return _inspecting;
		}
		set
		{
			_inspecting = value;
			this.onInspectionStateChanged?.Invoke(this);
		}
	}

	public bool NeedInspection
	{
		get
		{
			if (Inspected)
			{
				return false;
			}
			if (InInventory == null)
			{
				return false;
			}
			if (!InInventory.NeedInspection)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsBeingDestroyed => isBeingDestroyed;

	public bool Repairable
	{
		get
		{
			if (!UseDurability)
			{
				return false;
			}
			return Tags.Contains("Repairable");
		}
	}

	public string SoundKey
	{
		get
		{
			if (string.IsNullOrWhiteSpace(soundKey))
			{
				return "default";
			}
			return soundKey;
		}
	}

	public event Action<Item> onItemTreeChanged;

	public event Action<Item> onDestroy;

	public event Action<Item> onSetStackCount;

	public event Action<Item> onDurabilityChanged;

	public event Action<Item> onInspectionStateChanged;

	public event Action<Item, object> onUse;

	public static event Action<Item, object> onUseStatic;

	public event Action<Item> onChildChanged;

	public event Action<Item> onParentChanged;

	public event Action<Item> onPluggedIntoSlot;

	public event Action<Item> onUnpluggedFromSlot;

	public event Action<Item, Slot> onSlotContentChanged;

	public event Action<Item> onSlotTreeChanged;

	[SerializeField]
	private void CreateStatsComponent()
	{
		(stats = base.gameObject.AddComponent<StatCollection>()).Master = this;
	}

	[SerializeField]
	public void CreateSlotsComponent()
	{
		if (slots != null)
		{
			Debug.LogError("Slot component已存在");
		}
		else
		{
			(slots = base.gameObject.AddComponent<SlotCollection>()).Master = this;
		}
	}

	[SerializeField]
	public void CreateModifiersComponent()
	{
		if (modifiers == null)
		{
			ModifierDescriptionCollection modifierDescriptionCollection = base.gameObject.AddComponent<ModifierDescriptionCollection>();
			modifiers = modifierDescriptionCollection;
		}
		modifiers.Master = this;
	}

	[SerializeField]
	private void CreateInventoryComponent()
	{
		(inventory = base.gameObject.AddComponent<Inventory>()).AttachedToItem = this;
	}

	public bool IsUsable(object user)
	{
		if (usageUtilities != null)
		{
			return usageUtilities.IsUsable(this, user);
		}
		return false;
	}

	public void AddUsageUtilitiesComponent()
	{
	}

	private void Awake()
	{
		if (!initialized)
		{
			Initialize();
		}
		if ((bool)inventory)
		{
			inventory.onContentChanged += OnInventoryContentChanged;
		}
	}

	private void OnInventoryContentChanged(Inventory inventory, int index)
	{
		NotifyChildChanged();
	}

	public void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			agentUtilities.Initialize(this);
			Stats?.Initialize();
			Slots?.Initialize();
			Modifiers?.Initialize();
			modifiers?.ReapplyModifiers();
			HandleEffectsActive();
		}
	}

	public Item GetCharacterItem()
	{
		Item item = this;
		while (item != null)
		{
			if (item.IsCharacter)
			{
				return item;
			}
			item = item.ParentItem;
		}
		return null;
	}

	public bool IsInCharacterSlot()
	{
		Item item = null;
		Item item2 = this;
		if (item2.IsCharacter)
		{
			return false;
		}
		while (item2 != null)
		{
			if (item2.IsCharacter)
			{
				if (item.PluggedIntoSlot != null)
				{
					return true;
				}
				return false;
			}
			item = item2;
			item2 = item2.ParentItem;
		}
		return false;
	}

	public Item CreateInstance()
	{
		Item item = UnityEngine.Object.Instantiate(this);
		item.Initialize();
		return item;
	}

	public void Detach()
	{
		PluggedIntoSlot?.Unplug();
		InInventory?.RemoveItem(this);
	}

	internal void NotifyPluggedTo(Slot slot)
	{
		pluggedIntoSlot = slot;
		this.onPluggedIntoSlot?.Invoke(this);
		this.onParentChanged?.Invoke(this);
	}

	internal void NotifyUnpluggedFrom(Slot slot)
	{
		if (pluggedIntoSlot == slot)
		{
			pluggedIntoSlot = null;
			this.onUnpluggedFromSlot?.Invoke(this);
			this.onParentChanged?.Invoke(this);
			return;
		}
		Debug.LogError("物品 " + DisplayName + " 被通知从Slot移除，但当前Slot " + ((pluggedIntoSlot != null) ? (pluggedIntoSlot.Master.DisplayName + "/" + pluggedIntoSlot.Key) : "空") + " 与通知Slot " + ((slot != null) ? (slot.Master.DisplayName + "/" + slot.Key) : "空") + " 不匹配。");
	}

	internal void NotifySlotPlugged(Slot slot)
	{
		NotifyChildChanged();
		NotifySlotTreeChanged();
		this.onSlotContentChanged?.Invoke(this, slot);
	}

	internal void NotifySlotUnplugged(Slot slot)
	{
		NotifyChildChanged();
		NotifySlotTreeChanged();
		this.onSlotContentChanged?.Invoke(this, slot);
	}

	internal void NotifyRemovedFromInventory(Inventory inventory)
	{
		if (inventory == InInventory)
		{
			inInventory = null;
			this.onParentChanged?.Invoke(this);
		}
		else if (InInventory != null)
		{
			Debug.LogError("尝试从不是当前的Inventory中移除，已取消。");
		}
	}

	internal void NotifyAddedToInventory(Inventory inventory)
	{
		inInventory = inventory;
		this.onParentChanged?.Invoke(this);
	}

	internal void NotifyItemTreeChanged()
	{
		this.onItemTreeChanged?.Invoke(this);
		HandleEffectsActive();
	}

	private void HandleEffectsActive()
	{
		if (effects == null)
		{
			return;
		}
		bool active = IsCharacter || PluggedIntoSlot != null;
		if (UseDurability && Durability <= 0f)
		{
			active = false;
		}
		foreach (Effect effect in effects)
		{
			if (!(effect == null))
			{
				effect.gameObject.SetActive(active);
			}
		}
	}

	internal void InitiateNotifyItemTreeChanged()
	{
		List<Item> allConnected = this.GetAllConnected();
		if (allConnected == null)
		{
			return;
		}
		foreach (Item item in allConnected)
		{
			item.NotifyItemTreeChanged();
		}
	}

	internal void NotifyChildChanged()
	{
		RecalculateTotalWeight();
		this.onChildChanged?.Invoke(this);
		Item parentItem = ParentItem;
		if (parentItem != null)
		{
			parentItem.NotifyChildChanged();
		}
	}

	internal void NotifySlotTreeChanged()
	{
		this.onSlotTreeChanged?.Invoke(this);
		Item parentItem = ParentItem;
		if (parentItem != null)
		{
			parentItem.NotifySlotTreeChanged();
		}
	}

	public void Use(object user)
	{
		this.onUse?.Invoke(this, user);
		Item.onUseStatic?.Invoke(this, user);
		usageUtilities.Use(this, user);
	}

	public CustomData GetVariableEntry(string variableKey)
	{
		return Variables.GetEntry(variableKey);
	}

	public CustomData GetVariableEntry(int hash)
	{
		return Variables.GetEntry(hash);
	}

	public float GetFloat(string key, float defaultResult = 0f)
	{
		return Variables.GetFloat(key, defaultResult);
	}

	public int GetInt(string key, int defaultResult = 0)
	{
		return Variables.GetInt(key, defaultResult);
	}

	public bool GetBool(string key, bool defaultResult = false)
	{
		return Variables.GetBool(key, defaultResult);
	}

	public string GetString(string key, string defaultResult = null)
	{
		return Variables.GetString(key, defaultResult);
	}

	public float GetFloat(int hash, float defaultResult = 0f)
	{
		return Variables.GetFloat(hash, defaultResult);
	}

	public int GetInt(int hash, int defaultResult = 0)
	{
		return Variables.GetInt(hash, defaultResult);
	}

	public bool GetBool(int hash, bool defaultResult = false)
	{
		return Variables.GetBool(hash, defaultResult);
	}

	public string GetString(int hash, string defaultResult = null)
	{
		return Variables.GetString(hash, defaultResult);
	}

	public void SetFloat(string key, float value, bool createNewIfNotExist = true)
	{
		Variables.Set(key, value, createNewIfNotExist);
	}

	public void SetInt(string key, int value, bool createNewIfNotExist = true)
	{
		Variables.Set(key, value, createNewIfNotExist);
	}

	public void SetBool(string key, bool value, bool createNewIfNotExist = true)
	{
		Variables.Set(key, value, createNewIfNotExist);
	}

	public void SetString(string key, string value, bool createNewIfNotExist = true)
	{
		Variables.Set(key, value, createNewIfNotExist);
	}

	public void SetFloat(int hash, float value)
	{
		Variables.Set(hash, value);
	}

	public void SetInt(int hash, int value)
	{
		Variables.Set(hash, value);
	}

	public void SetBool(int hash, bool value)
	{
		Variables.Set(hash, value);
	}

	public void SetString(int hash, string value)
	{
		Variables.Set(hash, value);
	}

	internal void ForceSetStackCount(int value)
	{
		Debug.LogWarning($"正在强制将物品 {DisplayName} 的 Stack Count 设置为 {value}。");
		SetInt(StackCountVariableHash, value);
		this.onSetStackCount?.Invoke(this);
	}

	public void Combine(Item incomingItem)
	{
		if (incomingItem == null || incomingItem == this)
		{
			return;
		}
		if (!Stackable)
		{
			Debug.LogError("正在尝试组合物品，但物品 " + DisplayName + " 不能堆叠。");
			return;
		}
		if (TypeID != incomingItem.TypeID)
		{
			Debug.LogError("物品 " + DisplayName + " 与 " + incomingItem.DisplayName + " 类型不同，无法组合。");
			return;
		}
		int num = MaxStackCount - StackCount;
		if (num <= 0)
		{
			return;
		}
		_ = StackCount;
		_ = incomingItem.StackCount;
		int num2 = ((incomingItem.StackCount >= num) ? num : incomingItem.StackCount);
		int num3 = incomingItem.StackCount - num2;
		StackCount += num2;
		incomingItem.StackCount = num3;
		if (num3 <= 0)
		{
			incomingItem.Detach();
			if (Application.isPlaying)
			{
				incomingItem.DestroyTree();
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(incomingItem);
			}
		}
	}

	public void CombineInto(Item otherItem)
	{
		otherItem.Combine(this);
	}

	public async UniTask<Item> Split(int count)
	{
		if (!Stackable)
		{
			Debug.LogError("物品 " + DisplayName + " 无法被分割。");
		}
		if (count <= 0)
		{
			return null;
		}
		if (count > StackCount)
		{
			Debug.LogError($"物品 {DisplayName} 数量为{StackCount}，不足以分割出 {count} 。");
			return null;
		}
		if (count == StackCount)
		{
			Debug.LogError($"正在尝试分割物品 {DisplayName} ，但目标数量 {count} 与该物品总数量相同。无法分割。");
			return null;
		}
		StackCount -= count;
		Item item = await ItemAssetsCollection.InstantiateAsync(TypeID);
		if (item == null)
		{
			Debug.LogWarning($"物体 ID:{TypeID} ({DisplayName}) 创建失败。");
			return null;
		}
		item.Initialize();
		item.StackCount = count;
		item.Inspected = true;
		return item;
	}

	public override string ToString()
	{
		return displayName + " (物品)";
	}

	public void MarkDestroyed()
	{
		isBeingDestroyed = true;
	}

	private void OnDestroy()
	{
		isBeingDestroyed = true;
		Detach();
		agentUtilities.ReleaseActiveAgent();
		this.onDestroy?.Invoke(this);
	}

	public Stat GetStat(int hash)
	{
		if (Stats == null)
		{
			return null;
		}
		return Stats?.GetStat(hash);
	}

	public Stat GetStat(string key)
	{
		return Stats?.GetStat(key);
	}

	public float GetStatValue(int hash)
	{
		return GetStat(hash)?.Value ?? 0f;
	}

	public static Stat GetStat(Item item, int hash)
	{
		if (item == null)
		{
			return null;
		}
		return item.GetStat(hash);
	}

	public static float GetStatValue(Item item, int hash)
	{
		if (item == null)
		{
			return 0f;
		}
		return GetStat(item, hash)?.Value ?? 0f;
	}

	private void OnValidate()
	{
		base.transform.hideFlags = HideFlags.HideInInspector;
	}

	public void Validate(SelfValidationResult result)
	{
		if (Stats != null && Stats.gameObject != base.gameObject)
		{
			result.AddError("引用了其他物体上的Stats组件。").WithFix("改为引用本物体的Stats组件", delegate
			{
				stats = GetComponent<StatCollection>();
			});
		}
		if (Slots != null && Slots.gameObject != base.gameObject)
		{
			result.AddError("引用了其他物体上的Slots组件。").WithFix("改为引用本物体的Slots组件", delegate
			{
				slots = GetComponent<SlotCollection>();
			});
		}
		if (Modifiers != null && Modifiers.gameObject != base.gameObject)
		{
			result.AddError("引用了其他物体上的Modifiers组件。").WithFix("改为引用本物体的Modifiers组件", delegate
			{
				modifiers = GetComponent<ModifierDescriptionCollection>();
			});
		}
		if (Inventory != null && Inventory.gameObject != base.gameObject)
		{
			result.AddError("引用了其他物体上的Inventory组件。").WithFix("改为引用本物体的Inventory组件", delegate
			{
				inventory = GetComponent<Inventory>();
			});
		}
		if (Effects.Any((Effect e) => e == null))
		{
			result.AddError("Effects列表中有空物体。").WithFix("移除空Effect项目", delegate
			{
				Effects.RemoveAll((Effect e) => e == null);
			});
		}
		if (Effects.Any((Effect e) => !e.transform.IsChildOf(base.transform)))
		{
			result.AddError("引用了其他物体上的Effects。").WithFix("移除不正确的Effects", delegate
			{
				Effects.RemoveAll((Effect e) => !e.transform.IsChildOf(base.transform));
			});
		}
		if (Stackable)
		{
			if (Slots != null || Inventory != null)
			{
				result.AddError("可堆叠物体不应包含Slot、Inventory等独特信息。").WithFix("变为不可堆叠物体", delegate
				{
					maxStackCount = 1;
				});
			}
			if (Variables.Any((CustomData e) => e.Key != "Count"))
			{
				result.AddError("可堆叠物体不应包含特殊变量。");
			}
			if (!Variables.Any((CustomData e) => e.Key == "Count"))
			{
				result.AddWarning("可堆叠物体应包含Count变量，记录当前具体数量。(默认数量)").WithFix("添加Count变量。", delegate
				{
					variables.Add(new CustomData("Count", MaxStackCount));
				});
			}
		}
		else if (Variables.Any((CustomData e) => e.Key == "Count"))
		{
			result.AddWarning("不可堆叠物体包含了Count变量。建议删除。").WithFix("删除Count变量。", delegate
			{
				variables.Remove(variables.GetEntry("Count"));
			});
		}
	}

	public float RecalculateTotalWeight()
	{
		float num = 0f;
		num += SelfWeight;
		if (inventory != null)
		{
			inventory.RecalculateWeight();
			float cachedWeight = inventory.CachedWeight;
			num += cachedWeight;
		}
		if (slots != null)
		{
			foreach (Slot slot in slots)
			{
				if (slot != null && slot.Content != null)
				{
					float totalWeight = slot.Content.TotalWeight;
					num += totalWeight;
				}
			}
		}
		_cachedTotalWeight = num;
		return num;
	}

	public void AddEffect(Effect instance)
	{
		instance.SetItem(this);
		if (!effects.Contains(instance))
		{
			effects.Add(instance);
		}
	}

	private void CreateNewEffect()
	{
		GameObject obj = new GameObject("New Effect");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		Effect instance = obj.AddComponent<Effect>();
		AddEffect(instance);
	}

	public int GetTotalRawValue()
	{
		float num = Value;
		if (UseDurability && MaxDurability > 0f)
		{
			num = ((!(MaxDurability > 0f)) ? 0f : (num * (Durability / MaxDurability)));
		}
		int num2 = Mathf.FloorToInt(num) * ((!Stackable) ? 1 : StackCount);
		if (Slots != null)
		{
			foreach (Slot slot in Slots)
			{
				if (slot != null)
				{
					Item content = slot.Content;
					if (!(content == null))
					{
						num2 += content.GetTotalRawValue();
					}
				}
			}
		}
		if (Inventory != null)
		{
			foreach (Item item in Inventory)
			{
				if (!(item == null))
				{
					num2 += item.GetTotalRawValue();
				}
			}
		}
		return num2;
	}

	public int RemoveAllModifiersFrom(object endowmentEntry)
	{
		if (stats == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Stat stat in stats)
		{
			if (stat != null)
			{
				num += stat.RemoveAllModifiersFromSource(endowmentEntry);
			}
		}
		return num;
	}

	public bool AddModifier(string statKey, Modifier modifier)
	{
		if (stats == null)
		{
			return false;
		}
		Stat stat = stats[statKey];
		if (stat == null)
		{
			return false;
		}
		stat.AddModifier(modifier);
		return true;
	}
}
