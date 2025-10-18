using System;
using ItemStatsSystem;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemAmountDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IItemMetaDataProvider
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private string amountFormat = "( {possess} / {amount} )";

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color enoughColor;

	private int typeID;

	private long amount;

	private ItemMetaData metaData;

	public int TypeID => typeID;

	public ItemMetaData MetaData => metaData;

	public static event Action<ItemAmountDisplay> OnMouseEnter;

	public static event Action<ItemAmountDisplay> OnMouseExit;

	public ItemMetaData GetMetaData()
	{
		return metaData;
	}

	private void Awake()
	{
		ItemUtilities.OnPlayerItemOperation += Refresh;
		LevelManager.OnLevelInitialized += Refresh;
	}

	private void OnDestroy()
	{
		ItemUtilities.OnPlayerItemOperation -= Refresh;
		LevelManager.OnLevelInitialized -= Refresh;
	}

	public void Setup(int itemTypeID, long amount)
	{
		typeID = itemTypeID;
		this.amount = amount;
		Refresh();
	}

	private void Refresh()
	{
		int itemCount = ItemUtilities.GetItemCount(typeID);
		metaData = ItemAssetsCollection.GetMetaData(typeID);
		icon.sprite = metaData.icon;
		amountText.text = amountFormat.Format(new
		{
			amount = amount,
			possess = itemCount
		});
		bool flag = itemCount >= amount;
		background.color = (flag ? enoughColor : normalColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ItemAmountDisplay.OnMouseEnter?.Invoke(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ItemAmountDisplay.OnMouseExit?.Invoke(this);
	}
}
