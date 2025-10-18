using UnityEngine;

public class LittleMoveHUD : MonoBehaviour
{
	private Camera camera;

	private CharacterMainControl character;

	public float maxDistance = 2f;

	public float smoothTime;

	private Vector3 worldPos;

	private Vector3 velocityTemp;

	public Vector3 offset;

	private void LateUpdate()
	{
		if (!character)
		{
			if ((bool)LevelManager.Instance)
			{
				character = LevelManager.Instance.MainCharacter;
			}
			if (!character)
			{
				return;
			}
		}
		if (!camera)
		{
			camera = Camera.main;
			if (!camera)
			{
				return;
			}
		}
		Vector3 vector = character.transform.position + offset;
		worldPos = Vector3.SmoothDamp(worldPos, vector, ref velocityTemp, smoothTime);
		if (Vector3.Distance(worldPos, vector) > maxDistance)
		{
			worldPos = (worldPos - vector).normalized * maxDistance + vector;
		}
		Vector3 position = camera.WorldToScreenPoint(worldPos);
		base.transform.position = position;
	}
}
