using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI;

public class Menu : MonoBehaviour
{
	[SerializeField]
	private bool focused;

	[SerializeField]
	private MenuItem cursor;

	[SerializeField]
	private LayoutGroup layout;

	private HashSet<MenuItem> items = new HashSet<MenuItem>();

	public bool Focused
	{
		get
		{
			return focused;
		}
		set
		{
			SetFocused(value);
		}
	}

	public event Action<Menu, MenuItem> onSelectionChanged;

	public event Action<Menu, MenuItem> onConfirmed;

	public event Action<Menu, MenuItem> onCanceled;

	private void SetFocused(bool value)
	{
		focused = value;
		if (focused && cursor == null)
		{
			SelectDefault();
		}
		cursor?.NotifyMasterFocusStatusChanged();
	}

	public MenuItem GetSelected()
	{
		return cursor;
	}

	public T GetSelected<T>() where T : Component
	{
		if (cursor == null)
		{
			return null;
		}
		return cursor.GetComponent<T>();
	}

	public void Select(MenuItem toSelect)
	{
		if (toSelect.transform.parent != base.transform)
		{
			Debug.LogError("正在尝试选中不属于此菜单的项目。已取消。");
			return;
		}
		if (!items.Contains(toSelect))
		{
			items.Add(toSelect);
		}
		if (toSelect.Selectable)
		{
			if (cursor != null)
			{
				DeselectCurrent();
			}
			cursor = toSelect;
			cursor.NotifySelected();
			OnSelectionChanged();
		}
	}

	public void SelectDefault()
	{
		MenuItem[] componentsInChildren = GetComponentsInChildren<MenuItem>(includeInactive: false);
		if (componentsInChildren == null)
		{
			return;
		}
		foreach (MenuItem menuItem in componentsInChildren)
		{
			if (!(menuItem == null) && menuItem.Selectable)
			{
				Select(menuItem);
			}
		}
	}

	public void Confirm()
	{
		if (cursor != null)
		{
			cursor.NotifyConfirmed();
		}
		this.onConfirmed?.Invoke(this, cursor);
	}

	public void Cancel()
	{
		if (cursor != null)
		{
			cursor.NotifyCanceled();
		}
		this.onCanceled?.Invoke(this, cursor);
	}

	private void DeselectCurrent()
	{
		cursor.NotifyDeselected();
	}

	private void OnSelectionChanged()
	{
		this.onSelectionChanged?.Invoke(this, cursor);
	}

	public void Navigate(Vector2 direction)
	{
		if (cursor == null)
		{
			SelectDefault();
		}
		if (!(cursor == null) && !Mathf.Approximately(direction.sqrMagnitude, 0f))
		{
			MenuItem menuItem = FindClosestEntryInDirection(cursor, direction);
			if (!(menuItem == null))
			{
				Select(menuItem);
			}
		}
	}

	private MenuItem FindClosestEntryInDirection(MenuItem cursor, Vector2 direction)
	{
		if (cursor == null)
		{
			return null;
		}
		direction = direction.normalized;
		float num = Mathf.Cos(MathF.PI / 4f);
		MenuItem bestMatch = null;
		float bestSqrDist = float.MaxValue;
		float bestDot = num;
		foreach (MenuItem item in items)
		{
			MenuItem cur = item;
			if (cur == null || cur == cursor || !cur.Selectable)
			{
				continue;
			}
			Vector3 vector = cur.transform.localPosition - cursor.transform.localPosition;
			Vector3 normalized = vector.normalized;
			float dot = Vector3.Dot(normalized, direction);
			if (dot < num)
			{
				continue;
			}
			float sqrDist = vector.magnitude;
			if (!(sqrDist > bestSqrDist))
			{
				if (sqrDist < bestSqrDist)
				{
					SetBestAsCur();
				}
				else if (sqrDist == bestSqrDist && dot > bestDot)
				{
					SetBestAsCur();
				}
			}
			void SetBestAsCur()
			{
				bestMatch = cur;
				bestSqrDist = sqrDist;
				bestDot = dot;
			}
		}
		return bestMatch;
	}

	internal void Register(MenuItem menuItem)
	{
		items.Add(menuItem);
	}

	internal void Unegister(MenuItem menuItem)
	{
		items.Remove(menuItem);
	}
}
