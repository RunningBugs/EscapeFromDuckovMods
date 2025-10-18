using UnityEngine;

namespace Duckov.UI.Animations;

public class ToggleComponent : MonoBehaviour
{
	[SerializeField]
	private ToggleAnimation master;

	private bool Status
	{
		get
		{
			if (!master)
			{
				return false;
			}
			return master.Status;
		}
	}

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponent<ToggleAnimation>();
		}
		master.onSetToggle += OnSetToggle;
	}

	private void OnDestroy()
	{
		if (!(master == null))
		{
			master.onSetToggle -= OnSetToggle;
		}
	}

	protected virtual void OnSetToggle(ToggleAnimation master, bool value)
	{
	}
}
