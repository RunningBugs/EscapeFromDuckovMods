using UnityEngine;

namespace Duckov.PerkTrees;

[RequireComponent(typeof(Perk))]
public abstract class PerkBehaviour : MonoBehaviour
{
	private Perk master;

	protected Perk Master => master;

	private bool Unlocked
	{
		get
		{
			if (master == null)
			{
				return false;
			}
			return master.Unlocked;
		}
	}

	public virtual string Description => "";

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponent<Perk>();
		}
		master.onUnlockStateChanged += OnMasterUnlockStateChanged;
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		if (LevelManager.LevelInited)
		{
			OnLevelInitialized();
		}
		OnAwake();
	}

	private void OnLevelInitialized()
	{
		NotifyUnlockStateChanged(Unlocked);
	}

	private void OnDestroy()
	{
		OnOnDestroy();
		if (!(master == null))
		{
			master.onUnlockStateChanged -= OnMasterUnlockStateChanged;
			LevelManager.OnLevelInitialized -= OnLevelInitialized;
		}
	}

	private void OnValidate()
	{
		if (master == null)
		{
			master = GetComponent<Perk>();
		}
	}

	private void OnMasterUnlockStateChanged(Perk perk, bool unlocked)
	{
		if (perk != master)
		{
			Debug.LogError("Perk对象不匹配");
		}
		NotifyUnlockStateChanged(unlocked);
	}

	private void NotifyUnlockStateChanged(bool unlocked)
	{
		OnUnlockStateChanged(unlocked);
		if (unlocked)
		{
			OnUnlocked();
		}
		else
		{
			OnLocked();
		}
	}

	protected virtual void OnUnlockStateChanged(bool unlocked)
	{
	}

	protected virtual void OnUnlocked()
	{
	}

	protected virtual void OnLocked()
	{
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnOnDestroy()
	{
	}
}
