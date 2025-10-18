using System;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.MasterKeys.UI;

public class MasterKeysIndexEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[Serializable]
	public struct Look
	{
		public Color color;

		public Material material;

		public void ApplyTo(Graphic graphic)
		{
			graphic.material = material;
			graphic.color = color;
		}
	}

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Look notDiscoveredLook;

	[SerializeField]
	private Look activeLook;

	[SerializeField]
	private Sprite undiscoveredIcon;

	[ItemTypeID]
	private int itemID;

	private ItemMetaData metaData;

	private MasterKeysManager.Status status;

	private ISingleSelectionMenu<MasterKeysIndexEntry> menu;

	public int ItemID => itemID;

	public string DisplayName
	{
		get
		{
			if (status == null)
			{
				return "???";
			}
			if (!status.active)
			{
				return "???";
			}
			return metaData.DisplayName;
		}
	}

	public Sprite Icon
	{
		get
		{
			if (status == null)
			{
				return undiscoveredIcon;
			}
			if (!status.active)
			{
				return undiscoveredIcon;
			}
			return metaData.icon;
		}
	}

	public string Description
	{
		get
		{
			if (status == null)
			{
				return "???";
			}
			if (!status.active)
			{
				return "???";
			}
			return metaData.Description;
		}
	}

	public bool Active
	{
		get
		{
			if (status == null)
			{
				return false;
			}
			return status.active;
		}
	}

	internal event Action<MasterKeysIndexEntry> onPointerClicked;

	public void Setup(int itemID, ISingleSelectionMenu<MasterKeysIndexEntry> menu)
	{
		this.itemID = itemID;
		metaData = ItemAssetsCollection.GetMetaData(itemID);
		this.menu = menu;
		Refresh();
	}

	private void SetupNotDiscovered()
	{
		icon.sprite = (undiscoveredIcon ? undiscoveredIcon : metaData.icon);
		notDiscoveredLook.ApplyTo(icon);
		nameText.text = "???";
	}

	private void SetupActive()
	{
		icon.sprite = metaData.icon;
		activeLook.ApplyTo(icon);
		nameText.text = metaData.DisplayName;
	}

	private void Refresh()
	{
		status = MasterKeysManager.GetStatus(itemID);
		if (status != null)
		{
			if (status.active)
			{
				SetupActive();
			}
		}
		else
		{
			SetupNotDiscovered();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Refresh();
		menu?.SetSelection(this);
		this.onPointerClicked?.Invoke(this);
	}
}
