using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Carriable : MonoBehaviour
{
	private CA_Carry carrier;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private float selfWeight;

	public InteractableLootbox lootbox;

	private bool droping;

	private float startDropTime = -1f;

	private bool carring;

	private Inventory inventory
	{
		get
		{
			if (lootbox == null)
			{
				return null;
			}
			return lootbox.Inventory;
		}
	}

	public float GetWeight()
	{
		if ((bool)inventory)
		{
			return inventory.CachedWeight + selfWeight;
		}
		return selfWeight;
	}

	public void Take(CA_Carry _carrier)
	{
		if ((bool)_carrier)
		{
			if ((bool)carrier)
			{
				carrier.StopAction();
			}
			droping = false;
			carrier = _carrier;
			if ((bool)inventory)
			{
				inventory.RecalculateWeight();
			}
			rb.transform.SetParent(carrier.characterController.modelRoot);
			rb.velocity = Vector3.zero;
			rb.transform.position = carrier.characterController.modelRoot.TransformPoint(carrier.carryPoint);
			rb.transform.localRotation = Quaternion.identity;
			SetRigidbodyActive(active: false);
		}
	}

	private void SetRigidbodyActive(bool active)
	{
		if (active)
		{
			rb.isKinematic = false;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			if ((bool)lootbox && (bool)lootbox.interactCollider)
			{
				lootbox.interactCollider.isTrigger = false;
			}
		}
		else
		{
			rb.isKinematic = true;
			rb.interpolation = RigidbodyInterpolation.None;
			if ((bool)lootbox && (bool)lootbox.interactCollider)
			{
				lootbox.interactCollider.isTrigger = true;
			}
		}
	}

	public void Drop()
	{
		if (carrier.Running)
		{
			carrier.StopAction();
		}
		carrier = null;
		MultiSceneCore.MoveToActiveWithScene(rb.gameObject, SceneManager.GetActiveScene().buildIndex);
		DropTask().Forget();
	}

	public void OnCarriableUpdate(float deltaTime)
	{
		if ((bool)carrier)
		{
			Vector3 position = carrier.characterController.modelRoot.TransformPoint(carrier.carryPoint);
			if ((bool)carrier.characterController.RightHandSocket)
			{
				position.y = carrier.characterController.RightHandSocket.transform.position.y + carrier.carryPoint.y;
			}
			rb.transform.position = position;
		}
	}

	private async UniTaskVoid DropTask()
	{
		startDropTime = Time.time;
		droping = true;
		SetRigidbodyActive(active: true);
		rb.velocity = base.transform.forward * 1.5f + base.transform.up * 0.5f;
		while (Time.time - startDropTime < 3f)
		{
			await UniTask.WaitForEndOfFrame(this);
		}
		droping = false;
		SetRigidbodyActive(active: false);
	}
}
