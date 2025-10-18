using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Duckov.Scenes;

public class MultiSceneTeleporter : InteractableBase
{
	[SerializeField]
	private MultiSceneLocation target;

	[SerializeField]
	private bool teleportOnTriggerEnter;

	private const float CoolDown = 1f;

	private static float timeWhenTeleportFinished;

	protected override bool ShowUnityEvents => false;

	[SerializeField]
	public MultiSceneLocation Target => target;

	private static float TimeSinceTeleportFinished => Time.time - timeWhenTeleportFinished;

	private static bool Teleportable => TimeSinceTeleportFinished > 1f;

	protected override void Awake()
	{
		base.Awake();
	}

	private void OnDrawGizmosSelected()
	{
		Transform locationTransform = target.LocationTransform;
		if ((bool)locationTransform)
		{
			Gizmos.DrawLine(base.transform.position, locationTransform.position);
			Gizmos.DrawWireSphere(locationTransform.position, 0.25f);
		}
	}

	public void DoTeleport()
	{
		if (!Teleportable)
		{
			Debug.Log("not Teleportable");
		}
		else
		{
			TeleportTask().Forget();
		}
	}

	protected override bool IsInteractable()
	{
		return Teleportable;
	}

	private async UniTask TeleportTask()
	{
		timeWhenTeleportFinished = Time.time;
		await MultiSceneCore.Instance.LoadAndTeleport(target);
		timeWhenTeleportFinished = Time.time;
	}

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		coolTime = 2f;
		finishWhenTimeOut = true;
	}

	protected override void OnInteractFinished()
	{
		DoTeleport();
		StopInteract();
	}
}
