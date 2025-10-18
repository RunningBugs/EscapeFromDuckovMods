using Duckov.Utilities;
using ItemStatsSystem;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemDetailsDisplay : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow iconShadow;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI itemID;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private GameObject countContainer;

	[SerializeField]
	private TextMeshProUGUI count;

	[SerializeField]
	private GameObject durabilityContainer;

	[SerializeField]
	private TextMeshProUGUI durabilityText;

	[SerializeField]
	private TooltipsProvider durabilityToolTips;

	[SerializeField]
	[LocalizationKey("Default")]
	private string durabilityToolTipsFormatKey = "UI_DurabilityToolTips";

	[SerializeField]
	private Image durabilityFill;

	[SerializeField]
	private Image durabilityLoss;

	[SerializeField]
	private Gradient durabilityColorOverT;

	[SerializeField]
	private TextMeshProUGUI weightText;

	[SerializeField]
	private ItemSlotCollectionDisplay slotCollectionDisplay;

	[SerializeField]
	private RectTransform propertiesParent;

	[SerializeField]
	private BulletTypeDisplay bulletTypeDisplay;

	[SerializeField]
	private TagsDisplay tagsDisplay;

	[SerializeField]
	private GameObject usableIndicator;

	[SerializeField]
	private UsageUtilitiesDisplay usageUtilitiesDisplay;

	[SerializeField]
	private GameObject registeredIndicator;

	[SerializeField]
	private ItemVariableEntry variableEntryPrefab;

	[SerializeField]
	private ItemStatEntry statEntryPrefab;

	[SerializeField]
	private ItemModifierEntry modifierEntryPrefab;

	[SerializeField]
	private ItemEffectEntry effectEntryPrefab;

	[SerializeField]
	private string weightFormat = "{0:0.#} kg";

	private Item target;

	private PrefabPool<ItemVariableEntry> _variablePool;

	private PrefabPool<ItemStatEntry> _statPool;

	private PrefabPool<ItemModifierEntry> _modifierPool;

	private PrefabPool<ItemEffectEntry> _effectPool;

	private string DurabilityToolTipsFormat => durabilityToolTipsFormatKey.ToPlainText();

	public ItemSlotCollectionDisplay SlotCollectionDisplay => slotCollectionDisplay;

	private PrefabPool<ItemVariableEntry> VariablePool
	{
		get
		{
			if (_variablePool == null)
			{
				_variablePool = new PrefabPool<ItemVariableEntry>(variableEntryPrefab, propertiesParent);
			}
			return _variablePool;
		}
	}

	private PrefabPool<ItemStatEntry> StatPool
	{
		get
		{
			if (_statPool == null)
			{
				_statPool = new PrefabPool<ItemStatEntry>(statEntryPrefab, propertiesParent);
			}
			return _statPool;
		}
	}

	private PrefabPool<ItemModifierEntry> ModifierPool
	{
		get
		{
			if (_modifierPool == null)
			{
				_modifierPool = new PrefabPool<ItemModifierEntry>(modifierEntryPrefab, propertiesParent);
			}
			return _modifierPool;
		}
	}

	private PrefabPool<ItemEffectEntry> EffectPool
	{
		get
		{
			if (_effectPool == null)
			{
				_effectPool = new PrefabPool<ItemEffectEntry>(effectEntryPrefab, propertiesParent);
			}
			return _effectPool;
		}
	}

	public Item Target => target;

	internal void Setup(Item target)
	{
		UnregisterEvents();
		Clear();
		if (!(target == null))
		{
			this.target = target;
			icon.sprite = target.Icon;
			(float, Color, bool) shadowOffsetAndColorOfQuality = GameplayDataSettings.UIStyle.GetShadowOffsetAndColorOfQuality(target.DisplayQuality);
			iconShadow.IgnoreCasterColor = true;
			iconShadow.OffsetDistance = shadowOffsetAndColorOfQuality.Item1;
			iconShadow.Color = shadowOffsetAndColorOfQuality.Item2;
			iconShadow.Inset = shadowOffsetAndColorOfQuality.Item3;
			displayName.text = target.DisplayName;
			itemID.text = $"#{target.TypeID}";
			description.text = target.Description;
			countContainer.SetActive(target.Stackable);
			count.text = target.StackCount.ToString();
			tagsDisplay.Setup(target);
			usageUtilitiesDisplay.Setup(target);
			usableIndicator.gameObject.SetActive(target.UsageUtilities != null);
			RefreshDurability();
			slotCollectionDisplay.Setup(target);
			registeredIndicator.SetActive(target.IsRegistered());
			RefreshWeightText();
			SetupGunDisplays();
			SetupVariables();
			SetupConstants();
			SetupStats();
			SetupModifiers();
			SetupEffects();
			RegisterEvents();
		}
	}

	private void Awake()
	{
		SlotCollectionDisplay.onElementDoubleClicked += OnElementDoubleClicked;
	}

	private void OnElementDoubleClicked(ItemSlotCollectionDisplay collectionDisplay, SlotDisplay slotDisplay)
	{
		if (collectionDisplay.Editable)
		{
			Item item = slotDisplay.GetItem();
			if (!(item == null))
			{
				ItemUtilities.SendToPlayer(item, dontMerge: false, PlayerStorage.Instance != null);
			}
		}
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void Clear()
	{
		tagsDisplay.Clear();
		VariablePool.ReleaseAll();
		StatPool.ReleaseAll();
		ModifierPool.ReleaseAll();
		EffectPool.ReleaseAll();
	}

	private void SetupGunDisplays()
	{
		ItemSetting_Gun itemSetting_Gun = Target?.GetComponent<ItemSetting_Gun>();
		if (itemSetting_Gun == null)
		{
			bulletTypeDisplay.gameObject.SetActive(value: false);
			return;
		}
		bulletTypeDisplay.gameObject.SetActive(value: true);
		bulletTypeDisplay.Setup(itemSetting_Gun.TargetBulletID);
	}

	private void SetupVariables()
	{
		if (target.Variables == null)
		{
			return;
		}
		foreach (CustomData variable in target.Variables)
		{
			if (variable.Display)
			{
				ItemVariableEntry itemVariableEntry = VariablePool.Get(propertiesParent);
				itemVariableEntry.Setup(variable);
				itemVariableEntry.transform.SetAsLastSibling();
			}
		}
	}

	private void SetupConstants()
	{
		if (target.Constants == null)
		{
			return;
		}
		foreach (CustomData constant in target.Constants)
		{
			if (constant.Display)
			{
				ItemVariableEntry itemVariableEntry = VariablePool.Get(propertiesParent);
				itemVariableEntry.Setup(constant);
				itemVariableEntry.transform.SetAsLastSibling();
			}
		}
	}

	private void SetupStats()
	{
		if (target.Stats == null)
		{
			return;
		}
		foreach (Stat stat in target.Stats)
		{
			if (stat.Display)
			{
				ItemStatEntry itemStatEntry = StatPool.Get(propertiesParent);
				itemStatEntry.Setup(stat);
				itemStatEntry.transform.SetAsLastSibling();
			}
		}
	}

	private void SetupModifiers()
	{
		if (target.Modifiers == null)
		{
			return;
		}
		foreach (ModifierDescription modifier in target.Modifiers)
		{
			if (modifier.Display)
			{
				ItemModifierEntry itemModifierEntry = ModifierPool.Get(propertiesParent);
				itemModifierEntry.Setup(modifier);
				itemModifierEntry.transform.SetAsLastSibling();
			}
		}
	}

	private void SetupEffects()
	{
		foreach (Effect effect in target.Effects)
		{
			if (effect.Display)
			{
				ItemEffectEntry itemEffectEntry = EffectPool.Get(propertiesParent);
				itemEffectEntry.Setup(effect);
				itemEffectEntry.transform.SetAsLastSibling();
			}
		}
	}

	private void RegisterEvents()
	{
		if (!(target == null))
		{
			target.onDestroy += OnTargetDestroy;
			target.onChildChanged += OnTargetChildChanged;
			target.onSetStackCount += OnTargetSetStackCount;
			target.onDurabilityChanged += OnTargetDurabilityChanged;
		}
	}

	private void RefreshWeightText()
	{
		weightText.text = string.Format(weightFormat, target.TotalWeight);
	}

	private void OnTargetSetStackCount(Item item)
	{
		RefreshWeightText();
	}

	private void OnTargetChildChanged(Item obj)
	{
		RefreshWeightText();
	}

	internal void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.onDestroy -= OnTargetDestroy;
			target.onChildChanged -= OnTargetChildChanged;
			target.onSetStackCount -= OnTargetSetStackCount;
			target.onDurabilityChanged -= OnTargetDurabilityChanged;
		}
	}

	private void OnTargetDurabilityChanged(Item item)
	{
		RefreshDurability();
	}

	private void RefreshDurability()
	{
		bool useDurability = target.UseDurability;
		durabilityContainer.SetActive(useDurability);
		if (useDurability)
		{
			float durability = target.Durability;
			float maxDurability = target.MaxDurability;
			float maxDurabilityWithLoss = target.MaxDurabilityWithLoss;
			string lossPercentage = $"{target.DurabilityLoss * 100f:0}%";
			float num = durability / maxDurability;
			durabilityText.text = $"{durability:0} / {maxDurabilityWithLoss:0}";
			durabilityToolTips.text = DurabilityToolTipsFormat.Format(new
			{
				curDurability = durability,
				maxDurability = maxDurability,
				maxDurabilityWithLoss = maxDurabilityWithLoss,
				lossPercentage = lossPercentage
			});
			durabilityFill.fillAmount = num;
			durabilityFill.color = durabilityColorOverT.Evaluate(num);
			durabilityLoss.fillAmount = target.DurabilityLoss;
		}
	}

	private void OnTargetDestroy(Item item)
	{
	}
}
