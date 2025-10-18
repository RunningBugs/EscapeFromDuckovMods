using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
	public Door parent;

	private void OnTriggerEnter(Collider collision)
	{
		if (!parent.IsOpen && parent.NoRequireItem && (!parent.Interact || parent.Interact.gameObject.activeInHierarchy) && collision.gameObject.layer == LayerMask.NameToLayer("Character"))
		{
			CharacterMainControl component = collision.gameObject.GetComponent<CharacterMainControl>();
			if ((bool)component && component.Team != Teams.player)
			{
				parent.Open();
			}
		}
	}
}
