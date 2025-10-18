using UnityEngine;

public class MoveRing : MonoBehaviour
{
	public Renderer ring;

	public float runThreshold;

	private Material ringMat;

	private InputManager inputManager;

	private CharacterMainControl character => inputManager.characterMainControl;

	public void SetThreshold(float threshold)
	{
		runThreshold = threshold;
	}

	public void LateUpdate()
	{
		if (!inputManager)
		{
			if (!(LevelManager.Instance == null))
			{
				inputManager = LevelManager.Instance.InputManager;
			}
			return;
		}
		if (!character)
		{
			SetMove(Vector3.zero, 0f);
			return;
		}
		base.transform.position = character.transform.position + Vector3.up * 0.02f;
		SetThreshold(inputManager.runThreshold);
		SetMove(inputManager.WorldMoveInput.normalized, inputManager.WorldMoveInput.magnitude);
		SetRunning(character.Running);
		if (ring.enabled != character.gameObject.activeInHierarchy)
		{
			ring.enabled = character.gameObject.activeInHierarchy;
		}
	}

	public void SetMove(Vector3 direction, float value)
	{
		if (!ringMat)
		{
			if ((bool)ring)
			{
				ringMat = ring.material;
			}
		}
		else
		{
			ringMat.SetVector("_Direction", direction);
			ringMat.SetFloat("_Distance", value);
			ringMat.SetFloat("_Threshold", runThreshold);
		}
	}

	public void SetRunning(bool running)
	{
		ringMat.SetFloat("_Running", running ? 1 : 0);
	}
}
