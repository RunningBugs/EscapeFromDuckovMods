using UnityEngine;

namespace Duckov.Modding;

public abstract class ModBehaviour : MonoBehaviour
{
	public ModManager master { get; private set; }

	public ModInfo info { get; private set; }

	public void Setup(ModManager master, ModInfo info)
	{
		this.master = master;
		this.info = info;
		OnAfterSetup();
	}

	public void NotifyBeforeDeactivate()
	{
		OnBeforeDeactivate();
	}

	protected virtual void OnAfterSetup()
	{
	}

	protected virtual void OnBeforeDeactivate()
	{
	}
}
