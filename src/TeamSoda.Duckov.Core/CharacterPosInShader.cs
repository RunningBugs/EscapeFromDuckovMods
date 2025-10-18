using UnityEngine;

public class CharacterPosInShader : MonoBehaviour
{
	private int characterPosHash = Shader.PropertyToID("CharacterPos");

	private void Update()
	{
		if ((bool)CharacterMainControl.Main)
		{
			Shader.SetGlobalVector(characterPosHash, CharacterMainControl.Main.transform.position);
		}
	}
}
