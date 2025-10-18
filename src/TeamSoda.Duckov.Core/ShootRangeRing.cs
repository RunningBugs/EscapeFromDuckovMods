using UnityEngine;

public class ShootRangeRing : MonoBehaviour
{
	private CharacterMainControl character;

	public MeshRenderer ringRenderer;

	private ItemAgent_Gun gunAgent;

	private void Awake()
	{
	}

	private void Update()
	{
		if (!character)
		{
			character = LevelManager.Instance.MainCharacter;
			character.OnHoldAgentChanged += OnAgentChanged;
			OnAgentChanged(character.CurrentHoldItemAgent);
		}
		else if (ringRenderer.gameObject.activeInHierarchy && !gunAgent)
		{
			ringRenderer.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if ((bool)character)
		{
			base.transform.rotation = Quaternion.LookRotation(character.CurrentAimDirection, Vector3.up);
			base.transform.position = character.transform.position;
		}
	}

	private void OnDestroy()
	{
		if ((bool)character)
		{
			character.OnHoldAgentChanged -= OnAgentChanged;
		}
	}

	private void OnAgentChanged(DuckovItemAgent agent)
	{
		if (!(agent == null))
		{
			gunAgent = character.GetGun();
			if ((bool)gunAgent)
			{
				ringRenderer.gameObject.SetActive(value: true);
				ringRenderer.transform.localScale = Vector3.one * character.GetAimRange() * 0.5f;
			}
			else
			{
				ringRenderer.gameObject.SetActive(value: false);
			}
		}
	}
}
