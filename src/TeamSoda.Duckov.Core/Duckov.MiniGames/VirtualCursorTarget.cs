using UnityEngine.Events;

namespace Duckov.MiniGames;

public class VirtualCursorTarget : MiniGameBehaviour
{
	public UnityEvent onEnter;

	public UnityEvent onExit;

	public UnityEvent onClick;

	public bool IsHovering => VirtualCursor.IsHovering(this);

	public void OnCursorEnter()
	{
		onEnter?.Invoke();
	}

	public void OnCursorExit()
	{
		onExit?.Invoke();
	}

	public void OnClick()
	{
		onClick?.Invoke();
	}
}
