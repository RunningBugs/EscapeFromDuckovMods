using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class Zone : MonoBehaviour
{
	public bool onlyPlayerTeam;

	private HashSet<Health> healths;

	public bool setActiveByDistance = true;

	private Rigidbody rb;

	private int sceneBuildIndex = -1;

	public HashSet<Health> Healths => healths;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		healths = new HashSet<Health>();
		rb.isKinematic = true;
		rb.useGravity = false;
		sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
		if (setActiveByDistance)
		{
			SetActiveByPlayerDistance.Register(base.gameObject, sceneBuildIndex);
		}
	}

	private void OnDestroy()
	{
		if (setActiveByDistance)
		{
			SetActiveByPlayerDistance.Unregister(base.gameObject, sceneBuildIndex);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (LevelManager.LevelInited && other.gameObject.layer == LayerMask.NameToLayer("Character"))
		{
			Health component = other.GetComponent<Health>();
			if (!(component == null) && (!onlyPlayerTeam || component.team == Teams.player) && !healths.Contains(component))
			{
				healths.Add(component);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Character"))
		{
			Health component = other.GetComponent<Health>();
			if (!(component == null) && (!onlyPlayerTeam || component.team == Teams.player) && healths.Contains(component))
			{
				healths.Remove(component);
			}
		}
	}

	private void OnDisable()
	{
		healths.Clear();
	}
}
