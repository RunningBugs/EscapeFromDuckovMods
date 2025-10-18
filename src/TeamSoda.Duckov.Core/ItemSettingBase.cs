using ItemStatsSystem;
using UnityEngine;

public abstract class ItemSettingBase : MonoBehaviour
{
	protected Item _item;

	public Item Item
	{
		get
		{
			if (_item == null)
			{
				_item = GetComponent<Item>();
			}
			return _item;
		}
	}

	public void Awake()
	{
		if ((bool)Item)
		{
			SetMarkerParam(Item);
			OnInit();
		}
	}

	public virtual void OnInit()
	{
	}

	public virtual void Start()
	{
	}

	public abstract void SetMarkerParam(Item selfItem);
}
