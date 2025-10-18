using UnityEngine;

public class AISpecialAttachmentBase : MonoBehaviour
{
	public AICharacterController aiCharacterController;

	public CharacterMainControl character;

	public void Init(AICharacterController _ai, CharacterMainControl _character)
	{
		aiCharacterController = _ai;
		character = _character;
		OnInited();
	}

	protected virtual void OnInited()
	{
	}
}
