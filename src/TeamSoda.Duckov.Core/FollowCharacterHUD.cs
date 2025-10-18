using System;
using UnityEngine;

public class FollowCharacterHUD : MonoBehaviour
{
	public float maxDistance = 2f;

	public float smoothTime;

	private Vector3 worldPos;

	private Vector3 velocityTemp;

	public Vector3 offset;

	private void Awake()
	{
		GameCamera.OnCameraPosUpdate = (Action<GameCamera, CharacterMainControl>)Delegate.Combine(GameCamera.OnCameraPosUpdate, new Action<GameCamera, CharacterMainControl>(UpdatePos));
	}

	private void OnDestroy()
	{
		GameCamera.OnCameraPosUpdate = (Action<GameCamera, CharacterMainControl>)Delegate.Remove(GameCamera.OnCameraPosUpdate, new Action<GameCamera, CharacterMainControl>(UpdatePos));
	}

	private void UpdatePos(GameCamera gameCamera, CharacterMainControl target)
	{
		Camera renderCamera = gameCamera.renderCamera;
		Vector3 vector = target.transform.position + offset;
		worldPos = Vector3.SmoothDamp(worldPos, vector, ref velocityTemp, smoothTime);
		if (Vector3.Distance(worldPos, vector) > maxDistance)
		{
			worldPos = (worldPos - vector).normalized * maxDistance + vector;
		}
		Vector3 position = renderCamera.WorldToScreenPoint(worldPos);
		base.transform.position = position;
		if (target.gameObject.activeInHierarchy != base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(target.gameObject.activeInHierarchy);
		}
	}
}
