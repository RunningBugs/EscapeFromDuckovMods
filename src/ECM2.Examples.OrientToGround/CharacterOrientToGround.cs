using UnityEngine;

namespace ECM2.Examples.OrientToGround;

public class CharacterOrientToGround : MonoBehaviour, IColliderFilter
{
	public float maxSlopeAngle = 30f;

	public float alignRate = 10f;

	public float rayOffset = 0.1f;

	public LayerMask groundMask = 1;

	[Space(15f)]
	public bool drawRays = true;

	private readonly RaycastHit[] _hits = new RaycastHit[8];

	private Character _character;

	public bool Filter(Collider otherCollider)
	{
		CharacterMovement characterMovement = _character.GetCharacterMovement();
		if (otherCollider == characterMovement.collider)
		{
			return true;
		}
		return false;
	}

	private Vector3 ComputeAverageNormal()
	{
		CharacterMovement characterMovement = _character.GetCharacterMovement();
		Vector3 up = Vector3.up;
		Vector3 vector = _character.GetPosition() + up * (characterMovement.height * 0.5f);
		Vector3 direction = -up;
		float height = characterMovement.height;
		LayerMask layerMask = groundMask;
		Vector3 vector2 = Vector3.zero;
		float num = 0f - rayOffset;
		float num2 = 0f - rayOffset;
		int num3 = 0;
		for (int i = 0; i < 3; i++)
		{
			num2 = 0f - rayOffset;
			for (int j = 0; j < 3; j++)
			{
				if (CollisionDetection.Raycast(vector + new Vector3(num, 0f, num2), direction, height, layerMask, QueryTriggerInteraction.Ignore, out var closestHit, _hits, this) > 0 && Vector3.Angle(closestHit.normal, up) < maxSlopeAngle)
				{
					vector2 += closestHit.normal;
					if (drawRays)
					{
						Debug.DrawRay(closestHit.point, closestHit.normal, Color.yellow);
					}
					num3++;
				}
				num2 += rayOffset;
			}
			num += rayOffset;
		}
		if (num3 > 0)
		{
			vector2 /= (float)num3;
		}
		else
		{
			vector2 = up;
		}
		if (drawRays)
		{
			Debug.DrawRay(_character.GetPosition(), vector2 * 2f, Color.green);
		}
		return vector2;
	}

	private void OnAfterSimulationUpdated(float deltaTime)
	{
		Vector3 toDirection = (_character.IsWalking() ? ComputeAverageNormal() : Vector3.up);
		Quaternion rotation = _character.GetRotation();
		Quaternion quaternion = Quaternion.FromToRotation(rotation * Vector3.up, toDirection);
		rotation = Quaternion.Slerp(rotation, quaternion * rotation, alignRate * deltaTime);
		_character.SetRotation(rotation);
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void OnEnable()
	{
		_character.AfterSimulationUpdated += OnAfterSimulationUpdated;
	}

	private void OnDisable()
	{
		_character.AfterSimulationUpdated -= OnAfterSimulationUpdated;
	}
}
