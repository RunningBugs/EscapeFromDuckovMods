using UnityEngine;

public abstract class CharacterSpawnerComponentBase : MonoBehaviour
{
	public abstract void Init(CharacterSpawnerRoot root);

	public abstract void StartSpawn();
}
