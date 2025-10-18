using UnityEngine;

namespace ECM2.Walkthrough.Ex81;

public class ClickToMove : MonoBehaviour
{
	public Camera mainCamera;

	public Character character;

	public LayerMask groundMask;

	private NavMeshCharacter _navMeshCharacter;

	private void Awake()
	{
		_navMeshCharacter = character.GetComponent<NavMeshCharacter>();
	}

	private void Update()
	{
		if (Input.GetMouseButton(0) && Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity, groundMask))
		{
			_navMeshCharacter.MoveToDestination(hitInfo.point);
		}
	}
}
