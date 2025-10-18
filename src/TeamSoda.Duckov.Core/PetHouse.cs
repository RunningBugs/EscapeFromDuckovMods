using UnityEngine;

public class PetHouse : MonoBehaviour
{
	private static PetHouse instance;

	public Transform petMarker;

	private PetAI petTarget;

	public static PetHouse Instance => instance;

	private void Awake()
	{
		instance = this;
		if (LevelManager.LevelInited)
		{
			OnLevelInited();
		}
		else
		{
			LevelManager.OnLevelInitialized += OnLevelInited;
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnLevelInited;
		if ((bool)petTarget)
		{
			petTarget.SetStandBy(_standBy: false, petTarget.transform.position);
		}
	}

	private void OnLevelInited()
	{
		CharacterMainControl petCharacter = LevelManager.Instance.PetCharacter;
		petCharacter.SetPosition(petMarker.position);
		petTarget = petCharacter.GetComponentInChildren<PetAI>();
		if (petTarget != null)
		{
			petTarget.SetStandBy(_standBy: true, petMarker.position);
		}
	}
}
