using Duckov.Utilities;
using UnityEngine;

public class AimTargetFinder : MonoBehaviour
{
	private Vector3 searchPoint;

	public float searchRadius;

	private LayerMask damageReceiverLayers;

	private Collider[] overlapcColliders;

	private void Start()
	{
	}

	public Transform Find(bool search, Vector3 findPoint, ref CharacterMainControl foundCharacter)
	{
		Transform result = null;
		if (search)
		{
			result = Search(findPoint, ref foundCharacter);
		}
		return result;
	}

	private Transform Search(Vector3 findPoint, ref CharacterMainControl character)
	{
		character = null;
		if (overlapcColliders == null)
		{
			overlapcColliders = new Collider[6];
			damageReceiverLayers = GameplayDataSettings.Layers.damageReceiverLayerMask;
		}
		int num = Physics.OverlapSphereNonAlloc(findPoint, searchRadius, overlapcColliders, damageReceiverLayers);
		Collider collider = null;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				DamageReceiver component = overlapcColliders[i].GetComponent<DamageReceiver>();
				if (!(component == null) && component.Team != Teams.player)
				{
					collider = overlapcColliders[i];
					if (component.health != null)
					{
						character = component.health.GetComponent<CharacterMainControl>();
					}
					break;
				}
			}
		}
		if ((bool)collider)
		{
			return collider.transform;
		}
		return null;
	}
}
