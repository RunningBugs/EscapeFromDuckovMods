using Duckov.Utilities;
using UnityEngine;

public class Accessory_Lazer : AccessoryBase
{
	[SerializeField]
	private LineRenderer lineRenderer;

	[SerializeField]
	private GameObject hitMarker;

	private CharacterMainControl character;

	private Vector3[] localPoints;

	private RaycastHit[] raycastHits = new RaycastHit[4];

	private LayerMask hitLayers;

	protected override void OnInit()
	{
		hitLayers = (int)GameplayDataSettings.Layers.damageReceiverLayerMask | (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask | (int)GameplayDataSettings.Layers.fowBlockLayers | (int)GameplayDataSettings.Layers.halfObsticleLayer;
		HideHitMarker();
		lineRenderer.enabled = false;
	}

	private void Update()
	{
		if (character == null)
		{
			if (parentAgent == null || parentAgent.Holder == null)
			{
				return;
			}
			character = parentAgent.Holder;
		}
		bool flag = character.IsAiming();
		if (flag != lineRenderer.enabled)
		{
			lineRenderer.enabled = flag;
		}
		if (flag)
		{
			if (localPoints == null)
			{
				localPoints = new Vector3[2];
				lineRenderer.useWorldSpace = false;
				localPoints[0] = Vector3.zero;
			}
			Vector3 position = lineRenderer.transform.position;
			Vector3 currentAimPoint = character.GetCurrentAimPoint();
			Vector3 zero = Vector3.zero;
			zero = ((!(Vector3.Distance(character.transform.position, currentAimPoint) > 2f) || !character.IsMainCharacter) ? (lineRenderer.transform.position + character.CurrentAimDirection.normalized * character.GetAimRange()) : (lineRenderer.transform.position + (character.GetCurrentAimPoint() - lineRenderer.transform.position).normalized * character.GetAimRange()));
			if (CheckObsticle(position, zero, out var hitPoint))
			{
				ShowHitMarker(hitPoint);
				zero = hitPoint;
			}
			else
			{
				HideHitMarker();
			}
			localPoints[1] = lineRenderer.transform.InverseTransformPoint(zero);
			lineRenderer.SetPositions(localPoints);
		}
		else
		{
			HideHitMarker();
		}
	}

	private void ShowHitMarker(Vector3 point)
	{
		if (!hitMarker.activeSelf)
		{
			hitMarker.SetActive(value: true);
		}
		hitMarker.transform.position = point;
	}

	private void HideHitMarker()
	{
		if (hitMarker.activeSelf)
		{
			hitMarker.SetActive(value: false);
		}
	}

	private bool CheckObsticle(Vector3 startPoint, Vector3 endPoint, out Vector3 hitPoint)
	{
		bool flag = character.HasNearByHalfObsticle();
		Vector3 vector = endPoint - startPoint;
		float magnitude = vector.magnitude;
		vector.Normalize();
		int num = Physics.SphereCastNonAlloc(startPoint, 0.15f, vector, raycastHits, magnitude, hitLayers, QueryTriggerInteraction.Ignore);
		if (num > 0)
		{
			_ = Vector3.zero;
			float num2 = float.MaxValue;
			bool flag2 = false;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHits[i];
				DamageReceiver component = raycastHit.collider.GetComponent<DamageReceiver>();
				if (flag && (GameplayDataSettings.LayersData.IsLayerInLayerMask(raycastHit.collider.gameObject.layer, GameplayDataSettings.Layers.halfObsticleLayer) || ((bool)component && component.isHalfObsticle)))
				{
					continue;
				}
				if ((bool)component && (bool)component.health)
				{
					CharacterMainControl characterMainControl = component.health.TryGetCharacter();
					if (characterMainControl != null && (bool)characterMainControl.characterModel && characterMainControl.characterModel.invisable)
					{
						continue;
					}
				}
				if (!(raycastHit.collider.gameObject == character.mainDamageReceiver.gameObject) && raycastHit.distance != 0f)
				{
					flag2 = true;
					if (raycastHit.distance < num2)
					{
						_ = raycastHit.point;
						num2 = raycastHit.distance;
					}
				}
			}
			if (flag2)
			{
				hitPoint = startPoint + vector * num2;
				return true;
			}
		}
		hitPoint = Vector3.zero;
		return false;
	}
}
