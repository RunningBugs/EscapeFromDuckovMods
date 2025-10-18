using System;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public abstract class View : ManagedUIElement
{
	[HideInInspector]
	private static View _activeView;

	[SerializeField]
	private ViewTabs viewTabs;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private string sfx_Open;

	[SerializeField]
	private string sfx_Close;

	private bool autoClose = true;

	public static View ActiveView
	{
		get
		{
			return _activeView;
		}
		private set
		{
			View activeView = _activeView;
			_activeView = value;
			if (activeView != _activeView)
			{
				View.OnActiveViewChanged?.Invoke();
			}
		}
	}

	public static event Action OnActiveViewChanged;

	protected override void Awake()
	{
		base.Awake();
		if (exitButton != null)
		{
			exitButton.onClick.AddListener(base.Close);
		}
		UIInputManager.OnNavigate += OnNavigate;
		UIInputManager.OnConfirm += OnConfirm;
		UIInputManager.OnCancel += OnCancel;
		viewTabs = base.transform.parent.parent.GetComponent<ViewTabs>();
		if (autoClose)
		{
			Close();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UIInputManager.OnNavigate -= OnNavigate;
		UIInputManager.OnConfirm -= OnConfirm;
		UIInputManager.OnCancel -= OnCancel;
	}

	protected override void OnOpen()
	{
		autoClose = false;
		if (ActiveView != null && ActiveView != this)
		{
			ActiveView.Close();
		}
		ActiveView = this;
		ItemUIUtilities.Select(null);
		if (viewTabs != null)
		{
			viewTabs.Show();
		}
		if (base.gameObject == null)
		{
			Debug.LogError("GameObject不存在", base.gameObject);
		}
		InputManager.DisableInput(base.gameObject);
		AudioManager.Post(sfx_Open);
	}

	protected override void OnClose()
	{
		if (ActiveView == this)
		{
			ActiveView = null;
		}
		InputManager.ActiveInput(base.gameObject);
		AudioManager.Post(sfx_Close);
	}

	internal virtual void TryQuit()
	{
		Close();
	}

	public void OnNavigate(UIInputEventData eventData)
	{
		if (!eventData.Used && !(ActiveView != this))
		{
			OnNavigate(eventData.vector);
		}
	}

	public void OnConfirm(UIInputEventData eventData)
	{
		if (!eventData.Used && !(ActiveView != this))
		{
			OnConfirm();
		}
	}

	public void OnCancel(UIInputEventData eventData)
	{
		if (!eventData.Used && !(ActiveView == null) && !(ActiveView != this))
		{
			OnCancel();
			if (!eventData.Used)
			{
				TryQuit();
				eventData.Use();
			}
		}
	}

	protected virtual void OnNavigate(Vector2 vector)
	{
	}

	protected virtual void OnConfirm()
	{
	}

	protected virtual void OnCancel()
	{
	}

	protected static T GetViewInstance<T>() where T : View
	{
		return GameplayUIManager.GetViewInstance<T>();
	}
}
