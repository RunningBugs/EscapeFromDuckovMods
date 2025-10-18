using UnityEngine;

public abstract class CharacterActionBase : MonoBehaviour
{
	public enum ActionPriorities
	{
		Whatever,
		Reload,
		Attack,
		usingItem,
		Dash,
		Skills,
		Fishing,
		Interact
	}

	private bool running;

	protected float actionTimer;

	public bool progressHUD = true;

	public CharacterMainControl characterController;

	public bool Running => running;

	public float ActionTimer => actionTimer;

	public abstract ActionPriorities ActionPriority();

	public abstract bool CanMove();

	public abstract bool CanRun();

	public abstract bool CanUseHand();

	public abstract bool CanControlAim();

	public virtual bool CanEditInventory()
	{
		return false;
	}

	public void UpdateAction(float deltaTime)
	{
		actionTimer += deltaTime;
		OnUpdateAction(deltaTime);
	}

	protected virtual void OnUpdateAction(float deltaTime)
	{
	}

	protected virtual bool OnStart()
	{
		return true;
	}

	public virtual bool IsStopable()
	{
		return true;
	}

	protected virtual void OnStop()
	{
	}

	public abstract bool IsReady();

	public bool StartActionByCharacter(CharacterMainControl _character)
	{
		if (!IsReady())
		{
			return false;
		}
		characterController = _character;
		if (OnStart())
		{
			actionTimer = 0f;
			running = true;
			return true;
		}
		return false;
	}

	public bool StopAction()
	{
		if (!running)
		{
			return true;
		}
		if (IsStopable())
		{
			running = false;
			OnStop();
			return true;
		}
		return false;
	}
}
