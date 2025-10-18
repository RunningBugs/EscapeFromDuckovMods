using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemRepair_RepairAllPanel : MonoBehaviour
{
	[SerializeField]
	private ItemRepairView master;

	[SerializeField]
	private TextMeshProUGUI priceDisplay;

	[SerializeField]
	private ItemDisplay itemDisplayTemplate;

	[SerializeField]
	private Button button;

	[SerializeField]
	private GameObject placeholder;

	private PrefabPool<ItemDisplay> _pool;

	private bool needsRefresh;

	private PrefabPool<ItemDisplay> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<ItemDisplay>(itemDisplayTemplate, null, null, null, null, collectionCheck: true, 10, 10000, delegate(ItemDisplay e)
				{
					e.onPointerClick += OnPointerClickEntry;
				});
			}
			return _pool;
		}
	}

	private void OnPointerClickEntry(ItemDisplay display, PointerEventData data)
	{
		data.Use();
	}

	private void Awake()
	{
		itemDisplayTemplate.gameObject.SetActive(value: false);
		button.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		if (!(master == null))
		{
			List<Item> allEquippedItems = master.GetAllEquippedItems();
			master.RepairItems(allEquippedItems);
			needsRefresh = true;
		}
	}

	private void OnEnable()
	{
		ItemUtilities.OnPlayerItemOperation += OnPlayerItemOperation;
		ItemRepairView.OnRepaireOptionDone += OnRepairOptionDone;
	}

	private void OnDisable()
	{
		ItemUtilities.OnPlayerItemOperation -= OnPlayerItemOperation;
		ItemRepairView.OnRepaireOptionDone -= OnRepairOptionDone;
	}

	public void Setup(ItemRepairView master)
	{
		this.master = master;
		Refresh();
	}

	private void OnPlayerItemOperation()
	{
		needsRefresh = true;
	}

	private void OnRepairOptionDone()
	{
		needsRefresh = true;
	}

	private void Refresh()
	{
		needsRefresh = false;
		Pool.ReleaseAll();
		List<Item> list = (from e in master.GetAllEquippedItems()
			where e.Durability < e.MaxDurabilityWithLoss
			select e).ToList();
		int num = 0;
		if (list != null && list.Count > 0)
		{
			foreach (Item item in list)
			{
				Pool.Get().Setup(item);
			}
			num = master.CalculateRepairPrice(list);
			placeholder.SetActive(value: false);
			bool enough = new Cost(num).Enough;
			button.interactable = enough;
		}
		else
		{
			placeholder.SetActive(value: true);
			button.interactable = false;
		}
		priceDisplay.text = num.ToString();
	}

	private void Update()
	{
		if (needsRefresh)
		{
			Refresh();
		}
	}
}
