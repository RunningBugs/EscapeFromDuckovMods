using UnityEngine;

public class UIInputEventData
{
	private bool used;

	public Vector2 vector;

	public bool confirm;

	public bool cancel;

	public bool Used => used;

	public void Use()
	{
		used = true;
	}
}
