using System.Collections.Generic;
using UnityEngine;

public class MoveVisual : MonoBehaviour
{
	[SerializeField]
	private CharacterModel characterModel;

	public List<ParticleSystem> runParticles;

	private bool running;

	private CharacterMainControl Character
	{
		get
		{
			if (!characterModel)
			{
				return null;
			}
			return characterModel.characterMainControl;
		}
	}

	private void Awake()
	{
		foreach (ParticleSystem runParticle in runParticles)
		{
			ParticleSystem.EmissionModule emission = runParticle.emission;
			emission.enabled = running;
		}
	}

	private void Update()
	{
		if (!Character || Character.Running == running)
		{
			return;
		}
		running = Character.Running;
		foreach (ParticleSystem runParticle in runParticles)
		{
			ParticleSystem.EmissionModule emission = runParticle.emission;
			emission.enabled = running;
		}
	}
}
