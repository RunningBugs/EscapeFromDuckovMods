using ItemStatsSystem;
using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions;

public class UseDrug : ActionTask<AICharacterController>
{
	public bool stopMove;

	protected override string info
	{
		get
		{
			if (!stopMove)
			{
				return "打药";
			}
			return "原地打药";
		}
	}

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		Item drugItem = base.agent.GetDrugItem();
		if (drugItem == null)
		{
			EndAction(success: false);
		}
		else
		{
			base.agent.CharacterMainControl.UseItem(drugItem);
		}
	}

	protected override void OnUpdate()
	{
		if (stopMove && base.agent.IsMoving())
		{
			base.agent.StopMove();
		}
		if (!base.agent || !base.agent.CharacterMainControl)
		{
			EndAction(success: false);
		}
		else if (!base.agent.CharacterMainControl.useItemAction.Running)
		{
			EndAction(success: true);
		}
	}

	protected override void OnStop()
	{
		base.agent.CharacterMainControl.SwitchToFirstAvailableWeapon();
	}
}
