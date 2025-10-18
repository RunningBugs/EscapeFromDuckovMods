using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class SimpleTeleporter : InteractableBase
{
	public enum TransitionTypes
	{
		volumeFx,
		blackScreen
	}

	public Transform target;

	[SerializeField]
	private Transform selfTeleportPoint;

	[SerializeField]
	private TransitionTypes transitionType;

	[FormerlySerializedAs("fxTime")]
	public float transitionTime = 0.28f;

	private float delay = 0.3f;

	public Volume teleportVolume;

	private int fxShaderID = Shader.PropertyToID("TeleportFXStrength");

	private bool blackScreen;

	public Transform TeleportPoint
	{
		get
		{
			if (!selfTeleportPoint)
			{
				return base.transform;
			}
			return selfTeleportPoint;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		teleportVolume.gameObject.SetActive(value: false);
	}

	protected override void OnInteractFinished()
	{
		if ((bool)interactCharacter)
		{
			Teleport(interactCharacter).Forget();
		}
	}

	private async UniTask Teleport(CharacterMainControl targetCharacter)
	{
		switch (transitionType)
		{
		case TransitionTypes.volumeFx:
			VolumeFx(show: true, transitionTime).Forget();
			break;
		case TransitionTypes.blackScreen:
			blackScreen = true;
			BlackScreen.ShowAndReturnTask(null, 0f, transitionTime);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		await UniTask.WaitForSeconds(transitionTime + delay, ignoreTimeScale: true);
		if (targetCharacter != null)
		{
			targetCharacter.SetPosition(target.position);
			if ((bool)LevelManager.Instance)
			{
				LevelManager.Instance.GameCamera.ForceSyncPos();
			}
		}
		switch (transitionType)
		{
		case TransitionTypes.volumeFx:
			VolumeFx(show: false, transitionTime).Forget();
			break;
		case TransitionTypes.blackScreen:
			BlackScreen.HideAndReturnTask(null, 0f, transitionTime);
			blackScreen = false;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private async UniTask VolumeFx(bool show, float time)
	{
		float startTime = Time.time;
		bool end = false;
		teleportVolume.priority = 9999f;
		teleportVolume.gameObject.SetActive(value: true);
		while (!end)
		{
			float num = Time.time - startTime;
			float num2 = Mathf.Clamp01(num / time);
			if (!show)
			{
				num2 = 1f - num2;
			}
			teleportVolume.weight = num2;
			Shader.SetGlobalFloat(fxShaderID, num2);
			if (num > time)
			{
				if (!show)
				{
					teleportVolume.gameObject.SetActive(value: false);
				}
				end = true;
			}
			await UniTask.Yield();
		}
	}
}
