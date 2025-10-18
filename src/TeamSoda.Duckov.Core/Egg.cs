using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using UnityEngine;

public class Egg : MonoBehaviour
{
	public GameObject spawnFx;

	public CharacterMainControl fromCharacter;

	public Rigidbody rb;

	private float life;

	private CharacterRandomPreset characterPreset;

	private bool inited;

	private float timer;

	private bool spawned;

	private void Start()
	{
	}

	public void Init(Vector3 spawnPosition, Vector3 spawnVelocity, CharacterMainControl _fromCharacter, CharacterRandomPreset preset, float _life)
	{
		characterPreset = preset;
		base.transform.position = spawnPosition;
		if ((bool)rb)
		{
			rb.position = spawnPosition;
			rb.velocity = spawnVelocity;
		}
		fromCharacter = _fromCharacter;
		life = _life;
		inited = true;
	}

	private async UniTaskVoid Spawn()
	{
		if ((bool)spawnFx)
		{
			Object.Instantiate(spawnFx, base.transform.position, Quaternion.identity);
		}
		if (!fromCharacter)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_ = fromCharacter.IsMainCharacter;
		CharacterMainControl obj = await characterPreset.CreateCharacterAsync(base.transform.position + Vector3.down * 0.25f, Vector3.forward, MultiSceneCore.MainScene.Value.buildIndex, null, isLeader: false);
		AICharacterController componentInChildren = obj.GetComponentInChildren<AICharacterController>();
		obj.SetPosition(base.transform.position + Vector3.down * 0.25f);
		if ((bool)componentInChildren)
		{
			PetAI component = componentInChildren.GetComponent<PetAI>();
			if ((bool)component)
			{
				component.SetMaster(fromCharacter);
			}
			componentInChildren.leader = fromCharacter;
			if ((bool)fromCharacter)
			{
				componentInChildren.CharacterMainControl.SetTeam(fromCharacter.Team);
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (inited)
		{
			timer += Time.deltaTime;
			if (timer > life && !spawned)
			{
				spawned = true;
				Spawn().Forget();
			}
		}
	}
}
