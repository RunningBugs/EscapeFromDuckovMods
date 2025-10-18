using System;
using UnityEngine;

namespace Duckov.UI.Animations;

public class ToggleAnimation : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private bool status;

	public bool Status
	{
		get
		{
			return status;
		}
		protected set
		{
			SetToggle(value);
		}
	}

	public event Action<ToggleAnimation, bool> onSetToggle;

	public void SetToggle(bool value)
	{
		status = value;
		if (Application.isPlaying)
		{
			OnSetToggle(Status);
			this.onSetToggle?.Invoke(this, value);
		}
	}

	protected virtual void OnSetToggle(bool value)
	{
	}
}
