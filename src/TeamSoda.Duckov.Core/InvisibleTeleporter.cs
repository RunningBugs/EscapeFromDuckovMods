using System;
using Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

[Obsolete]
public class InvisibleTeleporter : MonoBehaviour, IDrawGizmos
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Space space;

	private bool UsePosition => target == null;

	private Vector3 TargetWorldPosition
	{
		get
		{
			if (target != null)
			{
				return target.transform.position;
			}
			return space switch
			{
				Space.World => position, 
				Space.Self => base.transform.TransformPoint(position), 
				_ => default(Vector3), 
			};
		}
	}

	public void Teleport()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		if (!(main == null))
		{
			GameCamera instance = GameCamera.Instance;
			Vector3 vector = instance.transform.position - main.transform.position;
			main.SetPosition(TargetWorldPosition);
			Vector3 vector2 = main.transform.position + vector;
			instance.transform.position = vector2;
		}
	}

	private void LateUpdate()
	{
		if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
		{
			Teleport();
		}
	}

	public void DrawGizmos()
	{
		if (GizmoContext.InActiveSelection(this))
		{
			CharacterMainControl main = CharacterMainControl.Main;
			if (main == null)
			{
				Draw.Arrow(base.transform.position, TargetWorldPosition);
			}
			else
			{
				Draw.Arrow(main.transform.position, TargetWorldPosition);
			}
		}
	}
}
