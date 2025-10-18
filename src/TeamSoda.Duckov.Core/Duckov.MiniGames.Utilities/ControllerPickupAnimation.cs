using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Duckov.MiniGames.Utilities;

public class ControllerPickupAnimation : MonoBehaviour
{
	[SerializeField]
	private Transform restTransform;

	[SerializeField]
	private Transform controllerTransform;

	[SerializeField]
	private float transitionTime = 1f;

	[SerializeField]
	private AnimationCurve pickupCurve;

	private int activeToken;

	private AnimationCurve pickupRotCurve => pickupCurve;

	private AnimationCurve pickupPosCurve => pickupCurve;

	private AnimationCurve putDownCurve => pickupCurve;

	public async UniTask PickUp(Transform endTransform)
	{
		if (controllerTransform == null)
		{
			return;
		}
		int token = ++activeToken;
		controllerTransform.DOKill();
		Vector3 fromPos = controllerTransform.position;
		Quaternion fromRot = controllerTransform.rotation;
		Vector3 toPos = endTransform.position;
		Quaternion toRot = endTransform.rotation;
		float time = 0f;
		while (time < transitionTime)
		{
			time += Time.deltaTime;
			float time2 = time / transitionTime;
			Vector3 position = Vector3.LerpUnclamped(fromPos, toPos, pickupPosCurve.Evaluate(time2));
			Quaternion rotation = Quaternion.SlerpUnclamped(fromRot, toRot, pickupRotCurve.Evaluate(time2));
			controllerTransform.SetPositionAndRotation(position, rotation);
			await UniTask.Yield();
			if (token != activeToken)
			{
				return;
			}
		}
		await controllerTransform.DOShakeRotation(0.4f, 10f);
		controllerTransform.SetPositionAndRotation(toPos, toRot);
	}

	public async UniTask PutDown()
	{
		if (controllerTransform == null)
		{
			return;
		}
		int token = ++activeToken;
		controllerTransform.DOKill();
		Vector3 fromPos = controllerTransform.position;
		Quaternion fromRot = controllerTransform.rotation;
		Vector3 toPos = restTransform.position;
		Quaternion toRot = restTransform.rotation;
		float time = 0f;
		while (time < transitionTime)
		{
			if (controllerTransform == null)
			{
				return;
			}
			time += Time.deltaTime;
			float time2 = time / transitionTime;
			Vector3 position = Vector3.LerpUnclamped(fromPos, toPos, pickupPosCurve.Evaluate(time2));
			Quaternion rotation = Quaternion.LerpUnclamped(fromRot, toRot, pickupRotCurve.Evaluate(time2));
			controllerTransform.SetPositionAndRotation(position, rotation);
			await UniTask.Yield();
			if (token != activeToken || controllerTransform == null)
			{
				return;
			}
		}
		controllerTransform.SetPositionAndRotation(toPos, toRot);
	}
}
