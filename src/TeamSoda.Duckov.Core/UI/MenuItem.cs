using System;
using UnityEngine;

namespace UI;

public class MenuItem : MonoBehaviour
{
	private Menu _master;

	[SerializeField]
	private bool selectable = true;

	private bool cacheSelected;

	public Action<MenuItem> onSelected;

	public Action<MenuItem> onDeselected;

	public Action<MenuItem> onConfirmed;

	public Action<MenuItem> onCanceled;

	public Action<MenuItem, bool> onFocusStatusChanged;

	public Menu Master
	{
		get
		{
			if (_master == null)
			{
				_master = base.transform.parent?.GetComponent<Menu>();
			}
			return _master;
		}
		set
		{
			_master = value;
		}
	}

	public bool Selectable
	{
		get
		{
			if (!base.gameObject.activeSelf)
			{
				return false;
			}
			return selectable;
		}
		set
		{
			selectable = value;
		}
	}

	public bool IsSelected => cacheSelected;

	private void OnTransformParentChanged()
	{
		if (!(Master == null))
		{
			Master.Register(this);
		}
	}

	private void OnEnable()
	{
		if (!(Master == null))
		{
			Master.Register(this);
		}
	}

	private void OnDisable()
	{
		if (!(Master == null))
		{
			Master.Unegister(this);
		}
	}

	public void Select()
	{
		if (Master == null)
		{
			Debug.LogError("Menu Item " + base.name + " 没有Master。");
		}
		else
		{
			Master.Select(this);
		}
	}

	internal void NotifySelected()
	{
		cacheSelected = true;
		onSelected?.Invoke(this);
	}

	internal void NotifyDeselected()
	{
		cacheSelected = false;
		onDeselected?.Invoke(this);
	}

	internal void NotifyConfirmed()
	{
		onConfirmed?.Invoke(this);
	}

	internal void NotifyCanceled()
	{
		onCanceled?.Invoke(this);
	}

	internal void NotifyMasterFocusStatusChanged()
	{
		onFocusStatusChanged?.Invoke(this, Master.Focused);
	}
}
