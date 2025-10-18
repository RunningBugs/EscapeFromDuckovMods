using System.Collections.Generic;
using EPOOutline;
using UnityEngine;

public class HalfObsticle : MonoBehaviour
{
	public Outlinable outline;

	public HealthSimpleBase health;

	public List<GameObject> parts;

	public GameObject defaultVisuals;

	public GameObject deadVisuals;

	public Collider airWallCollider;

	private bool dead;

	private void Awake()
	{
		outline.enabled = false;
		defaultVisuals.SetActive(value: true);
		deadVisuals.SetActive(value: false);
		health.OnDeadEvent += Dead;
		if ((bool)airWallCollider)
		{
			airWallCollider.gameObject.SetActive(value: true);
		}
	}

	private void OnValidate()
	{
	}

	public void Dead(DamageInfo dmgInfo)
	{
		if (!dead)
		{
			dead = true;
			defaultVisuals.SetActive(value: false);
			deadVisuals.SetActive(value: true);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		CharacterMainControl component = other.GetComponent<CharacterMainControl>();
		if ((bool)component)
		{
			component.AddnearByHalfObsticles(parts);
			if (component.IsMainCharacter)
			{
				outline.enabled = true;
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		CharacterMainControl component = other.GetComponent<CharacterMainControl>();
		if ((bool)component)
		{
			component.RemoveNearByHalfObsticles(parts);
			if (component.IsMainCharacter)
			{
				outline.enabled = false;
			}
		}
	}
}
