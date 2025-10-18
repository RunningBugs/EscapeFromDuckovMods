using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class PickupSearchedItem : ActionTask<AICharacterController>
{
	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if (base.agent == null || base.agent.CharacterMainControl == null || base.agent.searchedPickup == null)
		{
			EndAction(success: false);
		}
		else if (Vector3.Distance(base.agent.transform.position, base.agent.searchedPickup.transform.position) > 1.5f)
		{
			EndAction(success: false);
		}
		else if (base.agent.searchedPickup.ItemAgent != null)
		{
			base.agent.CharacterMainControl.PickupItem(base.agent.searchedPickup.ItemAgent.Item);
		}
	}

	protected override void OnUpdate()
	{
	}
}
