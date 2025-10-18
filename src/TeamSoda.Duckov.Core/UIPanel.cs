using Duckov.UI.Animations;
using UnityEngine;

public class UIPanel : MonoBehaviour
{
	[SerializeField]
	protected FadeGroup fadeGroup;

	[SerializeField]
	private bool hideWhenChildActive;

	private UIPanel parent;

	private UIPanel activeChild;

	protected virtual void OnOpen()
	{
	}

	protected virtual void OnClose()
	{
	}

	protected virtual void OnChildOpened(UIPanel child)
	{
	}

	protected virtual void OnChildClosed(UIPanel child)
	{
	}

	internal void Open(UIPanel parent = null, bool controlFadeGroup = true)
	{
		this.parent = parent;
		OnOpen();
		if (controlFadeGroup)
		{
			fadeGroup?.Show();
		}
	}

	public void Close()
	{
		if (activeChild != null)
		{
			activeChild.Close();
		}
		OnClose();
		parent?.NotifyChildClosed(this);
		fadeGroup?.Hide();
	}

	public void OpenChild(UIPanel childPanel)
	{
		if (!(childPanel == null))
		{
			if (activeChild != null)
			{
				activeChild.Close();
			}
			activeChild = childPanel;
			childPanel.Open(this);
			OnChildOpened(childPanel);
			if (hideWhenChildActive)
			{
				fadeGroup?.Hide();
			}
		}
	}

	private void NotifyChildClosed(UIPanel child)
	{
		OnChildClosed(child);
		if (hideWhenChildActive)
		{
			fadeGroup?.Show();
		}
	}
}
