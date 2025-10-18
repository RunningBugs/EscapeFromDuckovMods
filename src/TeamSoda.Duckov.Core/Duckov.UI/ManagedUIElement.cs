using System;
using UnityEngine;

namespace Duckov.UI;

public abstract class ManagedUIElement : MonoBehaviour
{
	[SerializeField]
	private ManagedUIElement parent;

	public bool open { get; private set; }

	protected virtual bool ShowOpenCloseButtons => true;

	public static event Action<ManagedUIElement> onOpen;

	public static event Action<ManagedUIElement> onClose;

	protected virtual void Awake()
	{
		RegisterEvents();
	}

	protected virtual void OnDestroy()
	{
		UnregisterEvents();
		if (open)
		{
			Close();
		}
	}

	public void Open(ManagedUIElement parent = null)
	{
		open = true;
		this.parent = parent;
		ManagedUIElement.onOpen?.Invoke(this);
		OnOpen();
	}

	public void Close()
	{
		open = false;
		parent = null;
		ManagedUIElement.onClose?.Invoke(this);
		OnClose();
	}

	private void RegisterEvents()
	{
		onOpen += OnManagedUIBehaviorOpen;
		onClose += OnManagedUIBehaviorClose;
	}

	private void UnregisterEvents()
	{
		onOpen -= OnManagedUIBehaviorOpen;
		onClose -= OnManagedUIBehaviorClose;
	}

	private void OnManagedUIBehaviorClose(ManagedUIElement obj)
	{
		if (obj != null && obj == parent)
		{
			Close();
		}
	}

	private void OnManagedUIBehaviorOpen(ManagedUIElement obj)
	{
	}

	protected virtual void OnOpen()
	{
	}

	protected virtual void OnClose()
	{
	}
}
